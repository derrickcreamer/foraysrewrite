using System;
using GameComponents.TKWindow;

namespace ForaysUI.ScreenUI{
	public class GLScreen : IScreen{
		public ForaysWindow Window;

		public GLScreen(int rows, int cols){
			Rows = rows;
			Cols = cols;
			Window = ForaysWindow.Create(cols, rows);
		}
		public int Rows {get;set;}
		public int Cols {get;set;}
		public void UpdateCursor(bool makeVisible){
			//todo
		}

		public bool HoldUpdates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool Update() => Window.WindowUpdate();
		public bool CursorVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CursorLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CursorTop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public void CleanUp(){
			//todo, is this call correct?
			Window?.Close();
		}
		public void Clear(){
			float[][] colors = new float[2][];
			colors[0] = Color.Gray.GetRGBA();
			colors[1] = Color.Black.GetRGBA();
			Window.TextSurface.InitializeOtherDataForSingleLayout(Rows*Cols, 0, 32, colors);
		}

		public void SetCursorPosition(int left, int top){
			throw new NotImplementedException();
		}

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
	}
}
