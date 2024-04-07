using System;
using System.Collections.Generic;
using Forays;
using GameComponents;

namespace ForaysUI.ScreenUI.MapRendering{
	// GameObjectGlyphs holds the info about what everything looks like and whether each tile can have any color variations,
	// while the MapUI tracks the concrete color variations and decides what actually gets drawn. todo update comment.
	public static class GameObjectGlyphs{
		private static Dictionary<CreatureType, ColorGlyph> creatures;
		private static Dictionary<TileType, ColorGlyph> tiles;
		private static Dictionary<FeatureType, ColorGlyph> features;
		private static Dictionary<ItemType, ColorGlyph> items;

		public static ColorGlyph Get(CreatureType type) => creatures[type];
		public static ColorGlyph Get(FeatureType type) => features[type];
		public static ColorGlyph Get(ItemType type) => items[type];
		public static ColorGlyph Get(TileType type, ulong colorVariationSeed, Point p){
			ColorGlyph cg = tiles[type];
			Func<ulong, Point, Color> getColorVariation;
			if(colorVariationFuncs.TryGetValue(type, out getColorVariation)){
				ulong hashValue = MicroHash(p, colorVariationSeed);
				Color color = getColorVariation(hashValue, p);
				if(cg.BackgroundColor == Color.Black) return new ColorGlyph(cg.GlyphIndex, color);
				else return new ColorGlyph(cg.GlyphIndex, Color.Black, color);
			}
			else return cg;
		}

		private static Dictionary<TileType, Func<ulong, Point, Color>> colorVariationFuncs;

		private static void Add(CreatureType type, char ch, Color color) => creatures.Add(type, new ColorGlyph(ch, color));
		private static void Add(TileType type, char ch, Color color, Color bgColor = Color.Black) => tiles.Add(type, new ColorGlyph(ch, color, bgColor));
		private static void Add(FeatureType type, char ch, Color color, Color bgColor = Color.Black) => features.Add(type, new ColorGlyph(ch, color, bgColor));
		private static void Add(ItemType type, char ch, Color color) => items.Add(type, new ColorGlyph(ch, color));

