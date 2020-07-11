using System;
using Forays;

namespace ForaysUI.ScreenUI.Notifications{
    public class PlayerTurnStart : GameUIObject{
        public PlayerTurnStart(GameUniverse g) : base(g){}
        public void Handle(PlayerTurnEvent.NotifyTurnStart n){
            //todo hold updates
            //draw:
            //...map
                //
            //...environmental desc
            //...messages
            //...status area
            //...additional UI
            //todo resume updates
            //window update, set suspend if false...

        }
    }
}
