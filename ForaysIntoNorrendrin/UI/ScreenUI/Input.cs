using System;
using System.Collections.Generic;
using System.IO;

namespace ForaysUI.ScreenUI{
	// This class exists so that 'using static' grants easy access to this Input:
	public static class StaticInput{
		public static AbstractInput Input;
	}
	public abstract class AbstractInput{
		protected Dictionary<ConsoleKeyInfo, ConsoleKeyInfo> globalRebindings;
		protected Dictionary<ConsoleKeyInfo, ConsoleKeyInfo> actionRebindings; //todo, needed? i think so.

		public abstract bool KeyIsAvailable {get;}

		public virtual void LoadKeyBindings(){
			string keysFilePath = Program.GetSavePathForFile("keys.txt");
			if(!File.Exists(keysFilePath)){
				byte[] defaultKeysFile = Program.GetEmbeddedFileBytes("Forays.UI.ScreenUI.keys.txt");
				File.WriteAllBytes(keysFilePath, defaultKeysFile); //todo, friendlier check for writability?
			}
			if(!File.Exists(keysFilePath)) return; //todo, should this just return?

			string[] lines = File.ReadAllLines(keysFilePath);
			ParseKeyBindingLines(lines);
		}
		protected virtual void ParseKeyBindingLines(IList<string> lines){
			globalRebindings = new Dictionary<ConsoleKeyInfo, ConsoleKeyInfo>();
			actionRebindings = new Dictionary<ConsoleKeyInfo, ConsoleKeyInfo>();
			// Keep track of the current mode with this var:
			Dictionary<ConsoleKeyInfo, ConsoleKeyInfo> rebindings = globalRebindings;
			char[] lineSplitChars = new char[] { ':' };
			foreach(string line in lines){
				if(line.Length == 0 || line[0] == '#') continue; // skip comments
				string[] parts = line.Split(lineSplitChars, StringSplitOptions.RemoveEmptyEntries);
				if(parts.Length == 1){
					if(string.Equals(parts[0], "global", StringComparison.OrdinalIgnoreCase))
						rebindings = globalRebindings;
					else if(string.Equals(parts[0], "action", StringComparison.OrdinalIgnoreCase))
						rebindings = actionRebindings;
				}
				else if(parts.Length == 2){
					AddKeyBinding(parts, rebindings);
				}
			}
		}
		protected virtual void AddKeyBinding(string[] bindingParts, Dictionary<ConsoleKeyInfo, ConsoleKeyInfo> rebindings){
			// bindingParts is the 2 halves of the binding which were separated by a colon.
			// Go through the 2 halves and see what's mapping to what.
			ConsoleModifiers[] mods = new ConsoleModifiers[2];
			ConsoleKey[] keys = new ConsoleKey[2];
			uint? scancode = null; // If scancode is present, then keys[0] is ignored
			bool rebindAll = false;
			for(int idx=0;idx<2;++idx){
				string[] tokens = bindingParts[idx].Split(' ');
				if(tokens.Length == 0) return;
				bool keyFound = false;
				foreach(string token in tokens){
					if(token == "") continue;
					if(string.Equals(token, "shift", StringComparison.OrdinalIgnoreCase)){
						mods[idx] |= ConsoleModifiers.Shift;
						continue;
					}
					else if(string.Equals(token, "alt", StringComparison.OrdinalIgnoreCase)){
						mods[idx] |= ConsoleModifiers.Alt;
						continue;
					}
					else if(string.Equals(token, "ctrl", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(token, "control", StringComparison.OrdinalIgnoreCase))
					{
						mods[idx] |= ConsoleModifiers.Control;
						continue;
					}
					else if(string.Equals(token, "all", StringComparison.OrdinalIgnoreCase)){
						rebindAll = true;
						continue;
					}
					if(keyFound) return; // If there are more tokens after the key on one half, skip this binding
					uint num;
					if(uint.TryParse(token, out num)){
						keyFound = true;
						if(idx == 0) scancode = num; // Scancodes can appear on the left only.
						else return; // If this is the right-hand side, skip this binding.
					}
					else{
						ConsoleKey key;
						if(Enum.TryParse<ConsoleKey>(token, true, out key)){
							if(key >= ConsoleKey.F21 && key <= ConsoleKey.F24) continue; // F21-F24 are reserved
							keys[idx] = key;
							keyFound = true;
						}
						else{
							return; // Unknown token encountered, so skip this binding
						}
					}
				}
				if(!keyFound) return; // If either half has no key, skip this binding
			}
			if(scancode != null){
				if(rebindAll)
					AddScancodeBindingAll(scancode.Value, keys[1]);
				else
					AddScancodeBinding(scancode.Value, mods[0], GetConsoleKeyInfo(keys[1], mods[1]));
			}
			else{
				if(rebindAll){
					for(int i=0;i<8;++i){
						bool shift = (i & 1) == 1;
						bool alt = (i & 2) == 2;
						bool ctrl = (i & 4) == 4;
						ConsoleKeyInfo allKey = new ConsoleKeyInfo(GetChar(keys[0], shift), keys[0], shift, alt, ctrl);
						ConsoleKeyInfo allValue = new ConsoleKeyInfo(GetChar(keys[1], shift), keys[1], shift, alt, ctrl);
						if(!rebindings.ContainsKey(allKey)) rebindings.Add(allKey, allValue);
					}
				}
				else{
					ConsoleKeyInfo key = GetConsoleKeyInfo(keys[0], mods[0]);
					ConsoleKeyInfo value = GetConsoleKeyInfo(keys[1], mods[1]);
					if(!rebindings.ContainsKey(key)) rebindings.Add(key, value);
				}
			}
		}
		protected virtual void AddScancodeBindingAll(uint scancode, ConsoleKey newKey) { }
		protected virtual void AddScancodeBinding(uint scancode, ConsoleModifiers scancodeMods, ConsoleKeyInfo newBinding) { }
		protected ConsoleKeyInfo GetConsoleKeyInfo(ConsoleKey key, ConsoleModifiers mods){
			bool shift = (mods & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
			bool alt = (mods & ConsoleModifiers.Alt) == ConsoleModifiers.Alt;
			bool ctrl = (mods & ConsoleModifiers.Control) == ConsoleModifiers.Control;
			char ch = GetChar(key, shift);
			return new ConsoleKeyInfo(ch, key, shift, alt, ctrl);
		}
		///<summary>Consume and ignore any keypresses that are waiting</summary>
		public abstract void FlushInput();

		///<summary>Wait for and return a keypress from user</summary>
		public abstract ConsoleKeyInfo ReadKey(bool showCursor = true);

		// This method tries to return the most correct char for the given arguments with no game-specific translation
		protected char GetChar(ConsoleKey key, bool shift){
			if(shift){
				switch(key){
					case ConsoleKey.A: return 'A';
					case ConsoleKey.B: return 'B';
					case ConsoleKey.C: return 'C';
					case ConsoleKey.D: return 'D';
					case ConsoleKey.E: return 'E';
					case ConsoleKey.F: return 'F';
					case ConsoleKey.G: return 'G';
					case ConsoleKey.H: return 'H';
					case ConsoleKey.I: return 'I';
					case ConsoleKey.J: return 'J';
					case ConsoleKey.K: return 'K';
					case ConsoleKey.L: return 'L';
					case ConsoleKey.M: return 'M';
					case ConsoleKey.N: return 'N';
					case ConsoleKey.O: return 'O';
					case ConsoleKey.P: return 'P';
					case ConsoleKey.Q: return 'Q';
					case ConsoleKey.R: return 'R';
					case ConsoleKey.S: return 'S';
					case ConsoleKey.T: return 'T';
					case ConsoleKey.U: return 'U';
					case ConsoleKey.V: return 'V';
					case ConsoleKey.W: return 'W';
					case ConsoleKey.X: return 'X';
					case ConsoleKey.Y: return 'Y';
					case ConsoleKey.Z: return 'Z';

					case ConsoleKey.D1: return '!';
					case ConsoleKey.D2: return '@';
					case ConsoleKey.D3: return '#';
					case ConsoleKey.D4: return '$';
					case ConsoleKey.D5: return '%';
					case ConsoleKey.D6: return '^';
					case ConsoleKey.D7: return '&';
					case ConsoleKey.D8: return '*';
					case ConsoleKey.D9: return '(';
					case ConsoleKey.D0: return ')';

					case ConsoleKey.OemComma: return '<';
					case ConsoleKey.OemPeriod: return '>';
					case ConsoleKey.OemMinus: return '_';
					case ConsoleKey.OemPlus: return '+';
					case ConsoleKey.Oem3: return '~';
					case ConsoleKey.Oem4: return '{';
					case ConsoleKey.Oem6: return '}';
					case ConsoleKey.Oem5: return '|';
					case ConsoleKey.Oem1: return ':';
					case ConsoleKey.Oem7: return '"';
					case ConsoleKey.Oem2: return '?';

					//Duplicated in shifted and unshifted:
					case ConsoleKey.NumPad0: return '0';
					case ConsoleKey.NumPad1: return '1';
					case ConsoleKey.NumPad2: return '2';
					case ConsoleKey.NumPad3: return '3';
					case ConsoleKey.NumPad4: return '4';
					case ConsoleKey.NumPad5: return '5';
					case ConsoleKey.NumPad6: return '6';
					case ConsoleKey.NumPad7: return '7';
					case ConsoleKey.NumPad8: return '8';
					case ConsoleKey.NumPad9: return '9';
					case ConsoleKey.Tab: return (char)9;
					case ConsoleKey.Enter: return (char)13;
					case ConsoleKey.Escape: return (char)27;
					case ConsoleKey.Spacebar: return ' ';
					case ConsoleKey.Divide: return '/';
					case ConsoleKey.Multiply: return '*';
					case ConsoleKey.Subtract: return '-';
					case ConsoleKey.Add: return '+';
					case ConsoleKey.Decimal: return '.';
					default: return (char)0;
				}

			}
			else{ // unshifted
				switch(key){
					case ConsoleKey.A: return 'a';
					case ConsoleKey.B: return 'b';
					case ConsoleKey.C: return 'c';
					case ConsoleKey.D: return 'd';
					case ConsoleKey.E: return 'e';
					case ConsoleKey.F: return 'f';
					case ConsoleKey.G: return 'g';
					case ConsoleKey.H: return 'h';
					case ConsoleKey.I: return 'i';
					case ConsoleKey.J: return 'j';
					case ConsoleKey.K: return 'k';
					case ConsoleKey.L: return 'l';
					case ConsoleKey.M: return 'm';
					case ConsoleKey.N: return 'n';
					case ConsoleKey.O: return 'o';
					case ConsoleKey.P: return 'p';
					case ConsoleKey.Q: return 'q';
					case ConsoleKey.R: return 'r';
					case ConsoleKey.S: return 's';
					case ConsoleKey.T: return 't';
					case ConsoleKey.U: return 'u';
					case ConsoleKey.V: return 'v';
					case ConsoleKey.W: return 'w';
					case ConsoleKey.X: return 'x';
					case ConsoleKey.Y: return 'y';
					case ConsoleKey.Z: return 'z';

					case ConsoleKey.D1: return '1';
					case ConsoleKey.D2: return '2';
					case ConsoleKey.D3: return '3';
					case ConsoleKey.D4: return '4';
					case ConsoleKey.D5: return '5';
					case ConsoleKey.D6: return '6';
					case ConsoleKey.D7: return '7';
					case ConsoleKey.D8: return '8';
					case ConsoleKey.D9: return '9';
					case ConsoleKey.D0: return '0';

					case ConsoleKey.OemComma: return ',';
					case ConsoleKey.OemPeriod: return '.';
					case ConsoleKey.OemMinus: return '-';
					case ConsoleKey.OemPlus: return '=';
					case ConsoleKey.Oem3: return '`';
					case ConsoleKey.Oem4: return '[';
					case ConsoleKey.Oem6: return ']';
					case ConsoleKey.Oem5: return '\\';
					case ConsoleKey.Oem1: return ';';
					case ConsoleKey.Oem7: return '\'';
					case ConsoleKey.Oem2: return '/';

					//Duplicated in shifted and unshifted:
					case ConsoleKey.NumPad0: return '0';
					case ConsoleKey.NumPad1: return '1';
					case ConsoleKey.NumPad2: return '2';
					case ConsoleKey.NumPad3: return '3';
					case ConsoleKey.NumPad4: return '4';
					case ConsoleKey.NumPad5: return '5';
					case ConsoleKey.NumPad6: return '6';
					case ConsoleKey.NumPad7: return '7';
					case ConsoleKey.NumPad8: return '8';
					case ConsoleKey.NumPad9: return '9';
					case ConsoleKey.Tab: return (char)9;
					case ConsoleKey.Enter: return (char)13;
					case ConsoleKey.Escape: return (char)27;
					case ConsoleKey.Spacebar: return ' ';
					case ConsoleKey.Divide: return '/';
					case ConsoleKey.Multiply: return '*';
					case ConsoleKey.Subtract: return '-';
					case ConsoleKey.Add: return '+';
					case ConsoleKey.Decimal: return '.';
					default: return (char)0;
				}
			}
		}
	}
}
