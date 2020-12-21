using System;
using Forays;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs (contains the constructor)
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		public bool DecideCancel(GameObject action){
			//todo, for targeting:
			//if target is already specified, do nothing
			//else, prompt for target
			//return (target==null)
			switch(action){
				case WalkAction e:
					if(e.IsBlockedByTerrain){
						//todo, get terrain name
						Messages.Add("There is a wall in the way. ");
						return true;
					}
					break;
			}
			return false;//todo
		}
	}
}
