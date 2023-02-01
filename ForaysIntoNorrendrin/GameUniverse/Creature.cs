using System;
using System.Collections.Generic;
using System.Linq;
using GameComponents;
using GameComponents.DirectionUtility;
using Hemlock;

namespace Forays {
	//todo, move stuFF:
	/*public interface IPhysicalObject {
		Point? Position { get; }
	}
	public enum GroupType { Normal, LeaderWithFollowers };
	public class CreatureGroup {
		public GroupType Type;
		List<Creature> Members;
	}*/
	public enum CreatureBehaviorState { Unaware, Searching, Tracking, Hunting };
	// Unaware: Affected by stealth.
	// 		-at start of level, many enemies will be set to Asleep/Dormant, which gives a penalty to notice the player
	//			-but this Asleep/Dormant status doesn't affect this behavior state beyond that.
	//		 	(once woken up, they are Unaware, Hunting, etc., as normal)
	// Wandering might become unstated...?
	// Hunting might be split into Hunting and Tracking, where Tracking means it can no longer see you.
	// Searching can still be "will spot you but doesn't know where you are"...*
	// (if a shout or shriek alerts an enemy, it probably goes to Searching.)
	//	(the difference between Tracking and Searching is that Searching ticks down back to Unaware, but Tracking doesn't.)


	public class Creature : CreatureBase /*CreatureBase, IPhysicalObject*/ {
		public bool HasPosition => Map.Creatures.Contains(this);
		///<summary>Returns creature position, or throws if creature is not on the map. Check HasPosition beforehand if necessary.</summary>
		public Point Position => Map.Creatures.GetPositionOf(this);

		public int CurrentHealth;

		public CreatureBehaviorState BehaviorState;
		public int InitialTurnsIdle; //todo, does this need a 'can start idle' flag on CreatureDefinition?
		//todo path for wandering?
		//todo last seen etc.?

		//inherent attributes too
		//  i think inherent attributes will be done like this:
		//    prototype has all attrs... all attrs are copied to each, because it needs them in its own hemlock state...
		//    and inherent ones are either named explicitly, OR done with a new 'indestructible' source in hemlock.
			// -- the above is no longer the plan. CreatureType in the status dict should work very well instead.
		public StatusTracker<Creature, CreatureType, Status, Skill, AiTrait, Counter> StatusTracker;

		public bool this[Status status] => StatusTracker[status] > 0;
		public int this[Skill skill] => StatusTracker[skill];
		public bool this[AiTrait trait] => StatusTracker[trait] > 0;
		public int this[Counter counter] => StatusTracker[counter];

		//attacks
		//spells
		//AI decider

		//target, inventory, attributes, spells, (maybe skills), exhaustion,
		//time of last action / time at which hp recovery will happen, current path,
		// target last known location, player visibility counter for stealth,
		// group

		// Eventually I might need to declare that certain statuses are 'always refresh', which means I track the
		//   EventScheduling and reuse it if it exists (by updating the delay on it). That means tracking, per creature,
		//   a status->scheduling association...then, on add, it would use a new Hemlock feature to check whether it
		//   _would_ be prevented... And then, this state would either need to be serialized, or I'd need to guarantee
		//   that it could be reconstructed (by disallowing any non-refreshing instances of those statuses).
		//   (problem with the always-refresh plan: my priority queue is O(n) for changing priority. Just try the 'event spam' version first.)

		public bool ApplyStatus(Status status, int duration){
			//todo, 'if status is always-refresh, look for existing scheduling'...
			StatusInstance<Creature> inst = StatusTracker.CreateStatusInstance(status);
			if(StatusTracker.AddStatusInstance(inst)){
				Q.ScheduleWithRelativeInitiative(new StatusExpirationEvent(this, inst), duration, RelativeInitiativeOrder.BeforeCurrent);
				return true;
			}
			else return false;
		}

		//todo, this will probably be just a getter, switching on species:
		// (but for now i need to set the player's Decider directly)
		public CancelDecider CancelDecider { get; set; }
		public Creature(GameUniverse g) : base(g) {
			//
			CurrentHealth = 3;
			//
			StatusTracker = g.CreatureRules.CreateStatusTracker(this);
		}

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.

		public bool CanSee(Creature target){
			if(!HasPosition || !target.HasPosition) return false;
			else return Position.HasLOS(target.Position, Map.Tiles);
		}

