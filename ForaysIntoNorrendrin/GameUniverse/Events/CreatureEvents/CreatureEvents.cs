﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameComponents;
using GameComponents.DirectionUtility;
using Hemlock;
using UtilityCollections;

namespace Forays {
	// CreatureAction is a base class for creature actions that'll use the creature's decider by default.
	public abstract class CreatureAction<TResult> : Event<TResult> where TResult : EventResult, new() { //todo, should this be renamed now that 'Action' is a less important distinction?
		public virtual Creature Creature { get; set; }
		public CreatureAction(Creature creature) : base(creature.GameUniverse) { this.Creature = creature; }
		public override ICancelDecider CancelDecider => Creature?.CancelDecider;
		public override bool IsInvalid => Creature == null || !Creature.HasPosition;
	}
	public interface IItemUseEvent { } //todo, not final. The idea here is that some actions might share interfaces that are easy to work with (for the UI),
	// such as item-related actions, or actions involving a choice of target.

	public class AiTurnEvent : SimpleEvent {
		public Creature Creature { get; set; }
		public AiTurnEvent(Creature creature) : base(creature.GameUniverse) {
			this.Creature = creature;
		}

		protected override void ExecuteSimpleEvent() {
			//todo, what checks whether creature is null? maybe the position check below can cover that too.
			//todo, if creature is dead?
			if(!Creature.HasPosition) return; // todo... creatures off the map shouldn't be getting turns

			int timeTaken = Creature.ExecuteMonsterTurn();
			Q.Schedule(new AiTurnEvent(Creature), timeTaken, Q.GetCurrentInitiative());

			return; //todo clean up


			List<Point> validPoints = Creature.Position.EnumeratePointsWithinChebyshevDistance(1, false, false)
				.Where(p => TileTypeAt(p) != TileType.Wall).ToList();
			Point dest = Creature.Position;
			if(validPoints.Count > 0)
				dest = validPoints[R.GetNext(validPoints.Count)];

			if(CreatureAt(dest) != null && CreatureAt(dest) != Creature){
				if(CreatureAt(dest) != Player) {
					//Notify(new NotifyPrintMessage{ Message = "The enemy glares." });
				}
				else {
					//Notify(new NotifyPrintMessage{ Message = "The enemy hits you."});
					Q.Execute(new MeleeAttackAction(Creature, Player));
				}
			}
			else {
				Q.Execute(new WalkAction(Creature, dest));
			}

			Q.Schedule(new AiTurnEvent(Creature), Turns(1), Q.GetCurrentInitiative());
		}
	}

	public class AiChangeBehaviorStateEvent : SimpleEvent {
		public Creature Creature { get; set; }
		public CreatureBehaviorState NewBehaviorState { get; set; }

		public AiChangeBehaviorStateEvent(Creature creature, CreatureBehaviorState newBehaviorState) : base(creature.GameUniverse) {
			this.Creature = creature;
			this.NewBehaviorState = newBehaviorState;
		}

		protected override void ExecuteSimpleEvent() {
			Creature.BehaviorState = NewBehaviorState;
		}
	}

	public class PlayerTurnEvent : SimpleEvent {
		//todo xml: must be Event<TResult>
		public GameObject ChosenAction { get; set; } = null;

		public PlayerTurnEvent(GameUniverse g) : base(g) { }

		///<summary>Populates the values of CellsVisibleThisTurn, CellsLitThisTurn, and CreaturesVisibleThisTurn.
		/// This idempotent method will always be called when this event executes, but is special in that
		/// it is allowed to be called in 'BeforeEventExecute' in order to make this data available to the UI.</summary>
		public void CalculateVisibility(){
			if(visibilityCalculated) return;
			visibilityCalculated = true;
			cellsVisibleThisTurn = new EasyHashSet<Point>();
			cellsLitThisTurn = new EasyHashSet<Point>();
			creaturesVisibleThisTurn = new List<Creature>();
			for(int i = 0; i < GameUniverse.MapHeight; i++) {
				for(int j = 0; j < GameUniverse.MapWidth; j++) {
					Point p = new Point(j, i);
					Creature creature = CreatureAt(p);
					if(creature != null && Player.CanSee(creature)){
						creaturesVisibleThisTurn.Add(creature);
					}
					if(Player.Position.HasLOS(p, Map.Tiles)){
						Map.Seen[p] = true;
						cellsVisibleThisTurn.Add(p);
						if(Map.Light.CellAppearsLitToObserver(p, Player.Position)){
							cellsLitThisTurn.Add(p);
						}
					}
				}
			}
		}

		// Note that these are just cached, not serialized:
		private bool visibilityCalculated;
		private EasyHashSet<Point> cellsVisibleThisTurn;
		private EasyHashSet<Point> cellsLitThisTurn;
		private List<Creature> creaturesVisibleThisTurn;
		// These properties work differently than all other Event properties (they aren't set when the event is created)
		public EasyHashSet<Point> CellsVisibleThisTurn => cellsVisibleThisTurn;
		public EasyHashSet<Point> CellsLitThisTurn => cellsLitThisTurn;
		public List<Creature> CreaturesVisibleThisTurn => creaturesVisibleThisTurn;

