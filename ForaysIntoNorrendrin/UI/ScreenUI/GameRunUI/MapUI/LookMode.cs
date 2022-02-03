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
			//todo... a few notes about travel destinations and interesting targets:
			// could i make it pick ONLY SEVERAL unexplored cells to be interesting? grab the closest and then DON'T pick any others nearby.
			// Autoexplore would be unaffected since it'd always use the closest.
			// Based on the options given, the first travel destination will be different: an explore target, or stairs if '>' is pressed.
			// (Interesting _look_ targets are completely different and done separately.)
			DijkstraMap playerMovementMap = new DijkstraMap(point => (!Map.Seen[point] || !TileDefinition.IsPassable(TileTypeAt(point)))? -1 : 10){
				IsSource = point => point == Player.Position
			};
			playerMovementMap.Scan();
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
					Point pathTarget = p;
					// First, figure out whether the destination cell is known-reachable:
					if(knownReachable == null){
						knownReachable = FloodFill.ScanToArray(Player.Position, CellIsKnownPassable);
					}
					if(!knownReachable[p]){
						// If not, then find the nearest known reachable spaces:
						if(distanceToKnownReachable == null){
							distanceToKnownReachable = new DijkstraMap(point => 1){
								IsSource = point => knownReachable[point]
							};
							distanceToKnownReachable.Scan();
						}
						if(distanceToKnownReachable[p] > 1){ // If distance is 1, then we can reach it anyway
							pathTarget = p.EnumeratePointsAtChebyshevDistance(distanceToKnownReachable[p], true, false) // We know the distance already, so check only those cells...
								.Where(nearby => nearby.ExistsBetweenMapEdges() && knownReachable[nearby]) // ...make sure only reachable ones are considered...
								.WhereLeast(nearby => p.GetHalfStepMetricDistance(nearby)) // ...get the nearest ones to the targeted point...
								.WhereLeast(nearby => Player.Position.GetHalfStepMetricDistance(nearby))[0]; // ...and finally get whichever one of those is closest to the player.
						}
					}
					List<Point> path = playerMovementMap.GetDownhillPath(pathTarget, preferCardinalDirections: true, includePathSource: true, includePathDestination: false, ignorePathSourceCost: true);
					path.Reverse();
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
				MapRenderer.DrawMap(e);
				bool hasLOS = e.CellsVisibleThisTurn[p];
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
						case ConsoleKey.X:
							if(travelMode){
								// TODO NEXT:  make continuous autoexplore no longer the default. Make 'x' actually set the chosen path.
								GameEventHandler.Autoexplore = true;
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
		public List<Point> GetTravelDestinations(TravelDestinationPriority priority){
			return new List<Point>{ new Point(11, 11), new Point(15, 15) };
		}
	}
}
