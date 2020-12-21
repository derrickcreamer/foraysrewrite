using System;
using Forays;
using GameComponents;
using static ForaysUI.ScreenUI.StaticScreen;

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
		public GameMenu GameMenu => GameRunUI.GameMenu;
	}
	// GameRunUI is kind of like the UI equivalent to GameUniverse.
	public class GameRunUI : GameObject{
		public MessageBuffer Messages;
		public Sidebar Sidebar;
		public GameMenu GameMenu;

		// todo, add option to change layout, which'll change these values, plus the matching ones in Messages and Sidebar:
		public int MapRowOffset = 3;
		public int MapColOffset = 21;
		public int EnviromentalFlavorStartRow = 3 + GameUniverse.MapHeight; //todo, more here?

		public GameRunUI(GameUniverse g) : base(g){
			Messages = new MessageBuffer(this);
			Sidebar = new Sidebar(this);
			GameMenu = new GameMenu(this);
		}
		public void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset, glyphIndex, color);
		public void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset);

		public void DrawGameUI(DrawOption map, DrawOption messages, DrawOption environmentalDesc,
			DrawOption sidebar, DrawOption bottomUI)
		{
			Screen.HoldUpdates();
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
			if(messages != DrawOption.DoNotDraw){
				Messages.Print(false);
			}
			//todo
			//...environmental desc
			//...status area
			//...additional UI

			Screen.ResumeUpdates();
		}
	}
	public enum DrawOption{ Normal, Darkened, DoNotDraw };
}
