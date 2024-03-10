using System;
using Forays;
using ForaysUI.ScreenUI.MapRendering;
using GameComponents;
using GrammarUtility;
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
		public MapUI MapUI => GameRunUI.MapUI;
		public MapRenderer MapRenderer => GameRunUI.MapRenderer;
		public EscapeMenu EscapeMenu => GameRunUI.EscapeMenu;
		public CharacterScreens CharacterScreens => GameRunUI.CharacterScreens;
		public GameEventHandler GameEventHandler => GameRunUI.GameEventHandler;
		public Grammar Grammar => ScreenUIMain.Grammar;
	}
	// GameRunUI is kind of like the UI equivalent to GameUniverse.
	public class GameRunUI : GameObject{
		public MessageBuffer Messages;
		public Sidebar Sidebar;
		public MapUI MapUI;
		public MapRenderer MapRenderer;
		public EscapeMenu EscapeMenu;
		public CharacterScreens CharacterScreens;
		public GameEventHandler GameEventHandler;

		public static int EnviromentalDescriptionRow;
		public static int CommandListRow;
		public static int ScreenEdgeDividerCol; // Used to keep these 2 columns blank
		public static int MidScreenDividerCol;

		public GameRunUI(GameUniverse g) : base(g){
			Messages = new MessageBuffer(this);
			Sidebar = new Sidebar(this);
			MapUI = new MapUI(this);
			MapRenderer = MapRenderer.Create(this);
			EscapeMenu = new EscapeMenu(this);
			CharacterScreens = new CharacterScreens(this);
			GameEventHandler = new GameEventHandler(this);
			UpdateSidebarOption(Option.IsSet(BoolOptionType.SidebarOnRight));
			UpdateMessagesOption(Option.IsSet(BoolOptionType.MessagesAtBottom));
		}
		public static void UpdateSidebarOption(bool sidebarOnRight){
			Sidebar.RowOffset = 0;
			if(sidebarOnRight){
				MapUI.ColOffset = 1;
				MessageBuffer.ColOffset = 1;
				Sidebar.ColOffset = MapUI.MapDisplayWidth + 2;
				MidScreenDividerCol = MapUI.MapDisplayWidth + 1;
				ScreenEdgeDividerCol = 0;
			}
			else{
				MapUI.ColOffset = 21;
				MessageBuffer.ColOffset = 21;
				Sidebar.ColOffset = 0;
				MidScreenDividerCol = Sidebar.Width;
				ScreenEdgeDividerCol = ScreenUIMain.Cols - 1;
			}
		}
		public static void UpdateMessagesOption(bool messagesAtBottom){
			if(messagesAtBottom){
				MapUI.RowOffset = 2;
				EnviromentalDescriptionRow = 1;
				CommandListRow = 0;
				MessageBuffer.RowOffset = MapUI.MapDisplayHeight + 2;
			}
			else{
				MapUI.RowOffset = 4;
				EnviromentalDescriptionRow = 4 + MapUI.MapDisplayHeight;
				CommandListRow = 5 + MapUI.MapDisplayHeight;
				MessageBuffer.RowOffset = 0;
			}
		}
		public void DrawGameUI(DrawOption sidebar, DrawOption messages, DrawOption environmentalDesc, DrawOption commands){
			Screen.HoldUpdates();
			Screen.Clear(0, MidScreenDividerCol, ScreenUIMain.Rows, 1);
			Screen.Clear(0, ScreenEdgeDividerCol, ScreenUIMain.Rows, 1);
			Sidebar.Draw(sidebar); //TODO NEXT: doesn't the sidebar need to get a list of visible stuff somehow? how?
			// WELLL, the old list was populated on every 'draw map' call, in the 'get color glyph' equivalent...
			// So I could guarantee no side effects and do that... but after discussion with RS the way to go is to
			// recalculate it every time the player moves and whenever anything else changes which could change visibility.
			if(messages != DrawOption.DoNotDraw){
				Messages.Print(false);
			}
			if(environmentalDesc != DrawOption.DoNotDraw){
				Color color = environmentalDesc == DrawOption.Darkened? Color.DarkEnvironmentDescription : Color.EnvironmentDescription;
				string envDesc = "You are in a maze of twisty little passages, all alike.".PadRight(MapUI.MapDisplayWidth);
				Screen.Write(EnviromentalDescriptionRow, MapUI.ColOffset, envDesc, color);
			}
			if(commands != DrawOption.DoNotDraw){
				Color commandColor = commands == DrawOption.Darkened? Color.DarkGray : Color.Cyan;
				Color textColor = commands == DrawOption.Darkened? Color.DarkGray : Color.Gray;
				string text = "[i]nventory    [e]quipment    Look around [Tab]    Actions [Enter]";
				Screen.Write(CommandListRow, MapUI.ColOffset, text, textColor);
				Screen.Write(CommandListRow, MapUI.ColOffset + 1, 'i', commandColor);
				Screen.Write(CommandListRow, MapUI.ColOffset + 16, 'e', commandColor);
				Screen.Write(CommandListRow, MapUI.ColOffset + 43, "Tab", commandColor);
				Screen.Write(CommandListRow, MapUI.ColOffset + 60, "Enter", commandColor);
				//todo, mouse buttons
			}
			Screen.ResumeUpdates();
		}
	}
	public enum DrawOption{ Normal, Darkened, DoNotDraw };
}
