using System;

namespace Forays {
	///<summary>Calculates hit/miss and calls MeleeHitEvent or MeleeMissEvent.</summary>
	public class MeleeAttackAction : CreatureAction<MeleeAttackAction.Result> {
		public Creature Target { get; set; } //todo - do attacks target creature, or cell? important question.


		public override bool IsInvalid => base.IsInvalid || Target == null;
		public bool IsOutOfRange => Creature?.Position.ChebyshevDistanceFrom(Target.Position) > 1; //todo, null check etc.

		public class Result : EventResult {
			//todo
			//attack hit, etc
		}

		public MeleeAttackAction(Creature creature, Creature target) : base(creature) { this.Target = target; }
		protected override Result Execute() {
			if(IsOutOfRange) return Failure();

			if(R.CoinFlip()){
				Q.Execute(new MeleeHitEvent(Creature, Target));
				//todo, result stuff?
				return Success();
			}
			else{
				Q.Execute(new MeleeMissEvent(Creature, Target));
				//todo, result stuff?
				return Failure(); //todo, would return AttackMissed, not failure
			}
		}

	}
	public class MeleeHitEvent : CreatureAction<MeleeHitEvent.Result> {
		public Creature Target { get; set; } //todo - do attacks target creature, or cell? important question.


		public override bool IsInvalid => base.IsInvalid || Target == null;

		public class Result : EventResult {
			//todo
			//attack hit, etc
		}

		public MeleeHitEvent(Creature creature, Creature target) : base(creature) { this.Target = target; }
		protected override Result Execute() {
			Q.Execute(new TakeDamageEvent(Target, 2)); //todo, pass back result from this?
			return Success();
		}

	}
	public class MeleeMissEvent : CreatureAction<MeleeMissEvent.Result> {
		public Creature Target { get; set; } //todo - do attacks target creature, or cell? important question.


		public override bool IsInvalid => base.IsInvalid || Target == null;

		public class Result : EventResult {
			//todo
			//attack hit, etc
		}

		public MeleeMissEvent(Creature creature, Creature target) : base(creature) { this.Target = target; }
		protected override Result Execute() {
			//todo
			return Success();
		}

	}
}
