using System;
using System.Collections.Generic;
using System.Linq;
using GameComponents;
using GameComponents.DirectionUtility;

namespace Forays {
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


	public class Enemy : Creature {
		public CreatureBehaviorState BehaviorState;
		public int InitialTurnsIdle; //todo, does this need a 'can start idle' flag on CreatureDefinition?
		//todo path for wandering?
		public Point LastKnownPlayerPosition;
		//todo last seen etc.?

		public bool this[AiTrait trait] => StatusTracker[trait] > 0;

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

		//todo, this will probably be just a getter, switching on species:
		// (but for now i need to set the player's Decider directly)
		public CancelDecider CancelDecider { get; set; }
		public Enemy(GameUniverse g) : base(g) {
			//todo
			CurrentHealth = 3;
			//
			StatusTracker = g.CreatureRules.CreateStatusTracker(this);
		}

			// i'm thinking Creature or CreatureBase or CreatureDefinition should have a static Create method that takes care of a few things....
			//   It would hide the ctor, mostly because there might be a few subclasses of Creature, and the only thing that should care about THAT is serialization.
			//   It MIGHT provide a few bool options for the classic stuff like 'schedule an event for this' or 'add this to the map'. Maybe, maybe not.

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
			//todo: i think ranged enemies are currently happy to stand in minor hazards - is this okay?
			DijkstraMap dm = new DijkstraMap(p => {
				if(Position.ChebyshevDistanceFrom(p) > 6) return -1;
				else if(IsConsideredImpassable(p, true)) return -1;
				else if(Position.ChebyshevDistanceFrom(p) == 1 && CreatureAt(p) != null) return 2; //todo, higher cost here or no?
				else return 1; //todo, use real movement costs here.
			});
			dm.IsSource = p => Position.ChebyshevDistanceFrom(p) <= 6 && !IsConsideredImpassable(p);
			dm.Scan();
			List<Point> points = dm.GetPossibleNextStepsDownhill(Position);
			if(points.Count == 0){
				//todo, panic and move randomly
			}
			else{
				//todo, if hunting/tracking, could use distance from target as a tiebreaker here:
				Point destination = R.ChooseFromList(points);
				return (int)Q.Execute(new WalkAction(this, destination)).Cost;
			}
			//todo (this method should use some of the same logic as normal turns to decide whether to walk through minor hazards)
			return Turns(1); //todo
		}
		private int Idle(){ // todo, what's the best name for this one? Idle vs Unaware vs...
		//todo, if idle or searching, we want to move to safety if in a MINOR hazard, too.
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
			return FeaturesAt(p).HasFeature(FeatureType.PoisonGas);
			//todo, need to cache any hazards that extend to other tiles.
			// single-cell hazards can be checked directly.
			return false;
			// todo: maybe player-adjacent cells should be considered minor hazards by anything with range >1 ?
		}
		private bool IsMajorHazard(Point p){
			//todo, check mindless here?
			return FeaturesAt(p).HasFeature(FeatureType.Fire);
			//todo, need to cache any hazards that extend to other tiles.
			// single-cell hazards can be checked directly.
			return false;
		}
		private bool IsConsideredImpassable(Point p, bool ignoreHazards = false){
			return !TileDefinition.IsPassable(Map.TileTypeAt(p)) || (!ignoreHazards && IsMajorHazard(p));
		}
		private int? MoveToIdealDistance(){
			int distRangeMin = 3; //todo, get actual range here
			int distRangeMax = 4;
			distRangeMin = this.OriginalType == CreatureType.Goblin? 1 : 3;
			distRangeMax = this.OriginalType == CreatureType.Goblin? 1 : 4;
			if (distRangeMax > 1)
				return MoveToRange(Player.Position, distRangeMin, distRangeMax);
			else
				return MoveToMeleeRange(Player.Position);
		}
		///<param name="rollToBraveHazards">Creatures should use only a single roll per turn for hazards, to prevent multiple rolls from greatly
		/// increasing the chance to brave hazards on any given turn.</param>
		private int? MoveToMeleeRange(Point targetPosition, int? rollToBraveHazards = null){
			Point idealStep = Position.PointInDir(Position.GetDirectionOfNeighbor(targetPosition));
			if(IsConsideredImpassable(idealStep) || !Map.CheckLOS(idealStep, targetPosition)){
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

				List<Point> idealPath = GetIdealPath(Position, targetPosition);
				int minorHazards = 0, majorHazards = 0;
				foreach(Point p in idealPath){
					if(IsMajorHazard(p)) ++majorHazards;
					else if(IsMinorHazard(p)) ++minorHazards; // Note that the initial hazard is counted here.
				}
				// Chance is divided once per hazard found. So, 1 hazard is 90+1 or a 91% chance to brave the hazard each turn.
				int chanceToBraveHazard = 180; // 2 hazards are 45+1 or a 46% chance to brave the hazard each turn.
				for(int i=0;i<minorHazards;++i){ // 3 are 22+1, or 23% chance, and so on...
					chanceToBraveHazard /= 2;
				}
				chanceToBraveHazard += 1; // Minimum 1% chance
				if(majorHazards > 0){
					chanceToBraveHazard = Math.Min(1, chanceToBraveHazard); // If there is a major hazard farther on, make it very unlikely but not impossible.
				}
				if(IsMinorHazard(Position)){ // If this location is already a hazard, might as well keep going.
					chanceToBraveHazard = 100;
				}
				bool braveHazards;
				if(rollToBraveHazards != null){
					braveHazards = (chanceToBraveHazard >= rollToBraveHazards.Value);
				}
				else{
					braveHazards = R.PercentChance(chanceToBraveHazard);
				}
				if(braveHazards){
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
					int dx = targetPosition.X - Position.X;
					int dy = targetPosition.Y - Position.Y;
					if(dx == 0 || dy == 0){ // If the target is in the same row or same column...
						Point[] twoAlternateSteps = GetTwoAlternateStepsForCardinalDirection(Position, targetPosition);
						if(twoAlternateSteps != null){
							if(R.CoinFlip()){ // Randomize the order in which these 2 cells are checked
								Point swap = twoAlternateSteps[0];
								twoAlternateSteps[0] = twoAlternateSteps[1];
								twoAlternateSteps[1] = swap;
							}
							foreach(Point p in twoAlternateSteps){
								if(!IsConsideredImpassable(p) && !IsMinorHazard(p)){
									return (int)Q.Execute(new WalkAction(this, p)).Cost;
								}
							}
						}

					}
					//todo, 20% chance to move randomly?
					//for now let's just end turn:
					return null;
				}
			}
		}
		//todo, this is untested and I'm not certain I want to use it yet. It might depend on how predictable I want enemies to be.
		// also needs desc -- should be called only when the ideal path is hazardous. Finds the closest-to-ideal path that doesn't go through
		//  any hazards. This one is for average enemies, NOT clever ones, which can do full pathfinding instead of this limited version.
		private Point? FindNextStepAroundHazards(Point targetPosition, List<Point> idealPath){ //todo, is this the best name?
			int dxTotal = targetPosition.X - Position.X;
			int dyTotal = targetPosition.Y - Position.Y;
			int dxTotalAbs = Math.Abs(dxTotal); //todo, is all of this needed?
			int dyTotalAbs = Math.Abs(dyTotal);
			if(dxTotal == 0 || dyTotal == 0 || dxTotalAbs == dyTotalAbs) return null; // Return early if this is a cardinal or diagonal line
			bool xMajor = dxTotalAbs > dyTotalAbs;
			Point minorAxisStepVector;
			if(xMajor) minorAxisStepVector = new Point(0, dyTotal > 0? -1 : 1); // Note that this vector's direction is reversed; it steps from the target toward the source.
			else minorAxisStepVector = new Point(dxTotal > 0? -1 : 1, 0);
			//todo clean up comments, add some examples.
			Point lastHazard = idealPath.FindLast(p => p != targetPosition && (IsConsideredImpassable(p) || IsMinorHazard(p)));
			Point nextWaypointAvoidingHazard = lastHazard;
			while(true){
				nextWaypointAvoidingHazard = nextWaypointAvoidingHazard + minorAxisStepVector;
				// If we go past the source on the minor axis, there is no path:
				if(xMajor){
					if(dyTotal > 0 && nextWaypointAvoidingHazard.Y < this.Position.Y) return null;
					if(dyTotal < 0 && nextWaypointAvoidingHazard.Y > this.Position.Y) return null;
				}
				else{
					if(dxTotal > 0 && nextWaypointAvoidingHazard.X < this.Position.X) return null;
					if(dxTotal < 0 && nextWaypointAvoidingHazard.X > this.Position.X) return null;
				}
				// If this cell is not a hazard, break out and see if we can connect it to source + target.
				if(!IsConsideredImpassable(nextWaypointAvoidingHazard) && !IsMinorHazard(nextWaypointAvoidingHazard)) break;
			}
			// First, ensure that this cell can connect to the target. If there is no valid path from this cell, I think it's guaranteed that there is no path at all.
			List<Point> waypointToTarget = GetIdealPath(nextWaypointAvoidingHazard, targetPosition);
			int waypointToTargetHazardIdx = waypointToTarget.FindLastIndex(p => p != targetPosition && (IsConsideredImpassable(p) || IsMinorHazard(p)));
			if(waypointToTargetHazardIdx != -1) return null; // No path
			// Now we need a path from the source to this waypoint:
			List<Point> sourceToWaypoint = GetIdealPath(Position, nextWaypointAvoidingHazard);
			int sourceToWaypointHazardIdx = sourceToWaypoint.FindLastIndex(p => p != targetPosition && (IsConsideredImpassable(p) || IsMinorHazard(p)));
			if(sourceToWaypointHazardIdx == -1){
				// If there is no hazard here, then we have a complete path. Move toward the waypoint:
				if(sourceToWaypoint.Count == 0) return nextWaypointAvoidingHazard; // If the source is adjacent to the waypoint the count could be 0.
				else return sourceToWaypoint[0];
			}
			else{
				// Recursively try to connect back to the source cell:
				Point? resultStep = FindNextStepAroundHazards(nextWaypointAvoidingHazard, sourceToWaypoint);
				return resultStep;
			}
			//new algo? :
			// find LAST haz in ideal.
			// increment in right (minor) direction.
			// (if new cell is haz, keep going)
			// try to connect new pos to target.
			// if it fails, is there really no path? I think this might be true.
			// otherwise we know we can reach the target from this new cell, so now try to find a path from source->newCell.
			// same idea here - if no haz, done (return first step of source->newCell!).
			//					if haz, find the last one and increment in the minor direction.
			// so this is recursive, and if we can connect the chain then it's a success.
			// so since the recursive part is the part closer to the source, maybe it looks like this?:
			// from ideal path:
			//		if no haz, done(success)
			//		else, find first non-haz in minor direction.
			//			if none, done(fail).
			//			else, goto 'from A'
			// from A:
			//		if no connection to target, done(fail).
			//		else, find ideal path from source to A and goto 'from ideal path'.-
			//create array. major axis length is (major axis difference - 1) because neither source nor target need to be included.
			// minor axis length is (minor axis difference + 1) because both source and target lines need to be included.
		}
		//todo desc -- returns what would be the ideal path if we ignore hazards, other creatures in the way, etc., but checking LOS at each step.
		private List<Point> GetIdealPath(Point source, Point destination){
			List<Point> result = new List<Point>();
			//find the dx and dy
			// loop...
			//   look at next cell
			//   if LOS, add it and move to next
			//   if no LOS, look at the alternative cell instead:
			//		if LOS, add it and move to next (from the alternative cell!)
			//      else, assume LOS is broken and just return what we have. Always return at least 1 point.
			//todo clean up comments
			Point currentPos = source;
			while(true){
				int dx = (currentPos.X < destination.X)? 1 : (currentPos.X > destination.X)? -1 : 0;
				int dy = (currentPos.Y < destination.Y)? 1 : (currentPos.Y > destination.Y)? -1 : 0;
				Point nextPos = new Point(currentPos.X + dx, currentPos.Y + dy);
				if(nextPos == destination) return result; // We're done - don't include the destination cell.
				if(Map.CheckLOS(source, nextPos) && !IsConsideredImpassable(nextPos, true)){
					result.Add(nextPos);
					currentPos = nextPos;
				}
				else{
					Point? alternativeStep = GetNextStepOnMajorAxisOnly(nextPos, destination);
					if(alternativeStep == null) return result; // This means we have no LOS on a straight diagonal, so it must be blocked.
					result.Add(alternativeStep.Value);
					currentPos = alternativeStep.Value;
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
		//todo desc -- returns null if same cell or not a cardinal line. Otherwise, returns smaller (x,y) value first.
		private static Point[] GetTwoAlternateStepsForCardinalDirection(Point start, Point destination){
			int dx = destination.X - start.X;
			int dy = destination.Y - start.Y;
			if((dx != 0 && dy != 0) || (dx == 0 && dy == 0)) return null;
			else if(dx > 0) return new Point[] { new Point(start.X + 1, start.Y - 1), new Point(start.X + 1, start.Y + 1) };
			else if(dx < 0) return new Point[] { new Point(start.X - 1, start.Y - 1), new Point(start.X - 1, start.Y + 1) };
			else if(dy > 0) return new Point[] { new Point(start.X - 1, start.Y + 1), new Point(start.X + 1, start.Y + 1) };
			else /*dy < 0*/ return new Point[] { new Point(start.X - 1, start.Y - 1), new Point(start.X + 1, start.Y - 1) };
		}
		private int? MoveToRange(Point targetPosition, int distRangeMin, int distRangeMax){
			// Find all points within range, in order of euclidean distance, and check for blocked movement, LOS from here, and LOS from target.
			// Note the least distance of any valid points so that we can consider those first. (I would have sorted by distance if I had a stable sort readily available.)
			List<Point> pointsWithinTargetRange = new List<Point>();
			int leastDistance = int.MaxValue;
			foreach(Point nearby in targetPosition.EnumeratePointsWithinChebyshevDistance(distRangeMax, true, true)){
				if(targetPosition.ChebyshevDistanceFrom(nearby) >= distRangeMin
					&& nearby.ExistsBetweenMapEdges()
					&& !IsConsideredImpassable(nearby)
					&& Map.CheckLOS(Position, nearby)
					&& Map.CheckLOS(targetPosition, nearby))
				{
					pointsWithinTargetRange.Add(nearby);
					int dist = Position.ChebyshevDistanceFrom(nearby);
					if(dist < leastDistance) leastDistance = dist;
				}
			}
			int rollToBraveHazards = R.Between(1, 100);
			foreach(Point potentialDestination in pointsWithinTargetRange){
				if(Position.ChebyshevDistanceFrom(potentialDestination) > leastDistance) continue; // On the first pass through, consider only the closest.
				int? result = MoveToMeleeRange(potentialDestination, rollToBraveHazards);
				if(result != null) return result;
			}
			foreach(Point potentialDestination in pointsWithinTargetRange){
				if(Position.ChebyshevDistanceFrom(potentialDestination) == leastDistance) continue; // Skip the ones already checked. Maybe inefficient but this code will rarely be reached.
				int? result = MoveToMeleeRange(potentialDestination, rollToBraveHazards);
				if(result != null) return result;
			}
			return null;
		}
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
