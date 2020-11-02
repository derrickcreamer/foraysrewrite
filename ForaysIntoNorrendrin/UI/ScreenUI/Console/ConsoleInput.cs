using System;
using static ForaysUI.ScreenUI.StaticScreen;

namespace ForaysUI.ScreenUI{
	public class ConsoleInput : IInput{
		public bool KeyIsAvailable => Console.KeyAvailable;

		public void FlushInput(){
			while(Console.KeyAvailable) Console.ReadKey(true);
		}

		public ConsoleKeyInfo ReadKey(bool showCursor = true){
			if(showCursor) Screen.CursorVisible = true; // todo, this might not be right...
			ConsoleKeyInfo rawKey = Console.ReadKey(true);
			Screen.CursorVisible = false; //todo...?
			bool shift = (rawKey.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
			//...  so, GetChar is called here...what does it convert?
			//  well, it goes from ConsoleKey to char so a new CKI can be built...
			//  makes sure top-row numbers give the expected things based on US layout...
			// numpad numbers, same...
			// tab, enter, escape... chars 9, 13, 27
			// more US layout stuff...
			// so let's leave a big TODO here:  figure out which ones come in with the wrong chars, and fix those.

			//todo, also rebindings, here or elsewhere?

			return rawKey;
		}
	}
}
