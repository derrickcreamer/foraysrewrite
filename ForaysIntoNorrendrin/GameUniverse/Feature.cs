using System;
using GameComponents;

namespace Forays {
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
		Water = 1 << 18,
		Ice = 1 << 19,
		CrackedIce = 1 << 20,
		BrokenIce = 1 << 21,
	};

	public static class FeatureDefinition{
		public static bool HasFeature(this FeatureType feature, FeatureType flag) => (feature & flag) == flag;
		private const FeatureType opaqueMask = FeatureType.Fog | FeatureType.ThickDust;
		public static bool IsOpaque(this FeatureType feature) => (feature & opaqueMask) > 0L;
		///<summary>Null means no light radius, while a radius of 0 will illuminate only this feature's cell.</summary>
		public static int? LightRadius(this FeatureType feature){ //todo, not used yet, right?
			if(feature.HasFeature(FeatureType.Fire)) return 1;
			return null;
		}
	}

	public class FeatureMap : PointArray<FeatureType>
	{
		public FeatureMap(int width, int height) : base(width, height){ }
		public void Add(Point p, FeatureType feature){
			this[p] |= feature;
		}
		public void Add(int x, int y, FeatureType feature){
			this[x, y] |= feature;
		}
		public void Remove(Point p, FeatureType feature){
			this[p] &= (~feature);
		}
		public void Remove(int x, int y, FeatureType feature){
			this[x, y] &= (~feature);
		}
	}
}
