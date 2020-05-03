using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UtilityCollections;

namespace GameComponents {
	public abstract class Initiative { }
	public enum RelativeInitiativeOrder { First, Last, BeforeCurrent, Current, AfterCurrent, BeforeTarget, Target, AfterTarget };
	public interface IEvent {
		void ExecuteEvent();
	}
	public abstract class EventScheduling {
		public IEvent Event { get; protected set; }
		public long CreationTick { get; protected set; }
		public long Delay { get; protected set; }
		protected Initiative initiative;
		public long ExecutionTick => CreationTick + Delay;
		protected EventScheduling(IEvent scheduledEvent, long currentTick, long delay, Initiative init) {
			Event = scheduledEvent;
			CreationTick = currentTick;
			Delay = delay;
			initiative = init;
		}
	}
	public class EventScheduler {
		private long currentTick;
		private PriorityQueue<InternalEventScheduling, InternalEventScheduling> pq;
		private OrderingCollection<Initiative> oc;
		private MultiValueDictionary<AutoInitiative, InternalEventScheduling> scheduledEventsForInitiatives;
		private InternalEventScheduling currentlyExecuting;

		public long CurrentTick => currentTick;

		private int CompareEventSchedulings(InternalEventScheduling first, InternalEventScheduling second) {
			int tickCompare = first.ExecutionTick.CompareTo(second.ExecutionTick);
			if(tickCompare != 0) return tickCompare;
			else return oc.Compare(first.Initiative, second.Initiative);
		}
		public EventScheduler() {
			pq = new PriorityQueue<InternalEventScheduling, InternalEventScheduling>(x => x, CompareEventSchedulings);
			oc = new OrderingCollection<Initiative>();
			scheduledEventsForInitiatives = new MultiValueDictionary<AutoInitiative, InternalEventScheduling>();
		}
		private EventScheduler(
			IEnumerable<Initiative> inits,
			IEnumerable<InternalEventScheduling> eventSchedulings,
			IEnumerable<IGrouping<AutoInitiative, InternalEventScheduling>> eventsForInits)
		{
			pq = new PriorityQueue<InternalEventScheduling, InternalEventScheduling>(x => x, eventSchedulings, CompareEventSchedulings);
			oc = new OrderingCollection<Initiative>(inits, null);
			scheduledEventsForInitiatives = new MultiValueDictionary<AutoInitiative, InternalEventScheduling>(eventsForInits, null);
		}
		public void ExecuteNextEvent() {
			InternalEventScheduling es = pq.Dequeue();
			currentTick = es.ExecutionTick;
			currentlyExecuting = es;
			currentlyExecuting.Event.ExecuteEvent();
			currentlyExecuting = null;
			RemoveScheduling(es);
		}
		private void RemoveScheduling(InternalEventScheduling es) { //todo, double-check this method
			if(es?.Initiative is AutoInitiative autoInit) {
				scheduledEventsForInitiatives.Remove(autoInit, es);
				if(!scheduledEventsForInitiatives.AnyValues(autoInit)) {
					// If there are no remaining scheduled events with a reference to it,
					// get rid of this AutoInitiative.
					scheduledEventsForInitiatives.Clear(autoInit);
					oc.Remove(autoInit);
				}
			}
		}
		//todo xml... explain basics, and def. explain that initiative is THE initiative that'll be used, as compared to the relative version.
		public EventScheduling Schedule(IEvent scheduledEvent, long ticksInFuture, Initiative initiative) {
			if(scheduledEvent == null) throw new ArgumentNullException(nameof(scheduledEvent));
			if(initiative == null) throw new ArgumentNullException(nameof(initiative)); // Initiatives must have been created in this class:
			else if(!oc.Contains(initiative)) throw new InvalidOperationException("Can't use an unordered initiative. Use CreateInitiative methods instead.");
			var es = new InternalEventScheduling(scheduledEvent, currentTick, ticksInFuture, initiative);
			pq.Enqueue(es);
			return es;
		}
		//todo xml: not sure how much to describe here, but it uses THIS tick and THIS initiative,
		// so it's kind of like adding it to a "right now" queue.
		public EventScheduling ScheduleNow(IEvent scheduledEvent) {
			if(scheduledEvent == null) throw new ArgumentNullException(nameof(scheduledEvent));
			if(currentlyExecuting == null) throw new InvalidOperationException("There is no currently executing event from which to get an initiative");
			var es = new InternalEventScheduling(scheduledEvent, currentTick, 0, currentlyExecuting.Initiative); //todo, this doesn't handle scheduledEventsForInitiatives, right? need a better helper for that.
			pq.Enqueue(es);
			return es;
		}
		// todo xml
		public EventScheduling ScheduleEndOfDuration(IEvent durationEndEvent, long ticksInFuture) { //todo, instead of (or MAYBE in addition to) this, there should be a ScheduleBefore(EventScheduling otherScheduling) as well as ScheduleAfter.
			if(durationEndEvent == null) throw new ArgumentNullException(nameof(durationEndEvent));  // (todo) ... what about a ticks param too? how else would THIS method be duplicated? Maybe the name SHOULD mention the initiative.
			AutoInitiative init = new AutoInitiative();
			oc.InsertBefore(currentlyExecuting?.Initiative, init);
			var es = new InternalEventScheduling(durationEndEvent, currentTick, ticksInFuture, init);
			scheduledEventsForInitiatives.Add(init, es);
			pq.Enqueue(es);
			return es;
		}
		//todo xml, maybe a note here about how this is relative to all currently extant initiatives
		public EventScheduling ScheduleWithRelativeInitiative(IEvent scheduledEvent, long ticksInFuture, RelativeInitiativeOrder relativeInitiativeOrder)
			=> ScheduleWithRelativeInitiativeInternal(scheduledEvent, ticksInFuture, relativeInitiativeOrder, null);

