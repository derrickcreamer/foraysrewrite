using System;

namespace Forays {
	public class TakeDamageEvent : Event<TakeDamageEvent.Result> { //todo, should all these become CreatureEvents now?
		public Creature Creature { get; set; }
		public int Amount { get; set; } //todo, any reason to have these as properties? pretty sure only the ones that need to belong to interfaces (Creature, probably targeting stuff, etc.) will need to be properties.
		// (it's possible that this event could eventually get a DamageSource property, or a DamageTypes collection, etc.)

		public override bool IsInvalid => Creature == null || Amount <= 0; //todo, duplicating this?

		public class Result : EventResult {
			//public bool CreatureWasAlreadyDead { get; set; } (might be used one day, might not)
			public bool CreatureIsNowDead { get; set; } //todo, technically this isn't being used yet either...
		}

		public TakeDamageEvent(Creature creature, int amount) : base(creature.GameUniverse) {
			this.Creature = creature;
			this.Amount = amount;
		}
		protected override Result Execute() {
			Creature.CurrentHealth -= Amount;
			if(Creature.CurrentHealth <= 0) {
				Q.Execute(new DieEvent(Creature));
			}
			return new Result { CreatureIsNowDead = Creature.CurrentHealth <= 0 };
		}
	}
	public class DieEvent : Event<SimpleEvent.NullResult> { //todo, should all these become CreatureEvents now?
		public Creature Creature { get; set; }

		public override bool IsInvalid => Creature == null; //todo remove?

		public DieEvent(Creature creature) : base(creature.GameUniverse) {
			this.Creature = creature;
		}
		protected override SimpleEvent.NullResult Execute() {
			if(!GameUniverse.DeadCreatures.Contains(Creature)) {
				if(Creature == Player){
					//...
					GameUniverse.GameOver = true;
				}
				GameUniverse.DeadCreatures.Add(Creature);
			}
			return null;
		}
	}
}
