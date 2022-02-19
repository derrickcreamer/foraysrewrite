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
		private List<Point> Path;
		private int NextPathIndex;
		private Dir8? walkDir;

		private void ChooseAction(PlayerTurnEvent e){
			do{
				GameRunUI.DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.Normal,
					environmentalDesc: DrawOption.Normal,
					commands: DrawOption.Normal
				);
				MapRenderer.UpdateAllSettings(Player.Position);
				MapRenderer.DrawMap(e);
				//window update, set suspend if false...
				/*if(!Screen.Update()){
					GameUniverse.Suspend = true;
					return;
				}*/
				//
				//
				if(walkDir != null){
					if(Input.KeyIsAvailable){
						Interrupt();
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
				else if(Autoexplore && (Path == null || NextPathIndex >= Path.Count)){
					if(Input.KeyIsAvailable){
						Interrupt();
					}
					else{
						Path = ChooseAutoexplorePath();
						NextPathIndex = 0;
						if(Path.Count == 0){
							Autoexplore = false;
							Path = null;
						}
					}
				}

				if(Path != null && NextPathIndex < Path.Count){
					if(Input.KeyIsAvailable){
						Interrupt();
					}
					else{
						Point next = Path[NextPathIndex];
						//todo check distance, walkable, etc.?
						Dir8 dir = Player.Position.GetDirectionOfNeighbor(next);
						Point pointInDir = Player.Position.PointInDir(dir);
						if(CreatureAt(pointInDir) != null){
							Interrupt();
						}
						else if(!TileDefinition.IsPassable(TileTypeAt(pointInDir))){
							if(Autoexplore){
								Path = ChooseAutoexplorePath();
								NextPathIndex = 0;
								if(Path.Count == 0){
									Autoexplore = false;
									Path = null;
								}
							}
							else{
								Interrupt();
							}
							//  ultimately, do I want to keep checking all the _visible_ cells in the path to see if any are blocked?
							//		...if one is blocked, what should happen?
							// ALSO, should autoexplore actually recalculate the path after every STEP?
							//
							// ...but AFTER THAT...
							// There's still more to consider regarding autoexplore AND travel destination calculation:
							//	-do I want to make it weigh AGAINST the stairs, once they're known?
							//	-it seems like I might want to allow some of the 'avoid this area' to _cross_ narrow bridges of known space, because
							//		currently exploration is happy to go into a room because of ONE small unknown area, even if the OTHER side of the room is touching the large unknown area.
							//		(that COULD be a tricky problem to solve...not sure.)
							//		(this happens on one of the premade levels near the bottom of the map)
						}
						else{
							e.ChosenAction = new WalkAction(Player, Player.Position.PointInDir(dir));
							NextPathIndex++;
							if(Autoexplore) Path = null; //todo, testing this.
							//todo, interruptedPath? (and index?)
							//todo, BUT, the interrupted path should probably only matter if it was MANUALLY chosen. autoexplore doesn't matter much.
							if(!Screen.WindowUpdate()) Program.Quit();
							Thread.Sleep(10); //todo, make configurable
							return;
						}
					}
				}
				/*else if(Autoexplore){
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
				}todo remove */
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
					case ConsoleKey.D: //todo, 'd' is definitely not 'descend'
						e.ChosenAction = new DescendAction(Player);
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
						if(Autoexplore) StartAutoexplorePathing(e);
						break;
					case ConsoleKey.X:
						MapUI.LookMode(e, true);
						if(Autoexplore) StartAutoexplorePathing(e);
						break;
				}
			} while(e.ChosenAction == null);
		}
		private void Interrupt(){
			if(Path != null && NextPathIndex < Path.Count){
				//todo, remember interrupted path here
			}
			Input.FlushInput();
			Autoexplore = false;
			Path = null;
			NextPathIndex = 0;
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
		private List<Point> ChooseAutoexplorePath(){
			//todo...need a better way to know autoexplore is done.
			List<Point> destinations = MapUI.GetTravelDestinations(MapUI.TravelDestinationPriority.Explore);
			Point destination = destinations[0];
			//todo, this could be A* eventually:
			DijkstraMap playerMovementMap = new DijkstraMap(point => (!Map.Seen[point] || !TileDefinition.IsPassable(TileTypeAt(point)))? -1 : 10);
			playerMovementMap.Scan(Player.Position);
			//todo, if destination valid?
			List<Point> path = playerMovementMap.GetDownhillPath(destination, preferCardinalDirections: true, includePathSource: true, includePathDestination: false);
			path.Reverse();
			return path;
		}
		private void StartAutoexplorePathing(PlayerTurnEvent e){
			Path = ChooseAutoexplorePath();
			NextPathIndex = 0;
			if(Path.Count == 0){
				Autoexplore = false;
				Path = null;
			}
			else{
				Point next = Path[NextPathIndex];
				//todo check distance, walkable, etc.?
				Dir8 dir = Player.Position.GetDirectionOfNeighbor(next);
				e.ChosenAction = new WalkAction(Player, Player.Position.PointInDir(dir));
				NextPathIndex++;
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
			var potentiallyReachable = FloodFill.ScanToArray(Player.Position, point => !point.IsMapEdge() && (!Map.Seen[point] || TileDefinition.IsPassable(TileTypeAt(point))));
			var dm2 = new DijkstraMap(p => Map.Seen[p]? -1 : 1){ //todo, seen==blocked?
				IsSource = p => (Map.Seen[p] || p.IsMapEdge())
			};
			dm2.Scan();
			//CharacterScreens.PrintDijkstraTest(dm2);
			foreach(Point p in Map.GetAllPoints(false)){
				if(dm2[p] == DijkstraMap.Unexplored || dm2[p] == DijkstraMap.Blocked) continue;
				dm2[p] = -(dm2[p] * dm2[p]);
			}
			dm2.RescanWithCurrentValues();
			//CharacterScreens.PrintDijkstraTest(dm2);
			var dm = new DijkstraMap(p => (!Map.Seen[p] || !TileDefinition.IsPassable(TileTypeAt(p)))? -1 : 1){
				IsSource = p => !Map.Seen[p] && potentiallyReachable[p],
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
