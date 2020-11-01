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
        public void BeforeGameEvent(GameObject gameEvent){ //todo, fix tabs/spaces in the whole project
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

							Screen.Write(i, j, ch, color);
						}
					}
                    //
                //...environmental desc
                //...messages (don't forget to flush message buffer)
                //...status area
                //...additional UI
                //todo resume updates
                //window update, set suspend if false...
                if(!Screen.Update()){
                    GameUniverse.Suspend = true;
                    return;
                }
                //
                //
                //and then CHOOSE ACTION HERE. Set e.ChosenAction!
                ConsoleKeyInfo key = Input.ReadKey(); //todo, just wait for a keypress here for now
                if(key.Key == ConsoleKey.W){
                    e.ChosenAction = new WalkAction(Player, Player.Position.Value.PointInDir(Dir8.N));
                }
                break;
            }
        }
        public void AfterGameEvent(GameObject gameEvent, EventResult eventResult){
        }
        public bool DecideCancel(GameObject action){
            return false;//todo
        }
        public void OnStatusStart(Creature creature, Status status){
            //todo
        }
        public void OnStatusEnd(Creature creature, Status status){
            //todo
        }
    }
}
