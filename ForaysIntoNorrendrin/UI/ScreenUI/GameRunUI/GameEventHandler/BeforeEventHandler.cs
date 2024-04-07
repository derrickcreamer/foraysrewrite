using System;
using Forays;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs (contains the constructor)
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		public void BeforeGameEvent(GameObject gameEvent){
			switch(gameEvent){
				case PlayerTurnEvent e:
					//todo okay so let's see what is seen, and record map memory here...
					ChooseAction(e);
					break;
				case MeleeHitEvent e:
					Messages.Add(e.Creature, "hit", e.Target);
					break;
				case MeleeMissEvent e:
					Messages.Add(e.Creature, "miss", e.Target);
					break;
				case DieEvent e:
					Messages.AddSimple(e.Creature, "die");
					break;
				case PickUpItemAction e:
					Messages.Add(Determinative.The, e.Creature, "pick up", Determinative.The, e.Item); //todo, does this verb need to be registered?
					break;
				case DropItemAction e:
					Messages.Add(Determinative.The, e.Creature, "drop", Determinative.The, e.Item, visibility: Visibility.RequireSubject, assumeObjectVisible: true);
					break;
				case IceCrackingEvent e:
					break;
				case IceBreakingEvent e:
					break;
				case AiChangeBehaviorStateEvent e:
					if(e.Enemy.BehaviorState == CreatureBehaviorState.Unaware && e.NewBehaviorState == CreatureBehaviorState.Hunting){
						Messages.Add(e.Enemy, "notice", Player);
					}
					else if(e.Enemy.BehaviorState == CreatureBehaviorState.Unaware && e.NewBehaviorState == CreatureBehaviorState.Searching){
						// This should only happen as the result of a shout/shriek/alarm - do I want a message for this?
					}
					else if(e.Enemy.BehaviorState == CreatureBehaviorState.Searching && e.NewBehaviorState == CreatureBehaviorState.Hunting){
						Messages.Add(e.Enemy, "notice", Player);
					}
					else if(e.Enemy.BehaviorState == CreatureBehaviorState.Searching && e.NewBehaviorState == CreatureBehaviorState.Unaware){
						Messages.AddSimple(e.Enemy, "stop searching");
					}
					else if(e.Enemy.BehaviorState == CreatureBehaviorState.Tracking && e.NewBehaviorState == CreatureBehaviorState.Searching){
						// Does this need a message?..
					}
					Messages.AddSimple(e.Enemy, $"state from {e.Enemy.BehaviorState} to {e.NewBehaviorState} change", visibility: Visibility.AlwaysVisible);
					break;
			}
		}
	}
}
