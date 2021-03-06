﻿using System;
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
		public virtual ICancelDecider CancelDecider { get; set; }
		public virtual bool NoCancel { get; set; }

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
	// SimpleEvent is for those rare event types that will never need a return value (like player and AI turns).
	public abstract class SimpleEvent : Event<SimpleEvent.NullResult>, IEvent {
		public SimpleEvent(GameUniverse g) : base(g){
			NoCancel = true;
		}
		protected abstract void ExecuteSimpleEvent();
		// Reimplement IEvent.ExecuteEvent in order to make use of EventQueue features:
		void IEvent.ExecuteEvent(){
			Q.Execute(this);
		}
		protected sealed override NullResult Execute() {
			ExecuteSimpleEvent();
			return null;
		}
		public class NullResult : EventResult { }
	}
	// EventResult is the base class for Event results, with cancellation & cost info ready if needed.
	public class EventResult {
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
		bool Cancels(GameObject action);
	}
	public abstract class CancelDecider : GameObject, ICancelDecider {
		public CancelDecider(GameUniverse g) : base(g) { }

		// todo, xml comment here to explain purpose
		public virtual bool? WillCancel(GameObject action) => null;
		public abstract bool Cancels(GameObject action);
	}
	public class PlayerCancelDecider : CancelDecider {
		public PlayerCancelDecider(GameUniverse g) : base(g) { }

		///<summary>Invoked to decide whether to cancel each action taken by the player</summary>
		public Func<GameObject, bool> DecideCancel;

		public override bool Cancels(GameObject action) => DecideCancel?.Invoke(action) ?? false;
	}
}
