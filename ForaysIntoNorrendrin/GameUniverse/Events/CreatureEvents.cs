using System;
using GameComponents;
using GameComponents.DirectionUtility;

namespace Forays {
	// CreatureAction is a base class for creature actions that'll use the creature's decider by default.
	public abstract class CreatureAction<TResult> : EasyAction<TResult> where TResult : ActionResult, new() {
		public virtual Creature Creature { get; set; }
		public CreatureAction(Creature creature) : base(creature.GameUniverse) { this.Creature = creature; }
		public override ICancelDecider Decider => Creature?.Decider;
		public override bool IsInvalid => Creature == null;
	}
	public interface IItemUseEvent { } //todo, not final
	public class NotifyItemHadNoEffect : EventNotify<IItemUseEvent> { } //todo: this isn't used yet, nor is it final

	public class WalkAction : CreatureAction<PassFailResult> {
		public Point Destination { get; set; }
		public bool IgnoreRange { get; set; } = false; //todo, should i have a naming convention for 'arg' properties vs. calculated properties?
													   // ...such as IgnoreRange vs. OutOfRange. if i had a convention it might suggest 'IgnoreRange' and 'IsOutOfRange' - would that work for most?
		public WalkAction(Creature creature, Point destination) : base(creature) {
			this.Destination = destination;
		}
		//todo, rename to IsOutOfRange, and should this actually check IgnoreRange, or should that be checked in Execute?
		public bool OutOfRange => !IgnoreRange && Creature.Position?.ChebyshevDistanceFrom(Destination) > 1;
		//todo: IsInvalid shows the call to base.IsValid which actually checks the same thing right now:
		public override bool IsInvalid => Creature == null || base.IsInvalid
			|| Destination.X < 0 || Destination.X >= GameUniverse.MapWidth
			|| Destination.Y < 0 || Destination.Y >= GameUniverse.MapHeight; /* or destination not on map */
		protected override PassFailResult ExecuteAction() {
			if(OutOfRange /*|| TerrainIsBlocking*/ || CreatureAt(Destination) != null) {
				// todo, there would be some kind of opportunity to print a message here.
				return Failure();
			}
			bool moved = Creatures.Move(Creature, Destination);
			if(moved) return Success();
			else return Failure();
		}
	}

	/*public class FireballEvent : CreatureEvent<ActionResult> {

		public class NotifyExplosion : EventNotify<FireballEvent> {
			public int CurrentRadius { get; set; }
		}

		public Point? Target { get; set; }

		public FireballEvent(Creature caster, Point? target) : base(caster) {
			this.Target = target;
		}

		protected override ActionResult ExecuteFinal() {
			if(Target == null) {
				//todo "you waste the spell"
				return Done();
			}
			for(int i = 0; i<=2; ++i) {
				//todo, animation? here's an attempt:
				Notify(new NotifyExplosion { CurrentRadius = i });
				foreach(Creature c in Creatures[Target.Value.EnumeratePointsAtManhattanDistance(i, true)]) {
					//c.State = CreatureState.Dead;
					//todo, does anything else need to be done here?
				}
			}
			return Done();
		}
	}*/

	public class AiTurnEvent : SimpleEvent {
		public Creature Creature { get; set; }
		public AiTurnEvent(Creature creature) : base(creature.GameUniverse) {
			this.Creature = creature;
		}

		protected override void ExecuteSimpleEvent() {

			//if(Creature.State == CreatureState.Dead) return;
			// todo: All this actual AI code *probably* won't go directly in the event like this.
			// It'll probably be a method on the Creature, and this event will just call it.
			/*foreach(Creature c in Creatures[Creature.Position?.EnumeratePointsAtChebyshevDistance(1, true, false)]) {
				if(c == Player) {
					//todo, message about being fangoriously devoured
					//Player.State = CreatureState.Dead;
					//todo, what else?
					return;
				}
			}*/
			// Otherwise, just change state:
			//if(Creature.State == CreatureState.Angry) Creature.State = CreatureState.Crazy;
			//else if(Creature.State == CreatureState.Crazy) Creature.State = CreatureState.Angry;

			if(Creature.Position == null) return; // todo... creatures off the map shouldn't be getting turns

			Point dest = Creature.Position.Value.PointInDir((Dir8)R.GetNext(9)+1);

			if(CreatureAt(dest) != null && CreatureAt(dest) != Creature){
				if(CreatureAt(dest) != Player) {
					Notify(new NotifyPrintMessage{ Message = "The enemy glares." });
				}
				else {
					Notify(new NotifyPrintMessage{ Message = "The enemy hits you."}); //todo, remove this when each event autom. sends a notify
					new AttackAction(Creature, Player).Execute();
				}
			}
			else {
				new WalkAction(Creature, dest).Execute();
			}

			Q.Schedule(new AiTurnEvent(Creature), 120, null); //todo, creature initiative
		}
	}

