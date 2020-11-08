using System;

namespace ForaysUI.ScreenUI{
	public class GLScreen : IScreen{
		public ForaysWindow Window;
		private bool cursorVisible;
		private int cursorRow, cursorCol;
		private bool holdUpdates;
		private int firstChangedIdx, lastChangedIdx;
		private ColorGlyph[] screenMemory;

		public GLScreen(int rows, int cols){
			Rows = rows;
			Cols = cols;
			firstChangedIdx = lastChangedIdx = -1;
			screenMemory = new ColorGlyph[Rows*Cols];
			Window = ForaysWindow.Create(cols, rows);
		}
		public int Rows {get;set;}
		public int Cols {get;set;}
		public void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black){
			float[][] colors = new float[2][];
			colors[0] = color.GetRGBA();
			colors[1] = bgColor.GetRGBA();
			int idx = col + row*Cols;
			Window.UpdateOtherSingleVertex(Window.TextSurface, idx, glyphIndex, colors);
		}
		public void Write(int row, int col, ColorGlyph cg){
			float[][] colors = new float[2][];
			colors[0] = cg.ForegroundColor.GetRGBA();
			colors[1] = cg.BackgroundColor.GetRGBA();
			int idx = col + row*Cols;
			Window.UpdateOtherSingleVertex(Window.TextSurface, idx, cg.GlyphIndex, colors);
		}
		public void Write(int row, int col, string str, Color color = Color.Gray, Color bgColor = Color.Black){
			int count = str.Length;
			int[] sprites = new int[count];
			float[][] colors = new float[2][];
			colors[0] = new float[4 * count];
			colors[1] = new float[4 * count];
			float[] fgRgba = color.GetRGBA();
			float[] bgRgba = bgColor.GetRGBA();
			for(int n=0;n<count;++n){
				sprites[n] = (int)str[n];
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
			int startIdx = col + row*Cols;
			Window.UpdateOtherVertexArray(Window.TextSurface, sprites, colors, startIdx);
		}
		public bool WindowUpdate() => Window.WindowUpdate();
		public void HoldUpdates(){
			holdUpdates = true;
		}
		public void ResumeUpdates(){
			holdUpdates = false;
			//todo
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
			float[][] colors = new float[2][];
			colors[0] = Color.Gray.GetRGBA();
			colors[1] = Color.Black.GetRGBA();
			Window.TextSurface.InitializeOtherDataForSingleLayout(Rows*Cols, 0, 32, colors);
		}
		public void CleanUp(){
			//todo, is this call correct?
			Window?.Close();
		}
	}
}
