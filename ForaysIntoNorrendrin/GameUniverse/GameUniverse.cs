using System;
using System.Collections.Generic;
using GameComponents;
using Hemlock;
using UtilityCollections;

namespace Forays {
	//todo move all this stuff out from before GameUniverse:
	public enum DungeonLevelType { Sparse, Cramped };
	/*public enum ShrineStatus { Undiscovered, Discovered, NextLevel };
	public enum Feat { Lunge, WhirlwindAttack, NeckSnap };
	public enum Spell { Fireball, Blink, MagicMissile, DetectMovement };*/
	public class GameUniverse {
		public const int MapHeight = 22;
		public const int MapWidth = 66;
		public const int TicksPerTurn = 120;

		public bool Suspend = true;
		public bool GameOver;

		public Creature Player;

		public EventQueue Q;

		public RNG R;
		public RNG MapRNG;

		public List<Creature> DeadCreatures; // (from this turn only) (name?)

		public List<DungeonLevelType> LevelTypes;
		public int CurrentDepth;
		//public DungeonLevelType CurrentLevelType{get;set;} //todo, this should eventually use the LevelTypes list
		public DungeonMap Map;

		public StatusRules CreatureRules;
		/*public DefaultValueDictionary<TileType, ShrineStatus> ShrineStatuses;
		public bool FeatGainedFromCurrentShrineSet;
		public int SpellbooksGenerated;
		public List<Feat> PlayerFeats;
		public int FinalLevelClock;
		public int[] FinalLevelCultistCount;
		public int FinalLevelDemonCount;*/
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
					//any notify here? maybe this actually becomes its own GameEvent?
					Map.Creatures.Remove(c);
				}
				DeadCreatures.Clear();
			}
		}
		public void InitializeNewGame(ulong? seed = null)
		{ // NEXT: (maybe.) fix the init here - is the Map actually created yet?

			R = new RNG(seed ?? (ulong)DateTime.Now.Ticks);
			MapRNG = new RNG(R.GetNext());
			Q = new EventQueue();
			Map = new DungeonMap(this);
			CurrentDepth = 1;

			//todo...while loading the rules, do i need a hook so that the UI can insert any message overrides it wants to?
			//hmmmmm.... is that what the Message/Effect split should be for? Basically, that the UI gets full control over the Message half?
			//    This is very interesting...see how well this lines up with reality.
			CreatureRules = new StatusRules(); // Also initializes creature definitions
			DeadCreatures = new List<Creature>();
			ItemDefinition.InitializeDefinitions();

			// now some setup. It seems likely that a bunch of this will be handed off to things like the dungeon generator:

			Player = new Creature(this) { CancelDecider = new PlayerCancelDecider(this) };
			Map.Creatures.Add(Player, new Point(15, 8));
			Initiative playerInitiative = Q.CreateInitiative(RelativeInitiativeOrder.First);
			Q.Schedule(new PlayerTurnEvent(this), TicksPerTurn, playerInitiative);

			Map.GenerateMap();
		}
	}
	public class GameObject {
		public GameUniverse GameUniverse;
		public GameObject(GameUniverse g) { GameUniverse = g; }

		//Note that not everything on GameUniverse will be reflected here - just the most common & useful:
		public Creature Player => GameUniverse.Player;
		public EventQueue Q => GameUniverse.Q;
		public int Turns(int numTurns) => numTurns * GameUniverse.TicksPerTurn;
		public RNG R => GameUniverse.R;
		public DungeonMap Map => GameUniverse.Map;
		//public Grid<Creature, Point> Creatures => GameUniverse.Creatures;
		public Creature CreatureAt(Point p) => GameUniverse.Map.Creatures[p];
		public TileType TileTypeAt(Point p) => GameUniverse.Map.Tiles[p.X, p.Y];
		/*public Grid<Creature, Point> Creatures => GameUniverse.Map.Creatures;
		public Creature CreatureAt(Point p) => GameUniverse.Map.Creatures[p];
		public Grid<Tile, Point> Tiles => GameUniverse.Map.Tiles;
		public Tile TileAt(Point p) => GameUniverse.Map.Tiles[p];
		public Grid<Item, Point> Items => GameUniverse.Map.Items;
		public Item ItemAt(Point p) => GameUniverse.Map.Items[p];*/
	}
	public static class ExtensionsForRNG{ // until I get around to putting these in the RNG class itself //todo, move to separate file
		public static int Roll(this RNG rng, int sides){
			if(sides < 1) return 0;
			return rng.GetNext(sides) + 1;
		}
		public static int Roll(this RNG rng, int dice, int sides){
			if(sides < 1) return 0;
			int total = 0;
			for(int i=0;i<dice;++i)
				total += rng.GetNext(sides) + 1;
			return total;
		}
		public static int Between(this RNG rng, int a, int b){ //inclusive
			int min, max;
			if(a < b){
				min = a;
				max = b;
			}
			else{
				min = b;
				max = a;
			}
			// 'between 2 and 7' can return 6 numbers, so get 0-5 first, then add:
			return rng.GetNext(max - min + 1) + min;
		}
		public static bool PercentChance(this RNG rng, int x){
			return x > rng.GetNext(100);
		}
		public static bool FractionalChance(this RNG rng, int x, int outOfY){
			if(x >= outOfY) return true;
			return x > rng.GetNext(outOfY);
		}
		// Rework Choose to avoid 'params' and array allocations:
		public static T Choose<T>(this RNG rng, T t0, T t1){
			if(rng.CoinFlip()) return t0;
			else return t1;
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2){
			switch(rng.GetNext(3)){
				case 0: return t0;
				case 1: return t1;
				default: return t2;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3){
			switch(rng.GetNext(4)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				default: return t3;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4){
			switch(rng.GetNext(5)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				default: return t4;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5){
			switch(rng.GetNext(6)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				default: return t5;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6){
			switch(rng.GetNext(7)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				default: return t6;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7){
			switch(rng.GetNext(8)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				default: return t7;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8){
			switch(rng.GetNext(9)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				default: return t8;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9){
			switch(rng.GetNext(10)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				default: return t9;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10){
			switch(rng.GetNext(11)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				default: return t10;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11){
			switch(rng.GetNext(12)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				default: return t11;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12){
			switch(rng.GetNext(13)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				default: return t12;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13){
			switch(rng.GetNext(14)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				default: return t13;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13, T t14){
			switch(rng.GetNext(15)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				case 13: return t13;
				default: return t14;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13, T t14, T t15){
			switch(rng.GetNext(16)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				case 13: return t13;
				case 14: return t14;
				default: return t15;
			}
		}
	}
}
