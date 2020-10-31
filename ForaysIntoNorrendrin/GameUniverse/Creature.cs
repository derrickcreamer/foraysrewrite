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
	public class Creature : CreatureBase /*CreatureBase, IPhysicalObject*/ {
		public Point? Position => Map.Creatures.TryGetPositionOf(this, out Point p)? p : (Point?)null;

		public int CurrentHealth;

		//inherent attributes too
		//  i think inherent attributes will be done like this:
		//    prototype has all attrs... all attrs are copied to each, because it needs them in its own hemlock state...
		//    and inherent ones are either named explicitly, OR done with a new 'indestructible' source in hemlock.
			// -- the above is no longer the plan. CreatureType in the status dict should work very well instead.
		public StatusTracker<Creature, CreatureType, Status, Skill, AiTrait, Counter> StatusTracker;

		public bool this[Status status] => StatusTracker[status] > 0;
		public int this[Skill skill] => StatusTracker[skill];
		public bool this[AiTrait trait] => StatusTracker[trait] > 0;
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
		}

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.

		///<summary>Return value is the cost of the action taken</summary>
		public int ExecuteMonsterTurn(){
			return 0;
			//hmm... a good amount of code goes at the start of each turn...
			// ... and a bit of it is shared by the player.

						// switch on state? Idle / Wandering / Searching / Hunting?
			// but note that the implementation might have more subdivisions within these. Searching could be investigating a sound,
			//   or trying to find the player after losing track. Hunting could be when the player is visible, or just while moving toward last_seen.

			// Maybe there's a 'single-minded' flag here, or maybe not. It'd be for things that run the same AI all the time.

			// is there actually an enum for this state? for now let's assume there is.

			// if hunting, call HuntingAI or whatever it's called
			// else
			//   check whether the player might be spotted...
			//   if we see the player, spend the turn shouting or something, and switch to Hunting
			//   otherwise, searching? ...
			//   otherwise, what things can switch something from idle/wandering to searching? sound and scent?
			//   if none of that, then, wandering? or just idle?

		}

		private int Hunt(){
			return 0;
		}
	}
}
