//todo, copyright/license on all files
using System;
using System.Collections.Generic;
using System.Text;

namespace ForaysUI {

	/// <summary>
	/// A multi-line word-wrapping string buffer.
	/// </summary>
	public class StringWrapBuffer {
		/// <summary>
		/// Whenever the buffer overflows beyond its capacity, listeners to this event will receive the current contents of the buffer, not including any overflow.
		/// Afterward, those contents will be emptied, and the buffer will now contain only the remaining overflow.
		/// </summary>
		public event Action<List<string>> BufferFull;

		protected int maxLines;
		protected int maxLength;
		protected int reservedSpace;
		protected HashSet<char> retainedSeparators;
		protected HashSet<char> discardedSeparators;
		protected List<string> previousLines;
		protected StringBuilder currentLine;

		/// <summary>Constructs a StringWrapBuffer with '-' (hyphen) as a retained separator, and ' ' (space) as a discarded separator.</summary>
		/// <param name="maxLines">If set to less than 1, lines will wrap but the buffer will never be full.</param>
		/// <param name="maxLength">Maximum length of a single line in the buffer. Lines that exceed this length will be wrapped based on specified separators. Must be 1 or greater.</param>
		/// <param name="reservedSpace">
		/// Changes word wrap behavior on the final line of the buffer.
		/// When the final line wraps, the wrapping will reserve this many characters (at the end) before deciding where to split the string.
		/// (Note that this value changes HOW word wrap happens on the final line, but does not change WHEN word wrap happens, because it does not actually reduce the max length.)</param>
		public StringWrapBuffer(int maxLines,int maxLength, int reservedSpace = 0) : this(maxLines,maxLength,reservedSpace,new char[] {'-'},new char[] {' '}) { }

		/// <param name="maxLines">If set to less than 1, lines will wrap but the buffer will never be full.</param>
		/// <param name="maxLength">Maximum length of a single line in the buffer. Lines that exceed this length will be wrapped based on specified separators. Must be 1 or greater.</param>
		/// <param name="reservedSpace">
		/// Changes word wrap behavior on the final line of the buffer.
		/// When the final line wraps, the wrapping will reserve this many characters (at the end) before deciding where to split the string.
		/// (Note that this value changes HOW word wrap happens on the final line, but does not change WHEN word wrap happens, because it does not actually reduce the max length.)</param>
		/// <param name="retainedSeparators">Separator characters that should be kept when they divide two words.
		/// For an example with '!' as a retained separator, "nine!three" becomes "nine!" and "three".</param>
		/// <param name="discardedSeparators">Separator characters that should be discarded when they divide two words (during word wrap only).
		/// For an example with '~' as a discarded separator, "seven~four" becomes "seven" and "four".</param>
		public StringWrapBuffer(int maxLines,int maxLength,int reservedSpace,ICollection<char> retainedSeparators,ICollection<char> discardedSeparators) {
			this.maxLines = maxLines;
			if(maxLength < 1) throw new ArgumentOutOfRangeException(nameof(maxLength),maxLength,"Max length must be at least 1.");
			this.maxLength = maxLength;
			currentLine = new StringBuilder(maxLength * 2);
			previousLines = new List<string>();
			if(retainedSeparators?.Count > 0) {
				this.retainedSeparators = new HashSet<char>(retainedSeparators);
			}
			if(discardedSeparators?.Count > 0) {
				this.discardedSeparators = new HashSet<char>(discardedSeparators);
			}
		}

		/// <summary>
		/// The maximum length of a single line in the buffer.
		/// </summary>
		public int MaxLength => maxLength;

		/// <summary>
		/// The maximum number of lines in the buffer. If zero or a negative number, there is no limit.
		/// </summary>
		public int MaxLines => maxLines;

		/// <summary>
		/// Changes word wrap behavior on the final line of the buffer.
		/// When the final line wraps, the wrapping will reserve this many characters (at the end) before deciding where to split the string.
		/// (Note that this value changes HOW word wrap happens on the final line, but does not change WHEN word wrap happens, because it does not actually reduce the max length.)
		/// </summary>
		public int ReservedSpace => reservedSpace;

