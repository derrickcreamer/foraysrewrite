using System;
using System.Collections.Generic;
using UtilityCollections;
using SunshineConsole;
using GameComponents;
using GameComponents.DirectionUtility;
using OpenTK.Graphics;
using System.Threading;
using OpenTK.Input;
using Forays;

namespace ForaysUI {
	class Program {
		static void Main(string[] args) {
			/*RNG r = new RNG(235634245);
			for(int j=0;j<1000;++j)
				r.GetNext(j+2);
			int a = r.GetNext(4);
			int b = r.GetNext(25);*/
			RunUI(); return;
			int i = 0;
			/*foreach(var x in EightDirections.Enumerate(false, true, true, Dir8.SW)) {
				var y = x;
			}
			Point p = new Point(0, 0);
			foreach(Point p2 in p.EnumeratePointsAtManhattanDistance(5, true)) {
				Point p3 = p2;
				if(++i >= 1000) break;
			}
			return;
			foreach(Point p4 in new Point(15, 15).EnumeratePointsByChebyshevDistance(false, true)) {
				if(++i >= 1000) break;
			}*/
			/*ConsoleWindow w = new ConsoleWindow(30, 30, "hmm");
			w.Write(15, 15, '@', OpenTK.Graphics.Color4.YellowGreen);
			foreach(var dir in Dir8.S.GetDirectionsInArc(2, false, true)) {
				Point p5 = new Point(15,15).PointInDir(dir);
				w.Write(30-p5.Y, p5.X, '#', OpenTK.Graphics.Color4.Azure);
				System.Threading.Thread.Sleep(1000);
				if(!w.WindowUpdate()) break;
			}
			System.Threading.Thread.Sleep(1000);
			foreach(var p7 in new Point(15,15).EnumeratePointsWithinChebyshevDistance(8, true, true)) {
				w.Write(30 - p7.Y, p7.X, '%', OpenTK.Graphics.Color4.RoyalBlue);
				if(!w.WindowUpdate()) break;
			}
			foreach(Point p4 in new Point(15, 15).EnumeratePointsByManhattanDistance(true, Dir4.W)) {
				w.Write(30-p4.Y, p4.X, '@', OpenTK.Graphics.Color4.Lime);
				System.Threading.Thread.Sleep(100);
				if(!w.WindowUpdate()) break;
			}*/
			//w.Write(0, 0, '!', OpenTK.Graphics.Color4.Azure);
			//w.WindowUpdate();
			//w.Exit();
		}
		const int wHeight = GameUniverse.MapHeight + 2;
		const int wWidth = GameUniverse.MapWidth;
		static GameUniverse g;
		static ConsoleWindow w;
		//static int turnsDeadCounter = 10;
		static Dir8? walkDir = null;
		static string lastMsg;

