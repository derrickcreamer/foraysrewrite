using System;
using GameComponents.TKWindow;

namespace ForaysUI.ScreenUI{
    public class GLScreen : IScreen
    {
        public GLWindow Window;

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
            //todo
        }

        public void SetCursorPosition(int left, int top)
        {
            throw new NotImplementedException();
        }

        public void Write(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
        {
            throw new NotImplementedException();
        }

        public void Write(int row, int col, ColorGlyph cg)
        {
            throw new NotImplementedException();
        }
    }
}
