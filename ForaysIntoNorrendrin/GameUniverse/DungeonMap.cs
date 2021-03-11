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

		public Grid<Creature, Point> Creatures;
		//public List<CreatureGroup> CreatureGroups;
		public PointArray<TileType> Tiles;

		public FeatureMap Features;

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
			Func<Point, bool> isInBounds = p => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
			Creatures = new Grid<Creature, Point>(isInBounds);
			Tiles = new PointArray<TileType>(Width, Height);
			Features = new FeatureMap(Width, Height);
			Traps = new Dictionary<Point, Trap>();
			Items = new Grid<Item, Point>(isInBounds);
			NeverInLineOfSight = new PointArray<bool>(Width, Height);
			Light = new LightMap(g);
			DirectionPlayerExited = new PointArray<Dir8>(Width, Height);
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
		public bool CellIsOpaque(Point p){
			TileType type = Tiles[p];
			if(TileDefinition.IsOpaque(type)) return true;
			if(Features[p].IsOpaque()) return true;
			return false;
		}
		public void GenerateMap() {
			CurrentLevelType = MapRNG.OneIn(4) ? DungeonLevelType.Cramped : DungeonLevelType.Sparse;
			int wallRarity = CurrentLevelType == DungeonLevelType.Cramped ? 6 : 20;
			int waterRarity = CurrentLevelType == DungeonLevelType.Cramped ? 50 : 8;
			string[] tempMap = new string[]{
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"--------www-----------------------------III-----------------------",
"--------wwwwww#----------------------IIIIIIIIIIIIIIIIII-----------",
"------wwwWWWWW#----------------------IIIIIIIIIIIIIIIIIIII---------",
"------wWWWWWWW#ww-------------------IIIIIiiiiiiiiiiiiIIII---------",
"------wwWWWWWWWWw--------------------IIIIiiiiiiiiiiiiiIIII--------",
"-------wwwWWWwwww--------------------IIIIiiiiiiiiiiiiiiIII--------",
"---------wwwwwww-----------------------IIIIIIiiiiiiiiiiIII--------",
"-------------------------------------------IIIIiiiiiiiiIII--------",
"-------------------------------------------IIIIIIiiiIIIIII--------",
"----------------------------------------------IIIIIIIIII----------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------",
"------------------------------------------------------------------"
			};

			for(int x=0;x<Width;++x)
				for(int y = 0; y<Height; ++y) {
					if(x == 0 || y == 0 || x == Width-1 || y == Height-1)
						Tiles[x,y] = TileType.Wall;
					else if(tempMap[y][x] == 'W'){
						Tiles[x,y] = TileType.DeepWater;
					}
					else if(tempMap[y][x] == 'w'){
						Tiles[x,y] = TileType.Floor;
						Features.Add(x, y, FeatureType.Water);
					}
					else if(tempMap[y][x] == 'I'){
						Tiles[x,y] = TileType.ThickIce;
					}
					else if(tempMap[y][x] == 'i'){
						Tiles[x,y] = TileType.DeepWater;
						Features.Add(x, y, FeatureType.Ice);
					}
					else if(tempMap[y][x] == '#'){
						Tiles[x,y] = TileType.Wall;
					}
					else Tiles[x,y] = TileType.Floor;
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
					}*/
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
			Tiles[Width / 3, Height / 3] = TileType.Staircase;
			Light.AddLightSource(new Point(Width / 3, Height / 3), 2);//todo remove
			for(int x=1;x<Width-1;++x)
				for(int y = 1; y<Height-1; ++y)
					Light.AddLightSource(new Point(x, y), 2);//todo remove

			int numEnemies = MapRNG.GetNext(9);
			for(int i = 0; i<numEnemies; ++i) {
				Creature c = new Creature(GameUniverse){ OriginalType = CreatureType.Goblin };
				Creatures.Add(c, new Point(MapRNG.GetNext(Width-2)+1, MapRNG.GetNext(Height-2)+1));
				Initiative initiative = Q.CreateInitiative(RelativeInitiativeOrder.Last);
				Q.Schedule(new AiTurnEvent(c), GameUniverse.TicksPerTurn * 10, initiative);
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
