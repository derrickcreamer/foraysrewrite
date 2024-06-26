﻿using System;
using System.Collections.Generic;
using GameComponents;
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
	public abstract class Creature : CreatureBase /*CreatureBase, IPhysicalObject*/ {
		public bool HasPosition => Map.Creatures.Contains(this);
		///<summary>Returns creature position, or throws if creature is not on the map. Check HasPosition beforehand if necessary.</summary>
		public Point Position => Map.Creatures.GetPositionOf(this);

		public int CurrentHealth;

		public List<Item> Inventory;

		//inherent attributes too
		//  i think inherent attributes will be done like this:
		//    prototype has all attrs... all attrs are copied to each, because it needs them in its own hemlock state...
		//    and inherent ones are either named explicitly, OR done with a new 'indestructible' source in hemlock.
			// -- the above is no longer the plan. CreatureType in the status dict should work very well instead.
		public StatusTracker<Creature, CreatureType, Status, Skill, AiTrait, Counter> StatusTracker;

		public bool this[Status status] => StatusTracker[status] > 0;
		public int this[Counter counter] => StatusTracker[counter];

		//attacks
		//spells
		//AI decider

		//target, inventory, attributes, spells, (maybe skills), exhaustion,
		//time of last action / time at which hp recovery will happen, current path,
		// target last known location, player visibility counter for stealth,
		// group

		// Eventually I might need to declare that certain statuses are 'always refresh', which means I track the
		//   EventScheduling and reuse it if it exists (by updating the delay on it). That means tracking, per creature,
		//   a status->scheduling association...then, on add, it would use a new Hemlock feature to check whether it
		//   _would_ be prevented... And then, this state would either need to be serialized, or I'd need to guarantee
		//   that it could be reconstructed (by disallowing any non-refreshing instances of those statuses).
		//   (problem with the always-refresh plan: my priority queue is O(n) for changing priority. Just try the 'event spam' version first.)

		public bool ApplyStatus(Status status, int duration){
			//todo, 'if status is always-refresh, look for existing scheduling'...
			StatusInstance<Creature> inst = StatusTracker.CreateStatusInstance(status);
			if(StatusTracker.AddStatusInstance(inst)){
				Q.ScheduleWithRelativeInitiative(new StatusExpirationEvent(this, inst), duration, RelativeInitiativeOrder.BeforeCurrent);
				return true;
			}
			else return false;
		}

		//todo, this will probably be just a getter, switching on species:
		// (but for now i need to set the player's Decider directly)
		public CancelDecider CancelDecider { get; set; }
		public Creature(GameUniverse g) : base(g) {
			//
			CurrentHealth = 3;
			//
			StatusTracker = g.CreatureRules.CreateStatusTracker(this);
			Inventory = new List<Item>();
		}

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.

		public bool CanSee(Creature target){
			if(!HasPosition || !target.HasPosition) return false;
			else return Map.CheckLOS(Position, target.Position);
		}
	}
}
