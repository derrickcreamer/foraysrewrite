using System;
using System.Collections.Generic;

namespace ForaysUI.ScreenUI{
	public class GLScreen : IScreen{
		public ForaysWindow Window;
		private bool cursorVisible;
		private int cursorRow, cursorCol;
		private int holdUpdatesCount;
		private int firstChangedIdx, lastChangedIdx;
		private ColorGlyph[] screenMemory;

		public GLScreen(int rows, int cols){
			GLColors.Initialize();
			Rows = rows;
			Cols = cols;
			ResetChangedIndices();
			screenMemory = new ColorGlyph[Rows*Cols];
			Window = ForaysWindow.Create(cols, rows);
		}
		public int Rows {get;set;}
		public int Cols {get;set;}
		public void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black){
			int idx = col + row*Cols;
			if(screenMemory[idx].Equals(glyphIndex, color, bgColor)) return;
			color = color.ResolveColor(); // Random colors won't match at first, but
			bgColor = bgColor.ResolveColor(); // might match after being resolved, so check again:
			if(screenMemory[idx].Equals(glyphIndex, color, bgColor)) return;
			screenMemory[idx] = new ColorGlyph(glyphIndex, color, bgColor);
			if(firstChangedIdx > idx) firstChangedIdx = idx;
			if(lastChangedIdx < idx) lastChangedIdx = idx;
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public void Write(int row, int col, ColorGlyph cg){
			int idx = col + row*Cols;
			if(screenMemory[idx].Equals(cg)) return;
			Color color = cg.ForegroundColor.ResolveColor(); // Random colors won't match at first, but
			Color bgColor = cg.BackgroundColor.ResolveColor(); // might match after being resolved, so check again:
			if(screenMemory[idx].Equals(cg.GlyphIndex, color, bgColor)) return;
			screenMemory[idx] = new ColorGlyph(cg.GlyphIndex, color, bgColor);
			if(firstChangedIdx > idx) firstChangedIdx = idx;
			if(lastChangedIdx < idx) lastChangedIdx = idx;
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public void Write(int row, int col, string str, Color color = Color.Gray, Color bgColor = Color.Black){
			int count;
			if(col + str.Length > Cols) count = Cols - col; // Cut off too-long strings
			else count = str.Length;
			int startIdx = col + row*Cols;
			for(int n=0;n<count;++n){
				int idx = startIdx + n;
				int glyphIndex = (int)str[n];
				if(screenMemory[idx].Equals(glyphIndex, color, bgColor))
					continue;
				Color charColor = color.ResolveColor(); // Random colors won't match at first, but
				Color charBgColor = bgColor.ResolveColor(); // might match after being resolved, so check again:
				if(screenMemory[idx].Equals(glyphIndex, charColor, charBgColor)) return;
				screenMemory[idx] = new ColorGlyph(glyphIndex, charColor, charBgColor);
				if(firstChangedIdx > idx) firstChangedIdx = idx;
				if(lastChangedIdx < idx) lastChangedIdx = idx;
			}
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public void Write(int startRow, int startCol, ColorGlyph[][] colorGlyphs){
			for(int i=0;i<colorGlyphs.Length;++i){
				if(startRow + i >= Rows) break;
				ColorGlyph[] line = colorGlyphs[i];
				int count;
				if(startCol + line.Length > Cols) count = Cols - startCol; // Cut off too-long strings
				else count = line.Length;
				int startIdx = startCol + (startRow + i)*Cols;
				for(int j=0;j<count;++j){
					int idx = startIdx + j;
					ColorGlyph cg = line[j];
					if(screenMemory[idx].Equals(cg))
						continue;
					Color color = cg.ForegroundColor.ResolveColor(); // Random colors won't match at first, but
					Color bgColor = cg.BackgroundColor.ResolveColor(); // might match after being resolved, so check again:
					if(screenMemory[idx].Equals(cg.GlyphIndex, color, bgColor)) return;
					screenMemory[idx] = new ColorGlyph(cg.GlyphIndex, color, bgColor);
					if(firstChangedIdx > idx) firstChangedIdx = idx;
					if(lastChangedIdx < idx) lastChangedIdx = idx;
				}
			}
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		private void SendDataToWindow(){
			if(lastChangedIdx == -1) return;
			int count = lastChangedIdx - firstChangedIdx + 1;
			int[] sprites = new int[count];
			float[][] colors = new float[2][];
			colors[0] = new float[4 * count];
			colors[1] = new float[4 * count];
			for(int n=0;n<count;++n){
				ColorGlyph cg = screenMemory[firstChangedIdx + n];
				float[] fgRgba = cg.ForegroundColor.GetRGBA();
				float[] bgRgba = cg.BackgroundColor.GetRGBA();
				sprites[n] = cg.GlyphIndex;
				int idx4 = n * 4;
				colors[0][idx4] = fgRgba[0];
				colors[0][idx4 + 1] = fgRgba[1];
				colors[0][idx4 + 2] = fgRgba[2];
				colors[0][idx4 + 3] = fgRgba[3];
				colors[1][idx4] = bgRgba[0];
				colors[1][idx4 + 1] = bgRgba[1];
				colors[1][idx4 + 2] = bgRgba[2];
				colors[1][idx4 + 3] = bgRgba[3];
			}
			Window.UpdateOtherVertexArray(Window.TextSurface, sprites, colors, firstChangedIdx);
			ResetChangedIndices();
		}
		private void ResetChangedIndices(){
			firstChangedIdx = int.MaxValue;
			lastChangedIdx = -1;
		}
		public bool WindowUpdate() => Window.WindowUpdate();
		public void HoldUpdates(){
			holdUpdatesCount++;
		}
		public void ResumeUpdates(bool forceResume = false){
			if(forceResume) holdUpdatesCount = 0;
			else if(holdUpdatesCount > 0) holdUpdatesCount--;

			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public void UpdateCursor(bool blinkOn){
			Window.CursorSurface.Disabled = !blinkOn;
			if(blinkOn)
				Window.CursorSurface.SetOffsetInWorldUnits(cursorCol, cursorRow);
		}
		public bool CursorVisible{
			get => cursorVisible;
			set{
				if(cursorVisible == value) return;
				cursorVisible = value;
				UpdateCursor(value);
			}
		}
		public int CursorRow{
			get => cursorRow;
			set{
				if(cursorRow == value) return;
				cursorRow = value;
				if(cursorVisible)
					Window.CursorSurface.SetOffsetInWorldUnits(cursorCol, cursorRow);
			}
		}
		public int CursorCol{
			get => cursorCol;
			set{
				if(cursorCol == value) return;
				cursorCol = value;
				if(cursorVisible)
					Window.CursorSurface.SetOffsetInWorldUnits(cursorCol, cursorRow);
			}
		}
		public void SetCursorPosition(int row, int col){
			if(cursorRow == row && cursorCol == col) return;
			cursorRow = row;
			cursorCol = col;
			if(cursorVisible)
				Window.CursorSurface.SetOffsetInWorldUnits(cursorCol, cursorRow);
		}
		public void Clear(){
			screenMemory = new ColorGlyph[Rows*Cols];
			firstChangedIdx = 0;
			lastChangedIdx = Rows*Cols - 1;
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public void Clear(int startRow, int startCol, int height, int width){
			holdUpdatesCount++;
			string blankLine = "".PadRight(width);
			for(int n=0;n<height;++n){
				Write(startRow + n, startCol, blankLine, Color.Black);
			}
			holdUpdatesCount--;
			if(holdUpdatesCount == 0) SendDataToWindow();
		}
		public ColorGlyph[][] GetCurrentScreen(){
			ColorGlyph[][] result = new ColorGlyph[Rows][];
			int idx = 0;
			for(int i=0;i<Rows;++i){
				ColorGlyph[] line = new ColorGlyph[Cols];
				for(int j=0;j<Cols;++j){
					line[j] = screenMemory[idx];
					idx++;
				}
				result[i] = line;
			}
			return result;
		}
		public ColorGlyph[][] GetCurrent(int startRow, int startCol, int height, int width){
			if(startRow + height > Rows) throw new ArgumentException("Start row + height is larger than screen height");
			if(startCol + width > Cols) throw new ArgumentException("Start col + width is larger than screen width");
			ColorGlyph[][] result = new ColorGlyph[height][];
			for(int i=0;i<height;++i){
				ColorGlyph[] line = new ColorGlyph[width];
				int startIdx = startCol + (startRow + i)*Cols;
				for(int j=0;j<width;++j){
					line[j] = screenMemory[startIdx + j];
				}
				result[i] = line;
			}
			return result;
		}
		public void CleanUp(){
			//todo, is this call correct?
			Window?.Close();
		}
		public IList<OptionsScreen.OptionEditInfo> GetAdditionalDisplayOptions(){
			return new[] {
				OptionsScreen.GetEditInfo((BoolOptionType)BoolOptionTypeGL.HideMousePath, "Hide the path shown when moving the mouse over the map"),
				new OptionsScreen.OptionEditInfo{
					Text = "Fullscreen",
					GetValue = () => Window.FullScreen? "yes" : "no",
					EditValue = (row, col) => {
						Window.ToggleFullScreen();
						System.Threading.Thread.Sleep(10);
						Window.WindowUpdate();
					}
				},
				//todo, vsync on/off/adaptive here
			};
		}
		//todo - once the 'load options' method is added, it should set the 'input delay ms' option separately for each screen.

		public ColorGlyph GetHighlighted(ColorGlyph cg, HighlightType highlightType){
			//todo, does this need to check color options?
			if(highlightType == HighlightType.Targeting){
				float[] colorRgba = cg.ForegroundColor.GetRGBA();
				float[] highlightColorRgba = Color.TargetingHighlight.GetRGBA();
				for(int i=0;i<4;++i){
					if(colorRgba[i] != highlightColorRgba[i]) // If any part is different, return the regular highlight:
						return new ColorGlyph(cg.GlyphIndex, cg.ForegroundColor, Color.TargetingHighlight);
				}
				return new ColorGlyph(cg.GlyphIndex, Color.Black, Color.TargetingHighlight); // Otherwise, draw it in black.
			}
			else{
				return new ColorGlyph(cg.GlyphIndex, cg.ForegroundColor, Color.Blue);
			}
		}
	}
}
