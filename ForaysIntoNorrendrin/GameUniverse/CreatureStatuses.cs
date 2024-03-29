using System;
using System.Collections.Generic;
using Hemlock;

namespace Forays {
	public enum Status {
		Stunned = CreatureType.LAST+1, Poisoned, Spored, CanOpenDoors, VulnerableToNeckSnap, LowLightVision, Stoneform, PseudoVampiric, BrutishStrength,
		Roots, Hasted, SilenceAura, ShadowCloak, MysticMind,
		LAST };
	public enum Skill { Combat = Status.LAST+1, Defense, Magic, Spirit, Stealth, LAST }; //todo, might want to reorder these so all the bools and ints are grouped together?
	public enum AiTrait { Mindless = Skill.LAST+1, Clever, Cowardly, LAST };
		//UnderstandsDoors can be rolled into 'clever', and any 'clever' enemy that understands doors but is incapable of opening them can wait on the other side.
	public enum Counter { Lifespan = AiTrait.LAST+1, Shielded, LAST };
	//todo, MIGHT have 'SpellEffect' here as an enum _if_ that becomes important, such as for antimagic effects.
		// The same idea will become important if certain statuses are 'always refresh' statuses if there are also OTHER effects that want to use those statuses.
		// (for example, if a spell set you on fire for 50 turns, it would get its own separate effect.)
	//todo,spells+feats? although these must be tracked in order, mustn't they?
	//    --probably duplicate that data just a bit for the player, so that creature type can imply spells, and they can also be tracked in order for the player.

	//spells,feats, species spawning info like group size, or restrictions on placement.
	// should the goal be to NOT ever check for specific CreatureType?
	//     (though I think a few special exceptions might be okay, such as phantom clones,
	//          or a rule like "wandering wizard starts with 3 random spells" that'd be hard to encode into the enums.
	//          Maybe just try to note when a creature type gets special rules, as a comment on the species definition?)

	public class StatusRules : StatusSystem<Creature, CreatureType, Status, Skill, AiTrait, Counter>{
		public Action<Creature, Status> OnStatusStart, OnStatusEnd;

		private void StatusStartCallback(Creature creature, int status, int oldValue, int newValue){
			OnStatusStart?.Invoke(creature, (Status)status);
		}
		private void StatusEndCallback(Creature creature, int status, int oldValue, int newValue){
			OnStatusEnd?.Invoke(creature, (Status)status);
		}

		///<summary>Constructs a new StatusSystem instance with all game rules in place</summary>
		public StatusRules(){
			if(StatusConverter<CreatureType, int>.Convert == null){ // Set the static stuff...should be fine if multiple threads do this:
				StatusConverter<CreatureType, int>.Convert = x => (int)x;
				StatusConverter      <Status, int>.Convert = x => (int)x;
				StatusConverter       <Skill, int>.Convert = x => (int)x;
				StatusConverter     <AiTrait, int>.Convert = x => (int)x;
				StatusConverter     <Counter, int>.Convert = x => (int)x;
			}
			foreach(Status status in Enum.GetValues(typeof(Status))){
				this[status].Aggregator = this.Bool;
				//statuses get marked as bool... A whole bunch of them need the callback done as below:
				// (will these eventually happen for all of them, for the UI's sake? not sure...performance?)

				this[status].Messages.Increased = StatusStartCallback;
				this[status].Messages.Decreased = StatusEndCallback;
			}

			//todo, could set Skill aggregator to Max to support hybrids etc.

			foreach(AiTrait trait in Enum.GetValues(typeof(AiTrait))){
				this[trait].Aggregator = this.Bool;
			}

			foreach(Counter counter in Enum.GetValues(typeof(Counter))){
				this[counter].SingleInstance = true;
			}

			//load in the rules from creature types...
			CreatureDefinition.InitializeDefinitions();
			foreach(CreatureDefinition def in CreatureDefinition.GetAllDefinitions()){
				var creatureType = this[def.OriginalType];
				if(def.DeclaredStatuses != null) foreach(Status status in def.DeclaredStatuses){
					creatureType.Feeds(status);
				}
				if(def.DeclaredSkills != null) foreach(KeyValuePair<Skill,int> pair in def.DeclaredSkills){
					creatureType.Feeds(pair.Value, pair.Key);
				}
				if(def.DeclaredAiTraits != null) foreach(AiTrait trait in def.DeclaredAiTraits){
					creatureType.Feeds(trait);
				}
				if(def.DeclaredCounters != null) foreach(KeyValuePair<Counter,int> pair in def.DeclaredCounters){
					creatureType.Feeds(pair.Value, pair.Key);
				}
				//todo, more here...spells etc.
			}

			//and then all the custom stuff, right?

			this.IgnoreRuleErrors = true; //todo, debug flag?
		}
	}
}
