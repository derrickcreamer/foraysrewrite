using System;

namespace Forays {
	public enum TileType { Floor, Wall, DeepWater, Staircase, ThickIce, FirePit, FirePitUnlit,
		Statue, Brush, PoppyField, GlowingFungus, StandingTorch, WallMountedTorch, Barrel,
		Demonstone, LightCrystal, GiantMushroom, DeepMud, Lava
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
		// todo: on the surface, having IsPassable seems like it might conflict with the idea that certain enemy types are able
		// or unable to move through certain terrain, but I think this might work
		// (1) as passability for projectiles etc., and (2) as a default for creature movement - maybe there's a dictionary that only exists if
		// a specific creature type overrides any of these.
		public static bool IsPassable(TileType type){
			switch(type){
				case TileType.Floor:
				case TileType.DeepWater:
				case TileType.Staircase:
				case TileType.ThickIce:
				case TileType.FirePit:
				case TileType.FirePitUnlit:
				case TileType.Brush:
				case TileType.PoppyField:
				case TileType.GlowingFungus:
				case TileType.WallMountedTorch:
				case TileType.Demonstone:
				case TileType.DeepMud:
				case TileType.Lava:
					return true;
				default:
					return false;
			}
		}
		public static bool IsOpaque(TileType type){
			switch(type){
				case TileType.Wall:
				case TileType.GiantMushroom:
					return true;
				default:
					return false;
			}
		}
		///<summary>Null means no light radius, while a radius of 0 will illuminate only this tile's cell.</summary>
		public static int? LightRadius(TileType type){
			switch(type){
				case TileType.FirePit:
					return 1;
				case TileType.GlowingFungus:
					return 0;
				case TileType.StandingTorch:
				case TileType.WallMountedTorch:
					return 3;
				case TileType.LightCrystal:
					return 5;
				case TileType.Lava:
					return 2;
				default:
					return null;
			}
		}
	}
}
