using System;
using Forays;

namespace ForaysUI.ScreenUI.Notifications{
    public class PlayerTurnStart : GameUIObject{
        public PlayerTurnStart(GameUniverse g) : base(g){}
        public void Handle(PlayerTurnEvent.NotifyTurnStart n){

        }
    }
}
