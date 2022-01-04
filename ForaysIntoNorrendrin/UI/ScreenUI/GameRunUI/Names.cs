using System;
using System.Collections.Generic;
using Forays;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	public static class Names{
		private static Dictionary<CreatureType, string> creatures;
		private static Dictionary<TileType, string> tiles;
		private static Dictionary<FeatureType, string> features;
		private static Dictionary<ItemType, string> items;
		private static Grammar grammar;

		public static string Get(CreatureType type) => creatures[type];
		public static string Get(TileType type) => tiles[type];
		///<summary>Returns the name of a single feature. Throws if the given FeatureType has more than one flag set.</summary>
		public static string GetSingleFeature(FeatureType type) => features[type];
		///<summary>Returns a collection of the names of all features within the given FeatureType.</summary>
		public static IEnumerable<string> GetAllFeatures(FeatureType type){
			ulong allFeatures = (ulong)type;
			for(int i=0;i<64;++i){
				ulong bitFlag = 1UL << i;
				if((allFeatures & bitFlag) > 0UL){
					yield return features[(FeatureType)bitFlag];
				}
			}
		}
		public static string Get(ItemType type) => items[type];

		private static void Add(CreatureType type, string name, bool noArticles = false){
			creatures[type] = grammar.AddNoun(name, noArticles: noArticles); // Add this name to the grammar object as well as the dictionary.
		}
		private static void Add(TileType type, string name, bool uncountable = false, bool noArticles = false){
			tiles[type] = grammar.AddNoun(name, uncountable: uncountable, noArticles: noArticles);
		}
		private static void Add(FeatureType type, string name, bool uncountable = false, bool noArticles = false){
			features[type] = grammar.AddNoun(name, uncountable: uncountable, noArticles: noArticles);
		}
		private static void Add(ItemType type, string name){
			items[type] = grammar.AddNoun(name);
		}
		public static void Initialize(Grammar grammar){
			if(Names.grammar != null) return; // Initialize only once
			Names.grammar = grammar;
			creatures = new Dictionary<CreatureType, string>();
			tiles = new Dictionary<TileType, string>();
			features = new Dictionary<FeatureType, string>();
			items = new Dictionary<ItemType, string>();

			grammar.AddNoun("something", noArticles: true);

			Add(CreatureType.Player, "you", noArticles: true);
			Add(CreatureType.Goblin, "goblin");

			Add(TileType.Wall, "wall");
			Add(TileType.Floor, "the ground", noArticles: true); // Use "the ground" as the name so it'll work with sentences that ask for 'a/an'.
			Add(TileType.DeepWater, "deep water", uncountable: true);
			Add(TileType.Staircase, "staircase"); //todo check
			Add(TileType.ThickIce, "thick ice", uncountable: true);
			Add(TileType.FirePit, "stone-ringed firepit~ (lit)"); //todo check
			Add(TileType.FirePitUnlit, "stone-ringed firepit~ (unlit)");
			Add(TileType.Statue, "statue");
			Add(TileType.Brush, "brush", uncountable: true);
			Add(TileType.PoppyField, "poppy field");
			Add(TileType.GlowingFungus, "patch~~ of glowing fungus");
			Add(TileType.StandingTorch, "standing torch sconce");
			Add(TileType.WallMountedTorch, "mounted torch sconce");
			Add(TileType.Barrel, "barrel~ of oil");
			Add(TileType.Demonstone, "demonstone", uncountable: true);
			Add(TileType.LightCrystal, "shining crystal");
			Add(TileType.GiantMushroom, "giant mushroom");
			Add(TileType.DeepMud, "deep mud", uncountable: true);
			Add(TileType.Lava, "lava", uncountable: true);

			Add(FeatureType.Fire, "fire", uncountable: true);
			Add(FeatureType.PoisonGas, "thick cloud~ of poison gas");
			Add(FeatureType.Fog, "cloud~ of fog");
			Add(FeatureType.Spores, "cloud~ of spores");
			Add(FeatureType.ThickDust, "thick cloud~ of dust");
			//todo, fill in the rest
			Add(FeatureType.Slime, "slime", uncountable: true);
			Add(FeatureType.Oil, "oil", uncountable: true);
			//todo
			Add(FeatureType.Water, "shallow water", uncountable: true);
			Add(FeatureType.Ice, "layer~ of ice");
			Add(FeatureType.CrackedIce, "layer~ of cracked ice");
			Add(FeatureType.BrokenIce, "chunks of ice", uncountable: true);

			Add(ItemType.PotionOfBrutishStrength, "potion~ of brutish strength");
		}
	}
}
