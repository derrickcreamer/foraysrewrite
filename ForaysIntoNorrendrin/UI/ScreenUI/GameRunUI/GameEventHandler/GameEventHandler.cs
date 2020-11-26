using System;
using Forays;
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
		public MessageBuffer Messages;
		public Sidebar Sidebar;

		// todo, add option to change layout, which'll change these values, plus the matching ones in Messages and Sidebar:
		public int MapRowOffset = 3;
		public int MapColOffset = 21;
		public int EnviromentalFlavorStartRow = 3 + GameUniverse.MapHeight; //todo, more here?

		public GameEventHandler(GameUniverse g) : base(g){
			Messages = new MessageBuffer(g);
			Sidebar = new Sidebar(g);
		}
		private void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset, glyphIndex, color);
		private void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset);
	}
}
