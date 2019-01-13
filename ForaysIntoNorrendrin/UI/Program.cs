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
			RNG r = new RNG(235634245);
			for(int j=0;j<1000;++j)
				r.GetNext(j+2);
			int a = r.GetNext(4);
			int b = r.GetNext(25);
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
			ConsoleWindow w = new ConsoleWindow(30, 30, "hmm");
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
			}
			//w.Write(0, 0, '!', OpenTK.Graphics.Color4.Azure);
			//w.WindowUpdate();
			w.Exit();
		}
		static GameUniverse g;
		static ConsoleWindow w;
		static int turnsDeadCounter = 10;
		static Dir4? walkDir = null;

		static void RunUI() {
			g = new GameUniverse();
			w = new ConsoleWindow(21, 30, "Roguelike Rewrite Tech Demo");
			g.OnNotify += HandleNotifications;
			g.Run();
		}
		static void WriteStatusString(ConsoleWindow w, string s) {
			s = s.PadRight(30);
			w.Write(20, 0, s, Color4.Lime);
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
				case PlayerTurnEvent.NotifyTurnStart n:
					w.HoldUpdates();
					for(int i = 0; i < 20; i++) {
						for(int j = 0; j < 30; j++) {
							w.Write(i, j, ' ', Color4.Black);
						}
					}
					/*foreach(var c in g.Map.Creatures) {
						char ch = 'C';
						if(c == g.Player) ch = '@';
						Color4 color = Color4.DarkCyan;
						/ *switch(c.State) {
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
						}* /
						w.Write(19-c.Position.Value.Y, c.Position.Value.X, ch, color);
					}*/
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
								n.Event.ChosenAction = new WalkEvent(g.Player, g.Player.Position.Value.PointInDir(walkDir.Value));
								Thread.Sleep(30);
								return;
							}
						}
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
								case Key.M:
									//n.Event.ChosenAction = new FireballEvent(g.Player, null);
									return;
							}
							if(dir != null) {
								n.Event.ChosenAction = new WalkEvent(g.Player, g.Player.Position.Value.PointInDir(dir.Value));
								if(w.KeyIsDown(Key.ShiftLeft) || w.KeyIsDown(Key.ShiftRight)) walkDir = dir;
								return;
							}
						}
						if(!w.WindowUpdate()) {
							g.Suspend = true;
							return;
						}
						else Thread.Sleep(10);
					}
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
			}
			return false;
		}
	}
}
