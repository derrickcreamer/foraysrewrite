using System;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;
using System.IO;
using Forays;
using GameComponents;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	public static class ScreenUIMain{
		public const int Rows = 28;
		public const int Cols = 88;

		public static bool GLMode;
		public static RNG RNG;
		public static Grammar Grammar;

		public static void Run(string[] args){
			//todo, set Program.SavePath from args here
			//todo check ifdef and args to see whether this is GL mode
			GLMode = true;

			RNG = new RNG((ulong)DateTime.Now.Ticks);

			if(GLMode){
				StaticScreen.Screen = new GLScreen(Rows, Cols);
				StaticInput.Input = new GLInput();
			}
			else{
				StaticScreen.Screen = new ConsoleScreen(Rows, Cols);
				StaticInput.Input = new ConsoleInput();
			}
			MapRendering.MapRenderer.SetFactoryMethod(ui => new MapRendering.BasicMapRenderer(ui));

			Option.Initialize(); //todo, load options here
			InitializeGrammar();
			Input.LoadKeyBindings();
			ShowTitleScreen();
			ShowMainMenu();
		}
		private static void InitializeGrammar(){
			Grammar = new Grammar();
			Names.Initialize(Grammar);
			Grammar.AddVerb("do", "does");
			Grammar.AddVerb("are", "is");
			Grammar.AddVerb("wake up", "wakes up");
			Grammar.AddVerb("feel", "looks"); // To automatically handle "you feel stronger" vs. "the foo looks stronger"
		}
		private static void ShowTitleScreen(){
			if(GLMode){
			}
			else{
			}
		}
		private static void ShowMainMenu(){
			const string version = "0.8.5"; //todo, move?
			const string header = "Forays into Norrendrin " + version;
			const string divider = "----------------------------"; // divider length should match header
			const int headerLength = 28;
			while(true){
				Screen.HoldUpdates();
				Screen.Clear();
				const int row = 8;
				const int col = (Cols - headerLength) / 2;
				Screen.Write(row+1, col, header, Color.Yellow);
				Screen.Write(row+2, col, divider, Color.Green);
				bool savedGame = File.Exists("forays.sav");
				string startOrResume = savedGame? "[a] Resume saved game" : "[a] Start a new game";
				Screen.Write(row+4, col+4, startOrResume);
				Screen.Write(row+5, col+4, "[b] How to play");
				Screen.Write(row+6, col+4, "[c] High scores"); //todo, replays?
				Screen.Write(row+7, col+4, "[d] Quit");
				//todo, mouse UI push button map/layer/whatever here.
				// ... I think all mouseover behavior needs a common element this time.
				// Highlights should be able to grab screen memory, change colors or draw over it, and then
				//   restore it once the 'remove highlight' method is called.
				for(int i=0;i<4;++i){
					Screen.Write(i + row+4, col+5, i + 'a', Color.Cyan);
					//todo mouse UI button
				}
				Screen.ResumeUpdates();
				//Screen.SetCursorPosition(10, 10); //todo...was this originally a workaround?
					// test what happens if the cursor is on the final row+col when asking for input.
				ConsoleKeyInfo command = Input.ReadKey(false);
				while(command.KeyChar != 'a' && command.KeyChar != 'b' && command.KeyChar != 'c' && command.KeyChar != 'd'){
					command = Input.ReadKey(false);
				}
				//todo pop mouse UI panel
				switch(command.KeyChar){ //todo, any use for a 'get one of THESE keys' method?
				case 'a':
					//todo, load options where?
					//todo, set screen NoClose to true where?
					//todo, push mouse UI layer in Map mode where? and pop it where?
					//todo, create mouse UI stats buttons where?
					if(savedGame){
						ResumeSavedGame();
					}
					else{
						StartNewGame();
					}
					break;
				case 'b':
					//todo ShowHelp() or Help.DisplayHelp();
					break;
				case 'c':
					ShowHighScores();
					break;
				case 'd':
					Program.Quit();
					break;
				}
			}
		}
		private static void StartNewGame(){
			//todo, player name...check files, etc.
			GameUniverse g = new GameUniverse();
			g.InitializeNewGame(); //todo seed
			RunGame(g);
		}
		private static void ResumeSavedGame(){
			GameUniverse g = new GameUniverse();
			//todo: load from file here
			RunGame(g);
		}
		private static void RunGame(GameUniverse g){
			GameRunUI gameUI = new GameRunUI(g);
			g.Q.BeforeEventExecute = gameUI.GameEventHandler.BeforeGameEvent;
			g.Q.AfterEventExecute = gameUI.GameEventHandler.AfterGameEvent;
			(g.Player.CancelDecider as PlayerCancelDecider).DecideCancel = gameUI.GameEventHandler.DecideCancel;
			g.CreatureRules.OnStatusStart = gameUI.GameEventHandler.OnStatusStart;
			g.CreatureRules.OnStatusEnd = gameUI.GameEventHandler.OnStatusEnd;
			//todo, try/catch? do I want a thing where I can get to the exceptions before they reach this point?
			g.Run();
			if(g.GameOver){
				//todo
			}
			else if(g.Suspend){
				//todo
			}
		}
		private static void ShowGameOverScreen(){
			//todo...needs GameUniverse here, right?
		}
		private static void ShowHighScores(){
			//todo
		}
		public static void CleanUp(){
			Screen?.CleanUp();
		}
	}
}
