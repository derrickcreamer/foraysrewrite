using System;
using System.Collections.Generic;
using Hemlock;
using static Forays.Status;

namespace Forays {
    public enum Status {
        Stunned = CreatureType.LAST+1, Poisoned, Spored, CanOpenDoors, VulnerableToNeckSnap, LowLightVision,
        LAST };
    public enum Skill { Combat = Status.LAST+1, Defense, Magic, Spirit, Stealth, LAST };
    public enum AiTrait { Aggressive = Skill.LAST+1, KeepsDistance, UnderstandsDoors, LAST };
    // todo, possible AI traits... 'understands doors' might be useful in combination with 'can open doors'...
    //    could encourage the AI to wait at a door rather than wander, and might even allow an Ambusher trait where
    public enum Counter { Lifespan = AiTrait.LAST+1, Shielded, LAST };
    //todo,spells+feats? although these must be tracked in order, mustn't they?
    //    --probably duplicate that data just a bit for the player, so that creature type can imply spells, and they can also be tracked in order for the player.

    //spells,feats, species spawning info like group size, or restrictions on placement.
    // should the goal be to NOT ever check for specific CreatureType?
    //     (though I think a few special exceptions might be okay, such as phantom clones,
    //          or a rule like "wandering wizard starts with 3 random spells" that'd be hard to encode into the enums.
    //          Maybe just try to note when a creature type gets special rules, as a comment on the species definition?)

    public static class StatusRules{
        private static StatusSystem<Creature, CreatureType, Status, Skill, AiTrait, Counter> rules;
        // TODO NEXT >>>  decide how this works. Since StatusSystem is exactly the type we want as part of GameUniverse, seems good to make this part stateless and just return a new one each time.
        public static StatusSystem<Creature, CreatureType, Status, Skill, AiTrait, Counter> GetRules(){
            if(rules != null) return rules;

            var r = new StatusSystem<Creature, CreatureType, Status, Skill, AiTrait, Counter>();
            r[Stunned].SingleInstance = true;
            r[Stunned].Messages.Increased = (creature, status, oldValue, newValue) =>{
                creature.Notify(new NotifyPrintMessage{ Message = "Stunned!" });
            };
            r[Stunned].Messages.Decreased = (creature, status, oldValue, newValue) =>{
                creature.Notify(new NotifyPrintMessage{ Message = "Unstunned!" });
            };

            r.IgnoreRuleErrors = true; //todo, debug flag?
            rules = r;
            return rules;
        }
    }
}
