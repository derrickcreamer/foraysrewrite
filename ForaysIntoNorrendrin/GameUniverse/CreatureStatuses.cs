using System;
using System.Collections.Generic;
using Hemlock;
using static Forays.Status;

namespace Forays {
    public enum Status { Stunned = CreatureType.LAST+1, Poisoned, Spored, LAST };
    public enum Skill { Combat = Status.LAST+1, Defense, Magic, Spirit, Stealth, LAST };
    public enum AiTrait { Aggressive = Skill.LAST+1, KeepsDistance, LAST };
    public enum Counter { Lifespan = AiTrait.LAST+1, Shielded, LAST };
    //todo,spells+feats? although these must be tracked in order, mustn't they?

    //...hmmm..... any utility in making CreatureType part of this? it could actually imply things based on THAT...
    //    ...although it might be awkward duplication to have the CreatureType in the dictionary and also as a standalone value...
    //          ...so it's probably worth at least CONSIDERING whether the standalone value is necessary.
    //                  - maybe it stays but becomes "original creature type" or something? that'd be a good sidestep, and useful too.

    //    ... does that also mean I don't need species templates at all? let's find out...
    //

    //spells,feats, species spawning info like group size, or restrictions on placement.
    // should the goal be to NOT ever check for specific CreatureType?
    //     (though I think a few special exceptions might be okay, such as phantom clones)

    public static class StatusRules{
        private static StatusSystem<Creature, CreatureType, Status, Skill, AiTrait, Counter> rules;

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
