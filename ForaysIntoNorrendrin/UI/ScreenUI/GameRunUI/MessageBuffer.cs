using System;
using System.Text;
using System.Collections.Generic;
using Forays;
using GrammarUtility;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;

namespace ForaysUI.ScreenUI{
	public enum Punctuation { Period, ExclamationPoint, QuestionMark, Ellipsis };
	public enum Visibility { RequireEither, RequireBoth, RequireSubject, RequireObject, AlwaysVisible };
	public class MessageBuffer : GameUIObject {
		public bool OmniscienceEnabled;
		public static int RowOffset; // Offset values are set by GameRunUI.UpdateMessagesOption
		public static int ColOffset;

		protected StringWrapBuffer buffer;
		protected List<string> log;
		protected string currentRepeatedString;
		protected int repetitionCount;
		protected bool interruptPlayer;
		protected StringBuilder sb;

		protected const int NUM_LINES = 4;
		protected const int MAX_LENGTH = GameUniverse.MapWidth;
		protected const string MORE = " [more] ";

		public MessageBuffer(GameRunUI ui) : base(ui) {
			buffer = new StringWrapBuffer(NUM_LINES, MAX_LENGTH, MORE.Length, null, new char[] {' '});
			buffer.BufferFull += HandleOverflow;
			log = new List<string>();
			sb = new StringBuilder();
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
				//todo, should this also darken the other UI sections?
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
		public void Add(Determinative subjectDeterminative, string subj, string verb, Determinative objectDeterminative, string obj,
			Punctuation punctuation = Punctuation.Period, bool noInterrupt = false, bool requireMorePrompt = false)
		{
			sb.Append(Grammar.Get(subjectDeterminative, subj, verb));
			if(obj != null){
				sb.Append(" ");
				sb.Append(Grammar.Get(objectDeterminative, obj));
			}
			if(punctuation == Punctuation.Period) sb.Append(".");
			else if(punctuation == Punctuation.ExclamationPoint) sb.Append("!");
			else if(punctuation == Punctuation.Ellipsis) sb.Append("...");
			else if(punctuation == Punctuation.QuestionMark) sb.Append("?");
			sb.Append(" ");
			string message = sb.ToString();
			sb.Clear();
			Add(message, noInterrupt, requireMorePrompt);
		}
		//todo, extra strings?
		public void Add(Determinative subjectDeterminative, Creature subj, string verb, Determinative objectDeterminative, Creature obj,
			Punctuation punctuation = Punctuation.Period, Visibility visibility = Visibility.RequireEither,
			bool assumeSubjectVisible = false, bool assumeObjectVisible = false, bool noInterrupt = false, bool requireMorePrompt = false)
		{
			bool subjectVisible = OmniscienceEnabled || assumeSubjectVisible || Player.CanSee(subj);
			bool objectVisible = obj != null && (OmniscienceEnabled || assumeObjectVisible || Player.CanSee(obj));
			if(visibility == Visibility.RequireEither && !subjectVisible && !objectVisible) return;
			if(visibility == Visibility.RequireBoth && (!subjectVisible || !objectVisible)) return;
			if(visibility == Visibility.RequireSubject && !subjectVisible) return;
			if(visibility == Visibility.RequireObject && !objectVisible) return;
			string subjectName = subjectVisible? Names.Get(subj.OriginalType) : "something";
			string objectName = objectVisible? Names.Get(obj.OriginalType)
				: (obj == null)? null : "something";
			Add(subjectDeterminative, subjectName, verb, objectDeterminative, objectName, punctuation, noInterrupt, requireMorePrompt);
		}
		// AddSimple for any sentence which has no object.
		// Each method also has an overload without Determinatives. These methods assume that "the" should be used.
		public void AddSimple(Determinative subjectDeterminative, Creature subj, string verb, Punctuation punctuation = Punctuation.Period,
			Visibility visibility = Visibility.RequireSubject, bool assumeSubjectVisible = false,
			bool noInterrupt = false, bool requireMorePrompt = false)
				=> Add(subjectDeterminative, subj, verb, Determinative.None, null, punctuation, visibility,
					assumeSubjectVisible, false, noInterrupt, requireMorePrompt);
		// Same as above, but assuming a determinative of "the":
		public void Add(Creature subj, string verb, Creature obj,
			Punctuation punctuation = Punctuation.Period, Visibility visibility = Visibility.RequireEither,
			bool assumeSubjectVisible = false, bool assumeObjectVisible = false, bool noInterrupt = false, bool requireMorePrompt = false)
				=> Add(Determinative.The, subj, verb, Determinative.The, obj, punctuation, visibility,
					assumeSubjectVisible, assumeObjectVisible, noInterrupt, requireMorePrompt);
		public void AddSimple(Creature subj, string verb,
			Punctuation punctuation = Punctuation.Period, Visibility visibility = Visibility.RequireSubject,
			bool assumeSubjectVisible = false, bool assumeObjectVisible = false, bool noInterrupt = false, bool requireMorePrompt = false)
				=> Add(Determinative.The, subj, verb, Determinative.None, null, punctuation, visibility, //todo, is assumeObjectVisible unused here?
					assumeSubjectVisible, false, noInterrupt, requireMorePrompt);
}
}
