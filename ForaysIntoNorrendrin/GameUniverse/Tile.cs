using System;

namespace Forays {
	public enum TileType { Floor, Wall, Water, Staircase
	};

	[Flags]
	public enum FeatureType : ulong {
		None = 0,
		Fire = 1,
		PoisonGas = 1 << 1,
		Fog = 1 << 2,
		Spores = 1 << 3,
		ThickDust = 1 << 4,
		PixieDust = 1 << 5,
		ConfusionGas = 1 << 6,
		Slime = 1 << 7,
		Oil = 1 << 8,
		Grenade = 1 << 9,
		Web = 1 << 10,
		ForasectEgg = 1 << 11,
		Teleportal = 1 << 12,
		InactiveTeleportal = 1 << 13,
		StableTeleportal = 1 << 14,
		Bones = 1 << 15,
		TrollCorpse = 1 << 16,
		TrollBloodwitchCorpse = 1 << 17,
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
		public static bool HasFeature(this FeatureType feature, FeatureType flag) => (feature & flag) == flag;
		private const FeatureType opaqueMask = FeatureType.Fog | FeatureType.ThickDust;
		public static bool IsOpaque(this FeatureType feature) => (feature & opaqueMask) > 0L;
		public static int LightRadius(this FeatureType feature){ //todo, not used yet, right?
			if(feature.HasFeature(FeatureType.Fire)) return 1;
			return 0;
		}
	}
}
