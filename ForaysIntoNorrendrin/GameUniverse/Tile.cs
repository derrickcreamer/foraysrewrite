using System;

namespace Forays {
	public enum TileType { Floor, Wall, Water, Staircase
	};

	public enum FeatureType { Fire, PoisonGas, Fog, Spores, ThickDust, PixieDust, ConfusionGas,
		Slime, Oil, Grenade, Web, ForasectEgg, Teleportal, InactiveTeleportal, StableTeleportal,
		Bones, TrollCorpse, TrollBloodwitchCorpse
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
				case TileType.Water:
				case TileType.Staircase:
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

	public static class FeatureDefinition{
		public static bool IsOpaque(FeatureType type){
			switch(type){
				case FeatureType.Fog:
				case FeatureType.ThickDust:
				return true;
			}
			return false;
		}
		public static int LightRadius(FeatureType type){ //todo, not used yet, right?
			switch(type){
				case FeatureType.Fire:
				return 1;
			}
			return 0;
		}
	}
}
