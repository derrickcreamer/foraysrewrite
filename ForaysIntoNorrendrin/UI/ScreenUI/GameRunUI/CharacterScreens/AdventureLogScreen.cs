using System;
using Forays;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// ActionsScreen.cs
	// AdventureLogScreen.cs
	// CharacterScreens.cs (main part, calls the others, has constructor and helpers)
	// EquipmentScreen.cs
	// InventoryScreen.cs
	public partial class CharacterScreens : GameUIObject{
		private CharacterScreen? ShowAdventureLog(PlayerTurnEvent e){
			while(true){
				const int rowOffset = 3;
				int colOffset = GameRunUI.MapColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, GameRunUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.AdventureLog);

				Screen.Write(rowOffset, colOffset, "Adventure log of Charname III: ");
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar);
				Screen.Write(rowOffset + 2, colOffset, "Entered the mountain pass 571 turns ago");
				Screen.Write(rowOffset + 3, colOffset, "Skills: (none)"); //todo
				Screen.Write(rowOffset + 5, colOffset, "[a] Discovered locations"); //todo
				Screen.Write(rowOffset + 5, colOffset + 1, 'a', Color.Cyan);
				Screen.Write(rowOffset + 6, colOffset, "[b] Discovered items"); //todo
				Screen.Write(rowOffset + 6, colOffset + 1, 'b', Color.Cyan);
				Screen.Write(rowOffset + 7, colOffset, "[c] Recent messages"); //todo
				Screen.Write(rowOffset + 7, colOffset + 1, 'c', Color.Cyan);
				Screen.Write(rowOffset + 8, colOffset, "[d] Pause adventure, change options, or get help"); //todo
				Screen.Write(rowOffset + 8, colOffset + 1, 'd', Color.Cyan);
				//Screen.Write(rowOffset + 9, colOffset, SeparatorBar); //todo, count?
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(rowOffset, colOffset + 31);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab:
						if(shift) return CharacterScreen.Actions;
						else return CharacterScreen.Inventory;
					case ConsoleKey.Escape:
					case ConsoleKey.Spacebar:
						return null;
					case ConsoleKey.Oem2: //todo check
						//todo help
						break;
					case ConsoleKey.A:
						break;
					case ConsoleKey.B:
						break;
					case ConsoleKey.C:
						break;
					case ConsoleKey.D:
						EscapeMenu.Open(false);
						if(GameUniverse.Suspend || GameUniverse.GameOver) return null;
						break;
				}
			}
		}
	}
}
