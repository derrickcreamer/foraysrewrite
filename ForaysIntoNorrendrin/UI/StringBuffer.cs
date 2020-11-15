//todo, copyright/license on all files
using System;
using System.Collections.Generic;

namespace ForaysUtilities {

	/// <summary>
	/// A multi-line word-wrapping string buffer.
	/// </summary>
	public class StringWrapBuffer {
		public StringWrapBuffer(int maxLines,int maxLength) : this(maxLines,maxLength,new char[] {'-'},new char[] {' '}) { }

		/// <param name="maxLines">If set to 0 or less, there's no limit to the number of lines.</param>
		/// <param name="maxLength">Must be 1 or greater.</param>
		/// <param name="retainedSeparators">Separator characters that should be kept when they divide two words.
		/// For an example with '!' as a retained separator, "nine!three" becomes "nine!" and "three".</param>
		/// <param name="discardedSeparators">Separator characters that should be discarded when they divide two words (during word wrap only).
		/// For an example with '!' as a discarded separator, "seven!four" becomes "seven" and "four".</param>
		public StringWrapBuffer(int maxLines,int maxLength,IEnumerable<char> retainedSeparators,IEnumerable<char> discardedSeparators) {
			this.maxLines = maxLines;
			if(maxLength < 1) throw new ArgumentOutOfRangeException("maxLength",maxLength,"Max length must be at least 1.");
			this.maxLength = maxLength;
			this.contents = new List<string>();
			this.createNewLine = true;
			this.reservedWrapData = null;
			this.reservedSpace = 0;
			if(retainedSeparators == null) {
				this.retainedSeparators = new char[0];
			}
			else {
				this.retainedSeparators = new HashSet<char>(retainedSeparators);
			}
			if(discardedSeparators == null) {
				this.discardedSeparators = new char[0];
			}
			else {
				this.discardedSeparators = new HashSet<char>(discardedSeparators);
			}
		}
		public void Add(string s) {
			if(string.IsNullOrEmpty(s)) return;
			if(createNewLine) {
				s = RemoveLeadingDiscardedSeparators(s);
				if(s != "") {
					createNewLine = false;
					if(contents.Count == maxLines && reservedSpace > 0 && reservedWrapData != null) {
						var reserveSplit = SplitOverflow(reservedWrapData,maxLength - reservedSpace);
						reservedWrapData = null;
						contents[contents.Count - 1] = reserveSplit[0]; //this string is resplit to make room for reserved space
						reserveSplit[1] = RemoveLeadingDiscardedSeparators(reserveSplit[1]);
						contents.Add(reserveSplit[1] + s); //if there's overflow from *that* line, it gets added before our new addition.
					}
					else {
						reservedWrapData = null;
						contents.Add(s);
					}
					CheckForBufferOverflow();
					CheckForLineOverflow();
				}
			}
			else {
				contents[contents.Count - 1] += s;
				CheckForLineOverflow();
			}
		}

		/// <summary>
		/// Empties the buffer, and returns the just-removed contents.
		/// </summary>
		public List<string> Clear() {
			var previousContents = Contents;
			contents = new List<string>();
			createNewLine = true;
			reservedWrapData = null;
			return previousContents;
		}

		/// <summary>
		/// A list of all (non-empty) strings in the buffer.
		/// </summary>
		public List<string> Contents => new List<string>(contents);

		/// <summary>
		/// The maximum length of a single line in the buffer.
		/// (Note that changing this value will NOT affect any lines that have already wrapped; it will only affect the current line and future lines.)
		/// </summary>
		public int MaxLength {
			get { return maxLength; }
			set {
				if(value < 1) throw new ArgumentOutOfRangeException("value",value,"Max length must be at least 1.");
				if(value - reservedSpace <= 0) throw new ArgumentOutOfRangeException("value",value,"Max length must be greater than ReservedSpace.");
				if(maxLength != value) {
					maxLength = value;
					CheckForLineOverflow();
				}
			}
		}

		/// <summary>
		/// The maximum number of lines in the buffer. If zero or a negative number, there is no limit.
		/// (Note: ReservedSpace is not respected during MaxLines changes.)
		/// </summary>
		public int MaxLines {
			get { return maxLines; }
			set {
				if(maxLines != value) {
					if(value > 0 && value < contents.Count) reservedWrapData = null;
					maxLines = value;
					CheckForBufferOverflow();
				}
			}
		}

