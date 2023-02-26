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
		public Point LastKnownPlayerPosition;
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
			// todo, check for inability to act here? asleep, etc.?
			if(!HasPosition){
				return ExecuteTurnNotOnMap();
			}
			if(IsMajorHazard(Position)){
				return MoveTowardSafety();
			}
			bool canSeePlayer = CanSee(Player);
			if(BehaviorState == CreatureBehaviorState.Hunting){
				if(!canSeePlayer){
					Q.Execute(new AiChangeBehaviorStateEvent(this, CreatureBehaviorState.Tracking));
				}
			}
			else{
				if(canSeePlayer){
					if(BehaviorState == CreatureBehaviorState.Unaware){
						if(R.CoinFlip()){ //todo, stealth here
							//todo, this will change to a shout for some enemies, which will create noise, right?
							LastKnownPlayerPosition = Player.Position;
							Q.Execute(new AiChangeBehaviorStateEvent(this, CreatureBehaviorState.Hunting));
							return Turns(1);
						}
					}
					else{
						LastKnownPlayerPosition = Player.Position;
						Q.Execute(new AiChangeBehaviorStateEvent(this, CreatureBehaviorState.Hunting));
					}
				}
			}
			switch(BehaviorState){
				case CreatureBehaviorState.Unaware:
					return Idle();
				case CreatureBehaviorState.Searching:
					return Search();
				case CreatureBehaviorState.Tracking:
					return Track();
				case CreatureBehaviorState.Hunting:
					return Hunt();
				default: throw new InvalidOperationException("Unknown state");
			}
		}
		private int ExecuteTurnNotOnMap(){
			return Turns(1); //todo
		}
		private int MoveTowardSafety(){
			//todo (this method should use some of the same logic as normal turns to decide whether to walk through minor hazards)
			return Turns(1); //todo
		}
		private int Idle(){ // todo, what's the best name for this one? Idle vs Unaware vs...
			if(InitialTurnsIdle > 0){
				InitialTurnsIdle--;
				if(InitialTurnsIdle == 0){
					//todo, this is just for the Asleep status now - does it even need a separate timer?
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
		private int Track(){
			if(!HasPosition) return Turns(1);
			return MoveToMeleeRange(LastKnownPlayerPosition) ?? Turns(1);
		}
		private int Hunt(){
			LastKnownPlayerPosition = Player.Position;
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
		private bool IsMinorHazard(Point p){
			//todo, need to cache any hazards that extend to other tiles.
			// single-cell hazards can be checked directly.
			return false;
			// todo: maybe player-adjacent cells should be considered minor hazards by anything with range >1 ?
		}
		private bool IsMajorHazard(Point p){
			//todo, need to cache any hazards that extend to other tiles.
			// single-cell hazards can be checked directly.
			return false;
		}
		private bool IsImpassable(Point p){
			return !TileDefinition.IsPassable(Map.TileTypeAt(p)) || IsMajorHazard(p);
		}
		private int? MoveToIdealDistance(){
			int distRangeMin = 1; //todo, get actual range here
			int distRangeMax = 1;
			if (distRangeMax > 1)
				return MoveToRange(distRangeMin, distRangeMax);
			else
				return MoveToMeleeRange(Player.Position);
		}
		private int? MoveToMeleeRange(Point targetPosition){
			//todo, this method probably needs checks so that enemies don't step out of LOS of the player by going around a corner too soon
			Point idealStep = Position.PointInDir(Position.GetDirectionOfNeighbor(targetPosition));
			if(IsImpassable(idealStep) || !idealStep.HasLOS(targetPosition, Map.Tiles)){
				//todo... clever enemies will do full pathfinding to get around.
				// Others will try the other cell(s) that'll get this enemy closer on one axis, if possible, but otherwise will wait on the other side.
				return TryAlternateStepTowardTarget(targetPosition);
			}
			else if(IsMinorHazard(idealStep)){
				//todo... most enemies will look at the 'ideal path' (ignoring hazards) to decide what to do here.
				// The thing that matters is how soon, on that path, there is another safe cell.
				// If the very next cell (after the minor hazard) is safe, they should walk through almost 100% of the time.
				//		(maybe base it on this PLUS current HP, so if it's almost dead it'll probably wait a turn instead? maybe 50% HP + next cell safe is still 95%-100%?)
				// If there is a major hazard on this path BEFORE a safe cell, it's way less likely to step. Maybe 5% or so.
				// Otherwise it scales based on the number of minor hazards before a safe cell. Maybe 15% per hazard, or maybe 5% + 20% after the first, whatever.
				// Either way, only AFTER that is decided will it check for alternate paths. If there's a safe cell right next to this one, and we decided to wait,
				// then take that one instead if it gets us closer on one axis. Otherwise just wait.
				//


				// THEREFORE:

				// we need to GET the ideal path here.
				// we count the following on that path:
				//   -number of minor hazards that appear on this path before a safe cell appears
				//   -does at least one major hazard appear before a safe cell?
				// if at least one major hazard exists, note this -- TODO, it should be much less likely to take a step here, but how much exactly?
				// else, determine the likelihood to take a step based on the number of minor hazards we found -- TODO, maybe 5% + 20% per?
				// Based on those two values, now we actually decide: step or no?
				// if yes, take the action and we're done.
				// else, we look for alternate paths:
				//    If this enemy is clever, we do real pathfinding to get us around.
				//    Else, we only look at adjacent cells which get us closer on the major axis. TODO: do we still have a chance for random movement here if we don't move yet?

				List<Point> idealPath = GetIdealPath(targetPosition);
				int minorHazardsBeforeSafety = 0, majorHazardsBeforeSafety = 0;
				foreach(Point p in idealPath){
					if(IsMajorHazard(p)) ++majorHazardsBeforeSafety;
					else if(IsMinorHazard(p)) ++minorHazardsBeforeSafety; // Note that the initial hazard is counted here.
					else break; // Stop counting if we encounter a safe location.
				}
				int chanceToBraveHazard = 115 - (minorHazardsBeforeSafety * 20); // 95% chance if the initial hazard was the only one. -20% per additional hazard.
				if(majorHazardsBeforeSafety > 0){
					chanceToBraveHazard = 2; // Let's just make it very unlikely but not impossible.
				}
				if(R.PercentChance(chanceToBraveHazard)){
					return (int)Q.Execute(new WalkAction(this, idealStep)).Cost;
				}
				else{
					return TryAlternateStepTowardTarget(targetPosition);
				}
			}
			else{
				return (int)Q.Execute(new WalkAction(this, idealStep)).Cost;
			}
		}
		private int? TryAlternateStepTowardTarget(Point targetPosition){
			if(false && this[AiTrait.Clever]){
				//todo, if clever, do real pathfinding here.
				return null;
			}
			else{
				Point? alternativeStep = GetNextStepOnMajorAxisOnly(Position, targetPosition);
				if(alternativeStep != null && !IsMajorHazard(alternativeStep.Value) && !IsMinorHazard(alternativeStep.Value)){
					return (int)Q.Execute(new WalkAction(this, alternativeStep.Value)).Cost;
				}
				else{
					//todo, 20% chance to move randomly?
					//for now let's just end turn:
					return null;
				}
			}
		}
		//todo desc -- returns what would be the ideal path if we ignore hazards, other creatures in the way, etc., but checking LOS at each step.
		private List<Point> GetIdealPath(Point destination){
			List<Point> result = new List<Point>();
			//find the dx and dy
			// loop...
			//   look at next cell
			//   if LOS, add it and move to next
			//   if no LOS, look at the alternative cell instead:
			//		if LOS, add it and move to next (from the alternative cell!)
			//      else, assume LOS is broken and just return what we have. Always return at least 1 point.
			//todo clean up comments
			int dx = (Position.X < destination.X)? 1 : (Position.X > destination.X)? -1 : 0;
			int dy = (Position.Y < destination.Y)? 1 : (Position.Y > destination.Y)? -1 : 0;
			Point nextPos = new Point(Position.X + dx, Position.Y + dy);
			while(true){
				if(Position.HasLOS(nextPos, Map.Tiles)){
					result.Add(nextPos);
					nextPos = new Point(nextPos.X + dx, nextPos.Y + dy);
				}
				else{
					Point? alternativeStep = GetNextStepOnMajorAxisOnly(nextPos, destination);
					if(alternativeStep == null) return result; // This means we have no LOS on a straight diagonal, so it must be blocked.
					result.Add(alternativeStep.Value);
					nextPos = alternativeStep.Value;
				}
			}
		}
		//todo desc -- returns the second choice cell, the one that gets closer on only the major axis, or null if there is no such cell.
		private static Point? GetNextStepOnMajorAxisOnly(Point start, Point destination){
			int dx = destination.X - start.X;
			int dy = destination.Y - start.Y;
			int dxAbs = Math.Abs(dx);
			int dyAbs = Math.Abs(dy);
			if(dxAbs == dyAbs) return null;
			else if(dxAbs > dyAbs){
				if(dx > 0) return new Point(start.X + 1, start.Y);
				else return new Point(start.X - 1, start.Y);
			}
			else{ //dxAbs < dyAbs
				if(dy > 0) return new Point(start.X, start.Y + 1);
				else return new Point(start.X, start.Y - 1);
			}
		}
		private int? MoveToRange(int distRangeMin, int distRangeMax){
			// let's plan range >1 with 'move to range' while considering minor hazards... there must be something reasonable to do by default.
			//
			// i think the main thing I can relax on is:  circle kiting isn't nearly as big a concern as the range grows.
			// but that's no reason not to TRY to move closer on both axes, right?
			// so let's... build the set of cells (at the correct range from the player) which this enemy has LOS to / can see,
			// and find the subset of those that are closest to this enemy...
			// (could alternatively check for danger BEFORE checking distance, but both should work...)
			//...
			//>>> re:the above, do this:
			// find all at dist X
			// filter out ones with no LOS
			// filter out ones that aren't closest to the enemy
			// use proximity to the cardinal direction as a tiebreaker here
			// (that is, if we can step toward that, do it. otherwise try another...)
			// -or- what if i did almost the same thing, but I built a list of best steps, in that order (so tiebreaker is preserved), and try any of them?
			//
			//
			//
			return null; //todo
		}
		/* old version, delete after using the important parts for ranged approaches: private int? MoveToIdealDistance2(){
			int distRangeMin = 1; //todo
			int distRangeMax = 1;
			DijkstraMap dm = new DijkstraMap(GetEffectiveCost);
			dm.IsSource = p => {
				int dist = p.ChebyshevDistanceFrom(Player.Position);
				return (dist >= distRangeMin && dist <= distRangeMax);
			};
			dm.Scan();
			List<Point> nextSteps = dm.GetPossibleNextStepsDownhill(Position);


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

		}*/
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
