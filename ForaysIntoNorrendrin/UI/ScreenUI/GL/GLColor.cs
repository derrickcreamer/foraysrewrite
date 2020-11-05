using System;

namespace ForaysUI.ScreenUI{
	public static class ExtensionsForColor{
		public static float[] GetRGBA(this Color color){
			switch(color){
				case Color.Gray: return new float[]{0.5f, 0.5f, 0.5f, 1.0f};
				case Color.Black: return new float[]{0.0f, 0.0f, 0.0f, 1.0f};
				case Color.White: return new float[]{1.0f, 1.0f, 1.0f, 1.0f};
				case Color.Cyan: return new float[]{0.0f, 1.0f, 1.0f, 1.0f};
				case Color.Todo: return new float[]{6.0f, 7.0f, 1.0f, 1.0f};
			}
			return null;//todo
		}
	}
}
