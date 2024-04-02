using System;
using System.Collections.Generic;
using GameComponents;
using GameComponents.DirectionUtility;
using UtilityCollections;

namespace Forays {
	public class DungeonMap : GameObject {
		const int Width = GameUniverse.MapWidth;
		const int Height = GameUniverse.MapHeight;

		public DungeonLevelType CurrentLevelType; //todo temporary

		public RNG MapRNG => GameUniverse.MapRNG;
		[Obsolete("CAUTION: This is the regular RNG, not the mapgen RNG.")]
		new public RNG R => GameUniverse.R;

		///<summary>CurrentDepthSeed is grabbed from MapRNG at level creation, to allow for
		/// deterministic effects that don't change as the RNG state changes.</summary>
		public ulong CurrentDepthSeed;

		public Grid<Creature, Point> Creatures;
		//public List<CreatureGroup> CreatureGroups;
		private PointArray<TileType> Tiles;

		private FeatureMap Features;

		public Dictionary<Point, Trap> Traps;
		public bool CellIsTrapped(Point p) => Traps.ContainsKey(p);
		//todo: shrines. i think they'll be handled similarly to traps now, with a dictionary and a struct. (idols could get the same treatment, or not.)

		public Grid<Item, Point> Items;

		///<summary>Used to mark cells that automatically fail LOS checks, as an optimization</summary>
		public PointArray<bool> NeverInLineOfSight;

		public LightMap Light;
		//todo, fire & ice events could work like this:
		//  just track whether we're jumping OVER a multiple of 120 (or whatever the turn counter is)
		// if so,stop for a moment.

		///<summary>Track which direction the player last exited each cell. Helps the AI track the player.</summary>
		public PointArray<Dir8> DirectionPlayerExited;

		//footsteps tracked here or elsewhere?
		//aesthetic features here, or are those strictly presentation with no game effect?
			// I think those are in-game. Those are something that, let's say, an AI-controlled player might consider,
			// much like the number of times a wand has been used. Those aren't things that the AI or the UI should need to track.
		//track burning objects here?

		public int DangerModifier;
		public PointArray<bool> Seen;