		public static void Initialize(){
			if(creatures != null) return; // Initialize only once
			creatures = new Dictionary<CreatureType, ColorGlyph>();
			tiles = new Dictionary<TileType, ColorGlyph>();
			features = new Dictionary<FeatureType, ColorGlyph>();
			items = new Dictionary<ItemType, ColorGlyph>();
			colorVariationFuncs = new Dictionary<TileType, Func<ulong, Point, Color>>();

			Add(CreatureType.Player, '@', Color.White);
			Add(CreatureType.Goblin, 'g', Color.Green);
			Add(CreatureType.Cleric, 'p', Color.Yellow);

			Add(TileType.Wall, '#', Color.Gray);
			Add(TileType.Floor, '.', Color.White);
			Add(TileType.DeepWater, '~', Color.Cyan, Color.DarkBlue);
			Add(TileType.Staircase, '>', Color.White);
			Add(TileType.ThickIce, '~', Color.White, Color.Gray);
			Add(TileType.FirePit, '0', Color.Red);
			Add(TileType.FirePitUnlit, '0', Color.TerrainDarkGray);
			Add(TileType.Statue, '5', Color.Gray);
			Add(TileType.Brush, '"', Color.DarkYellow);
			colorVariationFuncs[TileType.Brush] = (hash, p) => {
				if(hash.OneIn(20)) return Color.Green;
				else if(hash.CoinFlip()) return Color.Yellow;
				else return Color.DarkYellow;
			};
			Add(TileType.PoppyField, '"', Color.Red);
			Add(TileType.GlowingFungus, ',', Color.RandomGlowingFungus);
			Add(TileType.StandingTorch, '|', Color.RandomTorch); //todo check for new symbols
			Add(TileType.WallMountedTorch, '\'', Color.RandomTorch);
			Add(TileType.Barrel, '0', Color.DarkYellow);
			Add(TileType.Demonstone, '~', Color.RandomDoom);
			Add(TileType.LightCrystal, '#', Color.RandomLightning); //todo color?
			Add(TileType.GiantMushroom, '#', Color.Cyan); //todo color?
			Add(TileType.DeepMud, '~', Color.DarkGray, Color.DarkYellow);
			Add(TileType.Lava, '~', Color.DarkRed, Color.Red);

			Add(FeatureType.Fire, '&', Color.RandomFire);
			Add(FeatureType.PoisonGas, '*', Color.DarkGreen);
			Add(FeatureType.Fog, '*', Color.Gray);
			Add(FeatureType.Spores, '*', Color.DarkYellow);
			Add(FeatureType.ThickDust, '*', Color.TerrainDarkGray); //todo check
			Add(FeatureType.PixieDust, '*', Color.RandomGlowingFungus);
			Add(FeatureType.ConfusionGas, '*', Color.RandomConfusion);
			Add(FeatureType.Slime, ',', Color.Green);
			Add(FeatureType.Oil, ',', Color.DarkYellow);
			Add(FeatureType.Grenade, ',', Color.Red);
			Add(FeatureType.Web, ';', Color.White);
			Add(FeatureType.ForasectEgg, '%', Color.TerrainDarkGray);
			Add(FeatureType.Teleportal, '8', Color.White);
			Add(FeatureType.InactiveTeleportal, '8', Color.Gray); // todo, keeping all of these?
			Add(FeatureType.StableTeleportal, '8', Color.Magenta);
			Add(FeatureType.Bones, '%', Color.White);
			Add(FeatureType.TrollCorpse, '%', Color.DarkGreen);
			Add(FeatureType.TrollBloodwitchCorpse, '%', Color.DarkRed);
			Add(FeatureType.Ice, '~', Color.Cyan, Color.Gray);
			Add(FeatureType.CrackedIce, '~', Color.Red, Color.Gray);
			Add(FeatureType.BrokenIce, '~', Color.Gray, Color.DarkBlue);
			Add(FeatureType.Water, '~', Color.Cyan, Color.DarkCyan);

			Add(ItemType.PotionOfRoots, '!', Color.Cyan);
			Add(ItemType.PotionOfSilence, '!', Color.White);
			Add(ItemType.PotionOfBrutishStrength, '!', Color.Red);
			Add(ItemType.ScrollOfCalling, '?', Color.White);
			Add(ItemType.ScrollOfThunderclap, '?', Color.White);
			Add(ItemType.ScrollOfTrapClearing, '?', Color.White);
			Add(ItemType.OrbOfFlames, '*', Color.RandomCMY);
			Add(ItemType.OrbOfBlades, '*', Color.RandomDCMY);
			Add(ItemType.WandOfDustStorm, '\\', Color.Yellow);//todo, find a better way to do this for items

			//Add(ItemType., '!', Color.White);
			//todo, unIDed items? What gets registered here will probably be the indices based on the names, not the actual item types.
			// However... care should be taken here, because the potion colors should be the same during a replay, or when 2 players play the same seed...
			// I guess that's as simple as reading (but not changing) the current GameUniverse RNG value for this purpose.
		}
		private static ulong MicroHash(Point point, ulong seed){ // Thanks to Tommy Ettinger for this one!
			seed += ((ulong)point.Y + seed + ((ulong)point.X + seed) * 0xABC98388FB8FAC03UL) * 0x8CB92BA72F3D8DD7UL;
			return ((seed = (seed ^ seed >> 20) * 0xF1357AEA2E62A9C5UL) ^ seed >> 41);
		}
		private static int GetWithinRange(this ulong source, int upperExclusiveBound){
			return (int)(((ulong)upperExclusiveBound * (source & 0xFFFFFFFFUL)) >> 32);
		}
		private static bool CoinFlip(this ulong source) => source < 0x8000000000000000UL;
		private static bool OneIn(this ulong source, int x) => source.GetWithinRange(x) == 0;
	}
}
