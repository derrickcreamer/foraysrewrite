using System;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI.EventHandlers{
	///<summary>Base class for UI types that make heavy use of the GameUniverse</summary>
	public class GameUIObject : GameObject{
		public GameUIObject(GameUniverse g) : base(g){}

		public RNG ScreenRNG => ScreenUIMain.RNG;

		// To hopefully avoid any unintended usage:
		[Obsolete("CAUTION: This is the GameUniverse's RNG, not the UI's RNG.")]
		new public RNG R => base.R;
	}
	///<summary>todo</summary>
	public class GameEventHandler : GameUIObject{ //todo...unsure about how this will eventually be broken up and reorganized.

		public GameEventHandler(GameUniverse g) : base(g){
		}
		public void BeforeGameEvent(GameObject gameEvent){
			switch(gameEvent){
				case PlayerTurnEvent e:
				//
				//todo hold updates
				//draw:
				//...map
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
									color = Color.Cyan;
									break;
								case TileType.Staircase:
									ch = '>';
									color = Color.White;
									break;
							}

							Screen.Write(GameUniverse.MapHeight-1-i, j, ch, color);
						}
					}
					Screen.Write(GameUniverse.MapHeight-1-Player.Position.Value.Y, Player.Position.Value.X, '@', Color.White);
					//
				Screen.SetCursorPosition(Player.Position.Value.X, GameUniverse.MapHeight-1-Player.Position.Value.Y); //todo map offset etc.
				//...environmental desc
				//...messages (don't forget to flush message buffer)
				//...status area
				//...additional UI
				//todo resume updates
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
		public void AfterGameEvent(GameObject gameEvent, EventResult eventResult){
			// todo, print messages based on results here
		}
		public bool DecideCancel(GameObject action){
			//todo, for targeting:
			//if target is already specified, do nothing
			//else, prompt for target
			//return (target==null)
			//todo, for others:
			// if(e.IsBlockedByTerrain) { /*return true or whatever*/
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
