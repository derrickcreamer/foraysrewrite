using System;

namespace ForaysUI {
	public static class Program {
		public static bool Linux;

		public static void Quit(){
			ScreenUI.ScreenUIMain.CleanUp();
			Environment.Exit(0);
		}
		static void Main(string[] args) {
			{
				int os = (int)Environment.OSVersion.Platform;
				Linux = (os == 4 || os == 6 || os == 128);
			}
			ScreenUI.ScreenUIMain.Run(args);
		}
		public static string GetEmbeddedResourceFilePath(string filename){
			return $"Forays.ForaysImages.{filename}"; //todo check
		}
	}
}
