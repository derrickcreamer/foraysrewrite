using System;
using System.Collections.Generic;
using GameComponents;
using GameComponents.DirectionUtility;

namespace Forays{
	public static class ExtensionsForPoint{
		///<summary>Can be used for any 2 points, but note that this does NOT divide space equally into 8 octants.
		/// It returns a cardinal direction only if one axis has no change at all.</summary>
		public static Dir8 GetDirectionOfNeighbor(this Point source, Point neighbor){ //todo, maybe this should go into GameComponents.DirectionUtility
			if(neighbor.Y > source.Y) // North
				if(neighbor.X > source.X)
					return Dir8.NE;
				else if(neighbor.X < source.X)
					return Dir8.NW;
				else
					return Dir8.N;
			else if(neighbor.Y < source.Y) // South
				if(neighbor.X > source.X)
					return Dir8.SE;
				else if(neighbor.X < source.X)
					return Dir8.SW;
				else
					return Dir8.S;
			else
				if(neighbor.X > source.X)
					return Dir8.E;
				else if(neighbor.X < source.X)
					return Dir8.W;
				else
					return Dir8.Neutral;
		}
		public static IEnumerable<Point> GetNeighbors(this Point source){
			foreach(Dir8 dir in EightDirections.Enumerate(true, false, false)){
				yield return source.PointInDir(dir);
			}
		}
		///<summary>Indicates whether this Point represents a valid map position.</summary>
		public static bool ExistsOnMap(this Point p){
			return p.X>=0 && p.Y>=0 && p.X<=GameUniverse.MapWidth-1 && p.Y<=GameUniverse.MapHeight-1;
		}
		///<summary>Indicates whether this Point represents a valid map position, excluding map edges.</summary>
		public static bool ExistsBetweenMapEdges(this Point p){
			return p.X>0 && p.Y>0 && p.X<GameUniverse.MapWidth-1 && p.Y<GameUniverse.MapHeight-1;
		}
		public static bool IsMapEdge(this Point p){
			return p.X==0 || p.Y==0 || p.X==GameUniverse.MapWidth-1 || p.Y==GameUniverse.MapHeight-1;
		}
		///<summary>The distance is 10 per cardinal step plus 15 per diagonal step, so this metric is closer to Euclidean than the others.</summary>
		public static int GetHalfStepMetricDistance(this Point p1, Point p2){
			int dy = Math.Abs(p1.Y - p2.Y);
			int dx = Math.Abs(p1.X - p2.X);
			if(dx > dy) return dx*10 + dy*5;
			else return dy*10 + dx*5;
		}
		///<summary>Returns the point at the given distance in the given direction.</summary>
		public static Point PointInDir(this Point source, Dir8 dir, int distance){
			switch(dir){
				case Dir8.N: return new Point(source.X, source.Y + distance);
				case Dir8.S: return new Point(source.X, source.Y - distance);
				case Dir8.E: return new Point(source.X + distance, source.Y);
				case Dir8.W: return new Point(source.X - distance, source.Y);
				case Dir8.NE: return new Point(source.X + distance, source.Y + distance);
				case Dir8.SE: return new Point(source.X + distance, source.Y - distance);
				case Dir8.SW: return new Point(source.X - distance, source.Y - distance);
				case Dir8.NW: return new Point(source.X - distance, source.Y + distance);
				default: return source;
			}
		}
		///<summary>Get the points next to the target that are between target and observer - one or two results, or zero if target==obs.</summary>
		public static Point[] GetNeighborsBetween(this Point target, Point observer){
			int x1 = target.X;
			int y1 = target.Y;
			int x2 = observer.X;
			int y2 = observer.Y;
			int dx = Math.Abs(x2 - x1);
			int dy = Math.Abs(y2 - y1);
			int incrementX = x1 == x2? 0 : x1 < x2? 1 : -1;
			int incrementY = y1 == y2? 0 : y1 < y2? 1 : -1;
			if(dx == 0 && dy == 0) return new Point[0];
			Point p1 = new Point(x1 + incrementX, y1 + incrementY);
			if(dy < dx && dy != 0){
				return new Point[2] { p1, new Point(x1 + incrementX, y1) };
			}
			else if(dx < dy && dx != 0){
				return new Point[2] { p1, new Point(x1, y1 + incrementY) };
			}
			else{
				return new Point[1] { p1 };
			}
		}
	}
}
