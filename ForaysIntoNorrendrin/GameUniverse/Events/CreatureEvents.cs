﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameComponents;
using GameComponents.DirectionUtility;
using Hemlock;

namespace Forays {
	// CreatureAction is a base class for creature actions that'll use the creature's decider by default.
	public abstract class CreatureAction<TResult> : EasyAction<TResult> where TResult : ActionResult, new() {
		public virtual Creature Creature { get; set; }
		public CreatureAction(Creature creature) : base(creature.GameUniverse) { this.Creature = creature; }
		public override ICancelDecider Decider => Creature?.Decider;
		public override bool IsInvalid => Creature == null || Creature.Position == null;
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
		public bool IsBlockedByTerrain => TileTypeAt(Destination) == TileType.Wall;
		//todo: IsInvalid shows the call to base.IsValid which actually checks the same thing right now:
		public override bool IsInvalid => Creature == null || base.IsInvalid
			|| Destination.X < 0 || Destination.X >= GameUniverse.MapWidth
			|| Destination.Y < 0 || Destination.Y >= GameUniverse.MapHeight; /* or destination not on map */
		protected override PassFailResult ExecuteAction() {
			if(OutOfRange || IsBlockedByTerrain || CreatureAt(Destination) != null) {
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
			//todo, what checks whether creature is null? maybe the position check below can cover that too.
			//todo, if creature is dead?
			if(Creature.Position == null) return; // todo... creatures off the map shouldn't be getting turns

			int timeTaken = Creature.ExecuteMonsterTurn();
			Q.Schedule(new AiTurnEvent(Creature), timeTaken, Q.GetCurrentInitiative());

			return; //todo clean up


			List<Point> validPoints = Creature.Position.Value.EnumeratePointsWithinChebyshevDistance(1, false, false)
				.Where(p => TileTypeAt(p) != TileType.Wall).ToList();
			Point dest = Creature.Position.Value;
			if(validPoints.Count > 0)
				dest = validPoints[R.GetNext(validPoints.Count)];

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

			Q.Schedule(new AiTurnEvent(Creature), Turns(1), Q.GetCurrentInitiative());
		}
	}

	public class PlayerTurnEvent : SimpleEvent {
		public IActionEvent ChosenAction { get; set; } = null;

		public class NotifyTurnStart : EventNotify<PlayerTurnEvent> { }
		public class NotifyChooseAction : EventNotify<PlayerTurnEvent> { }
		public class NotifyTurnEnd : EventNotify<PlayerTurnEvent> {
			public IActionResult ActionResult { get; set; }
		}

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
				Q.ScheduleNow(new PlayerTurnEvent(GameUniverse));
				return;
			}
			if(ChosenAction == null) {
				//todo: it *might* be necessary to create & use a DoNothing action here, if important things happen during that action.
				Q.Schedule(new PlayerTurnEvent(GameUniverse), Turns(1), Q.GetCurrentInitiative()); // todo, player initiative
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
			IActionResult result = null;
			if(ChosenAction is WalkAction || ChosenAction is AttackAction || ChosenAction is DescendAction /*|| ChosenAction is FireballEvent*/) {
				result = ChosenAction.Execute();
				if(result.InvalidEvent) {
					throw new InvalidOperationException($"Invalid event passed to player turn action [{ChosenAction.GetType().ToString()}]");
				}
				if(result.Canceled) {
					Q.ScheduleNow(new PlayerTurnEvent(GameUniverse));
					//todo, does this reschedule at 0, or just loop and ask again?
				}
				else {
					var time = result.Cost;
					Q.Schedule(new PlayerTurnEvent(GameUniverse), time, Q.GetCurrentInitiative()); //todo, player initiative
				}
			}
			else {
				Q.Schedule(new PlayerTurnEvent(GameUniverse), Turns(1), Q.GetCurrentInitiative()); //todo, player initiative
			}
			//todo, should this be fired for canceled actions? thinking no..
			if(!result.Canceled)
				Notify(new NotifyTurnEnd { ActionResult = result });
		}
	}
	public class TakeDamageEvent : Event<TakeDamageEvent.Result> {
		public Creature Creature { get; set; }
		public int Amount { get; set; } //todo, any reason to have these as properties? pretty sure only the ones that need to belong to interfaces (Creature, probably targeting stuff, etc.) will need to be properties.
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
	public class DescendAction : CreatureAction<PassFailResult> {
		public override bool IsInvalid => base.IsInvalid || Creature != Player;
		protected override long Cost => 0;

		public DescendAction(Creature creature) : base(creature){ }
		protected override PassFailResult ExecuteAction() {
			if(Creature.TileTypeAt(Creature.Position.Value) != TileType.Staircase) return Failure();
			//todo, Grid.Clear method?
			//todo, repeated code here:
			GameUniverse.Creatures = new Grid<Creature, Point>(p => p.X >= 0 && p.X < GameUniverse.MapWidth && p.Y >= 0 && p.Y < GameUniverse.MapHeight);
			GameUniverse.CurrentDepth++;
			GameUniverse.CurrentLevelType = GameUniverse.MapRNG.OneIn(4) ? DungeonLevelType.Cramped : DungeonLevelType.Sparse;
			GameUniverse.GenerateMap();

			Creatures.Add(Player, new Point(15, 8));

			int numEnemies = GameUniverse.MapRNG.GetNext(8 + GameUniverse.CurrentDepth);
			for(int i = 0; i<numEnemies; ++i) {
				Creature c = new Creature(GameUniverse);
				Creatures.Add(c, new Point(GameUniverse.MapRNG.GetNext(GameUniverse.MapWidth-2)+1, GameUniverse.MapRNG.GetNext(GameUniverse.MapHeight-2)+1));
				Q.Schedule(new AiTurnEvent(c), Turns(10), Q.GetCurrentInitiative());
			}

			if(GameUniverse.CurrentLevelType == DungeonLevelType.Cramped) Player.ApplyStatus(Status.Stunned, Turns(5));
			return Success();
		}
	}
	public class StatusExpirationEvent : SimpleEvent {
		public Creature Creature { get; set; }
		public StatusInstance<Creature> StatusInstance;

		public StatusExpirationEvent(Creature creature, StatusInstance<Creature> statusInstance) : base(creature.GameUniverse) {
			this.Creature = creature;
			this.StatusInstance = statusInstance;
		}

		protected override void ExecuteSimpleEvent() {
			//if(Creature.State == CreatureState.Dead) return;
			//if(Creature.Position == null) todo, any checking here for dead/offmap?
			Creature.StatusTracker.RemoveStatusInstance(StatusInstance);
		}
	}
}