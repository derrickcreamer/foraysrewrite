using System;
using GameComponents;
using GameComponents.DirectionUtility;

namespace Forays {
	// CreatureEvent is a base class for creature actions - it'll use the creature's decider by default.
	public abstract class CreatureEvent<TResult> : EasyEvent<TResult> where TResult : ActionResult, new() {
		public virtual Creature Creature { get; set; }
		public CreatureEvent(Creature creature) : base(creature.GameUniverse) { this.Creature = creature; }
		public override ICancelDecider Decider => Creature?.Decider;
		public override bool IsInvalid => Creature == null;
	}
	public interface IItemUseEvent { } //todo, not final
	public class NotifyItemHadNoEffect : EventNotify<IItemUseEvent> { } //todo: this isn't used yet, nor is it final

	public class WalkEvent : CreatureEvent<PassFailResult> {
		public Point Destination { get; set; }
		public bool IgnoreRange { get; set; } = false; //todo, should i have a naming convention for 'arg' properties vs. calculated properties?
													   // ...such as IgnoreRange vs. OutOfRange. if i had a convention it might suggest 'IgnoreRange' and 'IsOutOfRange' - would that work for most?
		public WalkEvent(Creature creature, Point destination) : base(creature) {
			this.Destination = destination;
		}
		public bool OutOfRange => !IgnoreRange && Creature.Position?.ChebyshevDistanceFrom(Destination) > 1;
		//todo: IsInvalid shows the call to base.IsValid which actually checks the same thing right now:
		public override bool IsInvalid => Creature == null || base.IsInvalid
			|| Destination.X < 0 || Destination.X >= GameUniverse.MapWidth
			|| Destination.Y < 0 || Destination.Y >= GameUniverse.MapHeight; /* or destination not on map */
		protected override PassFailResult ExecuteFinal() {
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

		public override void ExecuteEvent() {

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
				Notify(new NotifyPrintMessage{ Message = "The enemy glares." });
			}
			else {
				new WalkEvent(Creature, dest).Execute();
			}

			Q.Schedule(new AiTurnEvent(Creature), 120, null); //todo, creature initiative
		}
	}

	public class PlayerTurnEvent : SimpleEvent {
		public IActionEvent ChosenAction { get; set; } = null;

		public class NotifyTurnStart : EventNotify<PlayerTurnEvent> { }
		public class NotifyChooseAction : EventNotify<PlayerTurnEvent> { }

		public PlayerTurnEvent(GameUniverse g) : base(g) { }

		public override void ExecuteEvent() {
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
			if(ChosenAction is WalkEvent /*|| ChosenAction is FireballEvent*/) {
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
}
