using System;
using Forays;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	public class Sidebar : GameUIObject{
		public int RowOffset = 0;
		public int ColOffset = 0;
		public const int Height = ScreenUIMain.Rows;
		public const int Width = 20;
		public Sidebar(GameRunUI ui) : base(ui){
			//todo
		}
		public void Draw(DrawOption drawOption = DrawOption.Normal){
			if(drawOption == DrawOption.DoNotDraw) return;
			Screen.HoldUpdates();
			Color textColor = drawOption == DrawOption.Darkened? Color.DarkGray : Color.Gray;
			Screen.Clear(RowOffset, ColOffset, Height, Width);
			Screen.Write(RowOffset, ColOffset,     "     Health: 93     ", textColor, Color.HealthBar); //todo
			Screen.Write(RowOffset + 1, ColOffset, "    -- Sword --     ", textColor); //todo
			Screen.Write(RowOffset + 2, ColOffset, "  -- Chainmail --   ", textColor); //todo
			Screen.Write(RowOffset + 3, ColOffset, "      Depth: 2      ", textColor); //todo
			Screen.Write(RowOffset + 5, ColOffset, "E: Frostling        ", textColor); //todo
			Screen.Write(RowOffset + 6, ColOffset, " HP: 20  (alerted)  ", textColor, Color.DarkerRed); //todo
			Screen.Write(RowOffset + 7, ColOffset, "       Webbed       ", textColor, Color.DarkerMagenta); //todo
			Screen.ResumeUpdates();
		}
	}
}
