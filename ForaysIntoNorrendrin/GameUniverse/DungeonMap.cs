using System;
using System.Collections.Generic;
using GameComponents;
using GameComponents.DirectionUtility;
using UtilityCollections;

namespace Forays {
	public class DungeonMap : GameObject {
		const int MapWidth = GameUniverse.MapWidth;
		const int MapHeight = GameUniverse.MapHeight;

		public DungeonLevelType CurrentLevelType; //todo temporary

		public RNG MapRNG => GameUniverse.MapRNG;
		[Obsolete("CAUTION: This is the regular RNG, not the mapgen RNG.")]
		new public RNG R => GameUniverse.R;

		public Grid<Creature, Point> Creatures;
		//public List<CreatureGroup> CreatureGroups;
		public PointArray<TileType> Tiles;

		public MultiValueDictionary<Point, FeatureType> Features;

		public Dictionary<Point, Trap> Traps;
		public bool CellIsTrapped(Point p) => Traps.ContainsKey(p);
		//todo: shrines. i think they'll be handled similarly to traps now, with a dictionary and a struct. (idols could get the same treatment, or not.)

		public Grid<Item, Point> Items;

		///<summary>Used to mark cells that automatically fail LOS checks, as an optimization</summary>
		public PointArray<bool> NeverInLineOfSight;

		public LightMap Light;

		///<summary>Track which direction the player last exited each cell. Helps the AI track the player.</summary>
		public PointArray<Dir8> DirectionPlayerExited;

		//footsteps tracked here or elsewhere?
		//aesthetic features here, or are those strictly presentation with no game effect?
			// I think those are in-game. Those are something that, let's say, an AI-controlled player might consider,
			// much like the number of times a wand has been used. Those aren't things that the AI or the UI should need to track.
		//track burning objects here?
		//items

		public int DangerModifier;
		public PointArray<bool> Seen;
		//todo... does the 'seen' map need to track WHAT was seen? tile+trap+features + optionally creatures?

		public DungeonMap(GameUniverse g) : base(g) {
			Func<Point, bool> isInBounds = p => p.X >= 0 && p.X < MapWidth && p.Y >= 0 && p.Y < MapHeight;
			Creatures = new Grid<Creature, Point>(isInBounds);
			Tiles = new PointArray<TileType>(MapWidth, MapHeight);
			Features = new MultiValueDictionary<Point, FeatureType>(); //todo, don't allow dupes, right?
			Traps = new Dictionary<Point, Trap>();
			Items = new Grid<Item, Point>(isInBounds);
			NeverInLineOfSight = new PointArray<bool>(MapWidth, MapHeight);
			Light = new LightMap(g);
			DirectionPlayerExited = new PointArray<Dir8>(MapWidth, MapHeight);
			//todo, more here?
			Seen = new PointArray<bool>(MapWidth, MapHeight);
		}
		public bool CellIsPassable(Point p){ // will get optional flags param if needed
			//check everything that could block a cell, which currently is probably just the tile type
			TileType type = Tiles[p];
			return TileDefinition.IsPassable(type);
		}
		public bool CellIsOpaque(Point p){ //todo, maybe cache this and recalculate when a tile changes or a feature is added/removed.
			TileType type = Tiles[p];
			if(TileDefinition.IsOpaque(type)) return true;
			if(Features.AnyValues(p)){
				foreach(FeatureType feature in Features[p]){
					if(FeatureDefinition.IsOpaque(feature)) return true;
				}
			}
			return false;
		}

		public void GenerateMap() {
			CurrentLevelType = MapRNG.OneIn(4) ? DungeonLevelType.Cramped : DungeonLevelType.Sparse;
			int wallRarity = CurrentLevelType == DungeonLevelType.Cramped ? 6 : 20;
			int waterRarity = CurrentLevelType == DungeonLevelType.Cramped ? 50 : 8;
			for(int x=0;x<MapWidth;++x)
				for(int y = 0; y<MapHeight; ++y) {
					if(x == 0 || y == 0 || x == MapWidth-1 || y == MapHeight-1)
						Tiles[x,y] = TileType.Wall;
					else if(MapRNG.OneIn(wallRarity))
						Tiles[x,y] = TileType.Wall;
					else if(MapRNG.OneIn(waterRarity))
						Tiles[x,y] = TileType.Water;
					else
						Tiles[x,y] = TileType.Floor;
				}
			Tiles[MapWidth / 3, MapHeight / 3] = TileType.Staircase;
			Light.AddLightSource(new Point(MapWidth / 3, MapHeight / 3), 2);//todo remove

			int numEnemies = MapRNG.GetNext(9);
			for(int i = 0; i<numEnemies; ++i) {
				Creature c = new Creature(GameUniverse){ OriginalType = CreatureType.Goblin };
				Creatures.Add(c, new Point(MapRNG.GetNext(MapWidth-2)+1, MapRNG.GetNext(MapHeight-2)+1));
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
