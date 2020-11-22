using System;
using System.Collections.Generic;
using Forays;

namespace ForaysUI.ScreenUI{
	public static class GLColors{
		private static float[][] rgbaArrays;
		public static void Initialize(){
			 // Use an array for fast indexing. Only the colors through Transparent will have values here:
			rgbaArrays = new float[(int)(Color.Transparent + 1)][];
			rgbaArrays[(int)Color.Black] = new float[]{0.0f, 0.0f, 0.0f, 1.0f};
			rgbaArrays[(int)Color.White] = new float[]{1.0f, 1.0f, 1.0f, 1.0f};
			rgbaArrays[(int)Color.Gray] = GetFloatsFromBytes(211, 211, 211);
			rgbaArrays[(int)Color.Red] = new float[]{1.0f, 0.0f, 0.0f, 1.0f};
			rgbaArrays[(int)Color.Green] = new float[]{0.0f, 1.0f, 0.0f, 1.0f};
			rgbaArrays[(int)Color.Blue] = GetFloatsFromBytes(35, 35, 255);
			rgbaArrays[(int)Color.Yellow] = GetFloatsFromBytes(255, 248, 0);
			rgbaArrays[(int)Color.Magenta] = new float[]{1.0f, 0.0f, 1.0f, 1.0f};
			rgbaArrays[(int)Color.Cyan] = new float[]{0.0f, 1.0f, 1.0f, 1.0f};
			rgbaArrays[(int)Color.DarkGray] = GetFloatsFromBytes(105, 105, 105);
			rgbaArrays[(int)Color.DarkRed] = GetFloatsFromBytes(139, 0, 0);
			rgbaArrays[(int)Color.DarkGreen] = GetFloatsFromBytes(0, 100, 0);
			rgbaArrays[(int)Color.DarkBlue] = GetFloatsFromBytes(15, 15, 149);
			rgbaArrays[(int)Color.DarkYellow] = GetFloatsFromBytes(184, 134, 11);
			rgbaArrays[(int)Color.DarkMagenta] = GetFloatsFromBytes(139, 0, 139);
			rgbaArrays[(int)Color.DarkCyan] = GetFloatsFromBytes(0, 139, 139);
			rgbaArrays[(int)Color.Transparent] = new float[]{0.0f, 0.0f, 0.0f, 0.0f};

			rgbaArrays[(int)Color.DarkerGray] = GetFloatsFromBytes(50, 50, 50);
			rgbaArrays[(int)Color.DarkerRed] = GetFloatsFromBytes(80, 0, 0); //(DarkRed is 139 red)
			// todo, will the ones like ForestGreen be used by others?
			//   (it would probably be easy to let one color be a reference to another...just have the same array in 2 places)
			rgbaArrays[(int)Color.DarkerMagenta] = GetFloatsFromBytes(80, 0, 80); //(DarkMagenta is 139 red and blue)
			rgbaArrays[(int)Color.ForestGreen] = GetFloatsFromBytes(20, 145, 20);
			rgbaArrays[(int)Color.DarkForestGreen] = GetFloatsFromBytes(10, 80, 10);

			// UI colors reference other colors by default, but can be overridden:
			rgbaArrays[(int)Color.OutOfSight] = rgbaArrays[(int)Color.DarkBlue]; //todo check...how does this interact with the option?
			//todo, check this. TerrainDarkGray exists to make sure 'unseen' differs from 'dark gray coloration'. Rename?
			rgbaArrays[(int)Color.TerrainDarkGray] = rgbaArrays[(int)Color.DarkGray];
			rgbaArrays[(int)Color.HealthBar] = rgbaArrays[(int)Color.DarkerRed];
			//todo, make these settable in UI.
			rgbaArrays[(int)Color.StatusEffectBar] = rgbaArrays[(int)Color.DarkerMagenta];
			rgbaArrays[(int)Color.EnvironmentDescription] = rgbaArrays[(int)Color.ForestGreen];
			rgbaArrays[(int)Color.DarkEnvironmentDescription] = rgbaArrays[(int)Color.DarkForestGreen];
		}
		private static float[] GetFloatsFromBytes(int r, int g, int b, int a = 255){
			const float one255th = 1.0f / 255.0f;
			return new float[]{ r * one255th, g * one255th, b * one255th, a * one255th};
		}
		public static float[] GetRGBA(this Color color) => rgbaArrays[(int)color];
		public static Color ResolveColor(this Color color){
			switch(color){
				case Color.RandomFire:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.DarkRed, Color.Yellow);
				case Color.RandomIce:
					return ScreenUIMain.RNG.Choose(Color.White, Color.Cyan, Color.Blue, Color.DarkBlue);
				case Color.RandomLightning:
					return ScreenUIMain.RNG.Choose(Color.White, Color.Yellow, Color.Yellow, Color.DarkYellow);
				case Color.RandomBreached:
					if(ScreenUIMain.RNG.OneIn(4)) return Color.DarkGreen;
					else return Color.Green;
				case Color.RandomExplosion:
					if(ScreenUIMain.RNG.OneIn(4)) return Color.Red;
					else return Color.DarkRed;
				case Color.RandomGlowingFungus:
					if(ScreenUIMain.RNG.OneIn(35)) return Color.DarkCyan;
					else return Color.Cyan;
				case Color.RandomTorch:
					if(ScreenUIMain.RNG.OneIn(8))
						if(ScreenUIMain.RNG.CoinFlip()) return Color.White;
						else return Color.Red;
					else
						return Color.Yellow;
				case Color.RandomDoom:
					if(ScreenUIMain.RNG.OneIn(6))
						if(ScreenUIMain.RNG.CoinFlip()) return Color.DarkRed;
						else return Color.DarkGray;
					else
						return Color.DarkMagenta;
				case Color.RandomConfusion:
					if(ScreenUIMain.RNG.OneIn(16))
						return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.White);
					else
						return Color.Magenta;
				case Color.RandomDark:
					return ScreenUIMain.RNG.Choose(Color.DarkBlue, Color.DarkCyan, Color.DarkGray, Color.DarkGreen, Color.DarkMagenta, Color.DarkRed, Color.DarkYellow);
				case Color.RandomBright:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Magenta, Color.White, Color.Gray);
				case Color.RandomRGB:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue);
				case Color.RandomDRGB:
					return ScreenUIMain.RNG.Choose(Color.DarkRed, Color.DarkGreen, Color.DarkBlue);
				case Color.RandomRGBW:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue, Color.White);
				case Color.RandomCMY:
					return ScreenUIMain.RNG.Choose(Color.Cyan, Color.Magenta, Color.Yellow);
				case Color.RandomDCMY:
					return ScreenUIMain.RNG.Choose(Color.DarkCyan, Color.DarkMagenta, Color.DarkYellow);
				case Color.RandomCMYW:
					return ScreenUIMain.RNG.Choose(Color.Cyan, Color.Magenta, Color.Yellow, Color.White);
				case Color.RandomRainbow:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Magenta, Color.DarkBlue, Color.DarkCyan, Color.DarkGreen, Color.DarkMagenta, Color.DarkRed, Color.DarkYellow);
				case Color.RandomAny:
					return ScreenUIMain.RNG.Choose(Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Yellow, Color.Magenta, Color.DarkBlue, Color.DarkCyan, Color.DarkGreen, Color.DarkMagenta, Color.DarkRed, Color.DarkYellow, Color.White, Color.Gray, Color.DarkGray);
				/*todo, handle these: case Color.OutOfSight:
				if(Global.Option(OptionType.DARK_GRAY_UNSEEN)){
						if(Screen.GLMode){
								return Color.DarkerGray;
						}
						else{
								return Color.DarkGray;
						}
				}
				else{
						return Color.DarkBlue;
				}
				case Color.TerrainDarkGray:
				if(Screen.GLMode || !Global.Option(OptionType.DARK_GRAY_UNSEEN)){
						return Color.DarkGray;
				}
				else{
						return Color.Gray;
				}
				case Color.HealthBar:
				if(Screen.GLMode){
						return Color.DarkerRed;
				}
				else{
						return Color.DarkRed;
				}
				case Color.StatusEffectBar:
				if(Screen.GLMode){
						return Color.DarkerMagenta;
				}
				else{
						return Color.DarkMagenta;
				}
				case Color.EnvironmentDescription:
				if(Screen.GLMode){
						return Color.ForestGreen;
				}
				else{
						return Color.Green;
				}
				case Color.DarkEnvironmentDescription:
				if(Screen.GLMode){
						return Color.DarkForestGreen;
				}
				else{
						return Color.DarkGreen;
				}*/
				default: return color;
			}
		}
	}
}
