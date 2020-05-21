using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;
using Hemlock;

namespace Forays {
	//todo, move stuFF:
	/*public interface IPhysicalObject {
		Point? Position { get; }
	}
	public enum GroupType { Normal, LeaderWithFollowers };
	public class CreatureGroup {
		public GroupType Type;
		List<Creature> Members;
	}*/
	public class Creature : GameObject /*CreatureBase, IPhysicalObject*/ {
		public Point? Position => Creatures.TryGetPositionOf(this, out Point p)? p : (Point?)null;

		public int CurHP;
		//public int CurMP { get; set; }

		//inherent attributes too
		//  i think inherent attributes will be done like this:
		//    prototype has all attrs... all attrs are copied to each, because it needs them in its own hemlock state...
		//    and inherent ones are either named explicitly, OR done with a new 'indestructible' source in hemlock.
			// -- the above is no longer the plan. CreatureType in the status dict should work very well instead.
		StatusTracker<Creature, CreatureType, Status, Skill, AiTrait, Counter> statusTracker;

		public bool HasStatus(Status status) => statusTracker[status] > 0;
		public int GetSkillValue(Skill skill) => statusTracker[skill];
		public bool HasAiTrait(AiTrait trait) => statusTracker[trait] > 0;
		public int GetCounterValue(Counter counter) => statusTracker[counter];

		// todo, OR, indexer... one for Status which returns bool, and one for Counter or NumericalStatus or NumericalAttribute, which returns int. Pick one of these.

		public bool this[Status status] => statusTracker[status] > 0;
		public int this[Skill skill] => statusTracker[skill];
		public bool this[AiTrait trait] => statusTracker[trait] > 0;
		public int this[Counter counter] => statusTracker[counter];

		//attacks
		//spells
		//AI decider

		//target, inventory, attributes, spells, (maybe skills), exhaustion,
		//time of last action / time at which hp recovery will happen, current path,
		// target last known location, player visibility counter for stealth,
		// group

		//todo, this will probably be just a getter, switching on species:
		// (but for now i need to set the player's Decider directly)
		public CancelDecider Decider { get; set; }
		public Creature(GameUniverse g) : base(g) {
			//
			CurHP = 3;
			//
			statusTracker = StatusRules.GetRules().CreateStatusTracker(this);
			Creature creature = null;
			if(creature[Status.Stunned]){
				int baseDamage = creature[Skill.Combat];
				//...
			}
		}

		//todo, this might be better in its own file:
		/*public static DefaultValueDictionary<CreatureType, Creature> CreatePrototypeCreatures(GameUniverse g) {
			//todo, default or not? should it throw? the code in GameUniverse actually makes a default 1hp creature - probably do that.
			// give it a good name though, like a mysterious spirit.
			return new DefaultValueDictionary<CreatureType, Creature> {
				[CreatureType.Goblin] = new Creature(g){ MaxHP = 10, MoveCost = 120, Type = CreatureType.Goblin }
			}; //todo: this definitely needs a helper like the old code, to avoid duplicating type.
		}*/
	}
}
