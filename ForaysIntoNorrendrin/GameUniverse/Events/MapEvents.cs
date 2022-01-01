using System;
using System.Collections.Generic;
using GameComponents;

namespace Forays {
	public abstract class SinglePointMapEvent<TResult> : Event<TResult> where TResult : EventResult, new() {
		public virtual Point Point { get; set; }
		public SinglePointMapEvent(Point point, GameUniverse g) : base(g) { this.Point = point; }
	}
	public abstract class MultiPointMapEvent<TResult> : Event<TResult> where TResult : EventResult, new() {
		public virtual List<Point> Points { get; set; }
		public MultiPointMapEvent(List<Point> points, GameUniverse g) : base(g) { this.Points = points; }
	}
	public class CheckForIceCrackingEvent : SinglePointMapEvent<SimpleEvent.NullResult> {
		public CheckForIceCrackingEvent(Point point, GameUniverse g) : base(point, g) { }
		//todo, out of bounds or not ice... public override bool IsInvalid =>
		protected override SimpleEvent.NullResult Execute() {
			// Thin ice might crack, and the crack might keep going:

			//todo, check levitate, small size, etc.
			// large == automatic break, right?
			const int chance = 7;
			if(R.OneIn(chance)){
				HashSet<Point> visited = new HashSet<Point>();
				List<Point> visitNext = new List<Point>();
				List<Point> cracked = new List<Point>();
				visited.Add(Point);
				visitNext.Add(Point);
				cracked.Add(Point);
				while(visitNext.Count > 0){
					Point p = visitNext[visitNext.Count - 1];
					visitNext.RemoveAt(visitNext.Count - 1);
					foreach(Point neighbor in p.GetNeighbors()){
						if(visited.Contains(neighbor)) continue;
						visited.Add(neighbor);
						if(TileTypeAt(neighbor) == TileType.DeepWater
							&& Map.FeaturesAt(neighbor).HasFeature(FeatureType.Ice)
							&& R.OneIn(chance))
						{
							cracked.Add(neighbor);
							visitNext.Add(neighbor);
						}
					}
				}
				Q.Execute(new IceCrackingEvent(cracked, GameUniverse));
			}
			return null;
		}
	}
	public class IceCrackingEvent : MultiPointMapEvent<SimpleEvent.NullResult> {
		public IceCrackingEvent(List<Point> points, GameUniverse g) : base(points, g) { }
		//todo, out of bounds or not ice... public override bool IsInvalid =>
		protected override SimpleEvent.NullResult Execute() {
			foreach(Point p in Points){
				//todo remove check once IsInvalid is done:
				if(!Map.FeaturesAt(p).HasFeature(FeatureType.Ice))
					throw new InvalidOperationException("not ice");
				Map.Features.Remove(p, FeatureType.Ice);
				Map.Features.Add(p, FeatureType.CrackedIce);
			}
			return null;
		}
	}
	public class CheckForIceBreakingEvent : SinglePointMapEvent<SimpleEvent.NullResult> {
		public CheckForIceBreakingEvent(Point point, GameUniverse g) : base(point, g) { }
		//todo, out of bounds or not ice... public override bool IsInvalid =>
		protected override SimpleEvent.NullResult Execute() {
			// Cracked ice might break. The chance is lower based on how much solid ground & non-cracked ice is adjacent.
			int chance = 1;
			foreach(Point neighbor in Point.GetNeighbors()){
				TileType tt = TileTypeAt(neighbor);
				if(tt != TileType.DeepWater) continue;
				FeatureType ft = FeaturesAt(neighbor);
				if(ft.HasFeature(FeatureType.Ice)) continue;
				else chance++;
			}
			if(R.FractionalChance(chance, 9)){
				Q.Execute(new IceBreakingEvent(Point, GameUniverse));
			}
			return null;
		}
	}
	public class IceBreakingEvent : SinglePointMapEvent<SimpleEvent.NullResult> {
		public IceBreakingEvent(Point point, GameUniverse g) : base(point, g) { }
		//todo, out of bounds or not ice... public override bool IsInvalid =>
		protected override SimpleEvent.NullResult Execute() {
			//todo remove check once IsInvalid is done:
			if(!Map.FeaturesAt(Point).HasFeature(FeatureType.CrackedIce))
				throw new InvalidOperationException("not ice");
			Map.Features.Remove(Point, FeatureType.CrackedIce);
			Map.Features.Add(Point, FeatureType.BrokenIce);
			return null;
		}
	}
}
