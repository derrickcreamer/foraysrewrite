using System;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	///<summary>Base class for UI types that make heavy use of the GameUniverse</summary>
	public class GameUIObject : GameObject{
		public GameRunUI GameRunUI;
		public GameUIObject(GameRunUI ui) : base(ui.GameUniverse){ GameRunUI = ui; }

		public RNG ScreenRNG => ScreenUIMain.RNG;

		// To hopefully avoid any unintended usage:
		[Obsolete("CAUTION: This is the GameUniverse's RNG, not the UI's RNG.")]
		new public RNG R => base.R;

		public MessageBuffer Messages => GameRunUI.Messages;
		public Sidebar Sidebar => GameRunUI.Sidebar;
		public EscapeMenu EscapeMenu => GameRunUI.EscapeMenu;
		public CharacterScreens CharacterScreens => GameRunUI.CharacterScreens;
	}
	// GameRunUI is kind of like the UI equivalent to GameUniverse.
	public class GameRunUI : GameObject{
		public MessageBuffer Messages;
		public Sidebar Sidebar;
		public EscapeMenu EscapeMenu;
		public CharacterScreens CharacterScreens;

		// Track display height/width separately to make it easier to change later:
		public const int MapDisplayHeight = GameUniverse.MapHeight;
		public const int MapDisplayWidth = GameUniverse.MapWidth;
		// todo, add option to change layout, which'll change these values, plus the matching ones in Messages and Sidebar:
		public int MapRowOffset = 4;
		public int MapColOffset = 21;
		public int EnviromentalDescriptionRow = 4 + MapDisplayHeight; //todo, more here?
		public int CommandListRow = 5 + MapDisplayHeight;

		public GameRunUI(GameUniverse g) : base(g){
			Messages = new MessageBuffer(this);
			Sidebar = new Sidebar(this);
			EscapeMenu = new EscapeMenu(this);
			CharacterScreens = new CharacterScreens(this);
		}
		public void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset, glyphIndex, color);
		public void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset);

		public void DrawGameUI(DrawOption sidebar, DrawOption messages, DrawOption map,
			DrawOption environmentalDesc, DrawOption commands)
		{
			Screen.HoldUpdates();
			Sidebar.Draw(sidebar);
			if(messages != DrawOption.DoNotDraw){
				Messages.Print(false);
			}
			if(map != DrawOption.DoNotDraw){
				for(int i = 0; i < GameUniverse.MapHeight; i++) {
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

						DrawToMap(i, j, ch, color);
					}
				}
				DrawToMap(Player.Position.Y, Player.Position.X, '@', Color.White);
			}
			if(environmentalDesc != DrawOption.DoNotDraw){
				Color color = environmentalDesc == DrawOption.Darkened? Color.DarkEnvironmentDescription : Color.EnvironmentDescription;
				string envDesc = "You are in a maze of twisty little passages, all alike.".PadRight(MapDisplayWidth);
				Screen.Write(EnviromentalDescriptionRow, MapColOffset, envDesc, color);
			}
			if(commands != DrawOption.DoNotDraw){
				Color commandColor = commands == DrawOption.Darkened? Color.DarkGray : Color.Cyan;
				Color textColor = commands == DrawOption.Darkened? Color.DarkGray : Color.Gray;
				string text = "[i]nventory    [e]quipment    Look around [Tab]    Actions [Enter]";
				Screen.Write(CommandListRow, MapColOffset, text, textColor);
				Screen.Write(CommandListRow, MapColOffset + 1, 'i', commandColor);
				Screen.Write(CommandListRow, MapColOffset + 16, 'e', commandColor);
				Screen.Write(CommandListRow, MapColOffset + 43, "Tab", commandColor);
				Screen.Write(CommandListRow, MapColOffset + 60, "Enter", commandColor);
				//todo, mouse buttons
			}
			Screen.ResumeUpdates();
		}
		public void LookMode(PlayerTurnEvent e){
			bool travelMode = false;
			Point p = Player.Position; //todo
			while(true){
				Screen.HoldUpdates();
				Screen.Clear(0, MapColOffset, 4, GameRunUI.MapDisplayWidth);
				Screen.Clear(EnviromentalDescriptionRow, MapColOffset, 2, GameRunUI.MapDisplayWidth);
				DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.DoNotDraw,
					map: DrawOption.Normal,
					environmentalDesc: DrawOption.DoNotDraw,
					commands: DrawOption.DoNotDraw
				);
				string moveCursorMsg = travelMode? "Travel mode -- Move cursor to choose destination." : "Move cursor to look around.";
				Screen.Write(0, MapColOffset, moveCursorMsg);
				Screen.Write(2, MapColOffset, "[Tab] to cycle between interesting targets          [m]ore details");
				Screen.Write(2, MapColOffset + 1, "Tab", Color.Cyan);
				Screen.Write(2, MapColOffset + 53, 'm', Color.Cyan);
				if(travelMode){
					Screen.Write(3, MapColOffset, "[x] to confirm destination           [Space] to cancel travel mode");
					Screen.Write(3, MapColOffset + 1, 'x', Color.Cyan);
					Screen.Write(3, MapColOffset + 38, "Space", Color.Cyan);
				}
				else{
					Screen.Write(3, MapColOffset, "[x] to enter travel mode");
					Screen.Write(3, MapColOffset + 1, 'x', Color.Cyan);
				}
				//todo, show highlight, and path if travel mode
				string lookDescription = "You see an unaware goblin on a staircase."; //todo
				//todo, handle 2-line wrap around
				Screen.Write(EnviromentalDescriptionRow, MapColOffset, lookDescription, Color.Green);
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
						//todo movement keys
						default:
							break;
					}
				}
			}
		}
	}
	public enum DrawOption{ Normal, Darkened, DoNotDraw };
}