		public DungeonMap(GameUniverse g) : base(g) {
			CurrentDepthSeed = MapRNG.GetNext();
			Func<Point, bool> isInBounds = p => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
			Creatures = new Grid<Creature, Point>(isInBounds);
			Tiles = new PointArray<TileType>(Width, Height);
			Features = new FeatureMap(Width, Height);
			Traps = new Dictionary<Point, Trap>();
			Items = new Grid<Item, Point>(isInBounds);
			NeverInLineOfSight = new PointArray<bool>(Width, Height);
			Light = new LightMap(g);
			DirectionPlayerExited = new PointArray<Dir8>(Width, Height);
			cellsVisibleToPlayer = new EasyHashSet<Point>();
			cellsLitToPlayer = new EasyHashSet<Point>();
			creaturesVisibleToPlayer = new List<Creature>();
			//todo, more here?
			Seen = new PointArray<bool>(Width, Height);
		}
		public IEnumerable<Point> GetAllPoints(bool includeEdges){
			int startX = 0, startY = 0, endX = Width, endY = Height;
			if(!includeEdges){
				startX = 1;
				startY = 1;
				endX = Width - 1;
				endY = Height - 1;
			}
			for(int x=startX;x<endX;++x){
				for(int y = startY; y<endY; ++y) {
					yield return new Point(x, y);
				}
			}
		}
		public bool CellIsPassable(Point p){ // will get optional flags param if needed
			//check everything that could block a cell, which currently is probably just the tile type
			TileType type = Tiles[p];
			return TileDefinition.IsPassable(type);
		}
		public bool CellIsOpaque(int x, int y) => CellIsOpaque(new Point(x, y));
		public bool CellIsOpaque(Point p){
			TileType type = Tiles[p];
			if(TileDefinition.IsOpaque(type)) return true;
			if(Features[p].IsOpaque()) return true;
			return false;
		}
		///<summary>Hold player visibility data changes so that the game state can be updated. Must
		/// call ResumeVisibilityUpdates when done. Multiple calls will stack on each other correctly.</summary>
		public void HoldVisibilityUpdates(){
			visibilityHolds++;
		}
		///<summary>Removes a single 'hold'. If there are no more holds, immediately recalculate player visibility data.</summary>
		public void ResumeVisibilityUpdates(){
			visibilityHolds--;
			if(visibilityHolds > 0) return;
			if(visibilityHolds < 0) throw new InvalidOperationException("Resume called without Hold");

			cellsVisibleToPlayer = new EasyHashSet<Point>();
			cellsLitToPlayer = new EasyHashSet<Point>();
			creaturesVisibleToPlayer = new List<Creature>();

			for(int i = 0; i < GameUniverse.MapHeight; i++) {
				for(int j = 0; j < GameUniverse.MapWidth; j++) {
					Point p = new Point(j, i);
					Creature creature = CreatureAt(p);
					if(creature != null && Player.CanSee(creature)){
						creaturesVisibleToPlayer.Add(creature);
					}
					if(!NeverInLineOfSight[p] && CheckLOS(Player.Position, p)){
						Seen[p] = true;
						cellsVisibleToPlayer.Add(p);
						if(Light.CellAppearsLitToObserver(p, Player.Position)){
							cellsLitToPlayer.Add(p);
						}
					}
				}
			}
			//todo: make sure this is also called for these things:
			// - status effects which alter player vis
			// - picking up/dropping/creating items which have light radius
			//	 - mostly covered by light radius, EXCEPT:
			// - items that grant sight directly
			// - creatures leaving the map (burrowing) - player shouldn't see it until it reappears.
		}
		// Note that these are just cached, not serialized:
		private int visibilityHolds;
		private EasyHashSet<Point> cellsVisibleToPlayer;
		private EasyHashSet<Point> cellsLitToPlayer;
		private List<Creature> creaturesVisibleToPlayer;
		public bool CellVisibleToPlayer(Point p) => cellsVisibleToPlayer.Contains(p);
		public bool CellLitToPlayer(Point p) => cellsLitToPlayer.Contains(p);
		public bool CreatureVisibleToPlayer(Creature creature) => creaturesVisibleToPlayer.Contains(creature);

		public TileType GetTile(Point p) => Tiles[p];
		public void SetTile(Point p, TileType tileType){
			TileType oldType = Tiles[p];
			if(oldType == tileType) return;
			bool needVisUpdate = TileDefinition.IsOpaque(oldType) != TileDefinition.IsOpaque(tileType);
			if(needVisUpdate) Light.UpdateBeforeOpacityChange(p);
			Tiles[p] = tileType;
			if(needVisUpdate) Light.UpdateAfterOpacityChange();
		}

