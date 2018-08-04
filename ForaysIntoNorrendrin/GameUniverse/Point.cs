using System;
using System.Collections.Generic;

namespace GameComponents {
	public struct Point : IEquatable<Point> {
		public readonly int X, Y;

		public Point(int x, int y) { this.X = x;  this.Y = y; }
		public static Point operator +(Point left, Point right) => new Point(left.X + right.X, left.Y + right.Y);
		public static Point operator -(Point left, Point right) => new Point(left.X - right.X, left.Y - right.Y);
		public static Point operator -(Point right) => new Point(-right.X, -right.Y);
		public static Point operator +(Point left, int i) => new Point(left.X + i, left.Y + i);
		public static Point operator -(Point left, int i) => new Point(left.X - i, left.Y - i);
		public static bool operator ==(Point left, Point right) => left.Equals(right);
		public static bool operator !=(Point left, Point right) => !left.Equals(right);
		public override int GetHashCode() { unchecked { return X * 7757 + Y; } }
		public override bool Equals(object other) {
			if(other is Point) return Equals((Point)other);
			else return false;
		}
		public bool Equals(Point other) => X == other.X && Y == other.Y;
		public static readonly Point Zero = new Point(0, 0);

		public int ChebyshevDistanceFrom(Point other) {
			int dy = Math.Abs(other.Y - this.Y);
			int dx = Math.Abs(other.X - this.X);
			if(dx > dy) return dx;
			else return dy;
		}
		public int ManhattanDistanceFrom(Point other) {
			int dy = Math.Abs(other.Y - this.Y);
			int dx = Math.Abs(other.X - this.X);
			return dx + dy;
		}
		public override string ToString() => $"Point ({X}, {Y})";
	}

	//add xml note that this doesn't work like System.Drawing.Rectangle or the other common ones -
	// so Right and Bottom actually correspond to points within this rectangle, not barely outside.
	public struct CellRectangle : IEquatable<CellRectangle> {
		public readonly Point Position, Size;
		public int X => Position.X;
		public int Y => Position.Y;
		public int Width => Size.X;
		public int Height => Size.Y;
		public int Left => Position.X;
		public int Right => Position.X + Size.X - 1;
		public int Top => Position.Y;
		public int Bottom => Position.Y + Size.Y - 1;
		public Point TopLeft => new Point(Left, Top);
		public Point BottomLeft => new Point(Left, Bottom);
		public Point TopRight => new Point(Right, Top);
		public Point BottomRight => new Point(Right, Bottom);
		public bool IsEmpty => Size.X <= 0 || Size.Y <= 0;

		public CellRectangle(Point position, Point size) { this.Position = position;  this.Size = size; }
		public static CellRectangle CreateFromSize(int x, int y, int width, int height) => new CellRectangle(new Point(x, y), new Point(width, height));
		public static CellRectangle CreateFromEdges(int left, int right, int top, int bottom) {
			return new CellRectangle(new Point(left, top), new Point(right-left + 1, bottom-top + 1));
		}
		public static CellRectangle CreateFromPoints(Point p1, Point p2) {
			int resultLeft = Math.Min(p1.X, p2.X);
			int resultRight = Math.Max(p1.X, p2.X);
			int resultTop = Math.Min(p1.Y, p2.Y);
			int resultBottom = Math.Max(p1.Y, p2.Y);
			return CreateFromEdges(resultLeft, resultRight, resultTop, resultBottom);
		}
		public IEnumerable<Point> Points {
			get {
				for(int i=Position.X; i<Position.X+Size.X; ++i) {
					for(int j=Position.Y; j<Position.Y+Size.Y; ++j) {
						yield return new Point(i, j);
					}
				}
			}
		}
		//todo, xml note that the arguments given to shrink/grow refer to how many layers to shrink,
		// so r.Shrink(1) will have width and height 2 less than 'r'.
		public CellRectangle Shrink(int i) => new CellRectangle(Position + i, Size - i*2);
		public CellRectangle Shrink(int dx, int dy) => new CellRectangle(Position + new Point(dx, dy), Size - new Point(dx*2, dy*2));
		public CellRectangle Grow(int i) => new CellRectangle(Position - i, Size + i*2);
		public CellRectangle Grow(int dx, int dy) => new CellRectangle(Position - new Point(dx, dy), Size + new Point(dx*2, dy*2));
		public CellRectangle Translate(Point p) => new CellRectangle(Position + p, Size); // todo: what about 90(+) degree rotation around a point?
		public bool Contains(Point p) => p.X >= Position.X && p.X < Position.X+Size.X && p.Y >= Position.Y && p.Y < Position.Y+Size.Y;
		public bool Contains(CellRectangle other) {
			return Position.X <= other.Position.X && Position.X + Size.X >= other.Position.X + other.Size.X
				&& Position.Y <= other.Position.Y && Position.Y + Size.Y >= other.Position.Y + other.Size.Y;
		}
		public bool Intersects(CellRectangle other) {
			if(this.Left > other.Right || other.Left > this.Right) return false;
			if(this.Top > other.Bottom || other.Top > this.Bottom) return false;
			return true;
		}
		public CellRectangle GetIntersection(CellRectangle other) {
			int resultLeft = Math.Max(this.Left, other.Left);
			int resultTop = Math.Max(this.Top, other.Top);
			int resultRight = Math.Min(this.Right, other.Right);
			int resultBottom = Math.Min(this.Bottom, other.Bottom);
			return CreateFromEdges(resultLeft, resultRight, resultTop, resultBottom);
		}
		public override int GetHashCode() { unchecked { return Position.GetHashCode() + Size.GetHashCode() * 5003; } }
		public override bool Equals(object other) {
			if(other is CellRectangle) return Equals((CellRectangle)other);
			else return false;
		}
		public bool Equals(CellRectangle other) => Position.Equals(other.Position) && Size.Equals(other.Size);
		public static bool operator ==(CellRectangle left, CellRectangle right) => left.Equals(right);
		public static bool operator !=(CellRectangle left, CellRectangle right) => !left.Equals(right);
		public override string ToString() => $"CellRectangle (Position ({X}, {Y}), Size ({Width}, {Height}))";
	}
}
