using System;
using GameComponents;

namespace Forays {
	public enum TileType { Floor, Wall }; //todo - all caps or not? make it consistent. leaning toward not all caps...
	public class Tile : GameObject, IPhysicalObject {
		public Point? Position => Tiles.TryGetPositionOf(this, out Point p) ? p : (Point?)null;
		public TileType Type;
		//revealed by light bool
		//current light radius

		public Tile(GameUniverse g) : base(g) { }

	}
}
