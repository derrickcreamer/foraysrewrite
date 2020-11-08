using System;

namespace ForaysUI.ScreenUI{
	// This class exists so that 'using static' grants easy access to this Screen:
	public static class StaticScreen{
		public static IScreen Screen;
	}
	///<summary>Represents an abstract terminal-like display</summary>
	public interface IScreen{
		int Rows {get;}
		int Cols {get;}
		//todo - screen memory is apparently not directly exposed. There MIGHT need to be a getter.
		// todo - is row from the top or bottom?
		void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black);
		//void Write(int row, int column, char ch, Color color, Color bgColor = Color.Black);
		void Write(int row, int col, ColorGlyph cg);

		void Write(int row, int col, string str, Color color = Color.Gray, Color bgColor = Color.Black);

		///<summary>If true, updates to the IScreen data will not be immediately drawn to the screen</summary>
		//todo bool HoldUpdates {get;set;}
		///<summary>If true, the cursor should be blinking at the position indicated by CursorTop and CursorLeft</summary>
		bool CursorVisible {get;set;}
		///<summary>Row position of the blinking cursor</summary>
		int CursorRow {get;set;}
		///<summary>Column position of the blinking cursor</summary>
		int CursorCol {get;set;}

		///<summary>Move the blinking cursor to a new position. Does not change cursor visibility.</summary>
		void SetCursorPosition(int row, int col);

		///<summary>Called immediately before exiting the program. Allows terminals to be reset to original config.</summary>
		void CleanUp();

		///<summary>Ensure that everything sent to the display is actually drawn, process input events,
		/// and return false if the window has been closed, if applicable.</summary>
		bool Update();

		//todo, holdupdates / resumeupdates methods?

		///<summary>Update screen memory to fill the screen with black</summary>
		void Clear();

		//todo - this one is interesting: GLUpdate calls WindowUpdate and therefore lets the gl window process events, returns false if exiting, ...
			// it DOES seem like a 'let input events be read' method could be useful, yeah.
			// Maybe look at how the 2 would actually implement it, before deciding.


		/*todo - some more interesting ones:
		GetCurrentScreen
		GetRect
		BoundsCheck?
		Blank (aka Clear)

		Write some colorglyphs to positions:
			Single
			2d array starting at position

		Write string with color+bgcolor starting at position

		>>> is colorstring needed here? (is there a performance reason to have it?)
			//checking... for the status bar, at least, it's not REALLY needed. It calculates how MANY rows each object takes,
			//  but it uses no other info about the display until actually drawing them. Leaning toward refactor.

		>>> any utility in having a bool to hint whether to group same-color glyphs before writing them? (or, should that actually be, before drawing them to screen?)


		>>> what about a whole 'SwapBuffer'-style method that replaces the whole screen memory with another and returns the old?
			-would need to decide on array style for that, 1D or 2D or jagged or what.
		*/

		//and now... Write, or WriteChar. Time to stop and figure out how I'll do colors here.


		// ALSO need to keep the mouse UI in mind...it'll basically be built on top of...the GL version only, not THIS one. Nevermind.
	}
}
