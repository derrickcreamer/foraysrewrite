using System;
using System.Collections.Generic;

namespace GameComponents.DirectionUtility {

	// Arranged like a numpad:
	public enum Dir8 { N = 8, NE = 9, E = 6, SE = 3, S = 2, SW = 1, W = 4, NW = 7, Neutral = 5, None = 0 };
	public enum Dir4 { N = 8, E = 6, S = 2, W = 4, Neutral = 5, None = 0 };

	public static class EightDirections {
		public readonly static Dir8[] Eight = { Dir8.N, Dir8.NE, Dir8.E, Dir8.SE, Dir8.S, Dir8.SW, Dir8.W, Dir8.NW };
		public readonly static Dir8[] Nine = { Dir8.Neutral, Dir8.N, Dir8.NE, Dir8.E, Dir8.SE, Dir8.S, Dir8.SW, Dir8.W, Dir8.NW };
		public readonly static Dir8[] Orthogonals = { Dir8.N, Dir8.E, Dir8.S, Dir8.W };
		public readonly static Dir8[] Diagonals = { Dir8.NE, Dir8.SE, Dir8.SW, Dir8.NW };
		//todo, old code had pairs...not sure what to do with these yet. add only if needed:
		//public static int[][] FourDirectionPairs = { new int[] { 8, 2 }, new int[] { 6, 4 } };
		//public static int[][] EightDirectionPairs = { new int[] { 8, 2 }, new int[] { 9, 1 }, new int[] { 6, 4 }, new int[] { 3, 7 } };

		public static IEnumerable<Dir8> Enumerate(bool clockwise, bool includeNeutral, bool orthogonalsFirst, Dir8 startDir = Dir8.N) {
			if(includeNeutral) yield return Dir8.Neutral;
			if(orthogonalsFirst) {
				Dir4 orthoDir, diagDir;
				if(startDir.IsOrthogonal()) {
					orthoDir = (Dir4)startDir;
					diagDir = (Dir4)startDir.Rotate(clockwise);
				}
				else {
					orthoDir = (Dir4)startDir.Rotate(clockwise);
					diagDir = (Dir4)startDir;
				}
				for(int i = 0; i<4; ++i) {
					yield return (Dir8)orthoDir;
					orthoDir = orthoDir.Rotate(clockwise);
				}
				for(int i = 0; i<4; ++i) {
					yield return (Dir8)diagDir;
					diagDir = diagDir.Rotate(clockwise);
				}
			}
			else {
				for(int i = 0; i<8; ++i) {
					yield return startDir;
					startDir = startDir.Rotate(clockwise);
				}
			}
		}
	}
	public static class FourDirections {
		public readonly static Dir4[] Four = { Dir4.N, Dir4.E, Dir4.S, Dir4.W };
		public readonly static Dir4[] Five = { Dir4.Neutral, Dir4.N, Dir4.E, Dir4.S, Dir4.W };
		public static IEnumerable<Dir4> Enumerate(bool clockwise, bool includeNeutral, Dir4 startDir = Dir4.N) {
			if(includeNeutral) yield return Dir4.Neutral;
			for(int i = 0; i<4; ++i) {
				yield return startDir;
				startDir = startDir.Rotate(clockwise);
			}
		}
	}
	public static class DirectionExtensions {
		public static Point PointInDir(this Point source, Dir8 dir) {
			switch(dir) {
				case Dir8.N: return new Point(source.X, source.Y + 1);
				case Dir8.NE: return new Point(source.X + 1, source.Y + 1);
				case Dir8.E: return new Point(source.X + 1, source.Y);
				case Dir8.SE: return new Point(source.X + 1, source.Y - 1);
				case Dir8.S: return new Point(source.X, source.Y - 1);
				case Dir8.SW: return new Point(source.X - 1, source.Y - 1);
				case Dir8.W: return new Point(source.X - 1, source.Y);
				case Dir8.NW: return new Point(source.X - 1, source.Y + 1);
				case Dir8.Neutral: return source;
			}
			throw new ArgumentException($"Invalid direction: {dir}");
		}
		public static Point PointInDir(this Point source, Dir4 dir) => source.PointInDir((Dir8)dir);

		public static bool IsOrthogonal(this Dir8 dir) => dir == Dir8.N || dir == Dir8.E || dir == Dir8.S || dir == Dir8.W;
		public static bool IsOrthogonal(this Dir4 dir) => dir == Dir4.N || dir == Dir4.E || dir == Dir4.S || dir == Dir4.W;
		public static bool IsDiagonal(this Dir8 dir) => dir == Dir8.NE || dir == Dir8.SE || dir == Dir8.SW || dir == Dir8.NW;
		public static bool IsValid(this Dir8 dir, bool allowNeutral = true) {
			if(dir == Dir8.Neutral) return allowNeutral;
			return dir == Dir8.N || dir == Dir8.E || dir == Dir8.S || dir == Dir8.W
				|| dir == Dir8.NE || dir == Dir8.SE || dir == Dir8.SW || dir == Dir8.NW;
		}
		public static bool IsValid(this Dir4 dir, bool allowNeutral = true) {
			if(dir == Dir4.Neutral) return allowNeutral;
			return dir == Dir4.N || dir == Dir4.E || dir == Dir4.S || dir == Dir4.W;
		}

