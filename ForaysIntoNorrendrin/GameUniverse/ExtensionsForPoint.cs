using System;
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
	}
}
