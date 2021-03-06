using System;
using System.Collections.Generic;
using GameComponents.TKWindow;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ForaysUI.ScreenUI{
	public class ForaysWindow : GLWindow{
		public Surface TextSurface;
		public Surface CursorSurface;

		public ForaysWindow(int w, int h, string title) : base(w, h, title){}

		public static ForaysWindow Create(int cols, int rows){
			ToolkitOptions.Default.EnableHighResolution = false; //todo - actually test this line, since it might not apply any more.
			ForaysWindow w = new ForaysWindow(DisplayDevice.Default.Width, DisplayDevice.Default.Height, "Forays into Norrendrin");
			w.VSync = VSyncMode.Off;
			//todo icon
			//todo, create a RNG for the UI and use it here:
			w.TimerFramesOffset = -8888888;
			w.SetWorldUnitsPerScreen(cols, rows);

			w.TextSurface = Surface.Create(w, @"Forays.UI.ScreenUI.GL.vga9_msdf.png",
				TextureMinFilter.Nearest, TextureMagFilter.Linear, TextureLoadSource.FromEmbedded, null,
				ShaderCollection.GetMsdfFS(2048, 4), false, 2, 4, 4);
			w.TextSurface.texture.Sprite.Add(GetIbmFontSpriteType());
			CellLayout.CreateGrid(w.TextSurface, cols, rows);
			w.TextSurface.InitializePositions(cols*rows);
			w.TextSurface.InitializeOtherDataForSingleLayout(cols*rows, 0, 32, Color.Black.GetRGBA(), Color.Black.GetRGBA());

			w.CursorSurface = Surface.Create(w, @"Forays.UI.ScreenUI.GL.vga9_msdf.png",
				TextureMinFilter.Nearest, TextureMagFilter.Linear, TextureLoadSource.FromEmbedded, null,
				ShaderCollection.GetMsdfFS(2048, 4), false, 2, 4, 4); //todo could be a bit off, but probably works
			w.CursorSurface.texture.Sprite = w.TextSurface.texture.Sprite; // Share the sprite definition as well as the texture index
			CellLayout.CreateGrid(w.CursorSurface, 1, 1, 0.8f, 0.125f, 0.0f, 0.75f); //todo, tweak later
			w.CursorSurface.InitializePositions(1);
			w.CursorSurface.InitializeOtherDataForSingleLayout(1, 0, 0, Color.Gray.GetRGBA(), Color.Black.GetRGBA());
			w.CursorSurface.Disabled = true; //todo check

			GL.Enable(EnableCap.Blend); //todo verify needed
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //todo verify needed

			w.WindowSizeRules = new ResizeRules{ MinWidth = 800, MinHeight = 600 };//todo, ratio? //todo make configurable?
			w.ViewportSizeRules = null; //todo add viewport rules
			w.NoShrinkToFit = false;

			//todo, closing handler?

			w.Visible = true;
			return w;
		}
		private static SpriteType GetIbmFontSpriteType(){
			const float px_width = 1.0f / (float)2048;
			const float px_height = 1.0f / (float)2048;
			const float texcoord_width = (float)32 * px_width;
			const float texcoord_height = (float)54 * px_height;
			SpriteType s = new SpriteType();
			s.X = idx => idx * texcoord_width + (0.5f * px_width);
			s.Y = idx => (idx / 64) * texcoord_height + (0.5f * px_height);
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			s.CalculateThroughIndex(1000); //todo, count and reduce to the correct number
			return s;
		}
	}
}
