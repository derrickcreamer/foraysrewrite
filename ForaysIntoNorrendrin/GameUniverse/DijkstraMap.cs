using System;
using System.Collections.Generic;
using GameComponents;
using GameComponents.DirectionUtility;
using Priority_Queue;

namespace Forays{
	public class DijkstraMap : PointArray<int>{
		public const int Unexplored = int.MaxValue;
		public const int Blocked = int.MinValue;

		public readonly Func<Point, int> GetCellCost;
		private SimplePriorityQueue<Point, int> frontier; // The PQ keeps track of which cell should be visited next

		public Func<Point, bool> IsSource;
		public Func<Point, int> GetSourceValue;
		//todo, IsPartial, for pathing stuff?
		public DijkstraMap(Func<Point, int> getCellCost) : base(GameUniverse.MapWidth, GameUniverse.MapHeight){
			if(getCellCost == null) throw new ArgumentNullException(nameof(getCellCost));
			GetCellCost = getCellCost;
			frontier = new SimplePriorityQueue<Point, int>();
			ResetValues();
		}
		public void ResetValues(){ // Set everything to Unexplored instead of zero:
			for(int i=0;i<Height;++i)
				for(int j=0;j<Width;++j)
					this[j,i] = Unexplored;
		}
		public void Scan(Point source){
			//todo, check assumptions about starting state here.
			// Start the frontier with the given source cell, which starts with a value of zero if not otherwise specified:
			int value = GetSourceValue?.Invoke(source) ?? 0;
			this[source] = value;
			frontier.Enqueue(source, value);
			ScanInternal();
		}
		public void Scan(IEnumerable<Point> sources){
			//todo, check assumptions about starting state here.
			foreach(Point source in sources){
				int value = GetSourceValue?.Invoke(source) ?? 0;
				this[source] = value;
				frontier.Enqueue(source, value);
			}
			ScanInternal();
		}
		public void Scan(){
			if(IsSource == null) throw new InvalidOperationException("Must specify 'IsSource' to use this method");
			//todo, check assumptions about starting state here.
			for(int i=0;i<Height;++i){
				for(int j=0;j<Width;++j){
					Point p = new Point(j, i);
					if(IsSource(p)){
						int value = GetSourceValue?.Invoke(p) ?? 0;
						this[p] = value;
						frontier.Enqueue(p, value);
					}
					else{
						this[j,i] = Unexplored;
					}
				}
			}
			ScanInternal();
		}
		private void ScanInternal(){
 			while(frontier.Count > 0){
				Point current = frontier.Dequeue();
				foreach(Dir8 dir in EightDirections.Enumerate(true, false, false)){
					Point neighbor = current.PointInDir(dir);
					if(!neighbor.ExistsOnMap()) continue;
					int neighborCost = GetCellCost(neighbor);
					if(neighborCost < 0){
						if(this[neighbor] == Unexplored) this[neighbor] = Blocked;
					}
					else{
						int totalCost = this[current] + neighborCost;
						if(this[neighbor] > totalCost){
							this[neighbor] = totalCost;
							frontier.Enqueue(neighbor, totalCost);
						}
					}
				}
			}
		}
		//todo xml, this one just uses the values that are present, and doesn't consider sources at all. (It adds every cell to the frontier.)
		public void RescanWithCurrentValues(){
			for(int i=0;i<Height;++i){
				for(int j=0;j<Width;++j){
					Point p = new Point(j, i);
					int value = this[p];
					if(value != Blocked && value != Unexplored) frontier.Enqueue(p, value);
				}
			}
			RescanInternal();
		}
		/* Might add these at some point -- these are the ones that would allow partial rescans by resetting values starting at the changed cell(s).
		1) Search outward from the given changed cell(s) as long as you can keep jumping to a cell with HIGHER total cost. This set of cells represents
			all the cells which might have been influenced by the changed cell(s).
		2) Note where this search stopped:  Each of the cells adjacent to this set (but not part of it) will be the frontier for the rescan.
		3) Reset the value of each cell in this set to Unexplored.
		3) Rescan the map from the frontier.
		(If any of the changed cells could be sources, that needs to be handled too.)
		public void Rescan(Point changedCell){ }
		public void Rescan(IEnumerable<Point> changedCells){ }*/

		private void RescanInternal(){ // Differs from ScanInternal in how it uses the frontier
 			while(frontier.Count > 0){
				Point current = frontier.Dequeue();
				foreach(Dir8 dir in EightDirections.Enumerate(true, false, false)){
					Point neighbor = current.PointInDir(dir);
					if(!neighbor.ExistsOnMap()) continue;
					if(this[neighbor] == Blocked) continue;
					int neighborCost = GetCellCost(neighbor);
					if(neighborCost < 0){
						if(this[neighbor] == Unexplored){
							frontier.TryRemove(neighbor); // (Not sure this can happen on a rescan if done correctly)
							this[neighbor] = Blocked;
						}
					}
					else{
						int totalCost = this[current] + neighborCost;
						if(this[neighbor] > totalCost){
							// Remove them before changing sort values, so the sort doesn't break:
							if(this[neighbor] != Unexplored) frontier.TryRemove(neighbor);
							this[neighbor] = totalCost;
							frontier.Enqueue(neighbor, totalCost);
						}
					}
				}
			}
		}

		///<summary>Get a list of points from pathSource to a local minimum on an already-scanned DijkstraMap.</summary>
		public List<Point> GetDownhillPath(Point pathSource, bool preferCardinalDirections, bool includePathSource = false, bool includePathDestination = true,
			bool ignorePathSourceCost = true, Func<Point, bool> earlyStopCondition = null) //todo, does preferCardinal need to be an enum that allows calculation from the source OR the target?
		{
			List<Point> path = new List<Point>();
			Point current = pathSource;
			List<Point> lowestPoints = new List<Point>();
			if(includePathSource) path.Add(current);
			while(true){
				int currentCost = this[current];
				int lowestCost = currentCost;
				if (ignorePathSourceCost || currentCost == Blocked){
					ignorePathSourceCost = false;
					currentCost = int.MaxValue;
					lowestCost = int.MaxValue;
				}
				foreach(Point neighbor in current.GetNeighbors()){
					if(!neighbor.ExistsBetweenMapEdges()) continue;
					int value = this[neighbor];
					if(value == Unexplored || value == Blocked) continue;
					if(value == lowestCost && value < currentCost){ // If there are several cells with the same cost, consider them all. However,
						lowestPoints.Add(neighbor); // we only want to do this for the ones that are LOWER than where we started.
					}
					else if(value < lowestCost){
						lowestPoints.Clear();
						lowestPoints.Add(neighbor);
						lowestCost = value;
					}
				}
				if(lowestPoints.Count == 0){
					if(!includePathDestination){
						path.RemoveAt(path.Count - 1); // It was already added, so simply remove it before returning
					}
					return path;
				}
				if(preferCardinalDirections){
					int lowestIndex = int.MaxValue;
					int lowestHalfStepCost = int.MaxValue;
					for(int i=0;i<lowestPoints.Count;++i){
						int value = current.GetHalfStepMetricDistance(lowestPoints[i]);
						if(value < lowestHalfStepCost){
							lowestIndex = i;
							lowestHalfStepCost = value;
						}
					}
					current = lowestPoints[lowestIndex];
				}
				else{
					current = lowestPoints[0];
				}
				if(earlyStopCondition?.Invoke(current) == true){
					if(includePathDestination) path.Add(current);
					return path;
				}
				path.Add(current);
				lowestPoints.Clear();
			}
		}
	}
}