		public FeatureType GetFeatures(Point p) => Features[p];
		public void AddFeature(Point p, FeatureType feature){
			if(Features.ExistsAt(p, feature)) return;
			bool needVisUpdate = FeatureDefinition.IsOpaque(feature);
			if(needVisUpdate) Light.UpdateBeforeOpacityChange(p);
			Features.Add(p, feature);
			if(needVisUpdate) Light.UpdateAfterOpacityChange();
		}
		public void RemoveFeature(Point p, FeatureType feature){
			if(!Features.ExistsAt(p, feature)) return;
			bool needVisUpdate = FeatureDefinition.IsOpaque(feature);
			if(needVisUpdate) Light.UpdateBeforeOpacityChange(p);
			Features.Remove(p, feature);
			if(needVisUpdate) Light.UpdateAfterOpacityChange();
		}
		public bool CheckLOS(Point source, Point destination){
			if(CellIsOpaque(destination)){
				Point[] neighbors = destination.GetNeighborsBetween(source);
				for(int i=0;i<neighbors.Length;++i){
					if(CellIsOpaque(neighbors[i])) continue;
					if(CheckReciprocalBresenhamLineOfSight(source, neighbors[i])) return true;
				}
				return false;
			}
			else return CheckReciprocalBresenhamLineOfSight(source, destination);
		}
		public bool CheckReciprocalBresenhamLineOfSight(Point source, Point destination){
			int x1 = source.X;
			int y1 = source.Y;
			int x2 = destination.X;
			int y2 = destination.Y;
			int dx = Math.Abs(x2 - x1);
			int dy = Math.Abs(y2 - y1);
			int incrementX = x1 == x2? 0 : x1 < x2? 1 : -1;
			int incrementY = y1 == y2? 0 : y1 < y2? 1 : -1;
			if(dx <= 1 && dy <= 1) return true; // Automatically pass the check for anything in the same or adjacent cells.
			// Next, handle simple cases that don't need Bresenham at all. These cases correspond to straight lines in the 8 directions:
			if(dx == 0 || dy == 0 || (y1+x1 == y2+x2) || (y1-x1 == y2-x2)){ // (if slope is undefined, 0, -1, or 1)
				do{
					x1 += incrementX; // Increment first, so that the opacity of 'source' is ignored.
					y1 += incrementY;
					if(CellIsOpaque(x1, y1)) return false;
				} while(x1 != x2 || y1 != y2);
				return true;
			}
			// If it wasn't a simple case, move on to reciprocal Bresenham:
			bool xMajor = (dx > dy);
			int er = 0; // error accumulator
			bool blockedA = false; // These 2 are used to track whether each of the 2 possible paths per line is blocked or not.
			bool blockedB = false; // (The result is equivalent to calculating 2 regular Bresenham lines, source->dest and dest->source.)
			if(xMajor){
				do{
					x1 += incrementX;
					er += dy;
					if(er<<1 > dx){
						y1 += incrementY;
						er -= dx;
					}
					if(CellIsOpaque(x1, y1)){
						if(blockedB || er<<1 != dx) return false;
						blockedA = true;
					}
					if(er<<1 == dx){ // This is the part that makes this reciprocal, by checking both options while crossing a corner.
						y1 += incrementY; // Increment Y, then check the new position:
						if(CellIsOpaque(x1, y1)){
							if(blockedA || er<<1 != dx) return false;
							blockedB = true;
						}
						er -= dx;
					}
				} while(x1 != x2);
			}
			else{ // Y-major
				do{
					y1 += incrementY;
					er += dx;
					if(er<<1 > dy){
						x1 += incrementX;
						er -= dy;
					}
					if(CellIsOpaque(x1, y1)){
						if(blockedB || er<<1 != dy) return false;
						blockedA = true;
					}
					if(er<<1 == dy){
						x1 += incrementX;
						if(CellIsOpaque(x1, y1)){
							if(blockedA || er<<1 != dy) return false;
							blockedB = true;
						}
						er -= dy;
					}
				} while(y1 != y2);
			}
			return true;
		}