		///<summary>Return value is the cost of the action taken</summary>
		public int ExecuteMonsterTurn(){
			if (false && false){ //todo, if the current tile is a major hazard, we want to escape it....but not if Mindless.
				return MoveTowardSafety();
				//todo, if we haven't spotted the player yet, is there a check for that here too?
			}
			if (HasPosition && BehaviorState != CreatureBehaviorState.Hunting){ //todo, what about Searching? is the HasPosition check needed here?
				if(CanSee(Player)){
					bool playerSpotted = true;
					if(BehaviorState == CreatureBehaviorState.Unaware){
						playerSpotted = R.CoinFlip();
					}
					if(playerSpotted){
						//todo, this will change to a shout for some enemies, which will create noise, right?
						Q.Execute(new AiChangeBehaviorStateEvent(this, CreatureBehaviorState.Hunting));
						return Turns(1);
					}
				}
				//todo, what about updating 'last seen'?
			}
			switch(BehaviorState){
				case CreatureBehaviorState.Unaware:
					return Idle();
				//todo case CreatureBehaviorState.Wandering:
					//return Wander();
				case CreatureBehaviorState.Searching:
					return Search();
				case CreatureBehaviorState.Hunting:
					return Hunt();
				default: throw new InvalidOperationException("Unknown state");
			}
			//hmm... a good amount of code goes at the start of each turn...
			// ... and a bit of it is shared by the player.

						// switch on state? Idle / Wandering / Searching / Hunting?
			// but note that the implementation might have more subdivisions within these. Searching could be investigating a sound,
			//   or trying to find the player after losing track. Hunting could be when the player is visible, or just while moving toward last_seen.

			// Maybe there's a 'single-minded' flag here, or maybe not. It'd be for things that run the same AI all the time.

			// is there actually an enum for this state? for now let's assume there is.

			// if hunting, call HuntingAI or whatever it's called
			// else
			//   check whether the player might be spotted...
			//   if we see the player, spend the turn shouting or something, and switch to Hunting
			//   otherwise, searching? ...
			//   otherwise, what things can switch something from idle/wandering to searching? sound and scent?
			//   if none of that, then, wandering? or just idle?

		}
		private int MoveTowardSafety(){
			//todo
		}
		private int Idle(){ // todo, what's the best name for this one? Idle vs Unaware vs...
			if(InitialTurnsIdle > 0){
				InitialTurnsIdle--;
				if(InitialTurnsIdle == 0){
					Q.Execute(new AiChangeBehaviorStateEvent(this, CreatureBehaviorState.Wandering));
				}
			}
			return Turns(1);
		}
		private int Wander(){
			//todo implement actual wandering:
			List<Point> validPoints = Position.GetNeighbors().Where(p => p.ExistsBetweenMapEdges() && TileTypeAt(p) != TileType.Wall).ToList();
			if(validPoints.Count == 0) return Turns(1);
			Point dest = R.ChooseFromList(validPoints);
			Q.Execute(new WalkAction(this, dest));
			return Turns(1);
		}
		private int Search(){
			//todo
			return Turns(1);
		}
		private int Hunt(){
			int? actionCost = TakeSpecialAction();
			//todo, this doesn't seem right. if TakeSpecialAction returns a value, is the turn already done?
			// also, do some things need to TakeSpecialAction while idle, wandering, etc.?
			if(!HasPosition) return actionCost ?? Turns(1);
			actionCost = MakeAttack()
				?? MoveToIdealDistance()
				?? Turns(1);
			return actionCost.Value;
		}
		private int? TakeSpecialAction(){
			return null;
			// TODO:
			//
			//  Can it be true that EVERYTHING in part 2 (move to ideal distance) and part 3 (make an attack) don't
			//     care about creaturetype, just statuses/AI traits?
			//   uh, hold on, swap 2 and 3, right?...
			//
			// if so, then the only big switch statement COULD be in _this_ method.
		}
		private int? MakeAttack(){
			int distRangeMin = 1; //todo
			int distRangeMax = 1;
			int currentDist = Position.ChebyshevDistanceFrom(Player.Position);
			if(currentDist >= distRangeMin && currentDist <= distRangeMax)
				return (int)Q.Execute(new MeleeAttackAction(this, Player)).Cost; //todo, should i make the whole thing work with long and not int?
			return null;
			//todo, this just looks at ranges, right?
			//maybe 'ability behavior' stuff goes here, like 'step back [or move randomly] after attack'
			// and 'slips past'
			//
		}
		private int? MoveToIdealDistance(){
			int distRangeMin = 1; //todo
			int distRangeMax = 1;
			DijkstraMap dm = new DijkstraMap(GetEffectiveCost);
			dm.IsSource = p => {
				int dist = p.ChebyshevDistanceFrom(Player.Position);
				return (dist >= distRangeMin && dist <= distRangeMax);
			};
			dm.Scan();
			List<Point> nextSteps = dm.GetPossibleNextStepsDownhill(Position);
			//TODO NEXT:

			// i was considering making 'normal' AI avoid hazards a bit more, but instead let's try a random chance to step through or step around.
			//
			// Then, did I need some kind of check so that certain enemies (mindless?) would only follow next steps that move closer to the player on at least one axis?
			//    But, do those use a dijkstra map at all? (if player visible, step toward. Maybe be able to step diagonally around a pillar.)


			int currentDist = Position.ChebyshevDistanceFrom(Player.Position);
			Dir8 dir = Position.GetDirectionOfNeighbor(Player.Position);
			if(currentDist > distRangeMax){ // advance
				//todo, will anything else be changed here?
			}
			else if(currentDist < distRangeMin){ // retreat
				dir = dir.Rotate(true, 4);
			}
			return (int)Q.Execute(new WalkAction(this, Position.PointInDir(dir))).Cost;
			//
			//todo, what needs to be done here?
			//each type needs a distance range. default 1...
			//then, what AI traits and abilities need to be considered here?
			//noneuclidean, slither, flit, light attraction, immobile, <frostling stuff>,
			// ...what about 'needs LOE' for ranged enemies?...
			//






			//  WOW, a lot of logic goes into just making enemies walk around.
			//  defining levels of danger...tracking them...checking nearby tiles...
			//  (danger levels change based on who's asking and on whether this enemy is chasing the player)
			//  check several tiles to see whether to immediately accept any...
			//  possibly go back and reconsider some tiles to see whether they can be accepted as a 2nd choice...
			//
			//

//maybe split it better, like: 'potential first picks' - any of these that are valid can be used right away?
//potential second picks... because these lists will change for noneuclidean, maybe even for 'needs LOE'...
// and then i guess 'backup plans' ... but is 1/2/3 really the way? or should it be arbitrary? might not need to be so complicated...
//the whole idea of the 3rd group is just to not get forever stuck on stuff...


			//pasting:
			//
			//
             /* todo remove   public bool AI_Step(PhysicalObject obj,bool flee){
                        if(HasAttr(AttrType.IMMOBILE) || (type == ActorType.MECHANICAL_KNIGHT && attrs[AttrType.COOLDOWN_1] == 2)) return false;
                        if(SlippedOrStruggled()) return true;
                        List<int> dirs = new List<int>();
                        List<int> sideways_directions = new List<int>();
                        AI_Step_Build_Direction_Lists(tile(),obj,flee,dirs,sideways_directions);
                        List<int> partially_blocked_dirs = new List<int>();
                        foreach(int i in dirs){
                                if(ActorInDirection(i) != null && ActorInDirection(i).IsHiddenFrom(this)){
                                        player_visibility_duration = -1;
                                        if(ActorInDirection(i) == player){
                                                attrs[AttrType.PLAYER_NOTICED]++;
                                        }
                                        target = player; //not extensible yet
                                        target_location = M.tile[player.row,player.col];
                                        string walks = " walks straight into you! ";
                                        if(!IsHiddenFrom(player)){
                                               B.Add(GetName(false,The) + " looks just as surprised as you. ");
                                        }
                                        return true;
                                }
                                Tile t = TileInDirection(i);
                                if(t.Is(TileType.RUBBLE) && (path == null || path.Count == 0 || t != M.tile[path[0]])){ //other tiles might go here eventually
                                        partially_blocked_dirs.Add(i);
                                }
                                else{
                                        if(AI_WillingToMove(tile(),t,obj) && AI_MoveOrOpen(i)){
                                                return true;
                                        }
                                }
                        }
                        foreach(int i in partially_blocked_dirs){
                                if(AI_WillingToMove(tile(),TileInDirection(i),obj) && AI_MoveOrOpen(i)){
                                        return true;
                                }
                        }
                        foreach(int i in sideways_directions){
                                if(AI_WillingToMove(tile(),TileInDirection(i),obj) && AI_MoveOrOpen(i)){
                                        return true;
                                }
                        }
                        return false;
                }*/

			//
			//
		}
		private int GetEffectiveCost(Point p){
			//todo
			//hmm, getCellCost. Need to consider obstacles for THIS enemy, with all inherent and temporary statuses,
				//as well as using different costs for Cowardly enemies.
				//so... for this creature, for each Point I need to know if it's blocked (major hazard or really just blocked),
				//  and what the cost is (could be increased for minor hazards)
				//  and of course the cost can change naturally for each type of monster, based on immunities etc.,

				// so I think hazards that don't extend beyond their tiles will just be checked directly,
				// and hazards that DO extend beyond their tiles will be tracked as tiles on the map are updated, so that data will be cached and ready.
				// Would it work to basically just have a dijkstra map of 'distance from hazard' ? Or maybe it just gets cut off and uses a radius like light sources.
			//
			return 1;
		}
	}
}
