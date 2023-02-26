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
				case IceCrackingEvent e:
					break;
				case IceBreakingEvent e:
					break;
				case AiChangeBehaviorStateEvent e:
					if(e.Creature.BehaviorState == CreatureBehaviorState.Unaware && e.NewBehaviorState == CreatureBehaviorState.Hunting){
						Messages.Add(e.Creature, "notice", Player);
					}
					else if(e.Creature.BehaviorState == CreatureBehaviorState.Unaware && e.NewBehaviorState == CreatureBehaviorState.Searching){
						// This should only happen as the result of a shout/shriek/alarm - do I want a message for this?
					}
					else if(e.Creature.BehaviorState == CreatureBehaviorState.Searching && e.NewBehaviorState == CreatureBehaviorState.Hunting){
						Messages.Add(e.Creature, "notice", Player);
					}
					else if(e.Creature.BehaviorState == CreatureBehaviorState.Searching && e.NewBehaviorState == CreatureBehaviorState.Unaware){
						Messages.AddSimple(e.Creature, "stop searching");
					}
					else if(e.Creature.BehaviorState == CreatureBehaviorState.Tracking && e.NewBehaviorState == CreatureBehaviorState.Searching){
						// Does this need a message?..
					}
					Messages.AddSimple(e.Creature, $"state from {e.Creature.BehaviorState} to {e.NewBehaviorState} change", visibility: Visibility.AlwaysVisible);
					break;
			}
		}
	}
}
