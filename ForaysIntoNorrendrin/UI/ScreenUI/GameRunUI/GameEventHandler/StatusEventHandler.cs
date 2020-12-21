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
		public void OnStatusStart(Creature creature, Status status){
			//todo
		}
		public void OnStatusEnd(Creature creature, Status status){
			//todo
		}
	}
}