		/// <summary>
		/// Changes word wrap behavior on the final line of the buffer.
		/// When the final line wraps, the wrapping will reserve this many characters (at the end) before deciding where to split the string.
		/// (Note that this value changes HOW word wrap happens on the final line. It does not change WHEN word wrap happens -- it does not reduce the max length.)
		/// </summary>
		public int ReservedSpace {
			get { return reservedSpace; }
			set {
				if(value < 0) throw new ArgumentOutOfRangeException("value",value,"Reserved space cannot be negative.");
				if(maxLength - value <= 0) throw new ArgumentOutOfRangeException("value",value,"Reserved space must be less than MaxLength.");
				reservedSpace = value;
			}
		}

		/// <summary>
		/// If there are characters in the reserved space, this method will cause the current line to wrap
		/// in accordance with the reserved space. (This method will affect the current line even
		/// if it isn't the final line.)
		/// </summary>
		public void ConfirmReservedSpace() {
			if(contents.Count > 0 && contents[contents.Count - 1].Length > maxLength - reservedSpace) {
				string line = reservedWrapData ?? contents[contents.Count - 1];
				var reservedSplit = SplitOverflow(line,maxLength - reservedSpace);
				reservedWrapData = null;
				contents[contents.Count - 1] = reservedSplit[0];
				createNewLine = true;
				Add(reservedSplit[1]);
			}
		}

		/// <summary>
		/// Whenever the buffer overflows beyond its capacity, listeners to this event will receive the current contents of the buffer, not including any overflow.
		/// Afterward, those contents will be emptied, and the buffer will now contain only the remaining overflow.
		/// </summary>
		public event Action<List<string>> BufferFull;

		protected int maxLines;
		protected int maxLength;
		protected ICollection<char> retainedSeparators;
		protected ICollection<char> discardedSeparators;
		protected List<string> contents;
		protected bool createNewLine;
		protected string reservedWrapData;
		protected int reservedSpace;
		protected void CheckForLineOverflow() {
			while(!createNewLine && contents[contents.Count - 1].Length > maxLength) {
				createNewLine = true; //no matter what, THIS line is done -- no more will be added to it.
				var maxSplit = SplitOverflow(contents[contents.Count - 1],maxLength);
				maxSplit[1] = RemoveLeadingDiscardedSeparators(maxSplit[1]);
				if(maxSplit[1] != "") { //if the default overflow would be printed...
					if(reservedSpace > 0 && contents.Count == maxLines) { //if this is the last line (and if reserved space matters)...
						var reservedSplit = SplitOverflow(contents[contents.Count - 1],maxLength - reservedSpace); //calculate the reserved space split.
						contents[contents.Count - 1] = reservedSplit[0];
						Add(reservedSplit[1]);
					}
					else {
						contents[contents.Count - 1] = maxSplit[0];
						Add(maxSplit[1]);
					}
				}
				else { //if the default overflow won't be printed, make note of the original string for reserved-space purposes.
					reservedWrapData = contents[contents.Count - 1];
					contents[contents.Count - 1] = maxSplit[0];
				}
			}
		}
		protected string RemoveLeadingDiscardedSeparators(string s) {
			int idx = -1;
			for(int i = 0;i<s.Length;++i) {
				if(discardedSeparators.Contains(s[i])) {
					idx = i;
				}
				else {
					break;
				}
			}
			if(idx == -1) return s;
			return s.Substring(idx + 1);
		}
		protected void CheckForBufferOverflow() {
			if(maxLines < 1) return;
			while(contents.Count > maxLines) {
				var fullBuffer = contents.GetRange(0,maxLines);
				contents = contents.GetRange(maxLines,contents.Count - maxLines);
				BufferFull?.Invoke(fullBuffer);
			}
		}
		protected string[] SplitOverflow(string s,int startIdx) {
			int overflowIdx = FindSplitIdx(s,startIdx);
			return new string[] { s.Substring(0,overflowIdx),s.Substring(overflowIdx) };
		}
		protected int FindSplitIdx(string s, int startIdx) {
			if(startIdx >= s.Length) startIdx = s.Length - 1;
			for(int tentativeIdx = startIdx;true;--tentativeIdx) { //at each step, we check tentativeIdx and tentativeIdx-1.
				if(tentativeIdx <= 0) { //if 0 is reached, there are no separators in this string.
					return startIdx;
				}
				if(retainedSeparators.Contains(s[tentativeIdx - 1])) { //a retained separator on the left of the tentativeIdx is always a valid split.
					return tentativeIdx;
				}
				if(!discardedSeparators.Contains(s[tentativeIdx - 1]) && discardedSeparators.Contains(s[tentativeIdx])) { //don't stop at the first discarded separator.
					return tentativeIdx;
				}
			}
		}
	}
}