		static void RunUI() {
			g = new GameUniverse();
			g.InitializeNewGame();
			w = new ConsoleWindow(wHeight, wWidth, "Roguelike Rewrite Tech Demo");
			g.OnNotify += HandleNotifications;
			lastMsg = "Welcome!";
			while(true) {
				g.Run();
				if(g.GameOver) {
					if(w.IsExiting) break;
					WriteStatusString(w, "GAME OVER - PLAY AGAIN? (Y/N)");
					Key key = Key.Unknown;
					while(key != Key.Y && key != Key.N) {
						Thread.Sleep(100);
						if(!w.WindowUpdate()) break;
						key = w.GetKey();
					}
					if(key != Key.Y) break;
					else {
						g.OnNotify -= HandleNotifications;
						g = new GameUniverse();
						g.InitializeNewGame();
						g.OnNotify += HandleNotifications;
						lastMsg = "Welcome back!";
						continue;
					}
				}
				//WriteStatusString(w, "PAUSED");
				while(w.KeyPressed == false) {
					Thread.Sleep(200);
					WriteStatusString(w, "PAUSED: " + DateTime.Now.Second);
				}
			}
			w.Exit();
		}
		static void WriteStatusString(ConsoleWindow w, string s) {
			s = (s ?? "").PadRight(wWidth);
			w.Write(wHeight - 2, 0, s, Color4.Lime, Color4.DimGray);
			w.WindowUpdate();
		}
		static void HandleNotifications(object o) {
			switch(o) {
				/*case FireballEvent.NotifyExplosion n:
					Dictionary<Point, char> chs = new Dictionary<Point, char>();
					Dictionary<Point, Color4> colors = new Dictionary<Point, Color4>();
					w.HoldUpdates();
					//todo null check on target?
					foreach(Point p in n.Event.Target.Value.EnumeratePointsAtManhattanDistance(n.CurrentRadius, true)) {
						w.Write(19-p.Y, p.X, '&', Color4.OrangeRed);
					}
					w.ResumeUpdates();
					w.WindowUpdate();
					Thread.Sleep(100);
					break;*/
				case NotifyPrintMessage n:
					lastMsg = n.Message;
					break;
				case PlayerTurnEvent.NotifyTurnStart n:
					w.HoldUpdates();
					for(int i = 0; i < GameUniverse.MapHeight - 1; i++) {
						for(int j = 0; j < GameUniverse.MapWidth; j++) {
							char ch = ' ';
							Color4 color = Color4.DimGray;
							switch(g.Tiles[j, GameUniverse.MapHeight-1-i]) {
								case TileType.Floor:
									ch = '.';
									break;
								case TileType.Wall:
									ch = '#';
									break;
								case TileType.Water:
									ch = '~';
									color = Color4.Cyan;
									break;
								case TileType.Staircase:
									ch = '>';
									color = Color4.White;
									break;
							}

							w.Write(i, j, ch, color);
						}
					}
					//if(g.Player.Position != null)
						//w.Write(wHeight-3-g.Player.Position.Value.Y, g.Player.Position.Value.X, '@', Color4.DarkCyan);
					WriteStatusString(w, lastMsg);
					w.Write(wHeight - 1, 0, (g.Q.CurrentTick / 120).ToString().PadRight(wWidth), Color4.DarkGray);
					w.Write(wHeight - 1, wWidth/2 - 2, $"HP: {g.Player.CurHP}",Color4.Pink);
					w.Write(wHeight - 1, wWidth - 5, $"D: {g.CurrentDepth}", Color4.CadetBlue);
					foreach(var c in g.Creatures) {
						char ch = 'C';
						if(c == g.Player) ch = '@';
						Color4 color = Color4.DarkCyan;
						/*switch(c.State) {
							case CreatureState.Angry:
								color = Color4.Red;
								break;
							case CreatureState.Crazy:
								color = Color4.Yellow;
								break;
							case CreatureState.Dead:
								ch = '%';
								color = Color4.Gray;
								break;
							case CreatureState.Normal:
							default:
								color = Color4.White;
								break;
						}*/
						w.Write(wHeight-3-c.Position.Value.Y, c.Position.Value.X, ch, color);
					}
					w.ResumeUpdates();
					if(!w.WindowUpdate()) g.Suspend = true;
					/*if(g.Player.State == CreatureState.Dead) {
						if(turnsDeadCounter-- > 0) {
							//schedule another turn for the player so we can keep updating for a little while:
							g.Q.Schedule(new PlayerTurnEvent(g), 120, null);
							Thread.Sleep(400);
						}
						else {
							g.Suspend = true;
						}
					}*/
					break;
				case PlayerTurnEvent.NotifyChooseAction n:
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
					}
				case PlayerTurnEvent.NotifyTurnEnd n:
					switch(n.Event.ChosenAction) {
						case WalkAction a:
							if((n.ActionResult as PassFailResult)?.Succeeded == true)
								if(g.Player.TileTypeAt(g.Player.Position.Value) == TileType.Water)
									lastMsg = "Splash!";
								else
									lastMsg = "";
							else if((n.ActionResult as PassFailResult)?.Succeeded == false)
								lastMsg = "You walk into the wall.";
							break;
						case DescendAction a:
							if((n.ActionResult as PassFailResult)?.Succeeded == false)
								lastMsg = "There are no stairs here.";
							break;
					}
					break;
				case PlayerCancelDecider.NotifyDecide n:
					n.CancelAction = DecideCancel(n.Action);
					break;
			}
		}
		static bool DecideCancel(object action) {
			switch(action) {
				// More events would go here eventually, but right now it's just for targeting:
				/*case FireballEvent e:
					if(e.Target != null) return false;
					WriteStatusString(w, "Fireball - choose a direction ");
					{
						bool done = false;
						while(!done) {
							if(w.KeyPressed) {
								Dir4? dir = null;
								switch(w.GetKey()) {
									case Key.Up:
									case Key.W:
										dir = Dir4.N;
										break;
									case Key.Down:
									case Key.S:
										dir = Dir4.S;
										break;
									case Key.Left:
									case Key.A:
										dir = Dir4.W;
										break;
									case Key.Right:
									case Key.D:
										dir = Dir4.E;
										break;
									case Key.Escape:
									case Key.Q:
										done = true;
										break;
								}
								if(dir != null) {
									Point current = e.Creature.Position.Value;
									while(true) {
										Point next = current.PointInDir(dir.Value);
										if(g.Map.Creatures[next] != null && g.Map.Creatures[next] != e.Creature) {
											current = next;
											break;
										}
										if(g.Map.Creatures.InBounds(next)) current = next;
										else break;
									}
									e.Target = current;
									done = true;
								}
							}
							if(!w.WindowUpdate()) {
								g.Suspend = true;
								return true;
							}
							else Thread.Sleep(10);
						}
					}
					WriteStatusString(w, "");
					return e.Target == null;*/
				case WalkAction e:
					if(e.IsBlockedByTerrain) {
						lastMsg = "There's a wall in the way";
						return true;
					}
					return false; //todo...not yet sure whether this stays here, or is handled in the input loop.
			}
			return false;
		}
	}
}
