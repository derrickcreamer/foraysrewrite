using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;
using Hemlock;
using static Forays.Status;
using static Forays.Skill; //etc. todo

namespace Forays {
	public enum CreatureType { Player = 0, Goblin, Rat, GoblinKing, Cleric, LAST };

	public static class CreatureDefinitions{
		//  hmmmm... wait a second, I need to figure out the relationship between this class / the prototypes / the status rules.
		//   Specifically, I know that the creature type will imply all these things, in the status rules, but will the prototype track that?
		//    If I want to check calculated ones, I can have a tracker, and add nothing but the creature type, and test the status, no problem.
		//     But will I need to test whether a creature type specifically declares a given status? maybe not!
		//  so, maybe I could have a single tracker, and when a "does this creature type have this status inherently?" request comes in, I could add that creature type
		//  (or even those creature types, in the case of mutations etc.) and then test the status, and it would remember the previous set of creature types to avoid extra work...
		//  The other option is to create a new tracker for each creature type.

		private class CreatureDefinition_2{
			public CreatureDefinition(CreatureType type, int maxHp, int moveCost, int lightRadius, params Status[] statuses){

			}
		}
		public static
Define(CreatureType.GiantBat, 10, 50, 0, Flying, Small, Blindsight)
    .WithAiTraits(Territorial)
    .WithAttack(1, AttackEffect.None, "& bites *")
    .WithAttack(1, AttackEffect.None, "& scratches *");

	}
	//todo...hmmm, where do attacks fit into this? If a creature has 2 identical attacks, should they be
	//    combined into one, and then should the UI be in charge of deciding what message to print?
	//		(the other option is maybe to have a big global list of attack 'descriptions' as an enum, and maybe
	//		 with a default for damage + crit effect, but those could be overridden...seems over-complicated though.)
	// Creature prototypes will be CreatureBase, which is Creature minus position, curHP, etc.
	// A new creature can be cloned from any CreatureBase.
	public class CreatureBase : GameObject {
		public CreatureType OriginalType;
		public int MaxHP;
		public int MaxMP;
		public int DefaultMoveCost;
		//todo, light radius?
		public CreatureBase(GameUniverse g) : base(g) { }

        private static DefaultValueDictionary<CreatureType, CreatureBase> prototypes;
	}
		//todo, this might be better in its own file:
		/*public static DefaultValueDictionary<CreatureType, Creature> CreatePrototypeCreatures(GameUniverse g) {
			//todo, default or not? should it throw? the code in GameUniverse actually makes a default 1hp creature - probably do that.
			// give it a good name though, like a mysterious spirit.
			return new DefaultValueDictionary<CreatureType, Creature> {
				[CreatureType.Goblin] = new Creature(g){ MaxHP = 10, MoveCost = 120, Type = CreatureType.Goblin }
			}; //todo: this definitely needs a helper like the old code, to avoid duplicating type.
		}*/
	public class CreatureDefinition : CreatureBase {
		private CreatureDefinition() : base(null){}
		// Declared, as opposed to calculated:
		public List<Status> DeclaredStatuses; // todo, is this the best access modifier?
		//public List<Spell> DeclaredSpells;
		//todo etc.

		//todo attacks

		//todo, 'getcalculatedstatus' methods?

		private static DefaultValueDictionary<CreatureType, CreatureDefinition> defs;

		public static bool HasDeclaredStatus(CreatureType type, Status status){
			if(defs == null) CreateDefinitions();
			return defs[type].DeclaredStatuses.Contains(status);
		}

		private static CreatureDefinition Define(CreatureType type, int maxHp, int moveCost, int lightRadius, params Status[] statuses){
			//todo, this needs to also take a StatusRules and load these rules into there, right?
			//   >>>>>>>>>>>  but does that change WHEN all this is called? Maybe it's the status rules that call -all of this-...
			//                     .........but no, wait, don't the DeclaredStatuses lists make it POSSIBLE to now call 'get status rules from creature types' or 'GetCreatureTypeStatusRules' at any time? That's good then!  <<<  TODO NEXT
			//  Okay, so, deciding between 3 things:
			// 1, use a real lock w/static
			// 2, static with no lock, just checks for null at each entry point
			// 3, instances, but this requires a separate class if I want to be able to call ...  no wait, see 4
			// 4, local methods inside a single method that returns a dictionary? this means each GameUniverse gets its own, which is fine.

			// from 4... i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.
			//
		}

		private static void CreateDefinitions(){
			var defs = new DefaultValueDictionary<CreatureType, CreatureDefinition>(); //todo, mysterious spirit as default value?

			// errr, todo... how is this going to line up with the Define(...).WithAiTraits(...).WithAttack(...) example above?

			CreatureDefinition.defs = defs;
		}

	}
}
