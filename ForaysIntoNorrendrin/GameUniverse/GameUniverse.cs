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
		public Creature Player;
		public EventScheduler Q;
		/*public DungeonMap Map;
		public int CurrentDepth;
		public List<DungeonLevelType> LevelTypes;
		public DefaultValueDictionary<TileType, ShrineStatus> ShrineStatuses;
		public bool FeatGainedFromCurrentShrineSet;
		public int SpellbooksGenerated;
		public List<Feat> PlayerFeats;
		public int FinalLevelClock;
		public int[] FinalLevelCultistCount;
		public int FinalLevelDemonCount;
		public DefaultValueDictionary<CreatureType, CreatureBase> Species;
		// tile defs
		// item defs
		// feature defs - probably static though?
		//item ID stuff: flavors, tried, IDed...
		//what knows item rarity, wand charges, etc.?
		public RNG R;*/

		/*
id status for all consumables <<< needs more detail, but, this tracks a bool per item type, plus a random identity for each item type, right?
unIDed color for all consumables <<< see above
actor, tile, and item prototypes or definitions <<< WhateverBase should work nicely here
*/

		public event Action<object> OnNotify;
		public T Notify<T>(T notification) {
			OnNotify?.Invoke(notification);
			return notification;
		}
		public void Run() {
			Suspend = false;
			while(!Suspend) {
				Q.ExecuteNextEvent();
			}
		}
		public GameUniverse() { // NEXT: (maybe.) fix the init here - is the Map actually created yet?
			//todo: actually, i think there should be a Start method that gets everything created and *ready* to Run. Maybe StartNew?
			/*ulong seed = (ulong)DateTime.Now.Ticks; //todo check - will this get passed in? maybe a null-defaulted param?
			R = new RNG(seed);*/

			Q = new EventScheduler();
			//Creatures = new Grid<Creature, Point>(p => p.X >= 0 && p.X < 30 && p.Y >= 0 && p.Y < 20);
			/*Species = new DefaultValueDictionary<CreatureType, CreatureBase>();
			Species.GetDefaultValue = () => new CreatureBase(this) { MaxHP = 1, MoveCost = 120 }; //todo check

			Map = new DungeonMap(this);*/

			// now some setup. It seems likely that a bunch of this will be handed off to things like the dungeon generator:
			Player = new Creature(this) { Decider = new PlayerCancelDecider(this) };
			//Creatures.Add(Player, new Point(15, 8));
			Q.Schedule(new PlayerTurnEvent(this), 120, null);
		}
	}
	public class GameObject {
		public GameUniverse GameUniverse;
		public GameObject(GameUniverse g) { GameUniverse = g; }

		public virtual T Notify<T>(T notification) => GameUniverse.Notify(notification);
		//Note that not everything on GameUniverse will be reflected here - just the most common & useful:
		public Creature Player => GameUniverse.Player;
		public EventScheduler Q => GameUniverse.Q;
		/*public RNG R => GameUniverse.R;
		public Grid<Creature, Point> Creatures => GameUniverse.Map.Creatures;
		public Creature CreatureAt(Point p) => GameUniverse.Map.Creatures[p];
		public Grid<Tile, Point> Tiles => GameUniverse.Map.Tiles;
		public Tile TileAt(Point p) => GameUniverse.Map.Tiles[p];
		public Grid<Item, Point> Items => GameUniverse.Map.Items;
		public Item ItemAt(Point p) => GameUniverse.Map.Items[p];*/
	}
}
