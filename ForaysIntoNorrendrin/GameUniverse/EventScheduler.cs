using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UtilityCollections;

namespace GameComponents {
	public abstract class Initiative { }
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
			// Any initiative is given priority over no initiative:
			if(first.Initiative == null) {
				if(second.Initiative == null) return 0;
				else return 1;
			}
			else if(second.Initiative == null) return -1;
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
			IEnumerable<IGrouping<AutoInitiative, InternalEventScheduling>> eventsForInits) {
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
		public EventScheduling Schedule(IEvent scheduledEvent, long ticksInFuture, Initiative initiative) {
			if(scheduledEvent == null) throw new ArgumentNullException(nameof(scheduledEvent));
			// Non-null initiatives must have been created in this class:
			if(initiative != null && !oc.Contains(initiative)) throw new InvalidOperationException("Can't use an unordered initiative. Use CreateInitiative methods instead.");
			var es = new InternalEventScheduling(scheduledEvent, currentTick, ticksInFuture, initiative);
			pq.Enqueue(es);
			return es;
		}
		//todo xml: not sure how much to describe here, but it uses THIS tick and THIS initiative,
		// so it's kind of like adding it to a "right now" queue.
		public EventScheduling ScheduleImmediately(IEvent scheduledEvent) {
			if(scheduledEvent == null) throw new ArgumentNullException(nameof(scheduledEvent));
			var es = new InternalEventScheduling(scheduledEvent, currentTick, 0, currentlyExecuting.Initiative);
			pq.Enqueue(es);
			return es;
		}
		//todo, consider naming this one "ScheduleEndOfDuration" or something
		// todo:  and the xml needs to explain it well, too.
		public EventScheduling ScheduleDuration(IEvent durationEndEvent, long ticksInFuture) {
			if(durationEndEvent == null) throw new ArgumentNullException(nameof(durationEndEvent));
			AutoInitiative init = new AutoInitiative();
			oc.InsertBefore(currentlyExecuting.Initiative, init);
			var es = new InternalEventScheduling(durationEndEvent, currentTick, ticksInFuture, init);
			scheduledEventsForInitiatives.Add(init, es);
			pq.Enqueue(es);
			return es;
		}
		//todo xml: returns false if not actually scheduled
		public bool CancelEventScheduling(EventScheduling eventScheduling) {
			if(eventScheduling == null) throw new ArgumentNullException(nameof(eventScheduling));
			var es = eventScheduling as InternalEventScheduling;
			if(es == null) throw new Exception("User-created subtypes of EventScheduling are not supported");
			bool result = pq.Remove(es);
			RemoveScheduling(es);
			return result;
		}
		//todo, xml, the current tick will be used if the given value has already passed, right?
		// and that this returns false if the ES is not scheduled, but still changes its value
		public bool ChangeEventScheduling(EventScheduling eventScheduling, long newExecutionTick) {
			if(eventScheduling == null) throw new ArgumentNullException(nameof(eventScheduling));
			var es = eventScheduling as InternalEventScheduling;
			if(es == null) throw new Exception("User-created subtypes of EventScheduling are not supported");
			if(newExecutionTick < currentTick) newExecutionTick = currentTick;
			// The new delay must be relative to the ES's creation time:
			long newDelay = newExecutionTick - es.CreationTick;
			return pq.ChangePriority(es, () => es.ChangeDelay(newDelay));
		}
		public Initiative CreateInitiativeBefore(Initiative beforeInitiative) {
			var init = new InternalInitiative();
			oc.InsertBefore(beforeInitiative, init);
			return init;
		}
		public Initiative CreateInitiativeAfter(Initiative afterInitiative) {
			var init = new InternalInitiative();
			oc.InsertAfter(afterInitiative, init);
			return init;
		}
		public Initiative CreateInitiativeAtStart() {
			var init = new InternalInitiative();
			oc.InsertAtStart(init);
			return init;
		}
		public Initiative CreateInitiativeAtEnd() {
			var init = new InternalInitiative();
			oc.InsertAtEnd(init);
			return init;
		}
		public Initiative CreateInitiativeBeforeCurrent() {
			var init = new InternalInitiative();
			oc.InsertBefore(currentlyExecuting?.Initiative, init);
			return init;
		}
		public Initiative CreateInitiativeAfterCurrent() {
			var init = new InternalInitiative();
			oc.InsertAfter(currentlyExecuting?.Initiative, init);
			return init;
		}
		public bool UnregisterInitiative(Initiative initiative) => oc.Remove(initiative);

		private class InternalInitiative : Initiative { }
		private class AutoInitiative : Initiative { }
		private class InternalEventScheduling : EventScheduling {
			// Delay needs to be changed during a reschedule, but this can't be exposed publicly:
			public void ChangeDelay(long newDelay) => Delay = newDelay;
			public Initiative Initiative => initiative;
			public InternalEventScheduling(IEvent scheduledEvent, long currentTick, long delay, Initiative init)
				: base(scheduledEvent, currentTick, delay, init) { }
		}

		public static void Serialize(EventScheduler scheduler,
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
					init = (objectsById[initId] as Initiative) ?? throw new Exception("Initiative not loaded properly");
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
