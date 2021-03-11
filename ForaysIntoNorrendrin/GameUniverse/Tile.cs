using System;

namespace Forays {
	public enum TileType { Floor, Wall, DeepWater, Staircase, ThickIce
	};

	public enum TrapType { Fire, Light, Teleport, SlidingWall, Grenade, Shock, Alarm, Darkness,
		PoisonGas, Blinding, Ice, Phantom, ScaldingOil, Fling, StoneRain
	};

	public struct Trap{
		public readonly TrapType Type;
		public readonly bool SafeForPlayer;
		public readonly bool TypeRevealedToPlayer;
		public Trap(TrapType type, bool safe, bool typeKnown){
			Type = type;
			SafeForPlayer = safe;
			TypeRevealedToPlayer = typeKnown;
		}
	}

	public static class TileDefinition{
		public static bool IsPassable(TileType type){
			switch(type){
				case TileType.Floor:
				case TileType.DeepWater:
				case TileType.Staircase:
				case TileType.ThickIce:
				return true;
			}
			return false;
		}
		public static bool IsOpaque(TileType type){
			switch(type){
				case TileType.Wall:
				return true;
			}
			return false;
		}
		public static int LightRadius(TileType type){
			/*switch(type){
			}*/
			return 0;
		}
	}
}
