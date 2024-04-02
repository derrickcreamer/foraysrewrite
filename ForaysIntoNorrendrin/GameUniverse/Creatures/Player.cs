using System;

namespace Forays {
	public class Player : Creature {
		public int this[Skill skill] => StatusTracker[skill];

		//attacks
		//spells
		//AI decider

		//spells, exhaustion,
		//time of last action / time at which hp recovery will happen,

		//todo, this will probably be just a getter, switching on species:
		// (but for now i need to set the player's Decider directly)
		public CancelDecider CancelDecider { get; set; }
		public Player(GameUniverse g) : base(g) {
			//todo
			CurrentHealth = 3;
			//
			StatusTracker = g.CreatureRules.CreateStatusTracker(this);
		}

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.

	}
}
