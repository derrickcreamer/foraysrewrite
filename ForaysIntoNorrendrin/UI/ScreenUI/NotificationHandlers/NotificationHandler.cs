using System;
using Forays;
using GameComponents;

namespace ForaysUI.ScreenUI.Notifications{
    ///<summary>Base class for UI types that make heavy use of the GameUniverse</summary>
    public class GameUIObject : GameObject{
        public GameUIObject(GameUniverse g) : base(g){}

        public RNG ScreenRNG => ScreenUI.RNG;

        // To hopefully avoid any unintended usage:
        [Obsolete("CAUTION: This is the GameUniverse's RNG, not the UI's RNG.")]
        new public RNG R => base.R;
    }
    ///<summary>All notifications from the GameUniverse are sent here to be handled</summary>
    public class NotificationHandler : GameUIObject{
        private PlayerTurnStart PlayerTurnStart;
        private PlayerChooseAction PlayerChooseAction;
        private PlayerTurnEnd PlayerTurnEnd;

        public NotificationHandler(GameUniverse g) : base(g){
            PlayerTurnStart = new PlayerTurnStart(g);
            PlayerChooseAction = new PlayerChooseAction(g);
            PlayerTurnEnd = new PlayerTurnEnd(g);
        }

        public void ReceiveNotification(object o){
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
        }
    }
}
