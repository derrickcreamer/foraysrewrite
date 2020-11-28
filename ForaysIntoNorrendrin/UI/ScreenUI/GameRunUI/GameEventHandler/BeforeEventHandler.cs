using System;
using Forays;
using GameComponents;
using static ForaysUI.ScreenUI.StaticScreen;

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
				case MeleeHitEvent e:
					if(e.Creature == Player)
						Messages.Add("You hit the enemy. ");
					else
						Messages.Add("The enemy hits you. ");
					break;
				case MeleeMissEvent e:
					if(e.Creature == Player)
						Messages.Add("You miss the enemy. ");
					else
						Messages.Add("The enemy misses you. ");
					break;
				case DieEvent e:
					if(e.Creature == Player)
						Messages.Add("You die. ");
					else
						Messages.Add("The enemy dies. ");
					break;
			}
		}
	}
}
