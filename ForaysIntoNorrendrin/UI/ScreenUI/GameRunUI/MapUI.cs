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

		private ColorGlyph[][] cachedMapDisplay; // Used only for lookmode etc. (Row-major because it's copied from the screen.)

		private PointArray<TileType> tilesLastSeen; // These are column-major because they align with game map coordinates.
		private PointArray<FeatureType> featuresLastSeen;
		private Dictionary<Point, TrapType> trapsLastSeen;
		//todo, shrines, idols?
		private Dictionary<Point, ItemType> itemsLastSeen;

		public MapUI(GameRunUI ui) : base(ui) {
			tilesLastSeen = new PointArray<TileType>(GameUniverse.MapWidth, GameUniverse.MapHeight);
			featuresLastSeen = new PointArray<FeatureType>(GameUniverse.MapWidth, GameUniverse.MapHeight);
			trapsLastSeen = new Dictionary<Point, TrapType>();
			itemsLastSeen = new Dictionary<Point, ItemType>();
		}
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
					Point p = new Point(j, i);
					Creature creature = CreatureAt(p);
					ItemType? item = ItemAt(p)?.Type; //todo check ID
					if(creature != null && Player.CanSee(creature)){ //todo, any optimizations for this CanSee check?
						DrawToMap(i, j, DetermineCreatureColorGlyph(creature.OriginalType, TileTypeAt(p), FeaturesAt(p), item));
						//todo, need to consider inverting colors when colors are the same.
						RecordMapMemory(p); // todo, should map memory always get recorded here?
					}
					else if(Player.Position.HasLOS(p, Map.Tiles)){
						Map.Seen[p] = true; //todo!!! This one does NOT stay here. Temporary hack to get map memory working. Should be done in the player turn action or similar.
						ColorGlyph cg = DetermineVisibleColorGlyph(TileTypeAt(p), FeaturesAt(p), item);
						if(!Map.Light.CellAppearsLitToObserver(p, Player.Position)){
							DrawToMap(i, j, cg.GlyphIndex, Color.DarkCyan); //todo, only some tiles get darkened this way, right?
						}
						else{
							DrawToMap(i, j, cg);
						}
						RecordMapMemory(p);
					}
					else if(false){ // todo, this is where the dcss-style option for seeing previous monster locations will be added
					}
					else if(Map.Seen[p]){
						DrawToMap(i, j, GetLastSeenColorGlyph(p, true));
					}
					else{
						DrawToMap(i, j, ' ', Color.White);
					}
				}
			}
			if(drawUsingCache) cachedMapDisplay = Screen.GetCurrent(RowOffset, ColOffset, MapDisplayHeight, MapDisplayWidth);
			else cachedMapDisplay = null;
		}
		private static ColorGlyph DetermineCreatureColorGlyph(CreatureType creature, TileType tile, FeatureType features, ItemType? item){
			//todo, add trap, shrine, etc.
			//todo features
			Color bgColor = Color.Black;
			if(features.HasFeature(FeatureType.Water)) bgColor = Color.DarkCyan;
			//else if(features.HasFeature(FeatureType.Lava)) bgColor = Color.DarkRed;
			else if(tile == TileType.ThickIce) bgColor = Color.Gray;
			else if(tile == TileType.DeepWater){
				if(features.HasFeature(FeatureType.Ice) || features.HasFeature(FeatureType.CrackedIce)) bgColor = Color.Gray;
				else bgColor = Color.DarkBlue;
			}
			ColorGlyph cg = GameObjectGlyphs.Get(creature);
			return new ColorGlyph(cg.GlyphIndex, cg.ForegroundColor, bgColor);
		}
		private static ColorGlyph DetermineVisibleColorGlyph(TileType tile, FeatureType features, ItemType? item){ //todo, add trap, shrine, etc.
			//todo features
			if(item != null) return GameObjectGlyphs.Get(item.Value);
			if(features.HasFeature(FeatureType.Ice)) return new ColorGlyph('~', Color.Cyan, Color.Gray);
			if(features.HasFeature(FeatureType.CrackedIce)) return new ColorGlyph('~', Color.Red, Color.Gray);
			if(features.HasFeature(FeatureType.BrokenIce)) return new ColorGlyph('~', Color.Gray, Color.DarkBlue);
			if(features.HasFeature(FeatureType.Water)) return new ColorGlyph('~', Color.Cyan, Color.DarkCyan);
			//todo, should change color for creatures based on background, right? do I need rules for which colors to modify/invert?
			//so... if background color... then return a modified version.
			//bg colors are currently black, gray, dark blue... plus targeting/mouseover stuff.

			return GameObjectGlyphs.Get(tile);
		}
		private ColorGlyph GetLastSeenColorGlyph(Point p, bool useOutOfSightColor){
			ItemType? lastKnownItem;
			if(itemsLastSeen.TryGetValue(p, out ItemType item)) lastKnownItem = item;
			else lastKnownItem = null; //todo ID?
			ColorGlyph cg = DetermineVisibleColorGlyph(tilesLastSeen[p], featuresLastSeen[p], lastKnownItem);
			if(useOutOfSightColor) return new ColorGlyph(cg.GlyphIndex, Color.OutOfSight, cg.BackgroundColor);
			else return cg;
		}
		private void RecordMapMemory(Point p){
			tilesLastSeen[p] = TileTypeAt(p);
			featuresLastSeen[p] = FeaturesAt(p);
			Item item = ItemAt(p);
			if(item != null) itemsLastSeen[p] = item.Type; //todo ID
			//todo traps etc.
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
				bool hasLOS = Player.Position.HasLOS(p, Map.Tiles);
				bool seen = Map.Seen[p];
				ColorGlyph currentGlyph = hasLOS? GetCachedAtMapPosition(p)
					: seen? GetLastSeenColorGlyph(p, true)
					: new ColorGlyph(' ', Color.White);
				ColorGlyph highlighted = Screen.GetHighlighted(currentGlyph, HighlightType.Targeting);
				DrawToMap(p.Y, p.X, highlighted);
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
			return GetDescriptionInternal(items, tileType, "You see ");
		}
		private string GetLastKnownDescriptionAtCell(Point p){
			List<string> items = new List<string>();
			if(false){ //todo, last known enemy position option
				items.Add("todo name");
			}
			ItemType itemType;
			if(itemsLastSeen.TryGetValue(p, out itemType)){
				//todo, ID?
				//todo, extra info?
				string itemExtra = "";
				items.Add(ScreenUIMain.Grammar.Get(Determinative.AAn, Names.Get(itemType), extraText: itemExtra));
			}
			TileType tileType = tilesLastSeen[p];
			//todo, check tile known status?
			//todo, features here
			//todo, traps, shrines, idols, etc.
			return GetDescriptionInternal(items, tileType, "You remember seeing ");
		}
		private string GetDescriptionInternal(List<string> items, TileType tileType, string initialText){
			string tileConnector = GetConnectingWordForTile(tileType);
			string tileName = ScreenUIMain.Grammar.Get(Determinative.AAn, Names.Get(tileType));
			if(items.Count > 0 && tileConnector == "and"){ // If it's "and", just handle it like the others:
				items.Add(tileName);
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(initialText);
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
			sb.Append(".");
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
