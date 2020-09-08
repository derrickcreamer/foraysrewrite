using System;
using System.Collections.Generic;
using GameComponents;

namespace Forays{
    public class EventQueue : EventScheduler{
        public List<IEvent> EventStack;

        public EventQueue(){
            EventStack = new List<IEvent>();
        }

        public TResult Execute<TResult>(Event<TResult> ev) where TResult : EventResult, new() {
			if(!ev.NoCancel && ev.Decider?.Cancels(ev) == true){
                return new TResult { Canceled = true };
            }
            EventStack.Add(ev);
            //todo notify
            TResult result;
            try{
                IEventQueueEvent<TResult> eqEvent = ev;
                result = eqEvent.Execute();
            }
            //todo, maybe this should catch, and notify to see if the UI thinks the exception should be swallowed or not?
            finally{
                //todo notify
                EventStack.RemoveAt(EventStack.Count - 1);
            }
			//todo, if I ever need some events to wait until after the current one is done, probably do that by adding
			//  a method to GameUniverse that'll push-execute-pop and then check for more waiting, & call it here.
            return result;
        }
    }
}
