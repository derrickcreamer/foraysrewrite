using System;

namespace ForaysUI.ScreenUI{
	// This class exists so that 'using static' grants easy access to this Input:
	public static class StaticInput{
		public static IInput Input;
	}
	public interface IInput{
		bool KeyIsAvailable {get;}

		//todo rebindings? where do those go?

		///<summary>Consume and ignore any keypresses that are waiting</summary>
		void FlushInput();

		///<summary>Wait for and return a keypress from user</summary>
		ConsoleKeyInfo ReadKey(bool showCursor = true);
	}
}
