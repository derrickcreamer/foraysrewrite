using System;
using GameComponents;

namespace Forays {
	public abstract class ItemEvent : Event<SimpleEvent.NullResult> {  //todo, not sure what to return here, so null for now.
		public virtual Item Item { get; set; }
		public ItemEvent(Item item) : base(item.GameUniverse) { this.Item = item; }
	}
    public class PotionEffectEvent : ItemEvent {
        public Creature User {get;set;}
        public PotionEffectEvent(Item item, Creature user) : base(item){ User = user; }
        protected override SimpleEvent.NullResult ExecuteEvent(){
            //todo, invalid if not potion? and if no user.
            if(Item == null || User == null) throw new InvalidOperationException(""); //todo
            switch(Item.Type){
                case ItemType.PotionOfHealing:
                {
                    User.CurrentHealth = User.MaxHealth;
                }
                break;
                case ItemType.PotionOfRegeneration:
                {
                    //User.ApplyStatus(Status.Regenerating, Turns(100)); //todo, wait, Regenerating is a Counter, isn't it?
                }
                break;
                case ItemType.PotionOfStoneform:
                {
                    User.ApplyStatus(Status.Stoneform, Turns(20 + R.Roll(2, 20)));
                }
                break;
                case ItemType.PotionOfVampirism:
                {
                    User.ApplyStatus(Status.PseudoVampiric, Turns(20 + R.Roll(2, 20)));
                }
                break;
                case ItemType.PotionOfBrutishStrength:
                {
                    User.ApplyStatus(Status.BrutishStrength, Turns(16 + R.Roll(3, 6)));
                }
                break;
                case ItemType.PotionOfRoots:
                {
                    User.ApplyStatus(Status.Roots, Turns(20 + R.Roll(20)));
                    //todo, call the appropriate event here to check whether the user just landed on a trap.
                }
                break;
                case ItemType.PotionOfHaste:
                {
                    User.ApplyStatus(Status.Hasted, Turns(10 + R.Roll(2, 10)));
                }
                break;
                case ItemType.PotionOfSilence:
                {
                    User.ApplyStatus(Status.SilenceAura, Turns(20 + R.Roll(2, 20)));
                }
                break;
                case ItemType.PotionOfCloaking:
                {
                    User.ApplyStatus(Status.ShadowCloak, Turns(30 + R.Roll(2, 20)));
                }
                break;
                case ItemType.PotionOfMysticMind:
                {
                    User.ApplyStatus(Status.MysticMind, Turns(60 + R.Roll(2, 20)));
                }
                break;
                default: throw new InvalidOperationException("Not a potion type: " + Item.Type.ToString());
            }
            return null;
        }
    }
    public class ScrollEffectEvent : ItemEvent {
        public Creature User {get;set;}
        public ScrollEffectEvent(Item item, Creature user) : base(item){ User = user; }
        protected override SimpleEvent.NullResult ExecuteEvent(){
            //todo, invalid if not potion? and if no user.
            if(Item == null || User == null) throw new InvalidOperationException(""); //todo
            switch(Item.Type){
                case ItemType.ScrollOfBlinking:
                {
                    //todo - call a Blink effect here?
                }
                break;
                case ItemType.ScrollOfPassage:
                {
                    //todo, call Passage effect here
                }
                break;
                case ItemType.ScrollOfTime:
                {
                    //todo, make sure fire event doesn't tick here... other stuff also doesn't tick, right?
                    //todo...CurrentTick is readonly...how do I set this?
                }
                break;
                case ItemType.ScrollOfKnowledge:
                {
                    if(User != Player) break;
                    /* todo...this one needs to:
                    find hidden things and remove them from any 'check for hidden' scheduled event...
                    mark non-floors as seen... mark things revealedByLight where applicable, including items on the ground...
                    ID all items in inventory, including wand charges
                    */
                }
                break;
                case ItemType.ScrollOfSunlight:
                {
                    Map.MagicalLightState = MagicalLightState.MagicalLight;
                    //todo...will I still use a 'return to normal lighting' event for this?
                    // (this one uses a call to KillEvents, so it might be useful to track it somewhere)
                }
                break;
                case ItemType.ScrollOfDarkness:
                {
                    //todo see above
                }
                break;
                case ItemType.ScrollOfRenewal:
                {
                    //todo...This one presents an interesting question.
                    // The UI might care whether anything was repaired, whether slime or oil was removed,
                    //   and whether one, or more than one wand was recharged.
                    //
                    // So, what's the best way to communicate that?
                    //   It could be done through the Results returned from a GameEvent, but it seems like
                    //   those return values should perhaps only be things that matter to the game universe.
                    //   Or, it could be done through notifications. That would require some new notification types
                    //   that are specific to the scroll of renewal, though -- or at LEAST specific to 'wand recharged',
                    //   'equipment repaired', and so on...
                    //
                    // Maybe *that* is a decent idea -- making the notification more general when possible:
                    //   like "game thing that isn't its own GameEvent happened". Well, it'll work here, anyway!
                    // So actually do this:
                    // check all statuses to repair, repair them, and note whether any happened.
                    // remove slime & oil. No need to note because hemlock handles that.
                    // Check for wands and recharge them, and note this.
                    // Notify for any repair and any recharge -- 1 notification each. Done.
                }
                break;
                case ItemType.ScrollOfCalling:
                {
                    //find target (differs if reader is NOT player)
                    //find cell in dir
                    //...or nearest valid one...
                    //call MoveEvent
                    //if none called, set the IDed flag to false
                }
                break;
                case ItemType.ScrollOfTrapClearing:
                {
                    //array of 5 lists, just like before
                    //go by distance
                    //use Concatenate instead of adding them all to a single list
                    //does this need a notify here?
                    //then, actually set off the traps, or set IDed to false if no traps.
                }
                break;
                case ItemType.ScrollOfEnchantment:
                {
                    //todo
                }
                break;
                case ItemType.ScrollOfThunderclap:
                {
                    //go out to distance 12...
                    //note creatures, and call the 'break fragile features' method or equivalent.
                    //then apply damage & stun
                    //make noise
                }
                break;
                case ItemType.ScrollOfFireRing:
                {
                    //determine cells (all within distance 3, minus corners and distance 1)
                    //add fire to all cells
                    //IDed=false if there were no cells
                }
                break;
                case ItemType.ScrollOfRage:
                {
                    //find all non-User creatures within dist 12 with LOS
                    //apply Enraged, duration 10-17 turns
                    //notify list of affected?
                }
                break;
                default: throw new InvalidOperationException("Not a scroll type: " + Item.Type.ToString());
            }
            return null;
        }
    }
    public class OrbBreakEvent : ItemEvent {
        public Point Position {get;set;}
        public OrbBreakEvent(Item item, Point position) : base(item){ Position = position; }
        protected override SimpleEvent.NullResult ExecuteEvent(){
            //todo, invalid if not orb? and if OOB?
            if(Item == null || false) throw new InvalidOperationException(""); //todo
            switch(Item.Type){
                case ItemType.OrbOfFreezing:
                {
                    //get all cells within distance 2 of position, that have LOE
                    //for each cell in a random order(?),
                    //  apply a Cold effect to that cell
                    //  and then apply Freezing to any creature in that cell.
                }
                break;
                case ItemType.OrbOfFlames:
                {
                    //dist 2, LOE
                    //for each cell, if tile in that cell is passable, add fire
                    //otherwise, apply a Fire effect to the cell.
                }
                break;
                case ItemType.OrbOfFog:
                {
                    //
                }
                break;
                case ItemType.OrbOfDetonation:
                {
                }
                break;
                case ItemType.OrbOfBreaching:
                {
                }
                break;
                case ItemType.OrbOfShielding:
                {
                }
                break;
                case ItemType.OrbOfTeleportal:
                {
                }
                break;
                case ItemType.OrbOfPain:
                {
                }
                break;
                case ItemType.OrbOfConfusion:
                {
                }
                break;
                case ItemType.OrbOfBlades:
                {
                }
                break;
               default: throw new InvalidOperationException("Not an orb type: " + Item.Type.ToString());
            }
            return null;
        }
    }
    public class WandEffectEvent : ItemEvent {
        public Point SourcePosition {get;set;}
        public Point TargetPosition {get;set;}
        public WandEffectEvent(Item item, Point source, Point target) : base(item){
            SourcePosition = source;
            TargetPosition = target;
        }
        protected override SimpleEvent.NullResult ExecuteEvent(){
            //todo, invalid if not wand? and if OOB?
            if(Item == null || false || false) throw new InvalidOperationException(""); //todo
            switch(Item.Type){
                case ItemType.WandOfDustStorm:
                {
                    //todo, figure out wands later
                }
                break;
                case ItemType.WandOfFleshToFire:
                {
                }
                break;
                case ItemType.WandOfInvisibility:
                {
                }
                break;
                case ItemType.WandOfReach:
                {
                }
                break;
                case ItemType.WandOfSlumber:
                {
                }
                break;
                case ItemType.WandOfTelekinesis:
                {
                }
                break;
                case ItemType.WandOfWebs:
                {
                }
                break;
               default: throw new InvalidOperationException("Not a wand type: " + Item.Type.ToString());
            }
            return null;
        }
    }
}