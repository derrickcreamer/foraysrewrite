using System;
using GameComponents;

namespace Forays {
	// This interface exists so that the EventQueue can easily execute events without forcing all of the raw Execute methods to be public:
	public interface IEventQueueEvent<TResult>{
		TResult Execute();
	}
	// Event<TResult> is the primary base Event type, because most events will return some kind of result.
	public abstract class Event<TResult> : GameObject, IEvent, IEventQueueEvent<TResult> where TResult : EventResult, new() {
		public Event(GameUniverse g) : base(g) { }
		// Because attempting to execute an invalid event should throw an exception, IsInvalid provides a way to test it first.
		public virtual bool IsInvalid => false;
		// This one holds the actual game logic and is only executed after IsInvalid and cancellation are checked:
		protected abstract TResult Execute();
		// Implement the IEvent method explicitly so it doesn't get in the way:
		void IEvent.ExecuteEvent() { Execute(); } //todo, should this one add this event to the stack or not? would only matter for turns and status expirations.
		// Likewise for IEventQueueEvent:
		TResult IEventQueueEvent<TResult>.Execute() => Execute();
		protected virtual long Cost => GameUniverse.TicksPerTurn;
		// If present, will be consulted while the event is executing:
		public virtual ICancelDecider Decider { get; set; }
		public virtual bool NoCancel { get; set; }
		public override T Notify<T>(T notification) {
			if(notification is IEventNotify eventNotify) {
				eventNotify.SetEvent(this); // Automatically associate this event with the notification when possible
			}
			return base.Notify(notification);
		}
		public T Notify<T>() where T : new() => Notify(new T());

		//todo, rearrange for readability

		//protected virtual TResult Cancel() => new TResult() { Canceled = true };
		protected virtual TResult Done() => new TResult(){ Cost = Cost };
		protected virtual TResult Success(){
			TResult result = Done();
			var pf = result as PassFailResult;
			if(pf != null) pf.Succeeded = true;
			return result;
		}
		protected virtual TResult Failure() {
			TResult result = Done();
			var pf = result as PassFailResult;
			if(pf != null) pf.Succeeded = false;
			return result;
		}
	}

	//todo, put the old comment for ActionEvent somewhere if needed:
		// ActionEvents are events that an entity performs as part of its turn. They have a (time) cost
	//   that determines how quickly that entity will receive another turn.


	// SimpleEvent is for those rare event types that will never need a return value (like player and AI turns).
	public abstract class SimpleEvent : Event<SimpleEvent.NullResult> {
		public SimpleEvent(GameUniverse g) : base(g) { }
		protected abstract void ExecuteSimpleEvent();
		protected sealed override NullResult Execute() {
			ExecuteSimpleEvent();
			return null;
		}
		public class NullResult : EventResult { }
	}
	// IActionResult and IActionEvent are used by the 'player turn' action. The UI supplies an IActionEvent that
	// can be executed to return an IActionResult, so the next turn can be scheduled at the correct time.
	public interface IActionResult {
		bool Canceled { get; }
		long Cost { get; }
	}
	public interface IActionEvent {
		IActionResult Execute();
	}
	// EventResult is the base class for Event results, with cancellation & cost info ready if needed.
	public class EventResult : IActionResult {
		public virtual bool Canceled { get; set; }
		//todo, xml: this value should be ignored if Canceled
		public virtual long Cost { get; set; } = GameUniverse.TicksPerTurn;
	}
	public class PassFailResult : EventResult {
		public virtual bool Succeeded { get; set; }
	}
	// The CancelDecider interface/class exists so that an action can be canceled by the entity
	// performing that action. It's used by both the AI and the UI to back out of ill-advised actions.
	//todo, xml: this should return false for types it doesn't recognize
	public interface ICancelDecider {
		bool Cancels(object action);
	}
	public abstract class CancelDecider : GameObject, ICancelDecider {
		public CancelDecider(GameUniverse g) : base(g) { }

		// todo, xml comment here to explain purpose
		public virtual bool? WillCancel(object action) => null;
		public abstract bool Cancels(object action);
	}
	public class PlayerCancelDecider : CancelDecider { //todo, make sure this is still right, and still needed
		public PlayerCancelDecider(GameUniverse g) : base(g) { }

		public class NotifyDecide { //todo, name? maybe notify decide cancellation?
			public object Action; //todo, name?
			public bool CancelAction;
		}
		public override bool Cancels(object action) {
			var result = Notify(new NotifyDecide { Action = action });
			return result.CancelAction;
		}
	}
	// This interface just makes EventNotify<T> easier to work with internally.
	public interface IEventNotify {
		void SetEvent(object o);
	}
	// This base class doesn't do anything on the UI side - it just makes defining these a bit easier.
	public class EventNotify<T> : IEventNotify {
		public T Event { get; set; }
		void IEventNotify.SetEvent(object o) { Event = (T)o; }
	}
}
