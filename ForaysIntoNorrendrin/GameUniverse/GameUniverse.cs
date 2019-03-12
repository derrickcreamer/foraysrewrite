using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;

namespace Forays {
	//todo move all this stuff out from before GameUniverse:
	/*public enum DungeonLevelType { Sparse, Cramped };
	public enum ShrineStatus { Undiscovered, Discovered, NextLevel };
	public enum Feat { Lunge, WhirlwindAttack, NeckSnap };
	public enum Spell { Fireball, Blink, MagicMissile, DetectMovement };*/
	public class GameUniverse {
		public bool Suspend;
		public bool GameOver;
		public Creature Player;
		public EventScheduler Q;
		public const int MapHeight = 20;
		public const int MapWidth = 30;
		public RNG R;
		public Grid<Creature, Point> Creatures;
		public List<Creature> DeadCreatures; // (from this turn only) (name?)
		public TileType[,] Tiles; //temporarily a 2d array...
		public int CurrentDepth;
		/*public DungeonMap Map;
		public List<DungeonLevelType> LevelTypes;
		public DefaultValueDictionary<TileType, ShrineStatus> ShrineStatuses;
		public bool FeatGainedFromCurrentShrineSet;
		public int SpellbooksGenerated;
		public List<Feat> PlayerFeats;
		public int FinalLevelClock;
		public int[] FinalLevelCultistCount;
		public int FinalLevelDemonCount;
		public DefaultValueDictionary<CreatureType, CreatureBase> Species;*/
		// tile defs
		// item defs
		// feature defs - probably static though?
		//item ID stuff: flavors, tried, IDed...
		//what knows item rarity, wand charges, etc.?

		/*
id status for all consumables <<< needs more detail, but, this tracks a bool per item type, plus a random identity for each item type, right?
unIDed color for all consumables <<< see above
actor, tile, and item prototypes or definitions <<< WhateverBase should work nicely here
*/

		public event System.Action<object> OnNotify;
		public T Notify<T>(T notification) {
			OnNotify?.Invoke(notification);
			return notification;
		}
		public void Run() {
			Suspend = false;
			while(!Suspend && !GameOver) {
				Q.ExecuteNextEvent();
				CleanUpGameState();
			}
		}
		public void CleanUpGameState() { // name? "checkfor..." ? "state based actions"?
			if(DeadCreatures.Count > 0) {
				foreach(Creature c in DeadCreatures) {
					//any notify here?
					Creatures.Remove(c);
				}
				DeadCreatures.Clear();
			}
		}
		public void InitializeNewGame(ulong? seed = null)
		{ // NEXT: (maybe.) fix the init here - is the Map actually created yet?

			//hm, should Suspend be true on create?

			R = new RNG(seed ?? (ulong)DateTime.Now.Ticks);
			Q = new EventScheduler();
			Creatures = new Grid<Creature, Point>(p => p.X >= 0 && p.X < MapWidth && p.Y >= 0 && p.Y < MapHeight);
			/*Species = new DefaultValueDictionary<CreatureType, CreatureBase>();
			Species.GetDefaultValue = () => new CreatureBase(this) { MaxHP = 1, MoveCost = 120 }; //todo check

			Map = new DungeonMap(this);*/

			DeadCreatures = new List<Creature>();

			// now some setup. It seems likely that a bunch of this will be handed off to things like the dungeon generator:

			CurrentDepth = 1;
			GenerateMap();

			Player = new Creature(this) { Decider = new PlayerCancelDecider(this) };
			Creatures.Add(Player, new Point(15, 8));
			Q.Schedule(new PlayerTurnEvent(this), 120, null);

			int numEnemies = R.GetNext(9);
			for(int i = 0; i<numEnemies; ++i) {
				Creature c = new Creature(this);
				Creatures.Add(c, new Point(R.GetNext(MapWidth), R.GetNext(MapHeight)));
				Q.Schedule(new AiTurnEvent(c), 1200, null);
			}
		}
		public void GenerateMap() {
			Tiles = new TileType[MapWidth, MapHeight];
			for(int x=0;x<MapWidth;++x)
				for(int y = 0; y<MapHeight; ++y) {
					if(x == 0 || y == 0 || x == MapWidth-1 || y == MapHeight-1)
						Tiles[x,y] = TileType.Wall;
					else if(R.OneIn(20))
						Tiles[x,y] = TileType.Wall;
					else if(R.OneIn(8))
						Tiles[x,y] = TileType.Water;
					else
						Tiles[x,y] = TileType.Floor;
				}
			Tiles[MapWidth / 3, MapHeight / 3] = TileType.Staircase;
		}
	}
	public class GameObject {
		public GameUniverse GameUniverse;
		public GameObject(GameUniverse g) { GameUniverse = g; }

		public virtual T Notify<T>(T notification) => GameUniverse.Notify(notification);
		//Note that not everything on GameUniverse will be reflected here - just the most common & useful:
		public Creature Player => GameUniverse.Player;
		public EventScheduler Q => GameUniverse.Q;
		public RNG R => GameUniverse.R;
		public Grid<Creature, Point> Creatures => GameUniverse.Creatures;
		public Creature CreatureAt(Point p) => GameUniverse.Creatures[p];
		public TileType TileTypeAt(Point p) => GameUniverse.Tiles[p.X, p.Y];
		/*public Grid<Creature, Point> Creatures => GameUniverse.Map.Creatures;
		public Creature CreatureAt(Point p) => GameUniverse.Map.Creatures[p];
		public Grid<Tile, Point> Tiles => GameUniverse.Map.Tiles;
		public Tile TileAt(Point p) => GameUniverse.Map.Tiles[p];
		public Grid<Item, Point> Items => GameUniverse.Map.Items;
		public Item ItemAt(Point p) => GameUniverse.Map.Items[p];*/
	}
	public class NotifyPrintMessage {
		public string Message;
		//what else will go here? will this type actually be used long-term? not sure.
	}
}
