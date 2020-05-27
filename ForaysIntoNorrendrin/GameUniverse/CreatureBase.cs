using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;
using Hemlock;
using static Forays.CreatureType;
using static Forays.Status;
using static Forays.AiTrait;
using static Forays.Skill; //etc. todo

namespace Forays {
	public enum CreatureType { Player = 0, MysteriousSpirit, Goblin, Rat, GoblinKing, Cleric, LAST };

	public class CreatureBase : GameObject { //todo...will CreatureBase stay? not 100% sure, but as long as it doesn't hurt anything it'll probably stay.
		public CreatureType OriginalType;
		public int MaxHealth;
		public int DefaultMoveCost;
		public int LightRadius; //todo, pretty sure that light radius won't stay here
		public CreatureBase(GameUniverse g) : base(g) { }

	}
	public class CreatureDefinition : CreatureBase {
		private CreatureDefinition() : base(null){}
		// Declared, as opposed to calculated:
		public List<Status> DeclaredStatuses;
		public Dictionary<Counter, int> DeclaredCounters;
		public List<AiTrait> DeclaredAiTraits;
		public Dictionary<Skill, int> DeclaredSkills;
		//public List<Spell> DeclaredSpells;
		//todo etc.
		//todo attacks
		//todo, 'getcalculatedstatus' methods? only when needed though.

		// Methods to check facts about the creature types:
		public static bool HasDeclaredStatus(CreatureType type, Status status){ //todo, keep this method (and add more) or get rid of it?
			return defs[type].DeclaredStatuses?.Contains(status) == true;
		}

		public static CreatureDefinition GetDefinition(CreatureType type) => defs[type];
		public static ICollection<CreatureDefinition> GetAllDefinitions() => defs.Values;

		// Initialization of Creature definitions:
		private static DefaultValueDictionary<CreatureType, CreatureDefinition> defs;
		private readonly static object lockObject = new object();
		///<summary>Must be called before any other static methods of this class</summary>
		public static void InitializeDefinitions(){
			lock(lockObject){
				if(defs == null) CreateDefinitions();
			}
		}
		private static CreatureDefinition Define(CreatureType type, int maxHp, params Status[] statuses)
			=> Define(type, maxHp, GameUniverse.TicksPerTurn, 0, statuses);
		private static CreatureDefinition Define(CreatureType type, int maxHp, int moveCost, int lightRadius, params Status[] statuses){
			CreatureDefinition def = new CreatureDefinition{ OriginalType = type, MaxHealth = maxHp, DefaultMoveCost = moveCost, LightRadius = lightRadius };
			def.DeclaredStatuses = new List<Status>(statuses);
			defs[type] = def;
			return def;
		}
		private CreatureDefinition WithAiTraits(params AiTrait[] traits){
			DeclaredAiTraits = new List<AiTrait>(traits);
			return this;
		}
		private CreatureDefinition WithCounter(Counter counter, int value){
			if(DeclaredCounters == null) DeclaredCounters = new Dictionary<Counter, int>();
			DeclaredCounters.Add(counter, value);
			return this;
		}
		private CreatureDefinition WithAttack(object placeholder/*todo, what params? damage, effect...*/){
			//todo...the FIRST attack defined should be the default attack, and shouldn't need to be specified.
			// Others can be used by index.
			return this;
	//todo... If a creature has 2 identical attacks, should they be
	//    combined into one, and then should the UI be in charge of deciding what message to print?
	//		(the other option is maybe to have a big global list of attack 'descriptions' as an enum, and maybe
	//		 with a default for damage + crit effect, but those could be overridden...seems over-complicated though.)

		}
		private static void CreateDefinitions(){
			defs = new DefaultValueDictionary<CreatureType, CreatureDefinition>();
			Define(MysteriousSpirit, 1); // If something goes wrong, let's create one of these rather than crashing.
			defs.GetDefaultValue = () => defs[MysteriousSpirit];

			Define(Goblin, 10, CanOpenDoors, VulnerableToNeckSnap, LowLightVision)
			//todo: can spawn with item. is SpawnRule a whole different enum?
				.WithAiTraits(Aggressive, UnderstandsDoors) //todo, will any smaller ones like UnderstandsDoors be grouped together at all? Hazards, too...
				.WithAttack("fake attack but this would really specify damage and crit effect, if any");

			Define(Rat, 5, LowLightVision)
				.WithAiTraits(KeepsDistance)
				.WithAttack("todo");

			//and so on!

		}

	}
}
