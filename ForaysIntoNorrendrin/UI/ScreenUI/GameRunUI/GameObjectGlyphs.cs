using System;
using System.Collections.Generic;
using Forays;

namespace ForaysUI.ScreenUI{
	// GameObjectGlyphs holds the info about what everything looks like and whether each tile can have any color variations,
	// while the MapUI tracks the concrete color variations and decides what actually gets drawn.
	public static class GameObjectGlyphs{
		private static Dictionary<CreatureType, ColorGlyph> creatures;
		private static Dictionary<TileType, ColorGlyph> tiles;
		private static Dictionary<FeatureType, ColorGlyph> features;
		private static Dictionary<ItemType, ColorGlyph> items;

		public static ColorGlyph Get(CreatureType type) => creatures[type];
		public static ColorGlyph Get(TileType type) => tiles[type];
		public static ColorGlyph Get(FeatureType type) => features[type];
		public static ColorGlyph Get(ItemType type) => items[type];

		private static void Add(CreatureType type, char ch, Color color) => creatures.Add(type, new ColorGlyph(ch, color));
		private static void Add(TileType type, char ch, Color color, Color bgColor = Color.Black) => tiles.Add(type, new ColorGlyph(ch, color, bgColor));
		private static void Add(FeatureType type, char ch, Color color, Color bgColor = Color.Black) => features.Add(type, new ColorGlyph(ch, color, bgColor));
		private static void Add(ItemType type, char ch, Color color) => items.Add(type, new ColorGlyph(ch, color));

//todo next
// move some parts of THIS to the specific screens.
// Change map drawing so it's more like... the screen is applying its own logic based on
//    all the items in that tile...
//  so it can handle gases.
//
		public static void Initialize(){
			if(creatures != null) return; // Initialize only once
			creatures = new Dictionary<CreatureType, ColorGlyph>();
			tiles = new Dictionary<TileType, ColorGlyph>();
			features = new Dictionary<FeatureType, ColorGlyph>();
			items = new Dictionary<ItemType, ColorGlyph>();

			Add(CreatureType.Player, '@', Color.White);
			Add(CreatureType.Goblin, 'g', Color.Green);

			Add(TileType.Wall, '#', Color.Gray);
			Add(TileType.Floor, '.', Color.White);
			Add(TileType.DeepWater, '~', Color.Cyan, Color.DarkBlue);
			Add(TileType.Staircase, '>', Color.White);
			Add(TileType.ThickIce, '~', Color.White, Color.Gray);
			Add(TileType.FirePit, '0', Color.Red);
			Add(TileType.FirePitUnlit, '0', Color.TerrainDarkGray);
			Add(TileType.Statue, '5', Color.Gray);
			Add(TileType.Brush, '"', Color.DarkYellow); //todo, brush gets special color code
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

			//Add(ItemType., '!', Color.White);
			//todo, unIDed items? What gets registered here will probably be the indices based on the names, not the actual item types.
			// However... care should be taken here, because the potion colors should be the same during a replay, or when 2 players play the same seed...
			// I guess that's as simple as reading (but not changing) the current GameUniverse RNG value for this purpose.
		}
	}
}