		//todo xml, make sure to reinforce here that ticksInFuture is from the CURRENT tick only.
		public EventScheduling ScheduleWithRelativeInitiative(IEvent scheduledEvent, long ticksInFuture, RelativeInitiativeOrder relativeInitiativeOrder, Initiative targetInitiative)
			=> ScheduleWithRelativeInitiativeInternal(scheduledEvent, ticksInFuture, relativeInitiativeOrder, targetInitiative);

		public EventScheduling ScheduleWithRelativeInitiative(IEvent scheduledEvent, long ticksInFuture, RelativeInitiativeOrder relativeInitiativeOrder, EventScheduling targetEventScheduling){
			if(targetEventScheduling == null) throw new ArgumentNullException(nameof(targetEventScheduling));
			var es = targetEventScheduling as InternalEventScheduling;
			if(es == null) throw new InvalidOperationException("User-created subtypes of EventScheduling are not supported");
			return ScheduleWithRelativeInitiativeInternal(scheduledEvent, ticksInFuture, relativeInitiativeOrder, es.Initiative);
			// (Commit. Maybe test. Then start turning AutoInitiative into a regular InternalInitiative with an internal bool.)
		}
		private EventScheduling ScheduleWithRelativeInitiativeInternal(IEvent scheduledEvent, long ticksInFuture, RelativeInitiativeOrder relativeInitiativeOrder, Initiative targetInitiative){
			if(scheduledEvent == null) throw new ArgumentNullException(nameof(scheduledEvent));
			switch(relativeInitiativeOrder){
				case RelativeInitiativeOrder.BeforeCurrent:
				case RelativeInitiativeOrder.Current:
				case RelativeInitiativeOrder.AfterCurrent:
					if(currentlyExecuting == null) throw new InvalidOperationException("There is no currently executing event from which to get an initiative");
					break;
				case RelativeInitiativeOrder.BeforeTarget:
				case RelativeInitiativeOrder.Target:
				case RelativeInitiativeOrder.AfterTarget:
					if(targetInitiative == null) throw new ArgumentNullException(nameof(targetInitiative), "The chosen RelativeInitiativeOrder requires a target initiative, but no initiative was passed in.");
					break;
			}
			Initiative init;
			if(relativeInitiativeOrder == RelativeInitiativeOrder.Current){
				init = currentlyExecuting.Initiative;
			}
			else if(relativeInitiativeOrder == RelativeInitiativeOrder.Target){
				init = targetInitiative;
				if(!oc.Contains(init)) throw new InvalidOperationException("Target initiative must exist in this event scheduler");
			}
			else{
				init = new AutoInitiative();
				InsertInitiative(relativeInitiativeOrder, init);
			}
			var es = new InternalEventScheduling(scheduledEvent, currentTick, ticksInFuture, init);
			if(init is AutoInitiative autoInit)
				scheduledEventsForInitiatives.Add(autoInit, es);
			pq.Enqueue(es);
			return es;
		}
		//todo xml: returns false if not actually scheduled
		public bool CancelEventScheduling(EventScheduling eventScheduling) {
			if(eventScheduling == null) throw new ArgumentNullException(nameof(eventScheduling));
			var es = eventScheduling as InternalEventScheduling;
			if(es == null) throw new InvalidOperationException("User-created subtypes of EventScheduling are not supported");
			bool result = pq.Remove(es);
			RemoveScheduling(es);
			return result; //todo, this method must throw if called on the currently executing event. Any other methods that need to consider that? probably reschedule method.
		}
		// todo xml-- note that this returns false if the ES is not scheduled, but still changes its value
		//  ... and that this does NOT change insertion order.
		public bool ChangeEventScheduling(EventScheduling eventScheduling, long newTicksInFuture) {
			if(eventScheduling == null) throw new ArgumentNullException(nameof(eventScheduling));
			var es = eventScheduling as InternalEventScheduling;
			if(es == null) throw new InvalidOperationException("User-created subtypes of EventScheduling are not supported");
			return pq.ChangePriority(es, () => es.ChangeDelay(newTicksInFuture));
		}
		/// <summary>
		/// Simply returns the numerical difference between the current tick and the tick at which the given EventScheduling would execute - therefore the result can be negative.
		/// Does not perform any validation such as checking whether the given EventScheduling was created by this EventScheduler.
		/// </summary>
		public long CalculateTicksInFuture(EventScheduling eventScheduling){
			if(eventScheduling == null) throw new ArgumentNullException(nameof(eventScheduling));
			return eventScheduling.ExecutionTick - currentTick;
		}
		//todo, add GetCurrentInitiative here - with xml doc notes about what it does
		public Initiative CreateInitiative(RelativeInitiativeOrder relativeOrder){
			if(relativeOrder == RelativeInitiativeOrder.BeforeCurrent || relativeOrder == RelativeInitiativeOrder.AfterCurrent && currentlyExecuting == null)
				throw new InvalidOperationException("There is no currently executing event from which to get an initiative");
			else if(relativeOrder == RelativeInitiativeOrder.BeforeTarget)
				throw new InvalidOperationException("BeforeTarget requires a target initiative - use other overload.");
			else if(relativeOrder == RelativeInitiativeOrder.AfterTarget)
				throw new InvalidOperationException("AfterTarget requires a target initiative - use other overload.");

			InternalInitiative init = new InternalInitiative();
			InsertInitiative(relativeOrder, init);
			return init;
		}
		//todo xml, ignores given init if first/last/__current is used
		public Initiative CreateInitiative(RelativeInitiativeOrder relativeOrder, Initiative targetInitiative){
			if(relativeOrder == RelativeInitiativeOrder.BeforeCurrent || relativeOrder == RelativeInitiativeOrder.AfterCurrent && currentlyExecuting == null)
				throw new InvalidOperationException("There is no currently executing event from which to get an initiative");
			else if(relativeOrder == RelativeInitiativeOrder.BeforeTarget || relativeOrder == RelativeInitiativeOrder.AfterTarget && targetInitiative == null)
				throw new ArgumentNullException(nameof(targetInitiative));

			InternalInitiative init = new InternalInitiative();
			InsertInitiative(relativeOrder, init, targetInitiative);
			return init;
		}
		//todo xml?
		public Initiative CreateInitiative(RelativeInitiativeOrder relativeOrder, EventScheduling targetEventScheduling){
			if(targetEventScheduling == null) throw new ArgumentNullException(nameof(targetEventScheduling));
			var es = targetEventScheduling as InternalEventScheduling;
			if(es == null) throw new InvalidOperationException("User-created subtypes of EventScheduling are not supported");
			return CreateInitiative(relativeOrder, es.Initiative);
		}
		//todo xml, no checks, just does the call
		private void InsertInitiative(RelativeInitiativeOrder relativeOrder, Initiative initToInsert, Initiative targetInitiative = null){
			switch(relativeOrder){
				case RelativeInitiativeOrder.First:
					oc.InsertAtStart(initToInsert);
					break;
				case RelativeInitiativeOrder.Last:
					oc.InsertAtEnd(initToInsert);
					break;
				case RelativeInitiativeOrder.BeforeCurrent:
					oc.InsertBefore(currentlyExecuting.Initiative, initToInsert);
					break;
				case RelativeInitiativeOrder.Current:
					throw new InvalidOperationException("Can't reinsert current initiative");
				case RelativeInitiativeOrder.AfterCurrent:
					oc.InsertAfter(currentlyExecuting.Initiative, initToInsert);
					break;
				case RelativeInitiativeOrder.BeforeTarget:
					oc.InsertBefore(targetInitiative, initToInsert); // This call throws if targetInitiative isn't in the collection
					break;
				case RelativeInitiativeOrder.Target:
					throw new InvalidOperationException("Can't reinsert target initiative");
				case RelativeInitiativeOrder.AfterTarget:
					oc.InsertAfter(targetInitiative, initToInsert); // This call throws if targetInitiative isn't in the collection
					break;
			}
			throw new InvalidOperationException("Unknown value for relativeOrder");
		}
		//todo xml
		public bool UnregisterInitiative(Initiative initiative){
			if(initiative == null) throw new ArgumentNullException(nameof(initiative));
			if(!oc.Contains(initiative)) return false;
			//if(initiative is AutoInitiative autoInit && scheduledEventsForInitiatives.AnyValues(autoInit))  //  TODO NEXT... this part is wrong...should check all events instead, but needs to be O(n) with validation or O(1) without.
				throw new InvalidOperationException("");
				oc.Contains
			return oc.Remove(initiative);
		}

