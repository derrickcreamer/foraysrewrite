using System;
using System.IO;
using System.Reflection;

namespace ForaysUI {
	public static class Program {
		public static bool Linux;
		public static string SavePath;

		public static void Quit(){
			ScreenUI.ScreenUIMain.CleanUp();
			Environment.Exit(0);
		}
		static void Main(string[] args) {
			SavePath = ".";

			{
				int os = (int)Environment.OSVersion.Platform;
				Linux = (os == 4 || os == 6 || os == 128);
			}

			ScreenUI.ScreenUIMain.Run(args);
		}
		public static byte[] GetEmbeddedFileBytes(string filePath){
			using(Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath))
			using(MemoryStream ms = new MemoryStream()){
				s.CopyTo(ms);
				return ms.ToArray();
			}
		}
		public static Stream GetEmbeddedFileStream(string filePath)
			=> Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
		public static string GetSavePathForFile(string filename)
			=> Path.Combine(SavePath, filename);
		public static string GetEmbeddedResourceFilePath(string filename){
			return $"Forays.ForaysImages.{filename}"; //todo check
		}
	}
}
