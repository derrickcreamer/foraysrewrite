using System;
using System.Collections.Generic;
using Forays;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	public static class Names{
		private static Dictionary<CreatureType, string> creatures;
		private static Dictionary<TileType, string> tiles;
		private static Dictionary<ItemType, string> items;
		private static Grammar grammar;

		public static string Get(CreatureType type) => creatures[type];
		public static string Get(TileType type) => tiles[type];
		public static string Get(ItemType type) => items[type];

		private static void Add(CreatureType type, string name, bool noArticles = false){
			creatures[type] = grammar.AddNoun(name, noArticles: noArticles); // Add this name to the grammar object as well as the dictionary.
		}
		private static void Add(TileType type, string name, bool uncountable = false){
			tiles[type] = grammar.AddNoun(name, uncountable: uncountable);
		}
		private static void Add(ItemType type, string name){
			items[type] = grammar.AddNoun(name);
		}
		public static void Initialize(Grammar grammar){
			if(Names.grammar != null) return; // Initialize only once
			Names.grammar = grammar;
			creatures = new Dictionary<CreatureType, string>();
			tiles = new Dictionary<TileType, string>();
			items = new Dictionary<ItemType, string>();

			grammar.AddNoun("something", noArticles: true);

			Add(CreatureType.Player, "you", noArticles: true);
			Add(CreatureType.Goblin, "goblin");

			Add(TileType.Wall, "wall");
			Add(TileType.Floor, "floor");
			Add(TileType.DeepWater, "deep water", uncountable: true);
			Add(TileType.Staircase, "staircase"); //todo check
			Add(TileType.ThickIce, "thick ice", uncountable: true);

			Add(ItemType.PotionOfBrutishStrength, "potion~ of brutish strength");
		}
	}
}
