using System;
using System.Collections.Generic;

namespace ForaysUI.ScreenUI{
	// This class exists so that 'using static' grants easy access to this Screen:
	public static class StaticScreen{
		public static IScreen Screen;
	}
	public enum HighlightType { TargetingValid, TargetingInvalid, TargetingValidFocused, TargetingInvalidFocused, Button };
	///<summary>Represents an abstract terminal-like display</summary>
	public interface IScreen{
		int Rows {get;}
		int Cols {get;}
		void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black);
		void Write(int row, int col, ColorGlyph cg);
		void Write(int row, int col, string str, Color color = Color.Gray, Color bgColor = Color.Black);
		void Write(int startRow, int startCol, ColorGlyph[][] colorGlyphs);

		///<summary>Ensure that everything sent to the display is actually drawn, process input events,
		/// and return false if the window has been closed, if applicable.</summary>
		bool WindowUpdate();
		///<summary>After calling HoldUpdates, changes to the IScreen data will not be drawn to the screen until ResumeUpdates is called. This uses a
		/// stack-like concept: if HoldUpdates is called multiple times, the screen won't update until ResumeUpdates is called the same number of times.</summary>
		void HoldUpdates();
		///<summary>After calling HoldUpdates, calling ResumeUpdates will send all changes to the screen, and resume normal operation afterward. This uses a
		/// stack-like concept: if HoldUpdates is called multiple times, the screen won't update until ResumeUpdates is called the same number of times.</summary>
		/// <param name="forceResume">If forceResume is true, return to the default state and immediately resume updates.</param>
		void ResumeUpdates(bool forceResume = false);

		///<summary>If true, the cursor should be blinking at the position indicated by CursorRow and CursorCol</summary>
		bool CursorVisible {get;set;}
		///<summary>Row position of the blinking cursor</summary>
		int CursorRow {get;set;}
		///<summary>Column position of the blinking cursor</summary>
		int CursorCol {get;set;}
		///<summary>Move the blinking cursor to a new position. Does not change cursor visibility.</summary>
		void SetCursorPosition(int row, int col);

		///<summary>Update screen memory to fill the screen with black</summary>
		void Clear();

		///<summary>Update screen memory to fill the specified rectangle with black</summary>
		void Clear(int startRow, int startCol, int height, int width);

		///<summary>Returns an array of the current contents of the entire screen.</summary>
		ColorGlyph[][] GetCurrentScreen();

		///<summary>Returns an array of the current contents of the screen within a rectangle of the given position and size.</summary>
		ColorGlyph[][] GetCurrent(int startRow, int startCol, int height, int width);

		///<summary>Called immediately before exiting the program. Allows terminals to be reset to original config.</summary>
		void CleanUp();

		///<summary>Allows each Screen to return additional options specific to its implementation.</summary>
		IList<OptionsScreen.OptionEditInfo> GetAdditionalDisplayOptions();
		//todo, need a method to attempt to load these additional options, when they're encountered in the saved options file.

		///<summary>Return the given ColorGlyph highlighted using the given type. This method allows each Screen implementation to
		/// apply its own logic to determine whether colors should be inverted, etc.</summary>
		ColorGlyph GetHighlighted(ColorGlyph cg, HighlightType highlightType);


		//todo - this one is interesting: GLUpdate calls WindowUpdate and therefore lets the gl window process events, returns false if exiting, ...
			// it DOES seem like a 'let input events be read' method could be useful, yeah.
			// Maybe look at how the 2 would actually implement it, before deciding.

	}
	public static class ScreenExtensions{
		public static void WriteListOfChoices(this IScreen screen, int rowOffset, int colOffset, IList<string> choices,
			Color textColor = Color.Gray, Color bgColor = Color.Black, Color letterColor = Color.Cyan, int linesBetweenEach = 0)
		{
			int row = rowOffset;
			for(int i=0;i<choices.Count;++i){
				screen.Write(row, colOffset, '[', textColor, bgColor);
				screen.Write(row, colOffset + 1, i + 'a', letterColor, bgColor);
				screen.Write(row, colOffset + 2, "] ", textColor, bgColor);
				screen.Write(row, colOffset + 4, choices[i], textColor, bgColor);
				row += linesBetweenEach + 1;
			}
		}
		public static void WriteSingleChoice(this IScreen screen, int rowOffset, int colOffset, string choiceText, int letterIndex,
			Color textColor = Color.Gray, Color bgColor = Color.Black, Color letterColor = Color.Cyan)
		{
			screen.Write(rowOffset, colOffset, '[', textColor, bgColor);
			screen.Write(rowOffset, colOffset + 1, letterIndex + 'a', letterColor, bgColor);
			screen.Write(rowOffset, colOffset + 2, "] ", textColor, bgColor);
			screen.Write(rowOffset, colOffset + 4, choiceText, textColor, bgColor);
		}
	}
}