	public class PlayerTurnEvent : SimpleEvent {
		public IActionEvent ChosenAction { get; set; } = null;

		public class NotifyTurnStart : EventNotify<PlayerTurnEvent> { }
		public class NotifyChooseAction : EventNotify<PlayerTurnEvent> { }

		public PlayerTurnEvent(GameUniverse g) : base(g) { }

		protected override void ExecuteSimpleEvent() {
			Notify<NotifyTurnStart>();
			//if(Player.State == CreatureState.Dead) return;
			Notify<NotifyChooseAction>();
			//todo, i wonder if it would save time, or be confusing, if I had THIS form and also another form for convenience...
			//  ...maybe there's still only one going out, but from in here we can Notify(this, SimpleNotification.PlayerTurnStarted); ?
			//  seems like it would run into the naming problems like before, but it would be a bit easier otherwise.

			if(GameUniverse.Suspend) {
				// (this should either reschedule, or use some kind of "don't remove the current event" feature on the queue...
				Q.ScheduleImmediately(new PlayerTurnEvent(GameUniverse));
				return;
			}
			if(ChosenAction == null) {
				//todo: it *might* be necessary to create & use a DoNothing action here, if important things happen during that action.
				Q.Schedule(new PlayerTurnEvent(GameUniverse), 120, null); // todo, player initiative
				return;
			}

			//then check the result of the hook and make sure a valid event was chosen

			//then execute

			/*switch(choice.ChosenAction) {
				case WalkEvent e:
					//todo, probably THIS one will be used if i'm going to check whether the player is actually the actor here.
					break;
				case FireballEvent e:
					break;
			}*/
			if(ChosenAction is WalkAction || ChosenAction is AttackAction /*|| ChosenAction is FireballEvent*/) {
				var result = ChosenAction.Execute(); //todo, wait, don't i need to check for cancellation here?
				if(result.InvalidEvent) {
					throw new InvalidOperationException($"Invalid event passed to player turn action [{ChosenAction.GetType().ToString()}]");
				}
				if(result.Canceled) {
					Q.ScheduleImmediately(new PlayerTurnEvent(GameUniverse));
					//todo, does this reschedule at 0, or just loop and ask again?
				}
				else {
					var time = result.Cost;
					Q.Schedule(new PlayerTurnEvent(GameUniverse), time, null); //todo, player initiative
				}
			}
			else {
				Q.Schedule(new PlayerTurnEvent(GameUniverse), 120, null); //todo, player initiative
			}
		}
	}
	public class TakeDamageEvent : Event<TakeDamageEvent.Result> {
		public Creature Creature { get; set; }
		public int Amount { get; set; }
		// (it's possible that this event could eventually get a DamageSource property, or a DamageTypes collection, etc.)

		public bool IsInvalid => Creature == null || Amount <= 0; //todo, duplicating this?

		public class Result : EventResult {
			//public bool CreatureWasAlreadyDead { get; set; } (might be used one day, might not)
			public bool CreatureIsNowDead { get; set; } //todo, technically this isn't being used yet either...
		}

		public TakeDamageEvent(Creature creature, int amount) : base(creature.GameUniverse) {
			this.Creature = creature;
			this.Amount = amount;
		}
		public override Result Execute() {
			if(IsInvalid) return new Result { InvalidEvent = true }; //todo, duplicating this?
			if(GameUniverse.DeadCreatures.Contains(Creature)) {
				//could do other stuff here, and set CreatureWasAlreadyDead if that is ever relevant to callers.
				return new Result { CreatureIsNowDead = true };
			}
			Creature.CurHP -= Amount;
			if(Creature.CurHP <= 0) {
				if(Creature == Player){
					//...
					GameUniverse.GameOver = true;
				}
				//todo, notify creature died here
				GameUniverse.DeadCreatures.Add(Creature);
			}
			return new Result { CreatureIsNowDead = Creature.CurHP <= 0 };
		}
	}
	public class AttackAction : CreatureAction<AttackAction.Result> {
		public Creature Target { get; set; } //todo - do attacks target creature, or cell? important question.


		public override bool IsInvalid => base.IsInvalid || Target == null;
		public bool IsOutOfRange => Creature?.Position?.ChebyshevDistanceFrom(Target.Position.Value) > 1; //todo, null check etc.

		public class Result : ActionResult {
			//todo
			//attack hit, etc
		}

		public AttackAction(Creature creature, Creature target) : base(creature) { this.Target = target; }
		protected override Result ExecuteAction() {
			if(IsOutOfRange) return Failure();

			if(R.CoinFlip()) return Failure(); //todo, would return AttackMissed, not failure

			new TakeDamageEvent(Target, 2).Execute();

			return Success();
		}

	}
}
