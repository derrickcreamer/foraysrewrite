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
		public static bool HasLOS(this Point source, Point destination, PointArray<TileType> map){ //todo, move this, to use NeverInLOS from DungeonMap
			if(TileDefinition.IsOpaque(map[destination])){
				Point[] neighbors = GetNeighborsBetween(destination, source);
				for(int i=0;i<neighbors.Length;++i){
					if(TileDefinition.IsOpaque(map[neighbors[i]])) continue;
					if(CheckReciprocalBresenhamLineOfSight(source, neighbors[i], map)) return true;
				}
				return false;
			}
			else return CheckReciprocalBresenhamLineOfSight(source, destination, map);
		}
		public static bool CheckReciprocalBresenhamLineOfSight(this Point source, Point destination, PointArray<TileType> map){ //todo, what to do with map?
			int x1 = source.X;
			int y1 = source.Y;
			int x2 = destination.X;
			int y2 = destination.Y;
			int dx = Math.Abs(x2 - x1);
			int dy = Math.Abs(y2 - y1);
			int incrementX = x1 == x2? 0 : x1 < x2? 1 : -1;
			int incrementY = y1 == y2? 0 : y1 < y2? 1 : -1;
			if(dx <= 1 && dy <= 1) return true; // Automatically pass the check for anything in the same or adjacent cells.
			// Next, handle simple cases that don't need Bresenham at all. These cases correspond to straight lines in the 8 directions:
			if(dx == 0 || dy == 0 || (y1+x1 == y2+x2) || (y1-x1 == y2-x2)){ // (if slope is undefined, 0, -1, or 1)
				do{
					x1 += incrementX; // Increment first, so that the opacity of 'source' is ignored.
					y1 += incrementY;
					if(TileDefinition.IsOpaque(map[x1, y1])) return false;
				} while(x1 != x2 || y1 != y2);
				return true;
			}
			// If it wasn't a simple case, move on to reciprocal Bresenham:
			bool xMajor = (dx > dy);
			int er = 0; // error accumulator
			bool blockedA = false; // These 2 are used to track whether each of the 2 possible paths per line is blocked or not.
			bool blockedB = false; // (The result is equivalent to calculating 2 regular Bresenham lines, source->dest and dest->source.)
			if(xMajor){
				do{
					x1 += incrementX;
					er += dy;
					if(er<<1 > dx){
						y1 += incrementY;
						er -= dx;
					}
					if(TileDefinition.IsOpaque(map[x1, y1])){
						if(blockedB || er<<1 != dx) return false;
						blockedA = true;
					}
					if(er<<1 == dx){ // This is the part that makes this reciprocal, by checking both options while crossing a corner.
						y1 += incrementY; // Increment Y, then check the new position:
						if(TileDefinition.IsOpaque(map[x1, y1])){
							if(blockedA || er<<1 != dx) return false;
							blockedB = true;
						}
						er -= dx;
					}
				} while(x1 != x2);
			}
			else{ // Y-major
				do{
					y1 += incrementY;
					er += dx;
					if(er<<1 > dy){
						x1 += incrementX;
						er -= dy;
					}
					if(TileDefinition.IsOpaque(map[x1, y1])){
						if(blockedB || er<<1 != dy) return false;
						blockedA = true;
					}
					if(er<<1 == dy){
						x1 += incrementX;
						if(TileDefinition.IsOpaque(map[x1, y1])){
							if(blockedA || er<<1 != dy) return false;
							blockedB = true;
						}
						er -= dy;
					}
				} while(y1 != y2);
			}
			return true;
		}
	}
}
