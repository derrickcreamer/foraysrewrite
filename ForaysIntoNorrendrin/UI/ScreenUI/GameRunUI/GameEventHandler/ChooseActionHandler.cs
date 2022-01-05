using System;
using System.Collections.Generic;
using System.Threading;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticInput;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs (contains the constructor)
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		public bool Autoexplore;
		private Dir8? walkDir;

		private void ChooseAction(PlayerTurnEvent e){
			do{
				GameRunUI.DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.Normal,
					map: DrawOption.Normal,
					environmentalDesc: DrawOption.Normal,
					commands: DrawOption.Normal
				);
				//window update, set suspend if false...
				/*if(!Screen.Update()){
					GameUniverse.Suspend = true;
					return;
				}*/
				//
				//
				MapUI.SetCursorPositionOnMap(Player.Position.Y, Player.Position.X);
				if(walkDir != null){
					if(Input.KeyIsAvailable){
						Input.FlushInput();
						walkDir = null;
					}
					else{
						Point targetPoint = Player.Position.PointInDir(walkDir.Value);
						if(!TileDefinition.IsPassable(TileTypeAt(targetPoint)) || CreatureAt(targetPoint) != null){
							walkDir = null;
						}
						else{
							if(!Screen.WindowUpdate()) Program.Quit();
							Thread.Sleep(10); //todo, make configurable
							e.ChosenAction = new WalkAction(Player, Player.Position.PointInDir(walkDir.Value));
							return;
						}
					}
				}
				else if(Autoexplore){
					if(Input.KeyIsAvailable){
						Input.FlushInput();
						Autoexplore = false;
					}
					else{
						//todo, check for interruptions etc. - Interruption should share code between walk+autoexplore.
						if(!Screen.WindowUpdate()) Program.Quit();
						Thread.Sleep(10); //todo, make configurable
						ChooseAutoexploreAction(e);
						if(e.ChosenAction != null) return;
					}
				}
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.NumPad8:
						ChooseActionFromDirection(e, Dir8.N, shift);
						break;
					case ConsoleKey.NumPad6:
						ChooseActionFromDirection(e, Dir8.E, shift);
						break;
					case ConsoleKey.NumPad4:
						ChooseActionFromDirection(e, Dir8.W, shift);
						break;
					case ConsoleKey.NumPad2:
						ChooseActionFromDirection(e, Dir8.S, shift);
						break;
					case ConsoleKey.NumPad9:
						ChooseActionFromDirection(e, Dir8.NE, shift);
						break;
					case ConsoleKey.NumPad3:
						ChooseActionFromDirection(e, Dir8.SE, shift);
						break;
					case ConsoleKey.NumPad1:
						ChooseActionFromDirection(e, Dir8.SW, shift);
						break;
					case ConsoleKey.NumPad7:
						ChooseActionFromDirection(e, Dir8.NW, shift);
						break;
					case ConsoleKey.Escape:
						EscapeMenu.Open();
						break;
					case ConsoleKey.I:
						CharacterScreens.Show(e, CharacterScreen.Inventory);
						break;
					case ConsoleKey.E:
						CharacterScreens.Show(e, CharacterScreen.Equipment);
						break;
					case ConsoleKey.Enter:
						CharacterScreens.Show(e, CharacterScreen.Actions);
						break;
					case ConsoleKey.V:
						CharacterScreens.Show(e, CharacterScreen.AdventureLog);
						break;
					case ConsoleKey.Tab:
						MapUI.LookMode(e);
						if(Autoexplore) ChooseAutoexploreAction(e);
						break;
					case ConsoleKey.X:
						MapUI.LookMode(e, true);
						if(Autoexplore) ChooseAutoexploreAction(e);
						break;
				}
			} while(e.ChosenAction == null);
		}
		private void ChooseActionFromDirection(PlayerTurnEvent e, Dir8 dir, bool shift){
			Point targetPoint = Player.Position.PointInDir(dir);
			Creature targetCreature = CreatureAt(targetPoint);
			if(targetCreature != null){
				e.ChosenAction = new MeleeAttackAction(Player, targetCreature);
				return;
			}
			// Check for wall sliding:
			bool wallSliding = false;
			if(!TileDefinition.IsPassable(TileTypeAt(targetPoint))){
				Point cwPoint = Player.Position.PointInDir(dir.Rotate(true));
				Point ccwPoint = Player.Position.PointInDir(dir.Rotate(false));
				if(TileDefinition.IsPassable(TileTypeAt(cwPoint)) && !TileDefinition.IsPassable(TileTypeAt(ccwPoint))) {
					wallSliding = true;
					dir = dir.Rotate(true);
					targetPoint = Player.Position.PointInDir(dir);
					targetCreature = CreatureAt(targetPoint);
				}
				else if(!TileDefinition.IsPassable(TileTypeAt(cwPoint)) && TileDefinition.IsPassable(TileTypeAt(ccwPoint))) {
					wallSliding = true;
					dir = dir.Rotate(false);
					targetPoint = Player.Position.PointInDir(dir);
					targetCreature = CreatureAt(targetPoint);
				}
			}
			if(targetCreature != null){
				e.ChosenAction = new MeleeAttackAction(Player, targetCreature);
			}
			else{
				e.ChosenAction = new WalkAction(Player, targetPoint);
				if(shift && !wallSliding) walkDir = dir; // Don't set walkDir for attacks or wall slides
			}
		}
		private void ChooseAutoexploreAction(PlayerTurnEvent e){
			// dijkstra map from unknown cells, or BFS stopping at unknown - todo.
			/*var dm = new DijkstraMap2(p => (!Map.Seen[p] || TileTypeAt(p) == TileType.Wall)? -1 : 10){
				IsSource = p => !Map.Seen[p]
			};
			dm.Scan();
			var dm2 = new DijkstraMap2(p => 10){
				IsSource = p => (Map.Seen[p] || p.IsMapEdge())
			};
			dm2.Scan();*/
			// make unexplored map first
			// use unexplored map to determine SOURCE VALUES in 2nd map -
			//    specifically, double the unexplored map values, negate, and rescan unexplored map.
			//    Then, negate again and use those values as starting values. So the one near the big unexplored issue might be 8,
			//    while the other is maybe 2.
			var dm2 = new DijkstraMap(p => Map.Seen[p]? -1 : 10){ //todo, seen==blocked?
				IsSource = p => (Map.Seen[p] || p.IsMapEdge())
			};
			dm2.Scan();
			//CharacterScreens.PrintDijkstraTest(dm2);
			foreach(Point p in Map.GetAllPoints(false)){
				//if(dm2[p] == DijkstraMap2.Unexplored || dm2[p] == DijkstraMap2.Blocked) continue;
				dm2[p] = (int)-(dm2[p] * 3.2f); //3.1411 is low enough to take the top path. 3.142 is high enough to take the bottom path.
			}
			dm2.RescanWithCurrentValues();
			//CharacterScreens.PrintDijkstraTest(dm2);
			var dm = new DijkstraMap(p => (!Map.Seen[p] || !TileDefinition.IsPassable(TileTypeAt(p)))? -1 : 10){
				IsSource = p => !Map.Seen[p],
				GetSourceValue = p => p.IsMapEdge()? DijkstraMap.Blocked : -(dm2[p])
			};
			dm.Scan();
			//CharacterScreens.PrintDijkstraTest(dm);
			int min = int.MaxValue;
			List<Point> valid = new List<Point>();
			foreach(Dir8 dir in EightDirections.Enumerate(true, false, true)){
				Point neighbor = Player.Position.PointInDir(dir);
				if(dm[neighbor] == DijkstraMap.Blocked) continue;
				if(dm[neighbor] < min){
					min = dm[neighbor];
					valid.Clear();
					valid.Add(neighbor);
				}
				else if(dm[neighbor] == min){
					valid.Add(neighbor);
				}
			}
			Point dest = ScreenRNG.ChooseFromList(valid);
			e.ChosenAction = new WalkAction(Player, dest);
		}
	}
}
