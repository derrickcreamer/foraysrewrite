using System;
using Forays;
using GameComponents;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// GameEventHandler.cs (contains the constructors and helpers)
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		public void BeforeGameEvent(GameObject gameEvent){
			switch(gameEvent){
				case PlayerTurnEvent e:
					ChooseAction(e);
					break;
				case MeleeHitEvent e:
					if(e.Creature == Player)
						Messages.Add("You hit the enemy. ");
					else
						Messages.Add("The enemy hits you. ");
					break;
				case MeleeMissEvent e:
					if(e.Creature == Player)
						Messages.Add("You miss the enemy. ");
					else
						Messages.Add("The enemy misses you. ");
					break;
				case DieEvent e:
					if(e.Creature == Player)
						Messages.Add("You die. ");
					else
						Messages.Add("The enemy dies. ");
					break;
			}
		}
	}
}
