using System;

namespace Forays {
	public class PlayerTurnEvent : SimpleEvent {
		//todo xml: must be Event<TResult>
		public GameObject ChosenAction { get; set; } = null;

		public PlayerTurnEvent(GameUniverse g) : base(g) { }

		protected override void ExecuteSimpleEvent() {
			//if(Player.State == CreatureState.Dead) return;

			if(GameUniverse.Suspend) {
				// (this should either reschedule, or use some kind of "don't remove the current event" feature on the queue...
				Q.ScheduleNow(new PlayerTurnEvent(GameUniverse));
				return;
			}
			EventResult result = null;
			switch(ChosenAction){
				// This section has some duplication because of how the type parameters to Q.Execute work:
				case WalkAction action: result = Q.Execute(action); break;
				case MeleeAttackAction action: result = Q.Execute(action); break;
				case DescendAction action: result = Q.Execute(action); break;
				case TodoChangeTerrainEvent action: result = Q.Execute(action); break;
				case PickUpItemAction action: result = Q.Execute(action); break;
				case DropItemAction action: result = Q.Execute(action); break;
				//todo, etc...
				default: break;
			}
			if(result?.Canceled == true){
				Q.ScheduleNow(new PlayerTurnEvent(GameUniverse));
				//todo, does this reschedule at 0, or just loop and ask again?
			}
			else{
				var time = result?.Cost ?? Turns(1);
				Q.Schedule(new PlayerTurnEvent(GameUniverse), time, Q.GetCurrentInitiative()); //todo, player initiative
			}
		}
	}
	public class DescendAction : CreatureAction<PassFailResult> {
		public override bool IsInvalid => base.IsInvalid || Creature != Player;
		protected override long Cost => 0;

		public DescendAction(Creature creature) : base(creature){ }
		protected override PassFailResult Execute() {
			if(TileTypeAt(Creature.Position) != TileType.Staircase) return Failure();
			Q.Execute(new MoveToNextLevelEvent(GameUniverse));
			return Success();
		}
	}
}
