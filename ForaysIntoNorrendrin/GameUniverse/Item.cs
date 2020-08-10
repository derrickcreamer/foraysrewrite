using System;
using GameComponents;
using UtilityCollections;

namespace Forays {
	public enum ItemType { PotionOfHealing, PotionOfRegeneration, PotionOfStoneform, PotionOfVampirism, PotionOfBrutishStrength,
		PotionOfRoots, PotionOfHaste, PotionOfSilence, PotionOfCloaking, PotionOfMysticMind,
		ScrollOfBlinking, ScrollOfPassage, ScrollOfTime, ScrollOfKnowledge, ScrollOfSunlight, ScrollOfDarkness, ScrollOfRenewal,
		ScrollOfCalling, ScrollOfTrapClearing, ScrollOfEnchantment, ScrollOfThunderclap, ScrollOfFireRing, ScrollOfRage,
		OrbOfFreezing, OrbOfFlames, OrbOfFog, OrbOfDetonation, OrbOfBreaching, OrbOfShielding, OrbOfTeleportal, OrbOfPain,
		OrbOfConfusion, OrbOfBlades,
		WandOfDustStorm, WandOfInvisibility, WandOfFleshToFire, WandOfWebs, WandOfSlumber, WandOfReach, WandOfTelekinesis,
		RollOfBandages, FlintAndSteel, BlastFungus, MagicTrinket
	};
	public enum ConsumableKind { Potion, Scroll, Orb, Wand, Other };

	public class Item : GameObject/*, IPhysicalObject*/ {
		public Point? Position => Map.Items.TryGetPositionOf(this, out Point p)? p : (Point?)null;
		public ItemType Type;
		//todo: quantity/doNotStack are now solely UI concepts.
		//'ignored' is also a UI concept.
		///<summary>The actual number of charges that this item has.</summary>
		public int Charges;
		///<summary>Wands only. The number of times this wand has been zapped while the true number of charges is unknown. Set to -1 when charges are known.</summary>
		public int TimesZapped;
		///<summary>Whether the player knows what ItemType *this* item is. (Some items need to be seen in the light to be known, but some don't.)</summary>
		public bool TypeRevealedToPlayer;

		public Item(GameUniverse g) : base(g) { }

	}

	//todo... gonna try something different from the approach I took with IsOpaque etc. for tiles...
	// I figure that one of these 2 approaches will probably be better and I'll convert the other to match.
	public struct ConsumableDefinition{
		public readonly ConsumableKind Kind;
		///<summary>If rarity is 0 or negative, this item will never be generated naturally</summary>
		public readonly int Rarity;
		public readonly bool TypeAlwaysRevealed;
		public readonly int MinCharges;
		public readonly int MaxCharges;
		public ConsumableDefinition(ConsumableKind kind, int rarity, bool alwaysRevealed = false, int minCharges = 0, int maxCharges = 0){
			Kind = kind;
			TypeAlwaysRevealed = alwaysRevealed;
			Rarity = rarity;
			MinCharges = minCharges;
			MaxCharges = maxCharges;
		}
	}


	public static class ItemDefinition{
		public static ConsumableDefinition GetConsumableDefinition(ItemType type) => defs[type];

		private static DefaultValueDictionary<ItemType, ConsumableDefinition> defs;
		private readonly static object lockObject = new object();
		///<summary>Must be called before any other static methods of this class</summary>
		public static void InitializeDefinitions(){
			lock(lockObject){
				if(defs == null) CreateDefinitions();
			}
		}
		private static void DefinePotion(ItemType type, int rarity){
			defs[type] = new ConsumableDefinition(ConsumableKind.Potion, rarity);
		}
		private static void DefineScroll(ItemType type, int rarity){
			defs[type] = new ConsumableDefinition(ConsumableKind.Scroll, rarity);
		}
		private static void DefineOrb(ItemType type, int rarity){
			defs[type] = new ConsumableDefinition(ConsumableKind.Orb, rarity);
		}
		private static void DefineWand(ItemType type, int rarity, int minCharges, int maxCharges){
			defs[type] = new ConsumableDefinition(ConsumableKind.Wand, rarity, false, minCharges, maxCharges);
		}
		private static void DefineOther(ItemType type, int rarity, bool alwaysRevealed = false, int minCharges = 0, int maxCharges = 0){
			defs[type] = new ConsumableDefinition(ConsumableKind.Other, rarity, alwaysRevealed, minCharges, maxCharges);
		}
		private static void CreateDefinitions(){
			defs = new DefaultValueDictionary<ItemType, ConsumableDefinition>();
			DefinePotion(ItemType.PotionOfHealing, 1);
			DefinePotion(ItemType.PotionOfRegeneration, 3);
			DefinePotion(ItemType.PotionOfStoneform, 1);
			DefinePotion(ItemType.PotionOfVampirism, 2);
			DefinePotion(ItemType.PotionOfBrutishStrength, 1);
			DefinePotion(ItemType.PotionOfRoots, 2);
			DefinePotion(ItemType.PotionOfHaste, 1);
			DefinePotion(ItemType.PotionOfSilence, 3);
			DefinePotion(ItemType.PotionOfCloaking, 1);
			DefinePotion(ItemType.PotionOfMysticMind, 1);

			DefineScroll(ItemType.ScrollOfBlinking, 1);
			DefineScroll(ItemType.ScrollOfPassage, 1);
			DefineScroll(ItemType.ScrollOfTime, 2);
			DefineScroll(ItemType.ScrollOfKnowledge, 1);
			DefineScroll(ItemType.ScrollOfSunlight, 3);
			DefineScroll(ItemType.ScrollOfDarkness, 3);
			DefineScroll(ItemType.ScrollOfRenewal, 2);
			DefineScroll(ItemType.ScrollOfCalling, 3);
			DefineScroll(ItemType.ScrollOfTrapClearing, 3);
			DefineScroll(ItemType.ScrollOfEnchantment, 7);
			DefineScroll(ItemType.ScrollOfThunderclap, 2);
			DefineScroll(ItemType.ScrollOfFireRing, 3);
			DefineScroll(ItemType.ScrollOfRage, 3);

			DefineOrb(ItemType.OrbOfFreezing, 1);
			DefineOrb(ItemType.OrbOfFlames, 1);
			DefineOrb(ItemType.OrbOfFog, 2);
			DefineOrb(ItemType.OrbOfDetonation, 2);
			DefineOrb(ItemType.OrbOfBreaching, 3);
			DefineOrb(ItemType.OrbOfShielding, 3);
			DefineOrb(ItemType.OrbOfTeleportal, 2);
			DefineOrb(ItemType.OrbOfPain, 2);
			DefineOrb(ItemType.OrbOfConfusion, 2);
			DefineOrb(ItemType.OrbOfBlades, 3);

			DefineWand(ItemType.WandOfDustStorm, 4, 2, 6);
			DefineWand(ItemType.WandOfInvisibility, 6, 2, 3);
			DefineWand(ItemType.WandOfFleshToFire, 6, 3, 6);
			DefineWand(ItemType.WandOfWebs, 4, 4, 9);
			DefineWand(ItemType.WandOfSlumber, 5, 2, 6);
			DefineWand(ItemType.WandOfReach, 6, 4, 7);
			DefineWand(ItemType.WandOfTelekinesis, 6, 2, 6);

			DefineOther(ItemType.RollOfBandages, 0, true, 5, 5);
			DefineOther(ItemType.FlintAndSteel, 0, true, 3, 3);
			DefineOther(ItemType.BlastFungus, 0, true, 0, 0);//todo check
			DefineOther(ItemType.MagicTrinket, 0);
		}
	}
}
