using System;
using System.Collections.Generic;
using System.Linq;
using Forays;
using ForaysUI.ScreenUI.MapRendering;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// LookMode.cs
	// MapDescription.cs
	// MapMemory.cs
	// MapUI.cs (has constructor)
	public partial class MapUI : GameUIObject {
		public enum TravelDestinationPriority { Explore, Stairs }
		public void LookMode(PlayerTurnEvent e, bool startInTravelMode = false, TravelDestinationPriority travelPriority = TravelDestinationPriority.Explore){
			bool travelMode = startInTravelMode;
			List<Point> travelDestinations = GetTravelDestinations(travelPriority); //todo, start this as null or not?
			DijkstraMap playerMovementMap = new DijkstraMap(point => (!Map.Seen[point] || !TileDefinition.IsPassable(TileTypeAt(point)))? -1 : 10);
			playerMovementMap.Scan(Player.Position);
			PointArray<bool> knownReachable = null;
			DijkstraMap distanceToKnownReachable = null;
			int travelIndex = 0;
			Point p = travelDestinations[0];
			while(true){
				Screen.HoldUpdates();
				Screen.Clear(MessageBuffer.RowOffset, ColOffset, 4, MapDisplayWidth);
				Screen.Clear(GameRunUI.EnviromentalDescriptionRow, ColOffset, 1, MapDisplayWidth);
				Screen.Clear(GameRunUI.CommandListRow, ColOffset, 1, MapDisplayWidth);
				GameRunUI.DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.DoNotDraw,
					environmentalDesc: DrawOption.DoNotDraw,
					commands: DrawOption.DoNotDraw
				);
				string moveCursorMsg = travelMode? "Travel mode -- Move cursor to choose destination." : "Move cursor to look around.";
				Screen.Write(MessageBuffer.RowOffset, ColOffset, moveCursorMsg);
				Screen.Write(MessageBuffer.RowOffset + 2, ColOffset, "[Tab] to cycle between interesting targets          [m]ore details");
				Screen.Write(MessageBuffer.RowOffset + 2, ColOffset + 1, "Tab", Color.Cyan);
				Screen.Write(MessageBuffer.RowOffset + 2, ColOffset + 53, 'm', Color.Cyan);
				if(travelMode){
					Screen.Write(MessageBuffer.RowOffset + 3, ColOffset, "[x] to confirm destination           [Space] to cancel travel mode");
					Screen.Write(MessageBuffer.RowOffset + 3, ColOffset + 1, 'x', Color.Cyan);
					Screen.Write(MessageBuffer.RowOffset + 3, ColOffset + 38, "Space", Color.Cyan);
				}
				else{
					Screen.Write(MessageBuffer.RowOffset + 3, ColOffset, "[x] to enter travel mode");
					Screen.Write(MessageBuffer.RowOffset + 3, ColOffset + 1, 'x', Color.Cyan);
				}
				Highlight highlight;
				if(travelMode){
					List<Point> path = GetPathToNearbyReachable(p, playerMovementMap, ref knownReachable, ref distanceToKnownReachable);
					highlight = new Highlight(MapHighlightType.Path){
						Source = Player.Position,
						Destination = p,
						LineOrPath = path
					};
				}
				else{
					highlight = new Highlight(MapHighlightType.SinglePoint) { Destination = p };
				}
				MapRenderer.UpdateAllSettings(p, highlight);
				MapRenderer.DrawMap();
				bool hasLOS = Map.CellVisibleToPlayer(p);
				bool seen = Map.Seen[p];
				string lookDescription = hasLOS? GetDescriptionAtCell(p)
					: seen? GetLastKnownDescriptionAtCell(p)
					: "";
				if(lookDescription.Length > MapDisplayWidth){
					int splitIdx = 0;
					for(int idx=MapDisplayWidth-1;idx>=0;--idx){
						if(lookDescription[idx] == ' '){
							splitIdx = idx;
							break;
						}
					}
					int firstLineRow = Option.IsSet(BoolOptionType.MessagesAtBottom)? GameRunUI.CommandListRow
						: GameRunUI.EnviromentalDescriptionRow; // Start printing at whichever is higher onscreen
					string firstLine = lookDescription.Substring(0, splitIdx);
					string secondLine = lookDescription.Substring(splitIdx + 1); // Remove the space
					if(secondLine.Length > MapDisplayWidth){
						firstLine = hasLOS? "You see many things here."
							: "You remember seeing many things here."; //todo, what should this say?
						secondLine = "(Press 'm' for more details)";
						//secondLine = "(Use the '[m]ore details' command for the full list)"; todo...any better options?
					}
					Screen.Write(firstLineRow, ColOffset, firstLine, Color.Green);
					Screen.Write(firstLineRow+1, ColOffset, secondLine, Color.Green);
				}
				else{
					Screen.Write(GameRunUI.EnviromentalDescriptionRow, ColOffset, lookDescription, Color.Green);
				}
				Screen.ResumeUpdates();
				bool needsRedraw = false;
				while(!needsRedraw){
					ConsoleKeyInfo key = Input.ReadKey();
					bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
					switch(key.Key){
						case ConsoleKey.Tab:
							if(shift) travelIndex--;
							else travelIndex++;
							if(travelIndex < 0) travelIndex = travelDestinations.Count - 1;
							else if(travelIndex >= travelDestinations.Count) travelIndex = 0;
							p = travelDestinations[travelIndex];
							needsRedraw = true;
							break;
						case ConsoleKey.Escape:
							return; // Done
						case ConsoleKey.Spacebar:
							if(travelMode){
								travelMode = false;
								needsRedraw = true;
							}
							else{
								return;
							}
							break;
						case ConsoleKey.Enter:
							if(travelMode) goto case ConsoleKey.X; // Allow Enter to confirm travel destination, or cancel look mode.
							else goto case ConsoleKey.Escape;
						case ConsoleKey.X:
							if(travelMode){
								List<Point> path = GetPathToNearbyReachable(p, playerMovementMap, ref knownReachable, ref distanceToKnownReachable);
								if(path.Count > 0){
									GameEventHandler.Path = path;
									GameEventHandler.NextPathIndex = 0;
								}
								//if(false) GameEventHandler.Autoexplore = true; //todo, option here
								return;
							}
							else{
								travelMode = true;
								needsRedraw = true;
							}
							break;
						case ConsoleKey.NumPad8:
							p = p.PointInDir(Dir8.N, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad6:
							p = p.PointInDir(Dir8.E, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad4:
							p = p.PointInDir(Dir8.W, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad2:
							p = p.PointInDir(Dir8.S, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad9:
							p = p.PointInDir(Dir8.NE, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad3:
							p = p.PointInDir(Dir8.SE, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad1:
							p = p.PointInDir(Dir8.SW, shift? 6 : 1);
							needsRedraw = true;
							break;
						case ConsoleKey.NumPad7:
							p = p.PointInDir(Dir8.NW, shift? 6 : 1);
							needsRedraw = true;
							break;
						default:
							break;
					}
					if(!p.ExistsOnMap()){
						int newX, newY;
						if(p.X < 0) newX = 0;
						else if(p.X >= GameUniverse.MapWidth) newX = GameUniverse.MapWidth - 1;
						else newX = p.X;
						if(p.Y < 0) newY = 0;
						else if(p.Y >= GameUniverse.MapHeight) newY = GameUniverse.MapHeight - 1;
						else newY = p.Y;
						p = new Point(newX, newY);
					}
				}
			}
		}
		private bool CellIsKnownPassable(Point point){
			return Map.Seen[point] && TileDefinition.IsPassable(TileTypeAt(point));
		}
		private List<Point> GetPathToNearbyReachable(Point potentialDestination, DijkstraMap playerMovementMap, ref PointArray<bool> knownReachable, ref DijkstraMap distanceToKnownReachable){
			Point destination = potentialDestination;
			// First, figure out whether the destination cell is known-reachable:
			if(knownReachable == null){
				knownReachable = FloodFill.ScanToArray(Player.Position, CellIsKnownPassable);
			}
			PointArray<bool> knownReachable2 = knownReachable; // Can't use a ref parameter inside a lambda, but using a 2nd variable works.
			if(!knownReachable[destination]){
				// If not, then find the nearest known reachable spaces:
				if(distanceToKnownReachable == null){
					distanceToKnownReachable = new DijkstraMap(point => 1){
						IsSource = point => knownReachable2[point]
					};
					distanceToKnownReachable.Scan();
				}
				if(distanceToKnownReachable[destination] > 1){ // If distance is 1, then we can reach it anyway
					destination = destination.EnumeratePointsAtChebyshevDistance(distanceToKnownReachable[destination], true, false) // We know the distance already, so check only those cells...
						.Where(nearby => nearby.ExistsBetweenMapEdges() && knownReachable2[nearby]) // ...make sure only reachable ones are considered...
						.WhereLeast(nearby => destination.GetHalfStepMetricDistance(nearby)) // ...get the nearest ones to the targeted point...
						.WhereLeast(nearby => Player.Position.GetHalfStepMetricDistance(nearby))[0]; // ...and finally get whichever one of those is closest to the player.
				}
			}
			if(destination == Player.Position) return new List<Point>();
			List<Point> path = playerMovementMap.GetDownhillPath(destination, preferCardinalDirections: true, includePathSource: true, includePathDestination: false, ignorePathSourceCost: true);
			path.Reverse();
			return path;
		}
		public List<Point> GetTravelDestinations(TravelDestinationPriority priority){
			List<Point> result = new List<Point>();
			Point? stairs = Map.GetAllPoints(false).FirstOrDefault(p => TileTypeAt(p) == TileType.Staircase);
			Point? exploreTarget = GetExploreDestination();
			if(priority == TravelDestinationPriority.Explore){
				if(exploreTarget != null) result.Add(exploreTarget.Value);
				if(stairs != null) result.Add(stairs.Value);
			}
			else if(priority == TravelDestinationPriority.Stairs){
				if(stairs != null) result.Add(stairs.Value);
				if(exploreTarget != null) result.Add(exploreTarget.Value);
			}
			result.Add(new Point(11, 11));
			result.Add(new Point(15, 15));
			result.Add(new Point(44, 8));
			result.Add(new Point(32, 19));
			return result;
			//todo... a few notes about travel destinations and interesting targets:
			// so, what are travel destinations?...
			// could i make it pick ONLY SEVERAL unexplored cells to be interesting? grab the closest and then DON'T pick any others nearby.
			// Autoexplore would be unaffected since it'd always use the closest.
			// Based on the options given, the first travel destination will be different: an explore target, or stairs if '>' is pressed.
			// (Interesting _look_ targets are completely different and done separately.)
			// so...
			// for now let's just find the closest unexplored, and add the stairs if known.
		}
		private Point? GetExploreDestination(){
			//todo, clean up later
			var potentiallyReachable = FloodFill.ScanToArray(Player.Position, point => !point.IsMapEdge() && (!Map.Seen[point] || TileDefinition.IsPassable(TileTypeAt(point))));
			var dm2 = new DijkstraMap(p => (Map.Seen[p] || p.IsMapEdge())? -1 : 1){ //todo, seen==blocked?
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
				GetSourceValue = p => -(dm2[p])
			};
			dm.Scan();
			//CharacterScreens.PrintDijkstraTest(dm2);
			//CharacterScreens.PrintDijkstraTest(dm);
			List<Point> playerPath = dm.GetDownhillPath(Player.Position, true, earlyStopCondition: p => !Map.Seen[p]);
			if(playerPath.Count == 0) return null;
			else return playerPath[playerPath.Count - 1];
		}
	}
}
