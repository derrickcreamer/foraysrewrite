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
		private CharacterScreen? ShowInventory(PlayerTurnEvent e, InventoryScreenMode? inventoryMode){
			while(true){
				const int rowOffset = 3;
				int colOffset = MapUI.ColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, MapUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.Inventory);

				Screen.Write(rowOffset, colOffset, "In your pack: "); //todo, change based on inventory mode
				//todo...i think the lines are good to set the section apart, but it's possible that i don't need both:
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar);
				Screen.Write(rowOffset + 2, colOffset, "[a] a potion of haste"); //todo
				Screen.Write(rowOffset + 2, colOffset + 1, 'a', Color.Cyan);
				Screen.Write(rowOffset + 3, colOffset, "[b] a slender wand");
				Screen.Write(rowOffset + 3, colOffset + 1, 'b', Color.Cyan);
				Screen.Write(rowOffset + 4, colOffset, "[c] an iridescent orb");
				Screen.Write(rowOffset + 4, colOffset + 1, 'c', Color.Cyan);
				Screen.Write(rowOffset + 5, colOffset, SeparatorBar); //todo, count?
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(rowOffset, colOffset + 14);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab:
						if(shift) return CharacterScreen.AdventureLog;
						else return CharacterScreen.Equipment;
					case ConsoleKey.Escape:
					case ConsoleKey.Spacebar:
						return null;
					case ConsoleKey.Oem2: //todo check
						//todo help
						break;
					default:
						int letterIndex = key.KeyChar - 'a';
						if(letterIndex >= 0 && letterIndex < 3){ //todo, inv count here
							//
						}
						break;
				}
			}
		}
	}
}
