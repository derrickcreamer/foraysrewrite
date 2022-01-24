using System;
using Forays;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// ActionsScreen.cs
	// AdventureLogScreen.cs
	// CharacterScreens.cs (main part, calls the others, has constructor and helpers)
	// EquipmentScreen.cs
	// InventoryScreen.cs
	public enum CharacterScreen { Inventory, Equipment, Actions, AdventureLog };
	public partial class CharacterScreens : GameUIObject{
		const string SeparatorBar = "------------------------------------------------------------------"; // 66, equal to map display width
		public CharacterScreens(GameRunUI ui) : base(ui){ }

		public void Show(PlayerTurnEvent e, CharacterScreen screen){
			CharacterScreen? nextScreen = screen;
			MapRenderer.HideMap();
			do{
				if(nextScreen == CharacterScreen.Inventory) nextScreen = ShowInventory(e);
				else if(nextScreen == CharacterScreen.Equipment) nextScreen = ShowEquipment(e);
				else if(nextScreen == CharacterScreen.Actions) nextScreen = ShowActions(e);
				else if(nextScreen == CharacterScreen.AdventureLog) nextScreen = ShowAdventureLog(e);
				else return;
			}
			while(nextScreen != null && !GameUniverse.Suspend && !GameUniverse.GameOver); // Escape menu might end game, so check
		}

		private void DrawCommonSections(CharacterScreen currentScreen){
			GameRunUI.DrawGameUI(
				sidebar: DrawOption.Darkened,
				messages: DrawOption.DoNotDraw,
				environmentalDesc: DrawOption.DoNotDraw,
				commands: DrawOption.DoNotDraw
			);
			const int rowOffset = 0;
			int colOffset = MapUI.ColOffset;
			const Color bgColor = Color.Black;
			Screen.Write(rowOffset, colOffset, "Inventory   Equipment   Actions   Adventure Log    [Tab] to switch", Color.DarkGray, bgColor);
			Screen.Write(rowOffset, colOffset + 51, "[Tab] to switch", Color.Gray, bgColor);
			Screen.Write(rowOffset, colOffset + 52, "Tab", Color.Cyan, bgColor);
			int highlightOffset;
			string highlightLabel;
			switch(currentScreen){
				case CharacterScreen.Inventory:
					highlightOffset = 0;
					highlightLabel = "Inventory";
					break;
				case CharacterScreen.Equipment:
					highlightOffset = 12;
					highlightLabel = "Equipment";
					break;
				case CharacterScreen.Actions:
					highlightOffset = 24;
					highlightLabel = "Actions";
					break;
				case CharacterScreen.AdventureLog:
					highlightOffset = 34;
					highlightLabel = "Adventure Log";
					break;
				default: throw new InvalidOperationException("Invalid screen");
			}
			Screen.Write(rowOffset, colOffset + highlightOffset, highlightLabel, Color.Green, bgColor);
		}
	}
}
