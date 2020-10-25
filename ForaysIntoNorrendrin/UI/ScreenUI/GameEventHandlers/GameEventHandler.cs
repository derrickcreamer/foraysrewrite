using System;
using Forays;
using GameComponents;

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
    public class GameEventHandler : GameUIObject{

        public GameEventHandler(GameUniverse g) : base(g){
        }
        // old code from old class, for reference. delete later, todo:
        /*public void ReceiveNotification(object o){
            switch(o){
                case NotifyPrintMessage n:
                //just buffer?
                break;
                case PlayerTurnEvent.NotifyTurnStart n:
                    PlayerTurnStart.Handle(n);
                //hmm, so this gets its own file...
                //it uses Screen...and GameUniverse consts and state data...
                //probably tells the message buffer to print...
                // (could check old code for more, but that's probably most of it)
                break;
                case PlayerTurnEvent.NotifyChooseAction n:
                    PlayerChooseAction.Handle(n);
                //needs to refer to some kind of state for walkDir etc.
                //uses Input
                //gameuniverse
                break;
                case PlayerTurnEvent.NotifyTurnEnd n:
                    PlayerTurnEnd.Handle(n);
                //might print messages for failed actions?
                //gu
                break;
                case PlayerCancelDecider.NotifyDecide n:
                //another switch probably... handles targeting as well as regular cancels
                break;
            }
        }*/
        public void BeforeGameEvent(GameObject gameEvent){
            switch(gameEvent){
                case PlayerTurnEvent e:
                //
                //todo hold updates
                //draw:
                //...map
                    //
                //...environmental desc
                //...messages (don't forget to flush message buffer)
                //...status area
                //...additional UI
                //todo resume updates
                //window update, set suspend if false...
                //
                //and then CHOOSE ACTION HERE. Set e.ChosenAction!
                break;
            }
        }
        public void AfterGameEvent(GameObject gameEvent, EventResult eventResult){
        }
    }
}
