using System;
using System.Collections.Generic;
using GameComponents;

namespace Forays{
	public class EventQueue : EventScheduler{
		public List<IEvent> EventStack;

		public EventQueue(){
			EventStack = new List<IEvent>();
		}
		///<summary>Hook that runs before each game event executes, to allow the UI to do what it needs to (printing messages etc.)
		/// The GameObject will be one of the subtypes of Forays.Event.</summary>
		public Action<GameObject> BeforeEventExecute;
		///<summary>Hook that runs after each game event executes, to allow the UI to do what it needs to (printing messages etc.)
		/// The GameObject will be one of the subtypes of Forays.Event (the same one sent in BeforeEventExecute) and the
		/// EventResult is likely to be a more specific type, from which you can retrieve more details about this event's execution.</summary>
		public Action<GameObject, EventResult> AfterEventExecute;

		public TResult Execute<TResult>(Event<TResult> ev) where TResult : EventResult, new() {
			if(!ev.NoCancel && ev.CancelDecider?.Cancels(ev) == true){
				return new TResult { Canceled = true };
			}
			if(ev.IsInvalid){ //todo, make this work with the OnException stuff below?
				throw new InvalidOperationException($"Event {ev.GetType().Name} is invalid and can't be executed");
			}
			EventStack.Add(ev);
			BeforeEventExecute?.Invoke(ev);
			TResult result;
			try {
				IEventQueueEvent<TResult> eqEvent = ev;
				result = eqEvent.Execute();
				AfterEventExecute?.Invoke(ev, result);
			}
			catch {
				//todo, maybe this should catch, and notify to see if the UI thinks the exception should be swallowed or not?
				throw;
				//todo...   OnException(ex, ev)
			}
			finally {
				EventStack.RemoveAt(EventStack.Count - 1);
			}
			//todo, if I ever need some events to wait until after the current one is done, probably do that by adding
			//  a method to GameUniverse that'll push-execute-pop and then check for more waiting, & call it here.
			return result;
		}
	}
}
