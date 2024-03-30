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
				int colOffset = MapUI.ColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, MapUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.Actions);

				Screen.Write(rowOffset, colOffset, "Available actions: ");
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar);
				string[] actions = new[] { //todo
					"Rest to recover health and repair equipment",
					"Descend staircase",
					"Close a door"
				};
				Screen.WriteListOfChoices(rowOffset + 2, colOffset, actions);
				Screen.Write(rowOffset + 5, colOffset, SeparatorBar); //todo, count?
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(rowOffset, colOffset + 19);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab: //todo, what about adding Insert as an alternative to Tab, so you can easily access this screen and switch with the numpad?
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
							if(e == null) break;
							if(letterIndex == 2)
							{
								e.ChosenAction = new TodoChangeTerrainEvent(Player.Position, GameUniverse);
								return null;
							}

							//remember to check whether the PlayerTurnEvent is null - that should be allowed.
						}
						break;
				}
			}
		}
	}
}
