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
			}
		}
	}
}