		public static Dir8 Rotate(this Dir8 dir, bool clockwise) {
			switch(dir) {
				case Dir8.N:
					if(clockwise) return Dir8.NE;
					else return Dir8.NW;
				case Dir8.NE:
					if(clockwise) return Dir8.E;
					else return Dir8.N;
				case Dir8.E:
					if(clockwise) return Dir8.SE;
					else return Dir8.NE;
				case Dir8.SE:
					if(clockwise) return Dir8.S;
					else return Dir8.E;
				case Dir8.S:
					if(clockwise) return Dir8.SW;
					else return Dir8.SE;
				case Dir8.SW:
					if(clockwise) return Dir8.W;
					else return Dir8.S;
				case Dir8.W:
					if(clockwise) return Dir8.NW;
					else return Dir8.SW;
				case Dir8.NW:
					if(clockwise) return Dir8.N;
					else return Dir8.W;
				case Dir8.Neutral: return Dir8.Neutral;
				default: return dir;
			}
		}
		public static Dir8 Rotate(this Dir8 dir, bool clockwise, int times) {
			if(dir == Dir8.Neutral) return Dir8.Neutral;
			if(times < 0) {
				times = -times;
				clockwise = !clockwise;
			}
			for(int i = 0; i<times; ++i) {
				dir = dir.Rotate(clockwise);
			}
			return dir;
		}
		public static Dir4 Rotate(this Dir4 dir, bool clockwise) {
			switch(dir) {
				case Dir4.N:
					if(clockwise) return Dir4.E;
					else return Dir4.W;
				case Dir4.E:
					if(clockwise) return Dir4.S;
					else return Dir4.N;
				case Dir4.S:
					if(clockwise) return Dir4.W;
					else return Dir4.E;
				case Dir4.W:
					if(clockwise) return Dir4.N;
					else return Dir4.S;
				// Special handling for diagonals, even though they aren't named Dir4 values:
				case (Dir4)Dir8.NE:
					if(clockwise) return (Dir4)Dir8.SE;
					else return (Dir4)Dir8.NW;
				case (Dir4)Dir8.SE:
					if(clockwise) return (Dir4)Dir8.SW;
					else return (Dir4)Dir8.NE;
				case (Dir4)Dir8.SW:
					if(clockwise) return (Dir4)Dir8.NW;
					else return (Dir4)Dir8.SE;
				case (Dir4)Dir8.NW:
					if(clockwise) return (Dir4)Dir8.NE;
					else return (Dir4)Dir8.SW;
				case Dir4.Neutral: return Dir4.Neutral;
				default: return dir;
			}
		}
		public static Dir4 Rotate(this Dir4 dir, bool clockwise, int times) {
			if(dir == Dir4.Neutral) return Dir4.Neutral;
			if(times < 0) {
				times = -times;
				clockwise = !clockwise;
			}
			for(int i = 0; i<times; ++i) {
				dir = dir.Rotate(clockwise);
			}
			return dir;
		}
		public static IEnumerable<Point> EnumeratePointsWithinChebyshevDistance(this Point source, int distance,
			bool clockwise, bool orderByEuclideanDistance, Dir8 startDir = Dir8.N)
		{
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal or diagonal");
			if(orderByEuclideanDistance && startDir.IsDiagonal()) {
				// If ordering by Euclidean distance, we'll be starting at an orthogonal direction anyway:
				startDir = startDir.Rotate(clockwise);
			}
			for(int currentDistance = 0; currentDistance <= distance; ++currentDistance) {
				foreach(Point p in source.PointsAtChebyshevDistanceInternal(currentDistance, clockwise, orderByEuclideanDistance, startDir)) {
					yield return p;
				}
			}
		}
		//todo needs xml notes
		public static IEnumerable<Dir8> GetDirectionsInArc(this Dir8 dir, int distance, bool clockwise, bool extendArcInBothDirections) {
			int startIdx;
			if(extendArcInBothDirections) {
				dir = dir.Rotate(!clockwise, distance);
				startIdx = -distance;
			}
			else startIdx = 0;

			for(int i = startIdx; i <= distance; ++i) {
				yield return dir;
				dir = dir.Rotate(clockwise);
			}
		}
		public static IEnumerable<Point> EnumeratePointsByChebyshevDistance(
			this Point source, bool clockwise, bool orderByEuclideanDistance, Dir8 startDir = Dir8.N)
		{
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal or diagonal");
			if(orderByEuclideanDistance && startDir.IsDiagonal()) {
				// If ordering by Euclidean distance, we'll be starting at an orthogonal direction anyway:
				startDir = startDir.Rotate(clockwise);
			}
			for(int distance = 0; ; ++distance) {
				foreach(Point p in source.PointsAtChebyshevDistanceInternal(distance, clockwise, orderByEuclideanDistance, startDir)) {
					yield return p;
				}
			}
		}
		public static IEnumerable<Point> EnumeratePointsAtChebyshevDistance(
			this Point source, int distance, bool clockwise, bool orderByEuclideanDistance, Dir8 startDir = Dir8.N)
		{
			if(distance < 0) throw new ArgumentException("distance must be nonnegative");
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal or diagonal");
			if(orderByEuclideanDistance && startDir.IsDiagonal()) {
				// If ordering by Euclidean distance, we'll be starting at an orthogonal direction anyway:
				startDir = startDir.Rotate(clockwise);
			}
			return source.PointsAtChebyshevDistanceInternal(distance, clockwise, orderByEuclideanDistance, startDir);
		}
		private static IEnumerable<Point> PointsAtChebyshevDistanceInternal(
			this Point source, int distance, bool clockwise, bool orderByEuclideanDistance, Dir8 startDir)
		{
			if(distance == 0) {
				yield return source;
				yield break;
			}
			if(orderByEuclideanDistance) {
				Dir4 orthoDir = (Dir4)startDir; // If this bool is true, startDir must be orthogonal
				foreach(Point p in source.StartingPointsAtChebyshevDistance(distance, clockwise, orthoDir)) yield return p;
				if(distance > 1) {
					var enumerators = new IEnumerator<Point>[4];
					Dir4 currentDir = orthoDir;
					for(int i = 0; i<4; ++i) {
						enumerators[i] = source.MiddlePointsAtChebyshevDistance(distance, clockwise, currentDir).GetEnumerator();
						currentDir = currentDir.Rotate(clockwise);
					}
					// MiddlePointsAtChebyshevDistance *must* provide pairs, so 2 points are yielded for each, per iteration:
					while(enumerators[0].MoveNext() && enumerators[1].MoveNext() && enumerators[2].MoveNext() && enumerators[3].MoveNext()) {
						for(int i = 0; i<4; ++i) {
							yield return enumerators[i].Current;
							enumerators[i].MoveNext();
							yield return enumerators[i].Current;
						}
					}
				}
				foreach(Point p in source.FinalPointsAtChebyshevDistance(distance, clockwise, orthoDir)) yield return p;
			}
			else {
				Dir8 currentDir = startDir;
				for(int i = 0; i<8; ++i) {
					foreach(Point p in source.PointsInOctantAtChebyshevDistance(distance, clockwise, currentDir)) yield return p;
					currentDir = currentDir.Rotate(clockwise);
				}
			}
		}

