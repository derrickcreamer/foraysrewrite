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
		private CharacterScreen? ShowActions(PlayerTurnEvent e){
			while(true){
				const int rowOffset = 3;
				int colOffset = GameRunUI.MapColOffset;
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, GameRunUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.Actions);

				Screen.Write(rowOffset, colOffset, "Available actions: ");
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar);
				Screen.Write(rowOffset + 2, colOffset, "[a] Rest to recover health and repair equipment"); //todo
				Screen.Write(rowOffset + 2, colOffset + 1, 'a', Color.Cyan);
				Screen.Write(rowOffset + 3, colOffset, SeparatorBar); //todo, count?
				Screen.SetCursorPosition(rowOffset, colOffset + 19);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab:
						if(shift) return CharacterScreen.Equipment;
						else return CharacterScreen.AdventureLog;
					case ConsoleKey.Escape:
					case ConsoleKey.Spacebar:
						return null;
					case ConsoleKey.Oem2: //todo check
						//todo help
						break;
					default:
						int letterIndex = key.KeyChar - 'a';
						if(letterIndex >= 0 && letterIndex < 3){ //todo
							//
						}
						break;
				}
			}
		}
	}
}
