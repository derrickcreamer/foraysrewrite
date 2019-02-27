using System;
using GameComponents;

namespace Forays {
	public enum TileType { Floor, Wall, Water };
	/*public class Tile : GameObject, IPhysicalObject {
		public Point? Position => Tiles.TryGetPositionOf(this, out Point p) ? p : (Point?)null;
		public TileType Type;
		//revealed by light bool
		//current light radius

		public Tile(GameUniverse g) : base(g) { }

	}*/
}
