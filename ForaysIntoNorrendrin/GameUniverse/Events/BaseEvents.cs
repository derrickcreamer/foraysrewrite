using System;
using GameComponents;

namespace Forays {
	// SimpleEvent is a separate branch here, used only for a few event types (like player and AI turns).
	// Most events will return Results objects and inherit from Event<TResult>.
	// (SimpleEvent wouldn't need to exist if void could be used as a type param...)
	//todo, xml: no return value
	public abstract class SimpleEvent : GameObject, IEvent {
		public SimpleEvent(GameUniverse g) : base(g) { }
		public abstract void ExecuteEvent();
		public override T Notify<T>(T notification) {
			if(notification is IEventNotify eventNotify) {
				eventNotify.SetEvent(this); // Automatically associate this event with the notification when possible
			}
			return base.Notify(notification);
		}
		public T Notify<T>() where T : new() => Notify(new T());
	}
	// Event<TResult> is the primary base Event type, because most events will return some kind of result.
	public abstract class Event<TResult> : GameObject, IEvent {
		public Event(GameUniverse g) : base(g) { }
		void IEvent.ExecuteEvent() { Execute(); }
		public abstract TResult Execute();
		public override T Notify<T>(T notification) {
			if(notification is IEventNotify eventNotify) {
				eventNotify.SetEvent(this); // Automatically associate this event with the notification when possible
			}
			return base.Notify(notification);
		}
		public T Notify<T>() where T : new() => Notify(new T());
	}
	// EventResult is a recommended base type for the TResult returned by an Event execution.
	// It has a bool InvalidEvent, which should be true if the event was in a bad state and could not be executed.
	public class EventResult {
		public virtual bool InvalidEvent { get; set; }
	}
	// IActionResult and IActionEvent are used by the 'player turn' action. The UI supplies an IActionEvent that
	// can be executed to return an IActionResult, so the next turn can be scheduled at the correct time.
	public interface IActionResult {
		bool InvalidEvent { get; }
		bool Canceled { get; }
		long Cost { get; }
	}
	public interface IActionEvent {
		IActionResult Execute();
	}
	// ActionResult is the result of an ActionEvent, and carries cancellation & cost info.
	public class ActionResult : EventResult, IActionResult {
		public virtual bool Canceled { get; set; }
		//todo, xml: this value should be ignored if InvalidEvent and/or Canceled
		public virtual long Cost { get; set; } = 120; //todo, default? "1.Turn()" or anything?
	}
	public class PassFailResult : ActionResult {
		public virtual bool Succeeded { get; set; }
	}
	// ActionEvents are events that an entity performs as part of its turn. They can be cancelled, and
	// they have a (time) cost that usually determines how quickly that entity will receive another turn.
	public abstract class ActionEvent<TResult> : Event<TResult>, IActionEvent where TResult : ActionResult, new() {
		//note that the NoCancel bool indicates that cancellations will be ignored and
		// SHOULD not be used, but it's not a hard requirement.
		public virtual bool NoCancel { get; set; }
		public ActionEvent(GameUniverse g) : base(g) { }
		IActionResult IActionEvent.Execute() => Execute();
		public virtual bool IsInvalid => false;
		protected virtual long Cost => 120L; // actually 1.Turn() or Turns(1) or whatever
		protected virtual TResult Error() => new TResult() { InvalidEvent = true };
		protected virtual TResult Cancel() => new TResult() { Canceled = true };
		protected virtual TResult Done() => new TResult(){ Cost = Cost };
		protected virtual TResult Success(){
			var result = Done();
			var pf = result as PassFailResult;
			if(pf != null) pf.Succeeded = true;
			return result;
		}
		protected virtual TResult Failure() {
			var result = Done();
			var pf = result as PassFailResult;
			if(pf != null) pf.Succeeded = false;
			return result;
		}
	}
	// The CancelDecider interface/class exists so that an action can be canceled by the entity
	// performing that action. It's used by both the AI and the UI to back out of ill-advised actions.
	//todo, xml: this should return false for types it doesn't recognize
	public interface ICancelDecider {
		bool Cancels(object action);
	}
	public abstract class CancelDecider : GameObject, ICancelDecider {
		public CancelDecider(GameUniverse g) : base(g) { }

		public virtual bool? WillCancel(object action) => null;
		public abstract bool Cancels(object action);
	}
	public class PlayerCancelDecider : CancelDecider { //todo, make sure this is still right, and still needed
		public PlayerCancelDecider(GameUniverse g) : base(g) { }

		public class NotifyDecide {
			public object Action;
			public bool CancelAction;
		}
		public override bool Cancels(object action) {
			var result = Notify(new NotifyDecide { Action = action });
			return result.CancelAction;
		}
	}
	// EasyEvent exists to reduce boilerplate for this standard pattern:
	// 1) If event can't be executed, return with IsInvalid=true.
	// 2) If event can be canceled and if the decider chooses to cancel, return with Canceled=true.
	// 3) Otherwise, execute normally and return result.
	public abstract class EasyEvent<TResult> : ActionEvent<TResult> where TResult : ActionResult, new() {
		public EasyEvent(GameUniverse g) : base(g) { }
		//todo, xml: null is fine
		public abstract ICancelDecider Decider { get; }
		//todo, xml: this happens after the validity check & cancel check
		protected abstract TResult ExecuteFinal();
		public sealed override TResult Execute() {
			if(IsInvalid) return Error();
			if(!NoCancel && Decider?.Cancels(this) == true) return Cancel();
			return ExecuteFinal();
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
