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
		public static ConsoleKey GetConsoleKey(OpenTK.Input.Key key){
			switch(key){
                case Key.A: return ConsoleKey.A;
                case Key.B: return ConsoleKey.B;
                case Key.C: return ConsoleKey.C;
                case Key.D: return ConsoleKey.D;
                case Key.E: return ConsoleKey.E;
                case Key.F: return ConsoleKey.F;
                case Key.G: return ConsoleKey.G;
                case Key.H: return ConsoleKey.H;
                case Key.I: return ConsoleKey.I;
                case Key.J: return ConsoleKey.J;
                case Key.K: return ConsoleKey.K;
                case Key.L: return ConsoleKey.L;
                case Key.M: return ConsoleKey.M;
                case Key.N: return ConsoleKey.N;
                case Key.O: return ConsoleKey.O;
                case Key.P: return ConsoleKey.P;
                case Key.Q: return ConsoleKey.Q;
                case Key.R: return ConsoleKey.R;
                case Key.S: return ConsoleKey.S;
                case Key.T: return ConsoleKey.T;
                case Key.U: return ConsoleKey.U;
                case Key.V: return ConsoleKey.V;
                case Key.W: return ConsoleKey.W;
                case Key.X: return ConsoleKey.X;
                case Key.Y: return ConsoleKey.Y;
                case Key.Z: return ConsoleKey.Z;

                case Key.Number0: return ConsoleKey.D0;
                case Key.Number1: return ConsoleKey.D1;
                case Key.Number2: return ConsoleKey.D2;
                case Key.Number3: return ConsoleKey.D3;
                case Key.Number4: return ConsoleKey.D4;
                case Key.Number5: return ConsoleKey.D5;
                case Key.Number6: return ConsoleKey.D6;
                case Key.Number7: return ConsoleKey.D7;
                case Key.Number8: return ConsoleKey.D8;
                case Key.Number9: return ConsoleKey.D9;

                case Key.Keypad0: return ConsoleKey.NumPad0;
                case Key.Keypad1: return ConsoleKey.NumPad1;
                case Key.Keypad2: return ConsoleKey.NumPad2;
                case Key.Keypad3: return ConsoleKey.NumPad3;
                case Key.Keypad4: return ConsoleKey.NumPad4;
                case Key.Keypad5: return ConsoleKey.NumPad5;
                case Key.Keypad6: return ConsoleKey.NumPad6;
                case Key.Keypad7: return ConsoleKey.NumPad7;
                case Key.Keypad8: return ConsoleKey.NumPad8;
                case Key.Keypad9: return ConsoleKey.NumPad9;

                case Key.F1: return ConsoleKey.F1;
                case Key.F2: return ConsoleKey.F2;
                case Key.F3: return ConsoleKey.F3;
                case Key.F4: return ConsoleKey.F4;
                case Key.F5: return ConsoleKey.F5;
                case Key.F6: return ConsoleKey.F6;
                case Key.F7: return ConsoleKey.F7;
                case Key.F8: return ConsoleKey.F8;
                case Key.F9: return ConsoleKey.F9;
                case Key.F10: return ConsoleKey.F10;
                case Key.F11: return ConsoleKey.F11;
                case Key.F12: return ConsoleKey.F12;
                case Key.F13: return ConsoleKey.F13;
                case Key.F14: return ConsoleKey.F14;
                case Key.F15: return ConsoleKey.F15;
                case Key.F16: return ConsoleKey.F16;
                case Key.F17: return ConsoleKey.F17;
                case Key.F18: return ConsoleKey.F18;
                case Key.F19: return ConsoleKey.F19;
                case Key.F20: return ConsoleKey.F20;
                case Key.F21: return ConsoleKey.F21;
                case Key.F22: return ConsoleKey.F22;
                case Key.F23: return ConsoleKey.F23;
                case Key.F24: return ConsoleKey.F24;

				case Key.BackSpace: return ConsoleKey.Backspace;
				case Key.Tab: return ConsoleKey.Tab;
				case Key.Enter:
				case Key.KeypadEnter:
					return ConsoleKey.Enter;
				case Key.Escape: return ConsoleKey.Escape;
				case Key.Space: return ConsoleKey.Spacebar;
				case Key.Delete: return ConsoleKey.Delete;
				case Key.Up: return ConsoleKey.UpArrow;
				case Key.Down: return ConsoleKey.DownArrow;
				case Key.Left: return ConsoleKey.LeftArrow;
				case Key.Right: return ConsoleKey.RightArrow;
				case Key.Comma: return ConsoleKey.OemComma;
				case Key.Period: return ConsoleKey.OemPeriod;
				case Key.Minus: return ConsoleKey.OemMinus;
				case Key.Plus: return ConsoleKey.OemPlus;
				case Key.Tilde: return ConsoleKey.Oem3;
				case Key.BracketLeft: return ConsoleKey.Oem4;
				case Key.BracketRight: return ConsoleKey.Oem6;
				case Key.BackSlash: return ConsoleKey.Oem5;
				case Key.Semicolon: return ConsoleKey.Oem1;
				case Key.Quote: return ConsoleKey.Oem7;
				case Key.Slash: return ConsoleKey.Oem2;
				case Key.KeypadDivide: return ConsoleKey.Divide;
				case Key.KeypadMultiply: return ConsoleKey.Multiply;
				case Key.KeypadMinus: return ConsoleKey.Subtract;
				case Key.KeypadAdd: return ConsoleKey.Add;
				case Key.KeypadDecimal: return ConsoleKey.Decimal;
				case Key.Home: return ConsoleKey.Home;
				case Key.End: return ConsoleKey.End;
				case Key.PageUp: return ConsoleKey.PageUp;
				case Key.PageDown: return ConsoleKey.PageDown;
				case Key.Clear: return ConsoleKey.Clear;
				case Key.Insert: return ConsoleKey.Insert;
				case Key.WinLeft: return ConsoleKey.LeftWindows;
				case Key.WinRight: return ConsoleKey.RightWindows;
				case Key.Menu: return ConsoleKey.Applications;

				default: return ConsoleKey.NoName;
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
