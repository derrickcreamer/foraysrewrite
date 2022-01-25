using System;
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
		public void LookMode(PlayerTurnEvent e, bool startInTravelMode = false){
			bool travelMode = startInTravelMode;
			Point p = Player.Position; //todo
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
				//todo, show path if travel mode
				bool hasLOS = Player.Position.HasLOS(p, Map.Tiles);
				bool seen = Map.Seen[p];
				/*ColorGlyph currentGlyph = hasLOS? GetCachedAtMapPosition(p)
					: seen? GetLastSeenColorGlyph(p, true)
					: new ColorGlyph(' ', Color.White);
				// todo, I think these next 2 lines will eventually be more like "start showing the position (Y,X) as highlighted with Targeting"
				//	so that the screen can interpret that as it wants.
				// Similarly, I think "show the blinking cursor at map position (Y,X)" will be added at some point, to support modes that show only part of the map etc.
				ColorGlyph highlighted = Screen.GetHighlighted(currentGlyph, HighlightType.Targeting);
				DrawToMap(p.Y, p.X, highlighted);*/
				Highlight highlight = new Highlight(MapHighlightType.SinglePoint) { Destination = p };
				MapRenderer.UpdateAllSettings(p, highlight);
				MapRenderer.DrawMap(e);
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
							//todo
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
	}
}
