using System;
using System.Collections.Generic;
using GameComponents;

namespace ForaysUI.ScreenUI.MapRendering{
	public enum MapHighlightType { SinglePoint, Path, Line }; //todo, should ExtendedLine be included?
	public class Highlight{
		public MapHighlightType Type;
		public Point Source;
		public Point Destination;
		public IList<Point> LineOrPath;
		public Point? BlockedPoint;
		public int? Radius;
		public IList<Point> CellsInRadius;
		public Highlight(MapHighlightType type) { Type = type; }
	}
}