		public void GenerateMap() { //todo, let's move the actual generation to another file/class
			CurrentLevelType = MapRNG.OneIn(4) ? DungeonLevelType.Cramped : DungeonLevelType.Sparse;
			int wallRarity = CurrentLevelType == DungeonLevelType.Cramped ? 6 : 20;
			int waterRarity = CurrentLevelType == DungeonLevelType.Cramped ? 50 : 8;
			string[][] tempMaps = new string[][]{
				new string[]{
"------------------------------------------------------------------",
"-------------------------------------------------#----------------",//for both melee and ranged:
"------------------------------------------------------------------",//far away with hazards in between
"-------------------------------------------------#----------------",//far away with hazards PLUS major hazards
"------------------------------------------------------------------",//far away STARTING IN HAZARD
"----------***-****-*****------------------------------------------",//
"---g-*-**-***-****-*****------------------------------------------",//
"-----*-**-***-****-*****----------------------------***-----***---",//for ranged:
"-----*-**-***-****-*****----------------------------**1-----*G*---",// too close starting in hazard
"-----*-**-***-****-*****------*---------------------***-----***---",//
"-----*-**-***-****-*****-----*-*----------------------------------",
"-----*-**-***-****-*****------*---------------------***-&&&-***---",
"---r-*-**-***-****-*****----------------------------**2-&R&-*R*---",
"-------**-***-****-*****----------------------------***-&&&-***---",
"----------***-****-*****------------------------------------------",
"--------------****-*****----------------------------*&*-&&&-&&&---",
"----------------------------------------------------*&1-&G&-&G&---",
"----------------------------------------------------*&*-&&&-&&*---",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"#>--------#------#BBBB-#------------------------------------------",
"###################-----------------------------------------------"
			},
				new string[]{
"------------------------------------------------------------------",
"-------------------------------------------------#----------------",
"-######.###############--------------------------#----------------",
"------------------------------------#####--------#----------------",
"#######.####################-----####---#################---------",
"--------------------------------------#----------#----------------",
"-######-#############-#-------###############----#----------------",
"----------------------#-------#------------------#----------------",
"-#-##################-#-------#-###########------#----------------",
"---------#------#-----###-----#-----------#------#----------------",
"-###-#####-####-#######-------###########-#------#----------------",
"##########-###-----#--###-----#---------#-#------#----------------",
"-------------------#--#-------#-#######-#-#------#----------------",
"---#######----BBBB------------#-#>------#-#------#----------------",
"#-----####-BBBBBBBBBBBBBBBBB--#-#########-#------#----------------",
"####--####-BBBBBBBBBBB--------#-----------#------#----------------",
"#-----####--BBBBBBBBB--#########################################--",
"#-##--####--BBB----BBBB#------------------------------------------",
"#--###-########-##-BBBB#------------------------------------------",
"##-###-##-#------#B-BBB#------------------------------------------",
"#>-#--###-#------#BBBB-#------------------------------------------",
"###################-----------------------------------------------"
			},
			new string[]{
"##################################################################",
"##########-############---###-###########-#------#-------------w5#",
"##############-ww5ww--#####5###---------#-#------#----------->-w5#",
"#-----#---------www---#---www-#-#######-#-#------####-############",
"#--####-######--------#-------#-#-------#-#------#,M,,,,,,M,,,M,,#",
"#######-######----------------###########-#------#,,,,M,M,,,M,,,M#",
"#---------------------#---------#----------------#,,M,,,,,,,,,M,,#",
"#---##########---##############-#######----------#M,,,,M,,M,M,,,M#",
"#---######---#---#-----########-##,,,M############,,,M,,,,,,,,,,,#",
"#w-----###---#---#-----#------#--,,M,,,---#--#----,M,,,,,M,,,,M,,#",
"#5w-##-###########-----#------####,,,,###-####-###,,,,M,,,,,M,,,M#",
"######-##-#------#-----#---------#,M,M#---,,,,--##M,M,,,M,,,,,M,,#",
"######-################----------######------www##,,,,M,,,M,M,,,M#",
"#----#--#---------------------------###------w5w##M,M,,,M,,,,,,,,#",
"#######.####################-----####-######-www#########,,M,,M,,#",
"#-----#-#---------#####---------------#----#----##------#M,,,,,,M#",
"#######-###########---######################----##------#,,,M,M,,#",
"#5------------------#-#-------#------------#----##------##########",
"##-##################-#-#-----#-##################---------------#",
"#-----####----------#---#---,,#-----------#------#---------------#",
"#-----#####------#--##5##--,,,#----------------------------------#",
"##################################################################"
			},
			new string[]{
"##################################################################",
"##############---####-#--#####,,#-##########--#####-#-I###i#######",
"#--------,,###--------#---------#--C--##------##------IIiiiiiWWW##",
"#----#####---##---------------#-----------##---#-#-----IIiiiiWW--#",
"#---##---#,-----------#---------#-------###----#---##--IIiiiii---#",
"#--####-####---------###------###-----#####----#-#######Iiiii----#",
"#---##---###---------####-------###---------####---##--IIiiiii--##",
"#--###########---##############-#######---###----#----IIiiiiiiI###",
"##-#######III#ii-###---###--###-##----#######--###################",
"##------##IIIIiiiiW#####--------#--------------#--#-------------##",
"##--##-#####IIIiiiiiWW##------####----###w####w###################",
"##---------##IIIiiiWW##---------#----wwWWWWiWWwWi##------------###",
"##--------###IIIIiiiWW#---------#----wwWiiiiiiWWiw-----####------#",
"#----#--#--######Iiiiii-------------###Wiii-iiiii#--------###--#>#",
"#-#####################--########----#iIII---III#########--#,,-,,#",
"#iii###----------######--#------#---##III-C----III#---##---#######",
"#iiiii####------##-##---##--,,###---------------I##--##---------##",
"###ii#####------#-------#---,,##-----------------#---##-----#---##",
"#iiiii####------#####---#---,,##---####---#-----##---#----###----#",
"#ii##i####----------#---#---,,###--#--#####-----##---###---------#",
"#iii#######------#--#####--,,,#-####------##--###------###-----###",
"##################################################################"
			},
			new string[]{
"------------------------------------------------------------------",
"-####---##-----------------#BBBBB-------###------#----------------",
"-####--.###############----#-BBBBBBBB------------#----------------",
"#--------------------------#--BBBBBB#####--------#----------------",
"#-###--.####################-BBBB####---#################---------",
"--------#-------------#----BBBBBB#----#-----######----------------",
"-###--#-#----########-#--BBB--###############--mm#################",
"---#--#-#----#BBBBB---#---BBB-#------#-------mmmm-----------------",
"#--####-#-####BBBB----#-------#-#----#--#-##--mmm#------mmmmmmm--#",
"#########-####----------------###########-##-mmmm#-----mmmmm->---#",
"#---------5#-#--------mmmmmmmm--#-----#-------mmm#------mmmmm----#",
"#---##########---mmmmmmmmmmmmm#-#######-----mmmm-#---------------#",
"----#-----#-----mmmmmmmmmmmmm---#########-#-mmmm------------------",
"---#####-------mmmmmmmmmmmmmm-#-#------##-#--mm--################-",
"#-----######-mmmmmmmmmmmmmmm--#-#########-#--mm--#----------------",
"####--####-#-mmmmmmmmmmm------#-----------#------#----------------",
"#-----####-#--mmmmmmmmm#-#######################################--",
"#-##--####-#--mmmmmmmmm#-B----#-----------------------------------",
"#--###-###########mmmmm#---B--#-----------------------------------",
"##-###-##-#------#######-----5#-----------------------------------",
"#--#--###-#------#-----#--BB--#-----------------------------------",
"###################-----------------------------------------------"
			}};

			string[] map = tempMaps[GameUniverse.CurrentDepth - 1];

			for(int x=0;x<Width;++x)
				for(int y = 0; y<Height; ++y) {
					if(x == 0 || y == 0 || x == Width-1 || y == Height-1)
						Tiles[x,y] = TileType.Wall;
					else if(map[y][x] == 'W'){
						Tiles[x,y] = TileType.DeepWater;
					}
					else if(map[y][x] == 'w'){
						Tiles[x,y] = TileType.Floor;
						Features.Add(x, y, FeatureType.Water);
					}
					else if(map[y][x] == 'I'){
						Tiles[x,y] = TileType.ThickIce;
					}
					else if(map[y][x] == 'i'){
						Tiles[x,y] = TileType.DeepWater;
						Features.Add(x, y, FeatureType.Ice);
					}
					else if(map[y][x] == '5'){
						Tiles[x,y] = TileType.Statue;
					}
					else if(map[y][x] == 'M'){
						Tiles[x,y] = TileType.GiantMushroom;
					}
					else if(map[y][x] == ','){
						Tiles[x,y] = TileType.GlowingFungus;
					}
					else if(map[y][x] == 'C'){
						Tiles[x,y] = TileType.LightCrystal;
					}
					else if(map[y][x] == '>'){
						Tiles[x,y] = TileType.Staircase;
					}
					else if(map[y][x] == 'B'){
						Tiles[x,y] = TileType.Brush;
					}
					else if(map[y][x] == 'm'){
						Tiles[x,y] = TileType.DeepMud;
					}
					else if(map[y][x] == '#'){
						Tiles[x,y] = TileType.Wall;
					}
					else if(map[y][x] == '*' || map[y][x] == '1' || map[y][x] == '2'){
						Tiles[x,y] = TileType.Floor;
						Features.Add(x, y, FeatureType.PoisonGas);
					}
					else if(map[y][x] == '&' || map[y][x] == 'G' || map[y][x] == 'R'){
						Tiles[x,y] = TileType.Floor;
						Features.Add(x, y, FeatureType.Fire);
					}
					else Tiles[x,y] = TileType.Floor;

					if(map[y][x] == 'g' || map[y][x] == 'G' || map[y][x] == '1'){
						Enemy c = new Enemy(GameUniverse){ OriginalType = CreatureType.Goblin };
						Creatures.Add(c, new Point(x, y));
						Initiative initiative = Q.CreateInitiative(RelativeInitiativeOrder.Last);
						Q.Schedule(new AiTurnEvent(c), GameUniverse.TicksPerTurn, initiative);
					}
					if(map[y][x] == 'r' || map[y][x] == 'R' || map[y][x] == '2'){
						Enemy c = new Enemy(GameUniverse){ OriginalType = CreatureType.Cleric };
						Creatures.Add(c, new Point(x, y));
						Initiative initiative = Q.CreateInitiative(RelativeInitiativeOrder.Last);
						Q.Schedule(new AiTurnEvent(c), GameUniverse.TicksPerTurn, initiative);
					}
					/*else if(x < 7 && y < 7){
						Tiles[x,y] = TileType.Floor;
						Features.Add(x, y, FeatureType.Water);
					}
					else if(x < 33 && x > 25 && y < 7){
						Tiles[x,y] = TileType.ThickIce;
					}
					else if(x < 7 && y > 17){
						Tiles[x,y] = TileType.DeepWater;
					}
					else {
						Tiles[x,y] = TileType.DeepWater;
						Features.Add(x, y, FeatureType.Ice);
					}/*
					/*else if(MapRNG.OneIn(wallRarity))
						Tiles[x,y] = TileType.Wall;
					else if(MapRNG.OneIn(waterRarity))
						Tiles[x,y] = TileType.DeepWater;
					else
						Tiles[x,y] = TileType.Floor;*/
				}
			/*for(int x=Width/3;x<Width;++x) {
				Tiles[x, Height/2] = TileType.Wall;
			}*/
			//Tiles[Width / 3, Height / 3] = TileType.Staircase;
			//Light.AddLightSource(new Point(Width / 3, Height / 3), 2);//todo remove
			for(int x=1;x<Width-1;++x)
				for(int y = 1; y<Height-1; ++y){
					Point p = new Point(x, y);
					int? rad = TileDefinition.LightRadius(TileTypeAt(p));
					if(rad != null){
						Light.AddLightSource(p, rad.Value);
					}
					//Light.AddLightSource(new Point(x, y), 2);//todo remove
				}

			int numEnemies = MapRNG.GetNext(9);
			for(int i = 0; i<numEnemies; ++i) {
				Enemy c = new Enemy(GameUniverse){ OriginalType = CreatureType.Goblin };
				Point p;
				do{
					p = new Point(MapRNG.GetNext(Width-2)+1, MapRNG.GetNext(Height-2)+1);
				} while(CreatureAt(p) != null || !TileDefinition.IsPassable(TileTypeAt(p)));

				Creatures.Add(c, p);
				Initiative initiative = Q.CreateInitiative(RelativeInitiativeOrder.Last);
				Q.Schedule(new AiTurnEvent(c), GameUniverse.TicksPerTurn, initiative);
			}

			int numItems = MapRNG.GetNext(5)+1;
			for(int i = 0; i<numItems; ++i) {
				ItemType type = ItemDefinition.ChooseItemTypeToGenerate(MapRNG);
				Item item = new Item(GameUniverse) { Type = type };
				Point p;
				do{
					p = new Point(MapRNG.GetNext(Width-2)+1, MapRNG.GetNext(Height-2)+1);
				} while(ItemAt(p) != null || !TileDefinition.IsPassable(TileTypeAt(p)));
				Items.Add(item, p);
			}

			/* for shrine placement:
		int count = 50;
		for(int i=0;i<NUM_LEVELS;++i){
			int depthChance = (NUM_LEVELS - i);
			int countOnThisLevel = 0;
			for(int j=0;j<count;++j){
				if(OneIn(depthChance)) countOnThisLevel++;
			}
			Console.WriteLine("Depth " + (i+1).ToString().PadLeft(2) + " has " + countOnThisLevel.ToString());
			count -= countOnThisLevel;
		}
		*/

		}
	}
}
