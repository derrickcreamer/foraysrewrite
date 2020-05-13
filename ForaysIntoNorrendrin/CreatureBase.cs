using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;
using Hemlock;

namespace Forays {
	public enum CreatureType { Player, Goblin, Rat, GoblinKing, Cleric };
	// Creature prototypes will be CreatureBase, which is Creature minus position, curHP, etc.
	// A new creature can be cloned from any CreatureBase.
	public class CreatureBase : GameObject {
		public CreatureType Type;
		public int MaxHP { get; set; }
		public int MaxMP { get; set; }
		public int DefaultMoveCost { get; set; }
		//public List<Spell> Spells;
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
}
