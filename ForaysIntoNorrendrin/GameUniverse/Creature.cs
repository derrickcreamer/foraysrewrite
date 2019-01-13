using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;

namespace Forays {
	//todo, move stuFF:
	/*public interface IPhysicalObject {
		Point? Position { get; }
	}
	public enum GroupType { Normal, LeaderWithFollowers };
	public class CreatureGroup {
		public GroupType Type;
		List<Creature> Members;
	}
	public enum CreatureType { Player, Goblin, Rat, GoblinKing, Cleric };
	// Creature prototypes will be CreatureBase, which is Creature minus position, curHP, etc.
	// A new creature can be cloned from any CreatureBase.
	public class CreatureBase : GameObject {
		public CreatureType Type;
		public int MaxHP { get; set; }
		public int MaxMP { get; set; }
		public int MoveCost { get; set; }
		public List<Spell> Spells;
		public CreatureBase(GameUniverse g) : base(g) {

		}
	}*/
	public class Creature : GameObject /*CreatureBase, IPhysicalObject*/ {
		public Point? Position; // => Creatures.TryGetPositionOf(this, out Point p)? p : (Point?)null;

		public int CurHP { get; set; }
		public int CurMP { get; set; }

		//inherent attributes too
		//  i think inherent attributes will be done like this:
		//    prototype has all attrs... all attrs are copied to each, because it needs them in its own hemlock state...
		//    and inherent ones are either named explicitly, OR done with a new 'indestructible' source in hemlock.
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
