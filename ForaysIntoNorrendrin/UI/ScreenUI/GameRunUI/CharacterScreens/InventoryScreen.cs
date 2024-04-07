using System;
using System.Linq;
using System.Collections.Generic;
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
		private CharacterScreen? ShowInventory(PlayerTurnEvent e, InventoryScreenMode inventoryMode){
			while(true){
				const int rowOffset = 3;
				int colOffset = MapUI.ColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, MapUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.Inventory);

				List<string> itemNames = Player.Inventory.Select(i => Names.Get(i.Type).ToString()).ToList(); //todo, get real final names here

				string inventoryMessage = $"In your pack (TODO {inventoryMode.ToString()}): "; //todo, change based on inventory mode
				Screen.Write(rowOffset, colOffset, inventoryMessage);
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar); //todo, handle empty
				Screen.WriteListOfChoices(rowOffset + 2, colOffset, itemNames);
				//todo...i think the lines are good to set the section apart, but it's possible that i don't need both:
				Screen.Write(rowOffset + 2 + itemNames.Count, colOffset, SeparatorBar);
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(rowOffset, colOffset + inventoryMessage.Length);
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
						if(letterIndex >= 0 && letterIndex < Player.Inventory.Count){
							switch(inventoryMode){
								case InventoryScreenMode.Apply:
									e.ChosenAction = new UseItemAction(Player, Player.Inventory[letterIndex]);
									return null;
								case InventoryScreenMode.Drop:
									e.ChosenAction = new DropItemAction(Player, Player.Inventory[letterIndex]);
									return null;
								case InventoryScreenMode.Fling:
									//todo
									return null;
								case InventoryScreenMode.Inventory:
								default:
									Screen.HoldUpdates();
									Screen.WriteListOfChoices(rowOffset + 2, colOffset, itemNames, textColor: Color.DarkGray, letterColor: Color.DarkCyan);
									Screen.WriteSingleChoice(rowOffset + 2 + letterIndex, colOffset, itemNames[letterIndex], letterIndex,
										textColor: Color.DarkGray, bgColor: Color.Grayscale20, letterColor: Color.DarkCyan);
									Screen.CursorVisible = false;
									Screen.ResumeUpdates();
									//todo, keeping this UI incomplete for now so I can do it right later
									key = Input.ReadKey();
									shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
									switch(key.Key){
										case ConsoleKey.A:
											e.ChosenAction = new UseItemAction(Player, Player.Inventory[letterIndex]);
											return null;
										case ConsoleKey.D:
											e.ChosenAction = new DropItemAction(Player, Player.Inventory[letterIndex]);
											return null;
										case ConsoleKey.F:
											//todo
											return null;
										default:
											break;
									}
									break;
							}
						}
						break;
				}
			}
		}
	}
}
