using System;
using Forays;
using GameComponents;
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
			ConsoleKeyInfo key = Input.ReadKey();
			switch(key.Key){
				case ConsoleKey.UpArrow:
				case ConsoleKey.NumPad8:
					ChooseActionFromDirection(e, Dir8.N);
					break;
				case ConsoleKey.RightArrow:
				case ConsoleKey.NumPad6:
					ChooseActionFromDirection(e, Dir8.E);
					break;
				case ConsoleKey.LeftArrow:
				case ConsoleKey.NumPad4:
					ChooseActionFromDirection(e, Dir8.W);
					break;
				case ConsoleKey.DownArrow:
				case ConsoleKey.NumPad2:
					ChooseActionFromDirection(e, Dir8.S);
					break;
				case ConsoleKey.NumPad9:
					ChooseActionFromDirection(e, Dir8.NE);
					break;
				case ConsoleKey.NumPad3:
					ChooseActionFromDirection(e, Dir8.SE);
					break;
				case ConsoleKey.NumPad1:
					ChooseActionFromDirection(e, Dir8.SW);
					break;
				case ConsoleKey.NumPad7:
					ChooseActionFromDirection(e, Dir8.NW);
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
		private void ChooseActionFromDirection(PlayerTurnEvent e, Dir8 dir){
			Point targetPoint = Player.Position.PointInDir(dir);
			// Check for wall sliding:
			if(CreatureAt(targetPoint) == null && TileTypeAt(targetPoint) == TileType.Wall){ //todo
				Point cwPoint = Player.Position.PointInDir(dir.Rotate(true));
				Point ccwPoint = Player.Position.PointInDir(dir.Rotate(false));
				if(TileTypeAt(cwPoint) != TileType.Wall && TileTypeAt(ccwPoint) == TileType.Wall) {
					targetPoint = Player.Position.PointInDir(dir.Rotate(true));
				}
				else if(TileTypeAt(cwPoint) == TileType.Wall && TileTypeAt(ccwPoint) != TileType.Wall) {
					targetPoint = Player.Position.PointInDir(dir.Rotate(false));
				}
			}
			Creature targetCreature = CreatureAt(targetPoint);
			if(CreatureAt(targetPoint) != null){
				e.ChosenAction = new MeleeAttackAction(Player, targetCreature);
			}
			else{
				e.ChosenAction = new WalkAction(Player, targetPoint);
			}
		}
	}
}
