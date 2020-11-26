using System;
using System.Collections.Generic;
using Forays;
using ForaysUI.ScreenUI;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;
namespace ForaysUI{
	public class MessageBuffer : GameUIObject {
		public bool OmniscienceEnabled; //todo, will this remain here?
		public int RowOffset = 0;
		public int ColOffset = 21;

		protected StringWrapBuffer buffer;
		protected List<string> log;
		protected string currentRepeatedString;
		protected int repetitionCount;
		protected bool interruptPlayer;

		protected const int NUM_LINES = 3;
		protected const int MAX_LENGTH = GameUniverse.MapWidth;
		protected const string MORE = " [more] ";

		public MessageBuffer(GameUniverse g) : base(g) {
			buffer = new StringWrapBuffer(NUM_LINES, MAX_LENGTH, MORE.Length, null, new char[] {' '});
			buffer.BufferFull += HandleOverflow;
			log = new List<string>();
		}
		//todo, ctor for deserialization?
		/// <param name="noInterrupt">By default, any printed message interrupts automatic actions (autoexplore etc.). Set to true to override this behavior.</param>
		/// <param name="requireMorePrompt">If true, the message buffer will add this message and then print its contents immediately, with the '[more]' prompt.</param>
		public void Add(string message, bool noInterrupt = false, bool requireMorePrompt = false){
			if(string.IsNullOrEmpty(message)) return;
			if(!noInterrupt) interruptPlayer = true;
			buffer.Add(Capitalize(message));
			if(requireMorePrompt) Print(true);
		}
		/// <param name="obj">This message will be added only if the player can see this object, or if omniscience is enabled.</param>
		/// <param name="noInterrupt">By default, any printed message interrupts automatic actions (autoexplore etc.). Set to true to override this behavior.</param>
		/// <param name="requireMorePrompt">If true, the message buffer will add this message and then print its contents immediately, with the '[more]' prompt.</param>
		public void AddIfVisible(string message, GameObject obj, bool noInterrupt = false, bool requireMorePrompt = false){
			//todo, GameObject isn't the final type here.
			if(string.IsNullOrEmpty(message)) return;
			if(OmniscienceEnabled || true/*todo, check CanSee here*/){
				if(!noInterrupt) interruptPlayer = true;
				buffer.Add(Capitalize(message));
				if(requireMorePrompt) Print(true);
			}
		}
		/// <param name="obj1">This message will be added only if the player can see obj1 or obj2, or if omniscience is enabled.</param>
		/// <param name="obj2">This message will be added only if the player can see obj1 or obj2, or if omniscience is enabled.</param>
		/// <param name="noInterrupt">By default, any printed message interrupts automatic actions (autoexplore etc.). Set to true to override this behavior.</param>
		/// <param name="requireMorePrompt">If true, the message buffer will add this message and then print its contents immediately, with the '[more]' prompt.</param>
		public void AddIfEitherVisible(string message, GameObject obj1, GameObject obj2, bool noInterrupt = false, bool requireMorePrompt = false){
			//todo, GameObject isn't the final type here.
			if(string.IsNullOrEmpty(message)) return;
			if(OmniscienceEnabled || true || false/*todo, check CanSee on both here*/){
				if(!noInterrupt) interruptPlayer = true;
				buffer.Add(Capitalize(message));
				if(requireMorePrompt) Print(true);
			}
		}
		/// <param name="condition">This message will be added only if 'condition' returns true, or if omniscience is enabled.</param>
		/// <param name="noInterrupt">By default, any printed message interrupts automatic actions (autoexplore etc.). Set to true to override this behavior.</param>
		/// <param name="requireMorePrompt">If true, the message buffer will add this message and then print its contents immediately, with the '[more]' prompt.</param>
		public void AddIfConditionMet(string message, Func<bool> condition, bool noInterrupt = false, bool requireMorePrompt = false){
			if(string.IsNullOrEmpty(message)) return;
			if(OmniscienceEnabled || condition()){
				if(!noInterrupt) interruptPlayer = true;
				buffer.Add(Capitalize(message));
				if(requireMorePrompt) Print(true);
			}
		}
		public void Print(bool requireMorePrompt) {
			if(requireMorePrompt) buffer.EnsureReservedSpace(false);
			DisplayLines(buffer.Clear(), requireMorePrompt, true);
			if(interruptPlayer) {
				//todo interrupt
				interruptPlayer = false;
			}
		}
		public void DisplayContents() {
			DisplayLines(buffer.GetContents(), false, false);
		}
		public List<string> GetMessageLog() { return new List<string>(log); }

