using System;
using Forays;

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
		public void AfterGameEvent(GameObject gameEvent, EventResult eventResult){
			// todo, print messages based on results here
			switch(gameEvent){
				case WalkAction e:
					if(eventResult.Canceled && e.IsBlockedByTerrain){ //todo, this is wrong. Canceled events don't reach this point. Print terrain messages here instead.
					}
					if(e.Creature == Player && Player.Position.Value == e.Destination){
						string msg = null;
						switch(TileTypeAt(Player.Position.Value)){ //todo, method here instead, so I can return instead of breaking?
							case TileType.Staircase:
								msg = "The stairway leads downward - press > to descend. "; //todo
								break;
						}
						if(msg != null) Messages.Add(msg);
					}
					break;
			}
		}
	}
}
