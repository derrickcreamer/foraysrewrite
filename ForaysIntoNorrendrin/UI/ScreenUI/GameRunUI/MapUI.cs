using System;
using System.Text;
using System.Collections.Generic;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using GrammarUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	public class MapUI : GameUIObject {
		// Track display height/width separately to make it easier to change later:
		public const int MapDisplayHeight = GameUniverse.MapHeight;
		public const int MapDisplayWidth = GameUniverse.MapWidth;

		public static int RowOffset;
		public static int ColOffset;

		private ColorGlyph[][] cachedMapDisplay; // Used only for lookmode etc.

		public MapUI(GameRunUI ui) : base(ui) { }
		public void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+RowOffset, col+ColOffset, glyphIndex, color, bgColor);
		public void DrawToMap(int row, int col, ColorGlyph cg)
			=> Screen.Write(GameUniverse.MapHeight-1-row+RowOffset, col+ColOffset, cg);
		public ColorGlyph GetCachedAtMapPosition(Point p) => cachedMapDisplay[GameUniverse.MapHeight-1-p.Y][p.X];
		public void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+RowOffset, col+ColOffset);

		public void DrawMap(bool drawUsingCache){
			if(drawUsingCache && cachedMapDisplay != null){
				Screen.Write(RowOffset, ColOffset, cachedMapDisplay);
				return;
			}
			for(int i = 0; i < GameUniverse.MapHeight; i++) { //todo, cache all LOS + lighting for player turn... conditionally? for certain commands?
				for(int j = 0; j < GameUniverse.MapWidth; j++) {
					char ch = ' ';
					Color color = Color.Gray;
					switch(this.TileTypeAt(new Point(j, i))) {
						case TileType.Floor:
							ch = '.';
							break;
						case TileType.Wall:
							ch = '#';
							break;
						case TileType.Water:
							ch = '~';
							color = Color.Blue;
							break;
						case TileType.Staircase:
							ch = '>';
							color = Color.RandomBreached;
							break;
					}

					if(this.CreatureAt(new Point(j, i))?.OriginalType == CreatureType.Goblin){
						ch = 'g';
						color = Color.Green;
					}
					if(!Player.Position.HasLOS(new Point(j, i), Map.Tiles)){
						color = Color.DarkBlue;
					}
					else{
						if(!Map.Light.CellAppearsLitToObserver(new Point(j, i), Player.Position)){
							color = Color.DarkCyan;
						}
					}

					DrawToMap(i, j, ch, color);
				}
			}
			DrawToMap(Player.Position.Y, Player.Position.X, '@', Color.White);
			if(drawUsingCache) cachedMapDisplay = Screen.GetCurrent(RowOffset, ColOffset, MapDisplayHeight, MapDisplayWidth);
			else cachedMapDisplay = null;
		}
		public void LookMode(PlayerTurnEvent e){
			bool travelMode = false;
			Point p = Player.Position; //todo
			while(true){
				Screen.HoldUpdates();
				Screen.Clear(MessageBuffer.RowOffset, ColOffset, 4, MapDisplayWidth);
				Screen.Clear(GameRunUI.EnviromentalDescriptionRow, ColOffset, 1, MapDisplayWidth);
				Screen.Clear(GameRunUI.CommandListRow, ColOffset, 1, MapDisplayWidth);
				GameRunUI.DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.DoNotDraw,
					map: DrawOption.Normal, drawMapUsingCache: true,
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
				ColorGlyph highlighted = Screen.GetHighlighted(GetCachedAtMapPosition(p), HighlightType.Targeting);
				DrawToMap(p.Y, p.X, highlighted);
				string lookDescription = GetDescriptionAtCell(p);
				//todo, handle 2-line wrap around
				Screen.Write(GameRunUI.EnviromentalDescriptionRow, ColOffset, lookDescription, Color.Green);
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
								e.ChosenAction = new WalkAction(Player, Player.Position.PointInDir(Dir8.NE)); //todo
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
		private string GetDescriptionAtCell(Point p){
			List<string> items = new List<string>();
			bool includeMonsters = true; //todo
			if(includeMonsters){
				Creature creature = CreatureAt(p);
				if(creature != null && creature != Player && Player.CanSee(creature)){
					string creatureStatus = "(unhurt, unaware)"; //todo
					items.Add(ScreenUIMain.Grammar.Get(Determinative.AAn, Names.Get(creature.OriginalType), extraText: creatureStatus));
				}
			}
			Item item = ItemAt(p);
			if(item != null){
				string itemExtra = "";
				//check item ID here, todo
				ItemType finalType = item.Type; //todo, check ID for this too
				items.Add(ScreenUIMain.Grammar.Get(Determinative.AAn, Names.Get(finalType), extraText: itemExtra));
			}
			TileType tileType = TileTypeAt(p);
			//todo, check tile known status?
			//todo, features here
			//todo, traps, shrines, idols, etc.
			string tileConnector = GetConnectingWordForTile(tileType);
			string tileName = ScreenUIMain.Grammar.Get(Determinative.AAn, Names.Get(tileType));
			if(items.Count > 0 && tileConnector == "and"){ // If it's "and", just handle it like the others:
				items.Add(tileName);
			}
			StringBuilder sb = new StringBuilder();
			sb.Append("You see ");
			if(items.Count == 0){
				sb.Append(tileName);
			}
			else if(items.Count == 1){
				sb.Append(items[0]);
				sb.Append(" ");
				sb.Append(tileConnector);
				sb.Append(" ");
				sb.Append(tileName);
			}
			else{
				AppendWordListWithCommas(sb, items);
				if(tileType != TileType.Floor && tileConnector != "and"){
					sb.Append(tileConnector);
					sb.Append(" ");
					sb.Append(tileName);
				}
			}
			sb.Append(". ");
			string result = sb.ToString();
			sb.Clear();
			return result;
		}
		private string GetConnectingWordForTile(TileType type){
			//todo
			switch(type){
				case TileType.Floor:
				case TileType.Staircase:
					return "on";
				//case TileType.: todo door
				default: return "and";
			}
		}
		private void AppendWordListWithCommas(StringBuilder sb, IList<string> words){
			if(words.Count == 0) return;
			else if(words.Count == 1) sb.Append(words[0]); // "one"
			else if(words.Count == 2){ // "one and two"
				sb.Append(words[0]);
				sb.Append(" and ");
				sb.Append(words[1]);
			}
			else{ // "one, two, and three"
				for(int i=0;i<words.Count - 1;++i){
					sb.Append(words[i]);
					sb.Append(", ");
				}
				sb.Append(" and ");
				sb.Append(words[words.Count - 1]);
			}
		}
	}
}
