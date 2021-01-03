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
		private CharacterScreen? ShowEquipment(PlayerTurnEvent e){
			while(true){
				const int rowOffset = 3;
				int colOffset = GameRunUI.MapColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, GameRunUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.Equipment);

				Screen.Write(rowOffset, colOffset, "Your equipment: ");
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar); //todo
				//todo, use WriteListOfChoices here
				Screen.Write(rowOffset + 2, colOffset, "[a] Sword");
				Screen.Write(rowOffset + 2, colOffset + 1, 'a', Color.Cyan);
				Screen.Write(rowOffset + 3, colOffset, "[b] Another sword");
				Screen.Write(rowOffset + 3, colOffset + 1, 'b', Color.Cyan);
				Screen.Write(rowOffset + 4, colOffset, "[c] Third sword");
				Screen.Write(rowOffset + 4, colOffset + 1, 'c', Color.Cyan);
				Screen.Write(rowOffset + 5, colOffset, SeparatorBar);
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(rowOffset, colOffset + 16);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab:
						if(shift) return CharacterScreen.Inventory;
						else return CharacterScreen.Actions;
					case ConsoleKey.Escape:
					case ConsoleKey.Spacebar:
						return null;
					case ConsoleKey.Oem2: //todo check
						//todo help
						break;
					default:
						break;
				}
			}
		}
	}
}
