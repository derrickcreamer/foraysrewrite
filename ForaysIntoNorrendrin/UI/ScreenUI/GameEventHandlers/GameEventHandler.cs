using System;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI.EventHandlers{
	//todo, it seems like this set of files (GameEventHandlers + MessageBuffer + Sidebar, maybe inventory menu stuff?) are the only ones that
	//    interact with the GameUniverse. The others are either more general (screen drawing, input) or deal with the other parts of the UI (main menu, help).
	//    Therefore, maybe this folder should get renamed, and maybe those files should be grouped in here. 'GameRunUI'?
	///<summary>todo</summary>
	public class GameEventHandler : GameUIObject{ //todo...unsure about how this will eventually be broken up and reorganized.
		public MessageBuffer Messages;
		public Sidebar Sidebar;

		// todo, add option to change layout, which'll change these values, plus the matching ones in Messages and Sidebar:
		public int MapRowOffset = 3;
		public int MapColOffset = 21;
		public int EnviromentalFlavorStartRow = 3 + GameUniverse.MapHeight; //todo, more here?

		public GameEventHandler(GameUniverse g) : base(g){
			Messages = new MessageBuffer(g);
			Sidebar = new Sidebar(g);
		}
		public void BeforeGameEvent(GameObject gameEvent){
			switch(gameEvent){
				case PlayerTurnEvent e:
					Screen.HoldUpdates();
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
					DrawToMap(Player.Position.Value.Y, Player.Position.Value.X, '@', Color.White);
					SetCursorPositionOnMap(Player.Position.Value.Y, Player.Position.Value.X);
				//...environmental desc
				Messages.Print(false);
				//...status area
				//...additional UI

				Screen.ResumeUpdates();

				//window update, set suspend if false...
				/*if(!Screen.Update()){
					GameUniverse.Suspend = true;
					return;
				}*/
				//
				//
				ChooseAction(e);
				break;
			}
		}
		private void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset, glyphIndex, color);
		private void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+MapRowOffset, col+MapColOffset);
		public void AfterGameEvent(GameObject gameEvent, EventResult eventResult){
			// todo, print messages based on results here
			switch(gameEvent){
				case WalkAction e:
					if(eventResult.Canceled && e.IsBlockedByTerrain){ //todo, this is wrong. Canceled events don't reach this point. Print terrain messages here instead.
					}
					if(e.Creature == Player && Player.Position.Value == e.Destination){
						string msg = null;
						switch(TileTypeAt(Player.Position.Value)){ //todo, method here instead, so I can return instead of breaking?
							case TileType.Staircase:
								msg = "The stairway leads downward - press > to descend. "; //todo
								break;
						}
						if(msg != null) Messages.Add(msg);
					}
					break;
				case AttackAction e:
					if(true){ //todo check actor, target, check result, etc.
string longMsg = "Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h. Aaaa bbbb cccc dddd eeee f g h.";
						Messages.AddIfEitherVisible("You strike! " + longMsg, e.Creature, e.Target);
					}
					break;
			}
		}
		public bool DecideCancel(GameObject action){
			//todo, for targeting:
			//if target is already specified, do nothing
			//else, prompt for target
			//return (target==null)
			switch(action){
				case WalkAction e:
					if(e.IsBlockedByTerrain){
						//todo, get terrain name
						Messages.Add("There is a wall in the way. ");
						return true;
					}
					break;
			}
			return false;//todo
		}
		public void OnStatusStart(Creature creature, Status status){
			//todo
		}
		public void OnStatusEnd(Creature creature, Status status){
			//todo
		}
		private void ChooseAction(PlayerTurnEvent e){
				ConsoleKeyInfo key = Input.ReadKey(); //todo, just wait for a keypress here for now
				switch(key.Key){
					case ConsoleKey.UpArrow:
					case ConsoleKey.NumPad8:
					Creature c = CreatureAt(Player.Position.Value.PointInDir(Dir8.N));
					if(c != null)
						e.ChosenAction = new AttackAction(Player, c);
					else
						e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.N));
					break;
					case ConsoleKey.RightArrow:
					case ConsoleKey.NumPad6:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.E));
					break;
					case ConsoleKey.LeftArrow:
					case ConsoleKey.NumPad4:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.W));
					break;
					case ConsoleKey.DownArrow:
					case ConsoleKey.NumPad2:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.S));
					break;
					case ConsoleKey.NumPad9:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.NE));
					break;
					case ConsoleKey.NumPad3:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.SE));
					break;
					case ConsoleKey.NumPad1:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.SW));
					break;
					case ConsoleKey.NumPad7:
					e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.NW));
					break;
				}
			// todo, old code pasted:
			/*
					while(true) {
						if(walkDir != null) {
							if(w.KeyPressed) {
								w.GetKey();
								walkDir = null;
							}
							else {
								n.Event.ChosenAction = new WalkAction(g.Player, g.Player.Position.Value.PointInDir(walkDir.Value));
								Thread.Sleep(30);
								return;
							}
						}
						if(w.KeyPressed) {
							Dir8? dir = null;
							switch(w.GetKey()) {
								case Key.Up:
								case Key.W:
									dir = Dir8.N;
									break;
								case Key.Down:
								case Key.X:
									dir = Dir8.S;
									break;
								case Key.Left:
								case Key.A:
									dir = Dir8.W;
									break;
								case Key.Right:
								case Key.D:
									dir = Dir8.E;
									break;
								case Key.Q:
									dir = Dir8.NW;
									break;
								case Key.E:
									dir = Dir8.NE;
									break;
								case Key.Z:
									dir = Dir8.SW;
									break;
								case Key.C:
									dir = Dir8.SE;
									break;
								case Key.N:
									n.Event.ChosenAction = new DescendAction(g.Player);
									return;
									//n.Event.ChosenAction = new FireballEvent(g.Player, null);
								case Key.Escape:
									g.Suspend = true;
									return;
							}
							if(dir != null) {
								Point targetPoint = g.Player.Position.Value.PointInDir(dir.Value);
								if(g.Creatures[targetPoint] == null) {
									if(g.Player.TileTypeAt(targetPoint) == TileType.Wall) { // Check for wall sliding:
										Point cwPoint = g.Player.Position.Value.PointInDir(dir.Value.Rotate(true));
										Point ccwPoint = g.Player.Position.Value.PointInDir(dir.Value.Rotate(false));
										if(g.Player.TileTypeAt(cwPoint) != TileType.Wall && g.Player.TileTypeAt(ccwPoint) == TileType.Wall) {
											dir = dir.Value.Rotate(true);
										}
										else if(g.Player.TileTypeAt(cwPoint) == TileType.Wall && g.Player.TileTypeAt(ccwPoint) != TileType.Wall) {
											dir = dir.Value.Rotate(false);
										}
									}
									n.Event.ChosenAction = new WalkAction(g.Player, g.Player.Position.Value.PointInDir(dir.Value));
									if(w.KeyIsDown(Key.ShiftLeft) || w.KeyIsDown(Key.ShiftRight)) walkDir = dir;
								}
								else {
									n.Event.ChosenAction = new AttackAction(g.Player, g.Creatures[targetPoint]);
									lastMsg = "You strike!";
								}
								return;
							}
						}
						if(!w.WindowUpdate()) {
							g.Suspend = true;
							g.GameOver = true;
							return;
						}
						else Thread.Sleep(10);
			*/
			//
		}
	}
}
