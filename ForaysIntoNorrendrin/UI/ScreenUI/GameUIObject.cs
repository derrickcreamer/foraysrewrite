using System;
using Forays;
using GameComponents;
namespace ForaysUI.ScreenUI{

	///<summary>Base class for UI types that make heavy use of the GameUniverse</summary>
	public class GameUIObject : GameObject{
		public GameUIObject(GameUniverse g) : base(g){}

		public RNG ScreenRNG => ScreenUIMain.RNG;

		// To hopefully avoid any unintended usage:
		[Obsolete("CAUTION: This is the GameUniverse's RNG, not the UI's RNG.")]
		new public RNG R => base.R;
	}
}
