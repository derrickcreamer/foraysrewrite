using System;
using Forays;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// GameEventHandler.cs (contains the constructors and helpers)
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
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
