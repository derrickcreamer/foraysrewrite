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
		public List<Point> Path;
		public int NextPathIndex;
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
				else if(Autoexplore){
					if(Input.KeyIsAvailable){
						Interrupt();
					}
					else{
						ChooseAutoexploreAction(e);
						if(e.ChosenAction != null){
							if(!Screen.WindowUpdate()) Program.Quit();
							Thread.Sleep(10); //todo, make configurable
						}
						else Interrupt("You don't see a path for further exploration. ");
						return;
					}
				}
				else if(Path != null && NextPathIndex < Path.Count){
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
							Interrupt();
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
							//todo, interruptedPath? (and index?)
							if(!Screen.WindowUpdate()) Program.Quit();
							Thread.Sleep(10); //todo, make configurable
							return;
						}
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
						break;
					case ConsoleKey.X:
						MapUI.LookMode(e, true);
						break;
				}
			} while(e.ChosenAction == null);
		}
		private void Interrupt(string message = null){
			if(Path != null && NextPathIndex < Path.Count){
				//todo, remember interrupted path here
			}
			Input.FlushInput();
			Autoexplore = false;
			Path = null;
			NextPathIndex = 0;
			if(message != null) Messages.Add(message);
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
			var potentiallyReachable = FloodFill.ScanToArray(Player.Position, point => !point.IsMapEdge() && (!Map.Seen[point] || TileDefinition.IsPassable(TileTypeAt(point))));
			var distanceToKnown = new DijkstraMap(p => (Map.Seen[p] || p.IsMapEdge())? -1 : 1){
				IsSource = p => (Map.Seen[p] || p.IsMapEdge())
			};
			distanceToKnown.Scan();
			foreach(Point p in Map.GetAllPoints(false)){
				if(distanceToKnown[p] == DijkstraMap.Unexplored || distanceToKnown[p] == DijkstraMap.Blocked) continue;
				distanceToKnown[p] = -(distanceToKnown[p] * distanceToKnown[p]);
			}
			distanceToKnown.RescanWithCurrentValues();
			var exploreMap = new DijkstraMap(p => (!Map.Seen[p] || !TileDefinition.IsPassable(TileTypeAt(p)))? -1 : 1){ // todo, needs items, shrines, etc. eventually
				IsSource = p => !Map.Seen[p] && potentiallyReachable[p],
				GetSourceValue = p => -(distanceToKnown[p])
			};
			exploreMap.Scan();
			List<Point> playerPath = exploreMap.GetDownhillPath(Player.Position, true, earlyStopCondition: p => true);
			if(playerPath.Count == 0) return;
			else e.ChosenAction = new WalkAction(Player, playerPath[0]);
		}
	}
}
