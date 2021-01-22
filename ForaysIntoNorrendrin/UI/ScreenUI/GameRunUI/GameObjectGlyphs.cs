using System;
using System.Collections.Generic;
using Forays;

namespace ForaysUI.ScreenUI{
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
		private static void Add(TileType type, char ch, Color color) => tiles.Add(type, new ColorGlyph(ch, color));
		private static void Add(FeatureType type, char ch, Color color) => features.Add(type, new ColorGlyph(ch, color));
		private static void Add(ItemType type, char ch, Color color) => items.Add(type, new ColorGlyph(ch, color));

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
			Add(TileType.Water, '~', Color.Blue);
			Add(TileType.Staircase, '>', Color.White);

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

			//Add(ItemType., '!', Color.White);
			//todo, unIDed items? What gets registered here will probably be the indices based on the names, not the actual item types.
			// However... care should be taken here, because the potion colors should be the same during a replay, or when 2 players play the same seed...
			// I guess that's as simple as reading (but not changing) the current GameUniverse RNG value for this purpose.
		}
	}
}
