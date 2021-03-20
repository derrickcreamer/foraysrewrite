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
				int colOffset = MapUI.ColOffset;
				Screen.HoldUpdates();
				Screen.Clear(0, colOffset, ScreenUIMain.Rows, MapUI.MapDisplayWidth);
				DrawCommonSections(CharacterScreen.AdventureLog);

				Screen.Write(rowOffset, colOffset, "Adventure log of Charname III: ");
				Screen.Write(rowOffset + 1, colOffset, SeparatorBar);
				Screen.Write(rowOffset + 2, colOffset, "Entered the mountain pass 571 turns ago");
				Screen.Write(rowOffset + 3, colOffset, "Skills: (none)"); //todo
				Screen.WriteListOfChoices(rowOffset + 5, colOffset, new[] {
					"Discovered locations", "Discovered items", "Recent messages", "Pause adventure, change options, or get help"
				});
				Screen.Write(rowOffset + 9, colOffset, SeparatorBar); //todo, count?
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
						RunDijkstraTest();
						break;
					case ConsoleKey.B:
					{//todo
						for(int i = 0;i<GameUniverse.MapHeight;++i){
							for(int j=0;j<GameUniverse.MapWidth;++j){
								if(dm[j,i] == DijkstraMap.Blocked || dm[j,i] == DijkstraMap.Unexplored) continue;
								dm[j,i] = (int)(-(dm[j,i]) * 1.4);
							}
						}
						//Map.Tiles[44, 20] = TileType.Staircase;
						//dm[44, 20] = 50;
						dm.RescanWithCurrentValues();
						int min = 99999;
						for(int i = 0;i<GameUniverse.MapHeight;++i){
							for(int j=0;j<GameUniverse.MapWidth;++j){
								if(dm[j,i] == DijkstraMap.Blocked || dm[j,i] == DijkstraMap.Unexplored) continue;
								if(dm[j,i] < min) min = dm[j,i];
							}
						}
						for(int i = 0;i<GameUniverse.MapHeight;++i){
							for(int j=0;j<GameUniverse.MapWidth;++j){
								if(dm[j,i] == DijkstraMap.Blocked || dm[j,i] == DijkstraMap.Unexplored) continue;
								dm[j,i] -= min;
							}
						}
						//PrintDijkstraTest();
					}
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
		int GetCellCost(GameComponents.Point p){ //todo, wait, return negatives here, not null...
			switch(TileTypeAt(p)){
				case TileType.DeepWater: return 50;
				case TileType.Staircase: return 20;
				case TileType.Wall: return -1;
				default: return 10;
			}
		}
		bool IsSource(GameComponents.Point p){
			return TileTypeAt(p) == TileType.Staircase || p == Player.Position;
		}
		int GetSourceValue(GameComponents.Point p){
			if(TileTypeAt(p) == TileType.Staircase) return 50;
			return 0;
		}
		static DijkstraMap dm;
		void RunDijkstraTest(){
			/*System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			PointArray<int> map;
			DijkstraMap dm1;
			DijkstraMap2 dm2 = null;
			Func<GameComponents.Point, int> f = GetCellCost;
			timer.Start();
			timer.Stop();
			string ms1 = timer.ElapsedMilliseconds.ToString();
			timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			//dm = new DijkstraMap(GetCellCost){ IsSource = IsSource, GetSourceValue = GetSourceValue };
			//for(int n = 0;n<10000;++n){
				dm2 = new DijkstraMap2(GetCellCost);
				dm2.Scan(Player.Position);
			//}
			timer.Stop();
			string ms2 = timer.ElapsedMilliseconds.ToString();
			Screen.Write(10, 10, "   " + ms1 + "   ", Color.Green);
			Screen.Write(11, 10, "   " + ms2 + "   ", Color.Green);
			Input.ReadKey();
			PrintDijkstraTest(dm2);*/
		}
		public void PrintDijkstraTest(PointArray<int> dm){
			Screen.HoldUpdates();
			int min = int.MaxValue;
			for(int i = 0;i<GameUniverse.MapHeight;++i){
				for(int j=0;j<GameUniverse.MapWidth;++j){
					if(dm[j,i] != DijkstraMap.Blocked && dm[j,i] < min){
						min = dm[j,i];
					}
				}
			}
			if(min < 0){
				for(int i = 0;i<GameUniverse.MapHeight;++i){
					for(int j=0;j<GameUniverse.MapWidth;++j){
						if(dm[j,i] != DijkstraMap.Blocked && dm[j,i] != DijkstraMap.Unexplored){
							dm[j,i] -= min;
						}
					}
				}
			}
			for(int i = 0;i<GameUniverse.MapHeight;++i){
				for(int j=0;j<GameUniverse.MapWidth;++j){
					int num = dm[j,i];
					char ch = '#';
					Color bgColor = Color.Gray;
					Color color = Color.Black;
					if(num != DijkstraMap.Unexplored && num != DijkstraMap.Blocked){
						bgColor = Color.Black;
						num = num/10;
						if(num < 10){
							color = Color.Yellow;
							ch = num.ToString()[0];
						}
						else{
							if(num<36){
								color = Color.Red;
								ch = (char)((num - 10) + 'a');
							}
							else if(num<62){
								color = Color.Magenta;
								ch = (char)((num - 36) + 'A');
							}
							else{
								color = Color.Blue;
								ch = '+';
							}
						}
					}
					MapUI.DrawToMap(i, j, ch, color, bgColor);
				}
			}
			Screen.ResumeUpdates();
			Input.ReadKey();
		}
	}
}
