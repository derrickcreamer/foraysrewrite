﻿using System;
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

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.
			//

	}
}
