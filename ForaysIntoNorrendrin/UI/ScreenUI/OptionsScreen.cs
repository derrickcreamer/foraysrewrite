using System;
using System.Collections.Generic;
using System.Linq;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	public static class OptionsScreen {
		public class OptionEditInfo{
			public string Text;
			// GetValue will be called at each loop, so any changes will be visible right away:
			public Func<string> GetValue;
			public EditValueMethod EditValue;
			// Pass row and right-hand edge values so that the method can draw to the screen in the right place:
			public delegate void EditValueMethod(int row, int rightHandColOffset);
		}
		private enum OptionsScreenType { Interface, Controls, Display, Other };
		const int RowOffset = 3;
		const int ColOffset = 11;
		const int Width = 66;
		const string SeparatorBar = "------------------------------------------------------------------"; // Must match Width above

		public static void Show(){
			OptionsScreenType? nextScreen = OptionsScreenType.Interface;
			do{
				if(nextScreen == OptionsScreenType.Interface) nextScreen = ShowInterfaceOptions();
				else if(nextScreen == OptionsScreenType.Controls) nextScreen = ShowControlOptions();
				else if(nextScreen == OptionsScreenType.Display) nextScreen = ShowDisplayOptions();
				else if(nextScreen == OptionsScreenType.Other) nextScreen = ShowOtherOptions();
				else return;
			}
			while(nextScreen != null);
		}
		private static void DrawCommonSections(OptionsScreenType currentScreen){
			const int row = 0;
			const int col = ColOffset;
			const Color bgColor = Color.Black;
			Screen.Clear();
			Screen.Write(row, col, "Game Interface    Controls    Display    Other", Color.DarkGray, bgColor);
			Screen.Write(row, col + 50, "[   ] to switch", Color.Gray, bgColor);
			Screen.Write(row, col + 51, "Tab", Color.Cyan, bgColor);
			int highlightOffset;
			string highlightLabel;
			switch(currentScreen){
				case OptionsScreenType.Interface:
					highlightOffset = 0;
					highlightLabel = "Game Interface";
					break;
				case OptionsScreenType.Controls:
					highlightOffset = 18;
					highlightLabel = "Controls";
					break;
				case OptionsScreenType.Display:
					highlightOffset = 30;
					highlightLabel = "Display";
					break;
				case OptionsScreenType.Other:
					highlightOffset = 41;
					highlightLabel = "Other";
					break;
				default: throw new InvalidOperationException("Invalid screen");
			}
			Screen.Write(row, col + highlightOffset, highlightLabel, Color.Green, bgColor);
		}

		private static OptionsScreenType? ShowInterfaceOptions(){
			List<OptionEditInfo> options = new List<OptionEditInfo>{
				GetEditInfo(BoolOptionType.NoWallSliding, "Disable wall sliding"),
				GetEditInfo(BoolOptionType.AutoPickUp, "Automatically pick up items (if safe)"),
				GetEditInfo(BoolOptionType.NoConfirmationBeforeResting, "Skip confirmation before resting"),
				GetEditInfo(BoolOptionType.DisableAdvancedCommands, "Disable advanced commands (e.g. '[a]pply item')"),
				GetEditInfo(BoolOptionType.EnableCarefulMode, "Enable careful mode (confirms before all actions)"), //todo, better desc?
				GetEditInfo(BoolOptionType.KeepItemLetters, "Items in inventory keep their letters once assigned"),
				GetEditInfo(BoolOptionType.NeverDisplayTips, "Never show tutorial tips"),
				GetEditInfo(BoolOptionType.AlwaysResetTips, "Reset tutorial tips before each game"),
			};
			return ShowEditableOptions(OptionsScreenType.Interface, "Game interface options: ", options);
		}
		private static OptionsScreenType? ShowControlOptions(){
			List<OptionEditInfo> options = new List<OptionEditInfo>{
				GetEditInfo(BoolOptionType.ChordedArrowKeysForDiagonals, "todo"),
				//todo toprow?
			};
			return ShowEditableOptions(OptionsScreenType.Controls, "Control options: ", options);
		}
		private static OptionsScreenType? ShowDisplayOptions(){
			List<OptionEditInfo> options = new List<OptionEditInfo>{
				GetEditInfo(IntOptionType.LoopDelayMs, "Delay while awaiting input (in milliseconds)"),
				GetEditInfo(BoolOptionType.SidebarOnRight, "Arrange screen with sidebar on right side instead of left"),
				GetEditInfo(BoolOptionType.MessagesAtBottom, "Arrange screen with messages at bottom instead of top"),
				//todo color options
			};
			IList<OptionEditInfo> additionalOptions = Screen.GetAdditionalDisplayOptions();
			if(additionalOptions != null) options.AddRange(additionalOptions);
			return ShowEditableOptions(OptionsScreenType.Display, "Display options: ", options);
		}
		private static OptionsScreenType? ShowOtherOptions(){
			List<OptionEditInfo> options = new List<OptionEditInfo>{
				GetEditInfo(BoolOptionType.ChordedArrowKeysForDiagonals, "todo path"),
			};
			return ShowEditableOptions(OptionsScreenType.Other, "Other options: ", options);
		}
		private static OptionsScreenType? ShowEditableOptions(OptionsScreenType currentScreen, string title, IList<OptionEditInfo> options){
			while(true){
				Screen.HoldUpdates();
				DrawCommonSections(currentScreen);
				Screen.Write(RowOffset, ColOffset, title);
				Screen.Write(RowOffset + 1, ColOffset, SeparatorBar);
				Screen.WriteListOfChoices(RowOffset + 2, ColOffset, options.Select(o => o.Text).ToArray());
				for(int i=0;i<options.Count;++i){
					string value = options[i].GetValue.Invoke();
					Screen.Write(RowOffset + 2 + i, ColOffset + Width - value.Length, value);
				}
				Screen.Write(RowOffset + 2 + options.Count, ColOffset, SeparatorBar);
				Screen.ResumeUpdates();
				Screen.SetCursorPosition(RowOffset, ColOffset + title.Length);
				ConsoleKeyInfo key = Input.ReadKey();
				bool shift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
				switch(key.Key){
					case ConsoleKey.Tab:
						int nextScreenIdx = shift? (int)currentScreen - 1 : (int)currentScreen + 1;
						if(nextScreenIdx < 0) nextScreenIdx = 3;
						if(nextScreenIdx > 3) nextScreenIdx = 0;
						return (OptionsScreenType)nextScreenIdx;
					case ConsoleKey.Escape:
					// todo, space too? case ConsoleKey.Spacebar:
						return null;
					//todo, help?
					default:
						int letterIndex = key.KeyChar - 'a';
						if(letterIndex >= 0 && letterIndex < options.Count){
							options[letterIndex].EditValue.Invoke(RowOffset + 2 + letterIndex, ColOffset + Width);
						}
						break;
				}
			}
		}
		public static OptionEditInfo GetEditInfo(BoolOptionType option, string text){
			return new OptionEditInfo{
				Text = text,
				GetValue = () => Option.IsSet(option)? "yes" : "no",
				EditValue = (i, j) => Option.Set(option, !Option.IsSet(option))
			};
		}
		public static OptionEditInfo GetEditInfo(IntOptionType option, string text, int maxDigits = 4){
			return new OptionEditInfo{
				Text = text,
				GetValue = () => Option.Value(option)?.ToString() ?? "",
				EditValue = (row, col) => {
					int? value = EnterInt(row, col, maxDigits);
					if(value != null) Option.Set(option, value.Value);
				}
			};
		}
		private static int? EnterInt(int row, int col, int maxDigits){ //todo, this one goes elsewhere, as a utility
			if(StaticInput.Input.ReadKey().Key == ConsoleKey.Escape) return null;
			//todo
			return 42;
		}
	}
}