		protected void HandleOverflow(List<string> lines) {
			DisplayLines(lines, true, true);
			if(interruptPlayer) {
				//todo interrupt
				interruptPlayer = false;
			}
		}
		protected void DisplayLines(List<string> lines, bool morePrompt, bool addToLog) {
			Screen.HoldUpdates();
			Screen.Clear(RowOffset, ColOffset, NUM_LINES, MAX_LENGTH);
			bool repeated = false;
			string xCount = null; // A string like "(x2)" or "(x127)"
			if(lines.Count == 1){ // Only check for repeats if printing a single line
				string previousLine = null;
				if(currentRepeatedString != null) previousLine = currentRepeatedString;
				else if(log.Count > 0) previousLine = log[log.Count - 1];

				if(previousLine != null && lines[0] == previousLine){
					xCount = "(x" + (repetitionCount + 2) + ")";
					// Repeat this line only if the "x2" part fits. Don't forget to make room for the "[more]" if needed:
					int max = morePrompt? MAX_LENGTH-MORE.Length : MAX_LENGTH;
					repeated = (previousLine.Length + xCount.Length <= MAX_LENGTH);
					if(repeated) currentRepeatedString = previousLine;
				}
			}
			int prevMsgsToPrint = NUM_LINES - lines.Count;
			int startLogIdx = log.Count - NUM_LINES + lines.Count; // Find a starting point in the previous message log. Negatives are allowed.
			if(repeated) startLogIdx--; // If the current message is a repeat, adjust the start index so the last message doesn't appear twice.
			for(int i=0;i<prevMsgsToPrint;++i){
				int logIdx = startLogIdx + i;
				if(logIdx < 0) continue;
				WriteToMessages(i, 0, log[logIdx], Color.DarkGray);
			}
			if(lines.Count == 0){
				Screen.ResumeUpdates();
				return;
			}
			for(int i=0;i<lines.Count;++i){
				WriteToMessages(prevMsgsToPrint + i, 0, lines[i]);
			}
			int currentColumn = lines[lines.Count - 1].Length;
			if(repeated){
				WriteToMessages(NUM_LINES - 1, currentColumn, xCount, Color.DarkGray);
				currentColumn += xCount.Length;
				if(addToLog){
					log[log.Count - 1] = lines[lines.Count - 1] + xCount;
					repetitionCount++;
				}
			}
			else if(addToLog){
				repetitionCount = 0;
				currentRepeatedString = null;
				foreach(string s in lines) log.Add(s);
			}
			if(morePrompt){
				WriteToMessages(NUM_LINES - 1, currentColumn, MORE, Color.Yellow);
				//todo mouse UI buttons
				Screen.ResumeUpdates();
				SetCursorPositionForMessages(NUM_LINES - 1, currentColumn + MORE.Length - 1); // Move cursor to the space at the end of " [more] "
				Input.ReadKey();
				//todo mouse UI buttons
			}
			else{
				Screen.ResumeUpdates();
			}
		}
		protected void WriteToMessages(int row, int col, string message, Color color = Color.Gray, Color bgColor = Color.Black)
			=> Screen.Write(RowOffset + row, ColOffset + col, message, color, bgColor);
		protected void SetCursorPositionForMessages(int row, int col) => Screen.SetCursorPosition(RowOffset + row, ColOffset + col);
		protected static string Capitalize(string s){
				char[] c = s.ToCharArray();
				c[0] = char.ToUpper(c[0]);
				return new string(c);
		}
	}
}

