using System;
using System.Collections.Generic;
using GameComponents;

namespace ForaysUI.ScreenUI.MapRenderer{
	public enum HighlightType { SinglePoint, Path, Line }; //todo, should ExtendedLine be included?
	public class Highlight{
		public HighlightType Type;
		public Point Source;
		public Point Destination;
		public IList<Point> LineOrPath;
		public Point? BlockedPoint;
		public int? Radius;
		public IList<Point> CellsInRadius;
		public Highlight(HighlightType type) { Type = type; }
	}
}
