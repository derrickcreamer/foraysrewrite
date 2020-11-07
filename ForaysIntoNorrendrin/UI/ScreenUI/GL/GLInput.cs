using System;
using System.Threading;
using OpenTK.Input;

namespace ForaysUI.ScreenUI{
	public class GLInput : IInput{
		public bool KeyPressed;
		public ConsoleKeyInfo LastKey;
		public GLScreen Screen;

		public GLInput(){
			Screen = StaticScreen.Screen as GLScreen;
			if(Screen == null) throw new InvalidOperationException("StaticScreen.Screen must be GLScreen");
			Screen.Window.KeyDown += KeyDownHandler;
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
		public void KeyDownHandler(object sender, KeyboardKeyEventArgs args){
			if(KeyPressed) return;
			ConsoleKey ck = GetConsoleKey(args.Key);
			if(ck != ConsoleKey.NoName){
				bool alt = Screen.Window.KeyIsDown(Key.LAlt) || Screen.Window.KeyIsDown(Key.RAlt);
				bool shift = Screen.Window.KeyIsDown(Key.LShift) || Screen.Window.KeyIsDown(Key.RShift);
				bool ctrl = Screen.Window.KeyIsDown(Key.LControl) || Screen.Window.KeyIsDown(Key.RControl);
				if(ck == ConsoleKey.Enter && alt){
					Screen.Window.ToggleFullScreen(); //todo, keeping this here or not?
				}
				else{
					KeyPressed = true;
					LastKey = new ConsoleKeyInfo(GetChar(ck,shift),ck,shift,alt,ctrl);
				}
			}
			//todo MouseUI.RemoveHighlight();
			//todo MouseUI.RemoveMouseover();
		}
		//todo, copied over, not checked carefully:
		public static ConsoleKey GetConsoleKey(Key key){ //convert from openTK's key enum to System.Console's key enum
				if(key >= Key.A && key <= Key.Z){
						return (ConsoleKey)(key - (Key.A - (int)ConsoleKey.A));
				}
				if(key >= Key.Number0 && key <= Key.Number9){
						return (ConsoleKey)(key - (Key.Number0 - (int)ConsoleKey.D0));
				}
				if(key >= Key.Keypad0 && key <= Key.Keypad9){
						return (ConsoleKey)(key - (Key.Keypad9 - (int)ConsoleKey.NumPad9));
				}
				if(key >= Key.F1 && key <= Key.F24){
						return (ConsoleKey)(key - (Key.F1 - (int)ConsoleKey.F1));
				}
				switch(key){
				case Key.BackSpace:
				return ConsoleKey.Backspace;
				case Key.Tab:
				return ConsoleKey.Tab;
				case Key.Enter:
				case Key.KeypadEnter:
				return ConsoleKey.Enter;
				case Key.Escape:
				return ConsoleKey.Escape;
				case Key.Space:
				return ConsoleKey.Spacebar;
				case Key.Delete:
				return ConsoleKey.Delete;
				case Key.Up:
				return ConsoleKey.UpArrow;
				case Key.Down:
				return ConsoleKey.DownArrow;
				case Key.Left:
				return ConsoleKey.LeftArrow;
				case Key.Right:
				return ConsoleKey.RightArrow;
				case Key.Comma:
				return ConsoleKey.OemComma;
				case Key.Period:
				return ConsoleKey.OemPeriod;
				case Key.Minus:
				return ConsoleKey.OemMinus;
				case Key.Plus:
				return ConsoleKey.OemPlus;
				case Key.Tilde:
				return ConsoleKey.Oem3;
				case Key.BracketLeft:
				return ConsoleKey.Oem4;
				case Key.BracketRight:
				return ConsoleKey.Oem6;
				case Key.BackSlash:
				return ConsoleKey.Oem5;
				case Key.Semicolon:
				return ConsoleKey.Oem1;
				case Key.Quote:
				return ConsoleKey.Oem7;
				case Key.Slash:
				return ConsoleKey.Oem2;
				case Key.KeypadDivide:
				return ConsoleKey.Divide;
				case Key.KeypadMultiply:
				return ConsoleKey.Multiply;
				case Key.KeypadMinus:
				return ConsoleKey.Subtract;
				case Key.KeypadAdd:
				return ConsoleKey.Add;
				case Key.KeypadDecimal:
				return ConsoleKey.Decimal;
				case Key.Home:
				return ConsoleKey.Home;
				case Key.End:
				return ConsoleKey.End;
				case Key.PageUp:
				return ConsoleKey.PageUp;
				case Key.PageDown:
				return ConsoleKey.PageDown;
				case Key.Clear:
				return ConsoleKey.Clear;
				case Key.Insert:
				return ConsoleKey.Insert;
				case Key.WinLeft:
				return ConsoleKey.LeftWindows;
				case Key.WinRight:
				return ConsoleKey.RightWindows;
				case Key.Menu:
				return ConsoleKey.Applications;
				default:
				return ConsoleKey.NoName;
				}
		}
		//todo, should this still be used?
		public static char GetChar(ConsoleKey k,bool shift){ //this method tries to return the most correct char for the given values. GetCommandChar(), OTOH, returns game-specific chars.
				if(k >= ConsoleKey.A && k <= ConsoleKey.Z){
						if(shift){
								return k.ToString()[0];
						}
						else{
								return k.ToString().ToLower()[0];
						}
				}
				if(k >= ConsoleKey.D0 && k <= ConsoleKey.D9){
						if(shift){
								switch(k){
								case ConsoleKey.D1:
								return '!';
								case ConsoleKey.D2:
								return '@';
								case ConsoleKey.D3:
								return '#';
								case ConsoleKey.D4:
								return '$';
								case ConsoleKey.D5:
								return '%';
								case ConsoleKey.D6:
								return '^';
								case ConsoleKey.D7:
								return '&';
								case ConsoleKey.D8:
								return '*';
								case ConsoleKey.D9:
								return '(';
								case ConsoleKey.D0:
								default:
								return ')';
								}
						}
						else{
								return k.ToString()[1];
						}
				}
				if(k >= ConsoleKey.NumPad0 && k <= ConsoleKey.NumPad9){
						return k.ToString()[6];
				}
				switch(k){
				case ConsoleKey.Tab:
				return (char)9;
				case ConsoleKey.Enter:
				return (char)13;
				case ConsoleKey.Escape:
				return (char)27;
				case ConsoleKey.Spacebar:
				return ' ';
				case ConsoleKey.OemComma:
				if(shift){
						return '<';
				}
				else{
						return ',';
				}
				case ConsoleKey.OemPeriod:
				if(shift){
						return '>';
				}
				else{
						return '.';
				}
				case ConsoleKey.OemMinus:
				if(shift){
						return '_';
				}
				else{
						return '-';
				}
				case ConsoleKey.OemPlus:
				if(shift){
						return '+';
				}
				else{
						return '=';
				}
				case ConsoleKey.Oem3:
				if(shift){
						return '~';
				}
				else{
						return '`';
				}
				case ConsoleKey.Oem4:
				if(shift){
						return '{';
				}
				else{
						return '[';
				}
				case ConsoleKey.Oem6:
				if(shift){
						return '}';
				}
				else{
						return ']';
				}
				case ConsoleKey.Oem5:
				if(shift){
						return '|';
				}
				else{
						return '\\';
				}
				case ConsoleKey.Oem1:
				if(shift){
						return ':';
				}
				else{
						return ';';
				}
				case ConsoleKey.Oem7:
				if(shift){
						return '"';
				}
				else{
						return '\'';
				}
				case ConsoleKey.Oem2:
				if(shift){
						return '?';
				}
				else{
						return '/';
				}
				case ConsoleKey.Divide:
				return '/';
				case ConsoleKey.Multiply:
				return '*';
				case ConsoleKey.Subtract:
				return '-';
				case ConsoleKey.Add:
				return '+';
				case ConsoleKey.Decimal:
				return '.';
				default:
				return (char)0;
				}
		}
	}
}
