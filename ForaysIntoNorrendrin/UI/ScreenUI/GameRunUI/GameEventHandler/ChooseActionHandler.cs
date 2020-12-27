using System;
using System.Threading;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticInput;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs (contains the constructor)
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		private Dir8? walkDir;

		private void ChooseAction(PlayerTurnEvent e){
			do{
				GameRunUI.DrawGameUI(
					sidebar: DrawOption.Normal,
					messages: DrawOption.Normal,
					map: DrawOption.Normal,
					environmentalDesc: DrawOption.Normal,
					commands: DrawOption.Normal
				);
				//window update, set suspend if false...
				/*if(!Screen.Update()){
					GameUniverse.Suspend = true;
					return;
				}*/
				//
				//
				GameRunUI.SetCursorPositionOnMap(Player.Position.Y, Player.Position.X);
				if(walkDir != null){
					if(Input.KeyIsAvailable){
						Input.FlushInput();
						walkDir = null;
					}
					else{
						Point targetPoint = Player.Position.PointInDir(walkDir.Value);
						if(TileTypeAt(targetPoint) == TileType.Wall || CreatureAt(targetPoint) != null){ //todo
							walkDir = null;
						}
						else{
							if(!Screen.WindowUpdate()) Program.Quit();
							Thread.Sleep(10); //todo, make configurable
							e.ChosenAction = new WalkAction(Player, Player.Position.PointInDir(walkDir.Value));
							return;
						}
					}
				}
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.NumPad8:
						ChooseActionFromDirection(e, Dir8.N, shift);
						break;
					case ConsoleKey.NumPad6:
						ChooseActionFromDirection(e, Dir8.E, shift);
						break;
					case ConsoleKey.NumPad4:
						ChooseActionFromDirection(e, Dir8.W, shift);
						break;
					case ConsoleKey.NumPad2:
						ChooseActionFromDirection(e, Dir8.S, shift);
						break;
					case ConsoleKey.NumPad9:
						ChooseActionFromDirection(e, Dir8.NE, shift);
						break;
					case ConsoleKey.NumPad3:
						ChooseActionFromDirection(e, Dir8.SE, shift);
						break;
					case ConsoleKey.NumPad1:
						ChooseActionFromDirection(e, Dir8.SW, shift);
						break;
					case ConsoleKey.NumPad7:
						ChooseActionFromDirection(e, Dir8.NW, shift);
						break;
					case ConsoleKey.Escape:
						GameMenu.Open();
						break;
				}
			} while(e.ChosenAction == null);
		}
		private void ChooseActionFromDirection(PlayerTurnEvent e, Dir8 dir, bool shift){
			Point targetPoint = Player.Position.PointInDir(dir);
			Creature targetCreature = CreatureAt(targetPoint);
			if(targetCreature != null){
				e.ChosenAction = new MeleeAttackAction(Player, targetCreature);
				return;
			}
			// Check for wall sliding:
			bool wallSliding = false;
			if(TileTypeAt(targetPoint) == TileType.Wall){ //todo, terrain types etc.
				Point cwPoint = Player.Position.PointInDir(dir.Rotate(true));
				Point ccwPoint = Player.Position.PointInDir(dir.Rotate(false));
				if(TileTypeAt(cwPoint) != TileType.Wall && TileTypeAt(ccwPoint) == TileType.Wall) {
					wallSliding = true;
					dir = dir.Rotate(true);
					targetPoint = Player.Position.PointInDir(dir);
					targetCreature = CreatureAt(targetPoint);
				}
				else if(TileTypeAt(cwPoint) == TileType.Wall && TileTypeAt(ccwPoint) != TileType.Wall) {
					wallSliding = true;
					dir = dir.Rotate(false);
					targetPoint = Player.Position.PointInDir(dir);
					targetCreature = CreatureAt(targetPoint);
				}
			}
			if(targetCreature != null){
				e.ChosenAction = new MeleeAttackAction(Player, targetCreature);
			}
			else{
				e.ChosenAction = new WalkAction(Player, targetPoint);
				if(shift && !wallSliding) walkDir = dir; // Don't set walkDir for attacks or wall slides
			}
		}
	}
}