		protected override void ExecuteSimpleEvent() {
			//if(Player.State == CreatureState.Dead) return;

			if(GameUniverse.Suspend) {
				// (this should either reschedule, or use some kind of "don't remove the current event" feature on the queue...
				Q.ScheduleNow(new PlayerTurnEvent(GameUniverse));
				return;
			}
			CalculateVisibility();
			EventResult result = null;
			switch(ChosenAction){
				// This section has some duplication because of how the type parameters to Q.Execute work:
				case WalkAction action: result = Q.Execute(action); break;
				case MeleeAttackAction action: result = Q.Execute(action); break;
				case DescendAction action: result = Q.Execute(action); break;
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
	public class WalkAction : CreatureAction<PassFailResult> { //todo, this should just be MoveAction, with some kind of movement type enum, right?
		public Point Destination { get; set; }
		public bool IgnoreRange { get; set; } = false; //todo, should i have a naming convention for 'arg' properties vs. calculated properties?
													   // ...such as IgnoreRange vs. OutOfRange. if i had a convention it might suggest 'IgnoreRange' and 'IsOutOfRange' - would that work for most?
		public WalkAction(Creature creature, Point destination) : base(creature) {
			this.Destination = destination;
		}
		//todo, rename to IsOutOfRange, and should this actually check IgnoreRange, or should that be checked in Execute?
		public bool OutOfRange => !IgnoreRange && Creature.Position.ChebyshevDistanceFrom(Destination) > 1;
		public bool IsBlockedByTerrain => !TileDefinition.IsPassable(TileTypeAt(Destination));
		//todo: IsInvalid shows the call to base.IsValid which actually checks the same thing right now:
		public override bool IsInvalid => base.IsInvalid || !Destination.ExistsOnMap();
		protected override PassFailResult Execute() {
			if(OutOfRange || IsBlockedByTerrain || CreatureAt(Destination) != null) {
				// todo, there would be some kind of opportunity to print a message here.
				return Failure();
			}
			bool canMove = !Map.Creatures.HasContents(Destination);
			if(canMove){
				if(Creature == Player) Map.Light.RemoveLightSource(Player.Position, 5);
				Map.Creatures.Move(Creature, Destination);
				if(Creature == Player) Map.Light.AddLightSource(Player.Position, 5);
				if(TileTypeAt(Destination) == TileType.DeepWater){
					if(Map.FeaturesAt(Destination).HasFeature(FeatureType.Ice)){
						Q.Execute(new CheckForIceCrackingEvent(Destination, GameUniverse));
					}
					else if(Map.FeaturesAt(Destination).HasFeature(FeatureType.CrackedIce)){
						Q.Execute(new CheckForIceBreakingEvent(Destination, GameUniverse));
					}
				}
				return Success();
			}
			else return Failure();
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
	public class UseItemAction : CreatureAction<PassFailResult> {
		public Item Item {get;set;}

		//todo...regarding IsInvalid...i feel like maybe invalid events should always throw if
		//  they're actually executed. It would improve the 'notify on every GameEvent start/end' plan.
		//Note that this also means that maybe IsInvalid should be part of ALL Events, not just Actions.
		public override bool IsInvalid => base.IsInvalid || Item == null;

		public UseItemAction(Creature creature, Item item) : base(creature){
			Item = item;
		}
		protected override PassFailResult Execute(){
			//todo
			//execute 'item effect event'
			// targeting happens now? because, for an orb, this is really 'throw'...
			// and what about wands?
			//	 Maybe each is actually separate? so WandEffectEvent?
			//	 ...And it would NOT assume that a creature is using the wand, but it WOULD assume that
			//        the wand is aiming *from* one specific cell *to* another specific cell.
			//(does that one have a Success?)
			// either return a result based on that one, or just a Success.
			//
			//todo
			//scrolls & potions, just use. Scrolls make noise first.
			//orbs can be thrown but can't really be used directly, unless using them means breaking them.
			//wands (and flint&steel) need targeting first... so the question is WHICH thing has the
			//  CancelDecider callback that lets you actually choose the target for a wand...
			switch(ItemDefinition.GetConsumableDefinition(Item.Type).Kind){
				case ConsumableKind.Potion:
				Q.Execute(new PotionEffectEvent(Item, Creature));
				break;
				case ConsumableKind.Scroll:
				//todo, make noise (6)
				Q.Execute(new ScrollEffectEvent(Item, Creature));
				break;
				case ConsumableKind.Orb:
				//todo, see above...what, if anything, goes here?
				break;
				case ConsumableKind.Wand:
				//todo, targeting etc.
				break;
				default:
				switch(Item.Type){
					case ItemType.BlastFungus:
					//todo, does nothing...needs to be thrown instead.
					break;
					case ItemType.FlintAndSteel:
					break;
					case ItemType.MagicTrinket: //todo, does nothing, probably remove from this list
					break;
					case ItemType.RollOfBandages:
					break;
				}
				break;
			}
			//todo, remove item from inventory, and remove from game?...
			return Success(); //todo, maybe not necessary
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