		private static IEnumerable<Point> StartingPointsAtChebyshevDistance(
			this Point source, int distance, bool clockwise, Dir4 startDir)
		{
			Dir4 currentDir = startDir;
			for(int i = 0; i<4; ++i) {
				switch(currentDir) {
					case Dir4.N: yield return new Point(source.X, source.Y + distance); break;
					case Dir4.E: yield return new Point(source.X + distance, source.Y); break;
					case Dir4.S: yield return new Point(source.X, source.Y - distance); break;
					case Dir4.W: yield return new Point(source.X - distance, source.Y); break;
				}
				currentDir = currentDir.Rotate(clockwise);
			}
		}
		private static IEnumerable<Point> FinalPointsAtChebyshevDistance(
			this Point source, int distance, bool clockwise, Dir4 startDir)
		{
			// Get the appropriate corner:
			Dir8 diagDir = ((Dir8)startDir).Rotate(clockwise);
			Dir4 currentDir = (Dir4)diagDir; // Use 4-way rotation around the diagonals
			for(int i = 0; i<4; ++i) {
				switch((Dir8)currentDir) {
					case Dir8.NE: yield return new Point(source.X + distance, source.Y + distance); break;
					case Dir8.SE: yield return new Point(source.X + distance, source.Y - distance); break;
					case Dir8.SW: yield return new Point(source.X - distance, source.Y - distance); break;
					case Dir8.NW: yield return new Point(source.X - distance, source.Y + distance); break;
				}
				currentDir = currentDir.Rotate(clockwise);
			}
		}
		private static IEnumerable<Point> MiddlePointsAtChebyshevDistance(
			this Point source, int distance, bool clockwise, Dir4 dir)
		{
			// Get the appropriate corner:
			Dir8 diagDir = ((Dir8)dir).Rotate(clockwise);
			switch(diagDir) {
				case Dir8.NE:
					// Each corner covers the same points, in the same pairs,
					// but the order depends on which way we're going:
					if(clockwise) {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y + distance);
							yield return new Point(source.X + distance, source.Y + i);
						}
					}
					else {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y + i);
							yield return new Point(source.X + i, source.Y + distance);
						}
					}
					break;
				case Dir8.SE:
					if(clockwise) {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y - i);
							yield return new Point(source.X + i, source.Y - distance);
						}
					}
					else {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y - distance);
							yield return new Point(source.X + distance, source.Y - i);
						}
					}
					break;
				case Dir8.SW:
					if(clockwise) {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y - distance);
							yield return new Point(source.X - distance, source.Y - i);
						}
					}
					else {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y - i);
							yield return new Point(source.X - i, source.Y - distance);
						}
					}
					break;
				case Dir8.NW:
					if(clockwise) {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y + i);
							yield return new Point(source.X - i, source.Y + distance);
						}
					}
					else {
						for(int i = 1; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y + distance);
							yield return new Point(source.X - distance, source.Y + i);
						}
					}
					break;
			}
		}
		private static IEnumerable<Point> PointsInOctantAtChebyshevDistance(this Point source, int distance, bool clockwise, Dir8 dir) {
			switch(dir) {
				case Dir8.N:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y + distance);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y + distance);
						}
					}
					break;
				case Dir8.NE:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y + distance - i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance - i, source.Y + distance);
						}
					}
					break;
				case Dir8.E:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y - i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y + i);
						}
					}
					break;
				case Dir8.SE:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance - i, source.Y - distance);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance, source.Y - distance + i);
						}
					}
					break;
				case Dir8.S:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y - distance);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y - distance);
						}
					}
					break;
				case Dir8.SW:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y - distance + i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance + i, source.Y - distance);
						}
					}
					break;
				case Dir8.W:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y + i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y - i);
						}
					}
					break;
				case Dir8.NW:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance + i, source.Y + distance);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance, source.Y + distance - i);
						}
					}
					break;
			}
		}
		public static IEnumerable<Point> EnumeratePointsWithinManhattanDistance(
			this Point source, int distance, bool clockwise, Dir4 startDir = Dir4.N)
		{
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal direction");
			for(int currentDistance = 0; currentDistance <= distance; ++currentDistance) {
				foreach(Point p in source.PointsAtManhattanDistanceInternal(currentDistance, clockwise, startDir)) {
					yield return p;
				}
			}
		}
		public static IEnumerable<Point> EnumeratePointsByManhattanDistance(
			this Point source, bool clockwise, Dir4 startDir = Dir4.N)
		{
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal direction");
			for(int distance = 0; ; ++distance) {
				foreach(Point p in source.PointsAtManhattanDistanceInternal(distance, clockwise, startDir)) {
					yield return p;
				}
			}
		}
		public static IEnumerable<Point> EnumeratePointsAtManhattanDistance(
			this Point source, int distance, bool clockwise, Dir4 startDir = Dir4.N)
		{
			if(distance < 0) throw new ArgumentException("distance must be nonnegative");
			if(!startDir.IsValid(false)) throw new ArgumentException("startDir must be an orthogonal direction");
			return source.PointsAtManhattanDistanceInternal(distance, clockwise, startDir);
		}
		private static IEnumerable<Point> PointsAtManhattanDistanceInternal(
			this Point source, int distance, bool clockwise, Dir4 startDir)
		{
			if(distance == 0) {
				yield return source;
				yield break;
			}
			Dir4 currentDir = startDir;
			for(int i = 0; i<4; ++i) {
				foreach(Point p in source.PointsInQuadrantAtManhattanDistance(distance, clockwise, currentDir)) yield return p;
				currentDir = currentDir.Rotate(clockwise);
			}
		}
		private static IEnumerable<Point> PointsInQuadrantAtManhattanDistance(
			this Point source, int distance, bool clockwise, Dir4 dir)
		{
			switch(dir) {
				case Dir4.N:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y + distance - i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y + distance - i);
						}
					}
					break;
				case Dir4.E:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance - i, source.Y - i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + distance - i, source.Y + i);
						}
					}
					break;
				case Dir4.S:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - i, source.Y - distance + i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X + i, source.Y - distance + i);
						}
					}
					break;
				case Dir4.W:
					if(clockwise) {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance + i, source.Y + i);
						}
					}
					else {
						for(int i = 0; i<distance; ++i) {
							yield return new Point(source.X - distance + i, source.Y - i);
						}
					}
					break;
			}
		}
	}
}
