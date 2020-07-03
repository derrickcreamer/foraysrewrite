using System;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;
using System.IO;
using Forays;
using GameComponents;

namespace ForaysUI.ScreenUI{
    public static class ScreenUI{
        const int Rows = 28;
        const int Cols = 88;

        public static bool GLMode;
        public static RNG RNG;

        public static void Run(string[] args){
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

            //todo... this is where Nym registers the translation of "you feel stronger" to "the foo looks stronger"
            //todo... load key rebindings
            ShowTitleScreen();
            ShowMainMenu();
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
                //Screen.HoldUpdates = true; //todo... how do I tell this to resume properly? see SSC.
                Screen.Clear();
                const int row = 8;
                const int col = (Cols - headerLength) / 2;
                Screen.Write(row+1, col, header, Color.Todo);
                Screen.Write(row+2, col, divider, Color.Todo);
                bool savedGame = File.Exists("forays.sav");
                string startOrResume = savedGame? "[a] Start a new game" : "[a] Resume saved game";
                Screen.Write(row+4, col+4, startOrResume);
                Screen.Write(row+5, col+4, "[b] How to play");
                Screen.Write(row+6, col+4, "[c] High scores"); //todo, replays?
                Screen.Write(row+7, col+4, "[d] Quit");
                //todo, mouse UI push button map/layer/whatever here.
                for(int i=0;i<4;++i){
                    Screen.Write(i + row+4, col+5, i + 'a', Color.Cyan);
                    //todo mouse UI button
                }
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
            g.OnNotify += new NotificationHandler(g).ReceiveNotification;
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
