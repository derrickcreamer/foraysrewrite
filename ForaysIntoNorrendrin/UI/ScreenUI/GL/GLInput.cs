using System;
using System.Threading;

namespace ForaysUI.ScreenUI{
	public class GLInput : IInput{
		public bool KeyPressed;
		public ConsoleKeyInfo LastKey;
		public GLScreen Screen;

		public GLInput(){
			Screen = StaticScreen.Screen as GLScreen;
			if(Screen == null) throw new InvalidOperationException("StaticScreen.Screen must be GLScreen");
		}
		public bool KeyIsAvailable => KeyPressed;
		public void FlushInput(){
			Screen.Window.ProcessEvents();
			KeyPressed = false;
		}
		public ConsoleKeyInfo ReadKey(bool showCursor = true){
			if(showCursor) Screen.CursorVisible = true; // todo, this might not be right...
			while(true){
				if(!Screen.Update()) Program.Quit();
				if(Screen.CursorVisible){
					TimeSpan elapsed = Screen.Window.Timer.Elapsed;
					Screen.UpdateCursor(elapsed.Milliseconds < 500); //todo test
				}
				Thread.Sleep(10); //todo configurable?
				if(KeyPressed){
					Screen.CursorVisible = false;
					KeyPressed = false;
					//todo rebindings?
					return LastKey;
				}
			}
		}
	}
}
