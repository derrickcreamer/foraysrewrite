using System;
using System.Collections.Generic;
using UtilityCollections;

namespace ForaysUI.ScreenUI{
	// Trying to divide options with a consistent plan:
	// Game interface: Changes the behavior of the interface in response to input or game state
	// Controls: Determines how input gets to the interface
	// Display: Screen, window, color, etc.
	// Other
	public enum BoolOptionType {
		// Interface options:
		NoWallSliding,
		AutoPickUp,
		NoConfirmationBeforeResting,
		NeverDisplayTips,
		AlwaysResetTips,
		DisableAdvancedCommands,
		EnableCarefulMode,
		KeepItemLetters,
		// Control options:
		ChordedArrowKeysForDiagonals,
		TopRowMovement, //todo, should this not be done as a set of bindings?
		// Display options:
		DarkGrayUnseen, //todo, full color options instead
		SidebarOnRight,
		MessagesAtBottom,
		// Other options:
		// (no Other options yet)
		// LAST exists so specific implementations of screens + inputs can define their own enum values after this one.
		LAST
	};
	public enum IntOptionType {
		// Interface options:
		// Control options:
		ChordedArrowKeysDelayMs,
		// Display options:
		LoopDelayMs,
		// Other options:
		// (no Other options yet)
		// LAST exists so specific implementations of screens + inputs can define their own enum values after this one.
		LAST
	};
	public static class Option{
		private static EasyHashSet<BoolOptionType> boolOptions; // bools default to false
		private static Dictionary<IntOptionType, int> intOptions; // ints have no default
		public static void Initialize(){
			boolOptions = new EasyHashSet<BoolOptionType>();
			intOptions = new Dictionary<IntOptionType, int>();
		}
		public static bool IsSet(BoolOptionType option) => boolOptions[option];
		public static int? Value(IntOptionType option){
			int result;
			if(intOptions.TryGetValue(option, out result)) return result;
			else return null;
		}
		public static void Set(BoolOptionType option, bool value) => boolOptions[option] = value;
		public static void Set(IntOptionType option, int value) => intOptions[option] = value;
	}
}
