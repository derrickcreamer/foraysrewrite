using System;
using static ForaysUI.ScreenUI.StaticScreen;
using static ForaysUI.ScreenUI.StaticInput;
using System.IO;

namespace ForaysUI.ScreenUI{
    public class ScreenUI : IForaysUI{
        const int Rows = 28;
        const int Cols = 88;

        public bool GLMode;
        public void Run(string[] args){
            //todo check ifdef and args to see whether this is GL mode
            GLMode = true;

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
        private void ShowTitleScreen(){
            if(GLMode){
            }
            else{
            }
        }
        private void ShowMainMenu(){
            const string version = "0.8.5"; //todo, move?
            const string header = "Forays into Norrendrin " + version;
            const string divider = "----------------------------"; // divider length should match header
            const int headerLength = 28;
            //todo, mouse UI push button map/layer/whatever here.
            while(true){
                Screen.HoldUpdates = true; //todo... how do I tell this to resume properly? see SSC.
                Screen.Clear();
                const int row = 8;
                const int col = (Cols - headerLength) / 2;
                Screen.Write(row+1, col, header, Color.Todo);
                Screen.Write(row+2, col, divider, Color.Todo);
                bool savedGame = File.Exists("forays.sav");
                string startOrResume = savedGame? "[a] Start a new game" : "[a] Resume saved game";
                Screen.Write(row+4, col+4, startOrResume);
                Screen.Write(row+5, col+4, "[b] How to play");
                Screen.Write(row+6, col+4, "[c] High scores");
                Screen.Write(row+7, col+4, "[d] Quit");
                //create mouse buttons todo
                //Screen.SetCursorPosition(todo);
                ConsoleKeyInfo command = Input.ReadKey(false);
            }
        }
        //main menu will eventually run the game...
        // which will need to figure out how to hook into the notifications,
        //   and to read input etc.
        //   I guess input probably gets a similar thing to Screen?
        //   (which isn't the same as the game vs. playback split,
        //         because the playback UI probably uses that Input static class too...)
        private void ShowGameOverScreen(){
            //todo...needs GameUniverse here, right?
        }
        private void ShowHighScores(){
            //todo
        }
        public void Quit(){
            Screen?.CleanUp();
        }
    }
}