		/// <summary>
		/// A list of all (non-empty) strings in the buffer.
		/// </summary>
		public List<string> GetContents() {
			var lines = new List<string>(previousLines);
			string current = currentLine.ToString();
			if(!string.IsNullOrEmpty(current)) lines.Add(current);
			return lines;
		}
		/// <summary>
		/// Empties the buffer, and returns the just-removed contents.
		/// </summary>
		public List<string> Clear() {
			List<string> previousContents = previousLines;
			previousContents.Add(currentLine.ToString());
			previousLines = new List<string>();
			currentLine.Clear();
			return previousContents;
		}
		public void Add(string s) {
			if(string.IsNullOrEmpty(s)) return;
			currentLine.Append(s);
			CheckForLineOverflow();
		}
		/// <summary>
		/// If there are characters in the reserved space at the end of the current line, this method will cause the current line to wrap
		/// in accordance with the reserved space.
		/// </summary>
		/// <param name="lastLineOnly">If false, this method reserves space at the end of the current line *regardless* of how many lines are
		/// currently in the buffer. If true, this method only does anything if the current line is the final line before the buffer fills.</param>
		/// <param name="overrideReservedSpace">If not null, this value is used instead of the ReservedSpace specified when this object was constructed.</param>
		public void EnsureReservedSpace(bool lastLineOnly, int? overrideReservedSpace = null) {
			if(lastLineOnly && previousLines.Count + 1 < maxLines) return;
			int startIdx = maxLength - (overrideReservedSpace ?? this.reservedSpace);
			if(startIdx <= 0) throw new InvalidOperationException("Can't reserve more space than maxLength");
			while(currentLine.Length > startIdx) {
				// Split the current line, and add the first part to this.previousLines.
				SplitOverflow(startIdx);
				CheckForBufferOverflow();
			}
		}
		/// <summary>
		/// Finishes the current line - no more will be added to the current line. Further strings will be added on a new line.
		/// If the current line is the final line in the buffer, the reserved space will be considered and some portion of this line might be wrapped to the new line.
		/// </summary>
		/// <param name="wrapIfEmpty">If false, nothing will happen if the current line is empty.</param>
		public void WrapCurrentLine(bool wrapIfEmpty) {
			// If current line is in reserved space (last line + long enough string), do this like a normal overflow:
			if(previousLines.Count + 1 == maxLines && currentLine.Length > maxLength-reservedSpace){
				SplitOverflow(maxLength - reservedSpace);
				CheckForBufferOverflow();
			}
			else{ // Otherwise, skip the splitting code:
				previousLines.Add(currentLine.ToString());
				currentLine.Clear();
			}
		}
		protected void CheckForLineOverflow() {
			while(currentLine.Length > maxLength) {
				// Split the current line, and add the first part to this.previousLines.
				// Start at an index equal to maxLength, unless this line will fill the buffer:
				int startIdx = (previousLines.Count + 1 == maxLines)? maxLength-reservedSpace : maxLength;
				SplitOverflow(startIdx);
				CheckForBufferOverflow();
			}
		}
		protected void CheckForBufferOverflow() {
			if(maxLines >= 1 && previousLines.Count == maxLines) {
				BufferFull?.Invoke(previousLines);
				previousLines.Clear();
			}
		}
		protected void SplitOverflow(int startIdx) {
			int splitIdx, numDiscardedSeparators;
			FindSplitIdx(currentLine.ToString(), startIdx, out splitIdx, out numDiscardedSeparators);
			previousLines.Add(currentLine.ToString(0, splitIdx));
			currentLine.Remove(0, splitIdx + numDiscardedSeparators);
		}
		protected void FindSplitIdx(string s, int startIdx, out int splitIdx, out int numDiscardedSeparators) {
			if(startIdx >= s.Length) startIdx = s.Length - 1;
			numDiscardedSeparators = 0;
			for(int tentativeIdx = startIdx;true;--tentativeIdx) { //at each step, we check tentativeIdx and tentativeIdx-1.
				if(tentativeIdx <= 0) { //if 0 is reached, there are no separators in this string.
					splitIdx = startIdx;
					return;
				}
				if(retainedSeparators?.Contains(s[tentativeIdx - 1]) == true) { //a retained separator on the left of the tentativeIdx is always a valid split.
					splitIdx = tentativeIdx;
					return;
				}
				// Note that idx-1 is checked for retained separators, while idx is checked for discarded ones:
				if(discardedSeparators?.Contains(s[tentativeIdx]) == true) {
					splitIdx = tentativeIdx;
					numDiscardedSeparators = 1; // Could extend this later to look for multiple discarded separators.
					return;
				}
			}
		}
	}
}