		private class InternalInitiative : Initiative { }
		private class AutoInitiative : Initiative { }
		private class InternalEventScheduling : EventScheduling {
			// Delay needs to be changed during a reschedule, but this can't be exposed publicly:
			public void ChangeDelay(long newDelay) => Delay = newDelay;
			public Initiative Initiative => initiative;
			public InternalEventScheduling(IEvent scheduledEvent, long currentTick, long delay, Initiative init)
				: base(scheduledEvent, currentTick, delay, init) { }
		}

		public static void Serialize(EventScheduler scheduler, // TODO, MADE CHANGES, PROBABLY BROKE.
			BinaryWriter writer,
			Action<EventScheduling, BinaryWriter> onSaveEventScheduling,
			Action<Initiative, BinaryWriter> onSaveInitiative,
			Action<IEvent, BinaryWriter> saveIEvent)
		{
			//todo test this
			if(saveIEvent == null) throw new ArgumentNullException(nameof(saveIEvent));
			if(scheduler == null) throw new ArgumentNullException(nameof(scheduler));
			if(writer == null) throw new ArgumentNullException(nameof(writer));
			//so a simple dict here for ids...
			var objIds = new Dictionary<object, int>();
			int nextId = 1;
			//seems like the OC is the best place to start here.
			//so, simply go through the OC and add each init to the dict, and write the ID for each.
			//almost forgot, write the count first...
			writer.Write(scheduler.oc.Count);
			foreach(var init in scheduler.oc) {
				int newId = nextId++;
				objIds[init] = newId;
				writer.Write(newId);
				// if the init is autoinit, write true.
				if(init is AutoInitiative) writer.Write(true);
				else {
					// otherwise, write false and call onSaveInitiative.
					writer.Write(false);
					onSaveInitiative?.Invoke(init, writer);
				}
			}
			// (the OC can now be recreated)
			//now write the MVD? no, do that last.
			//now, write the PQ:
			//write the count...for each ES in insertion order...
			writer.Write(scheduler.pq.Count);
			foreach(var es in scheduler.pq.GetAllInInsertionOrder()) {
				//add the ES to the dict and write the ID...if not already written (actually, can't be already written i think):
				int newId = nextId++;
				objIds[es] = newId;
				writer.Write(newId);
				//call saveIEvent...
				saveIEvent(es.Event, writer);
				//write creation tick, delay, and init ID.
				writer.Write(es.CreationTick);
				writer.Write(es.Delay);
				// The initiative MUST be in the OC and therefore must be in the dictionary already, unless it's null:
				if(es.Initiative == null) writer.Write(0);
				else writer.Write(objIds[es.Initiative]);
				// then call onSaveEventScheduling.
				onSaveEventScheduling?.Invoke(es, writer);
			}
			// (the PQ can now be recreated)
			//NOW write the MVD:
			//since every autoinit must be in the OC, and every ES must be in the PQ, these all already have IDs.
			//write the number of groups first...
			writer.Write(scheduler.scheduledEventsForInitiatives.GetKeyCount());
			//for each group, write the init ID and the count, then for each ES, write its ID.
			foreach(var group in scheduler.scheduledEventsForInitiatives) {
				writer.Write(objIds[group.Key]);
				var events = group.ToArray();
				writer.Write(events.Length);
				foreach(var ev in events) {
					writer.Write(objIds[ev]);
				}
			}
			// (the MVD can now be recreated)
		}
		public static EventScheduler Deserialize(BinaryReader reader,
			Action<EventScheduling, BinaryReader> onLoadEventScheduling,
			Action<Initiative, BinaryReader> onLoadInitiative,
			Func<BinaryReader, IEvent> loadIEvent)
		{
			if(loadIEvent == null) throw new ArgumentNullException(nameof(loadIEvent));
			if(reader == null) throw new ArgumentNullException(nameof(reader));
			//dict of id -> obj this time...
			var objectsById = new Dictionary<int, object>();
			//read OC count...for each one...
			int ocCount = reader.ReadInt32();
			var inits = new List<Initiative>();
			for(int i = 0; i<ocCount; ++i) {
				//read the ID...read the bool. If true, create an autoinit and add it.
				int id = reader.ReadInt32();
				bool isAuto = reader.ReadBoolean();
				Initiative init;
				if(isAuto) {
					init = new AutoInitiative();
				}
				else {
					//if false, create an internalInit and add it, then call onLoadInitiative.
					init = new InternalInitiative();
					onLoadInitiative?.Invoke(init, reader);
				}
				objectsById.Add(id, init);
				inits.Add(init);
			}
			// 'inits' now ready for use
			// (the OC should now be complete, and all non-auto inits have been associated with their user-assigned IDs again)
			//read PQ count...for each...
			int pqCount = reader.ReadInt32();
			var events = new List<InternalEventScheduling>();
			for(int i = 0; i<pqCount; ++i) {
				//read the ID...
				int id = reader.ReadInt32();
				//call loadIEvent...
				IEvent ievent = loadIEvent(reader);
				//read creation tick, delay, and init ID.
				long creationTick = reader.ReadInt64();
				long delay = reader.ReadInt64();
				int initId = reader.ReadInt32();
				Initiative init;
				if(initId == 0) init = null;
				else {
					init = (objectsById[initId] as Initiative) ?? throw new Exception("Initiative not loaded properly"); //todo, exception type?
				}
				var es = new InternalEventScheduling(ievent, creationTick, delay, init);
				// then call onLoadEventScheduling.
				onLoadEventScheduling?.Invoke(es, reader);
				events.Add(es);
			}
			// 'events' now ready for use
			// (the PQ should now be complete)
			// finally, the MVD:
			//read a count of groups...
			int eventsForInitsCount = reader.ReadInt32();
			var eventsForInits = new List<IGrouping<AutoInitiative, InternalEventScheduling>>();
			for(int i = 0; i<eventsForInitsCount; ++i) {
				//read an init ID for each group...
				int initId = reader.ReadInt32();
				var init = (objectsById[initId] as AutoInitiative) ?? throw new Exception("Initiative not loaded correctly");
				//read a count for each group...
				int groupCount = reader.ReadInt32();
				var esGroup = new List<InternalEventScheduling>();
				//read ES ids
				for(int j = 0; j<groupCount; ++j) {
					int esId = reader.ReadInt32();
					var es = (objectsById[esId] as InternalEventScheduling) ?? throw new Exception("Event scheduling not loaded correctly");
					esGroup.Add(es);
				}
				eventsForInits.Add(new Grouping<AutoInitiative, InternalEventScheduling>(init, esGroup));
			}
			// 'eventsForInits' now ready for use
			return new EventScheduler(inits, events, eventsForInits);
		}
	}
}
