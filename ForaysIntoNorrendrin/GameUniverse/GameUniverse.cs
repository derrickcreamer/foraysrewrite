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
			Map.Light.AddLightSource(Player.Position, 5);
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
		public Creature CreatureAt(Point p) => GameUniverse.Map.Creatures[p];
		public TileType TileTypeAt(Point p) => GameUniverse.Map.Tiles[p];
		public FeatureType FeaturesAt(Point p) => GameUniverse.Map.Features[p];
		public Item ItemAt(Point p) => GameUniverse.Map.Items[p];
	}
}
