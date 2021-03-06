using System;
using System.Collections.Generic;

namespace ForaysUI.ScreenUI{
	public class ConsoleScreen : IScreen
	{
		//todo, track current colors here

		public ConsoleScreen(int rows, int cols){
			Rows = rows;
			Cols = cols;
		}
		public int Rows {get;set;}
		public int Cols {get;set;}
		public void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black){
			throw new NotImplementedException();
		}
		public void Write(int row, int col, ColorGlyph cg){
			throw new NotImplementedException();
		}
		public void Write(int row, int col, string str, Color color, Color bgColor = Color.Black){
			throw new NotImplementedException();
		}
		public void Write(int startRow, int startCol, ColorGlyph[][] colorGlyphs){
			throw new NotImplementedException();
		}
		public bool WindowUpdate() => true;
		public void HoldUpdates(){

		}
		public void ResumeUpdates(bool forceResume = false){

		}
		public bool CursorVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CursorRow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CursorCol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public void SetCursorPosition(int row, int col){
			throw new NotImplementedException();
		}
		public void Clear(){
			CursorVisible = false; //todo
			/*for(int i=0;i<Global.SCREEN_H;++i){
					WriteString(i,0,"".PadRight(Global.SCREEN_W));
					for(int j=0;j<Global.SCREEN_W;++j){
							memory[i,j].c = ' ';
							memory[i,j].color = Color.Black;
							memory[i,j].bgcolor = Color.Black;
					}
			}*/
		}
		public void Clear(int startRow, int startCol, int height, int width){
			//todo
		}
		public ColorGlyph[][] GetCurrentScreen(){
			throw new NotImplementedException();
		}
		public ColorGlyph[][] GetCurrent(int startRow, int startCol, int height, int width){
			throw new NotImplementedException();
		}
		public void CleanUp(){
			if(Program.Linux){
				Clear();
				ResetColors();
				SetCursorPosition(0,0);
				CursorVisible = true;
			}
		}
		public void ResetColors(){
			//todo
		}
		public IList<OptionsScreen.OptionEditInfo> GetAdditionalDisplayOptions() => null;

		public ColorGlyph GetHighlighted(ColorGlyph cg, HighlightType highlightType)
		{
			throw new NotImplementedException();
		}
	}
}
