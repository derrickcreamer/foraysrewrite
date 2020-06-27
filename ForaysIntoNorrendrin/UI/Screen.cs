using System;

namespace ForaysUI{
    public interface IScreen{
        //todo - screen memory is apparently not directly exposed. There MIGHT need to be a getter.

        ///<summary>If true, updates to the IScreen data will not be immediately drawn to the screen (if applicable)</summary>
        bool HoldUpdates {get;set;}
        ///<summary>If true, the cursor should be blinking at the position indicated by CursorTop and CursorLeft</summary>
        bool CursorVisible {get;set;}
        ///<summary>Position of the blinking cursor relative to the left of the screen</summary>
        int CursorLeft {get;set;}
        ///<summary>Position of the blinking cursor relative to the top of the screen</summary>
        int CursorTop {get;set;}

        ///<summary>Move the blinking cursor to a new position. Does not change cursor visibility.</summary>
        void SetCursorPosition(int left, int top);

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

        >>> any utility in having a bool to hint whether to group same-color glyphs before writing them? (or, should that actually be, before drawing them to screen?)


        >>> what about a whole 'SwapBuffer'-style method that replaces the whole screen memory with another and returns the old?
            -would need to decide on array style for that, 1D or 2D or jagged or what.
        */

        //and now... Write, or WriteChar. Time to stop and figure out how I'll do colors here.


        // ALSO need to keep the mouse UI in mind...it'll basically be built on top of...the GL version only, not THIS one. Nevermind.
    }
}
