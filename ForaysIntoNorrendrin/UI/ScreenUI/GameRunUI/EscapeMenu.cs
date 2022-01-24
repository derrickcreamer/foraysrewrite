using System;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	public class EscapeMenu : GameUIObject{
		const int Height = 11;
		const int Width = 23;
		const int RowOffset = (ScreenUIMain.Rows - Height) / 2;
		const int ColOffset = (ScreenUIMain.Cols - Width) / 2;
		public EscapeMenu(GameRunUI ui) : base(ui){ }

		public void Open(bool drawBottomUI = true){
			Screen.CursorVisible = false;
			Screen.HoldUpdates();
			DrawOption bottomUI = drawBottomUI? DrawOption.Darkened : DrawOption.DoNotDraw;
			GameRunUI.DrawGameUI(
				sidebar: DrawOption.Darkened,
				messages: DrawOption.DoNotDraw,
				environmentalDesc: bottomUI,
				commands: bottomUI
			);
			//todo, probably refactor this into a box-drawing utility:
			const Color cornerColor = Color.Blue;
			const Color edgeBgColor = Color.DarkBlue;
			const char cornerChar = ' ';
			Screen.Write(RowOffset, ColOffset, cornerChar, cornerColor, edgeBgColor);
			Screen.Write(RowOffset, ColOffset + Width - 1, cornerChar, cornerColor, edgeBgColor);
			Screen.Write(RowOffset + Height - 1, ColOffset, cornerChar, cornerColor, edgeBgColor);
			Screen.Write(RowOffset + Height - 1, ColOffset + Width - 1, cornerChar, cornerColor, edgeBgColor);

			const Color edgeColor = Color.Gray;
			const char topBottomChar = ' ';
			const char sidesChar = ' ';
			string topBottomBar = "".PadRight(Width - 2, topBottomChar); //todo refactor
			Screen.Write(RowOffset, ColOffset + 1, topBottomBar, edgeColor, edgeBgColor);
			Screen.Write(RowOffset + Height - 1, ColOffset + 1, topBottomBar, edgeColor, edgeBgColor);
			for(int i=RowOffset + 1; i<RowOffset + Height - 1; ++i){
				Screen.Write(i, ColOffset, sidesChar, edgeColor, edgeBgColor);
				Screen.Clear(RowOffset + 1, ColOffset + 1, Height - 2, Width - 2);
				Screen.Write(i, ColOffset + Width - 1, sidesChar, edgeColor, edgeBgColor);
			}
			const int optionOffsetCol = ColOffset + 3;
			const int optionOffsetRow = RowOffset + 2;
			string[] menuItems = new[] {
				"Resume game", "Help", "Options", "Save or quit"
			};
			Screen.WriteListOfChoices(optionOffsetRow, optionOffsetCol, menuItems, linesBetweenEach: 1);
			Screen.ResumeUpdates();
			ConsoleKeyInfo key = Input.ReadKey(false);
			switch(key.Key){
				case ConsoleKey.A:
					break;
				case ConsoleKey.B:
					//todo
					break;
				case ConsoleKey.C:
					OptionsScreen.Show();
					//todo
					break;
				case ConsoleKey.D:
					//todo, ask for confirm
					Program.Quit();
					break;
			}
		}
	}
}
