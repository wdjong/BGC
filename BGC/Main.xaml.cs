// To do list:
// Make sure basic learning works with temperaments
// Prepare for fine tuning: make co-efficients properties of player
// Set player zero with Normal values
// Vary player 2 co-efficient for a parameter, measure response, use feedback
// Find the gradient, use it to adjust the co-efficient, get a score, repeat 
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BGC
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {

        // Database / File IO info
        const string FileName = @"SavedBGGame.bin"; // save state of game
        const string GameHistFile = @"GameHist.txt"; // history of winning and losing temperament
        const string GameDumpFile = @"GameDump"; // dice throws and current board position

        // Game & Move info
        private BGGame aBGGame = new BGGame();
        private BGRules aBGRules = new BGRules();
        private bool endGame = false; // to do with an individual game
        private bool inThread = false; // to do with a series of computer v computer games
        private bool exitThread = false; // set to true to escape c vs c games
        private int mTurns;
        private int C0Wins; // count of win
        private int numMoves; // old D number of Dice moves 2 or 4 after throw
        private bool checkerHit = false;
        private int cVsCgameNumber; // Setting: when a computer is playing another computer you can set it to do it lots of times
        private bool cVsCshowMoves; // Setting: // Show move outcome
        private bool cVsCshowGames; // Setting: // Show game outcome
        private int cVsCSecPause = 0; // 3 second pause

        // Board info
        private BGBoard2 aBGBoard = new BGBoard2(); // This is the board window used for visually representing the board
        private int[] p0Board = new int[26]; // see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819
        private int[] p1Board = new int[26]; // each player has an array of positions and moves from high points to low (home)
        private int[] p0Board1 = new int[26];
        private int[] p1Board1 = new int[26]; // Backup

        // Player info initialised in initPlayer routines
        private int player; // designates the player whose turn it is
        private int opponent; // the person whose turn it is not
        private List<BGPlayer> players = new List<BGPlayer> { new BGPlayer(), new BGPlayer() }; // a list of players
        private int useProgrammedTemp0; //Setting: // 0 random, 1 running, 2 make points 1, 3 blitz, 4 not blot, 5 adjacent, 6 bear in, 7 norm
        private int useProgrammedTemp1; //Setting: // 0 random, 1 running, 2 make points 1, 3 blitz, 4 not blot, 5 adjacent, 6 bear in, 7 norm 
        private bool allowSameTemp = false; // 
        List<int> aMovePlay1 = new List<int>() { };
        private bool useProgrammedMove1 = false; // *** use preprogrammed moves
        List<int> aMoveRec1 = new List<int>(); // record player 1 moves
        private int m1Next = 0; // move player 1 index

        // Dice throw info initialised in initDice 
        private int di1;
        private int di2;
        private bool useProgrammedDice = false; //  *** also set temp above
        List<int> aDiceList = new List<int>() { };
        private int dlNext = 0; // diceList index
        private Random aDice = new Random();

        // General purpose
        private string vbCrLf = Environment.NewLine;

        // Constructor
        public Main()
        {
            InitializeComponent();
            cVsCgameNumber = Properties.Settings.Default.CVsCgameNumber;
            cVsCSecPause = Properties.Settings.Default.CVsCSecPause;
            cVsCshowMoves = Properties.Settings.Default.CVsCshowMoves;
            cVsCshowGames = Properties.Settings.Default.CVsCshowGames;
            useProgrammedTemp0 = Properties.Settings.Default.UseProgrammedTemp0;
            useProgrammedTemp1 = Properties.Settings.Default.UseProgrammedTemp1;
        }

        // Form Control methods
        private void BtnDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string dumpText = "";
                string diceList = "";
                foreach (var d in aDiceList)
                {
                    diceList += (diceList == "") ? "" + d.ToString() : ", " + d.ToString();
                }
                dumpText += "DiceList: " + diceList + Environment.NewLine;

                dumpText += "Temper: " + players[0].Temper.ToString(); // 0 random, 1 running, 2 make points 1, 3 blitz, 4 not blot, 5 adjacent, 6 bear in, 7 norm
                dumpText += ", " + players[1].Temper.ToString() + Environment.NewLine;

                string board0 = "";
                foreach (int p in p0Board)
                {
                    board0 += (board0 == "") ? "" + p.ToString() : ", " + p.ToString();
                }
                dumpText += "Board0: " + board0 + Environment.NewLine;

                string board1 = "";
                foreach (int p in p1Board)
                {
                    board1 += (board1 == "") ? "" + p.ToString() : ", " + p.ToString();
                }
                dumpText += "Board1: " + board1 + Environment.NewLine;
                dumpText += txtNotation.Text;

                string timeStamp = DateTime.Now.ToString("yyyymmddhhmm");
                File.WriteAllText(AppPath + GameDumpFile + timeStamp + ".txt", dumpText);
                MessageBox.Show(GameDumpFile + timeStamp + ".txt" + " was created in " + AppPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            // Here we're reading in a game...
            // It seems to assume a C vs H game at the moment see below
            try
            {
                if (File.Exists(FileName))
                {
                    Stream aStream = File.OpenRead(FileName);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    aBGGame = (BGGame)deserializer.Deserialize(aStream);
                    aStream.Close();
                }
                else
                {
                    throw new System.Exception(FileName + " wasn't found.");
                }
                txtNotation.Text = aBGGame.Notation;
                endGame = aBGGame.EndGame;
                player = aBGGame.Player;
                di1 = aBGGame.DiceHi;
                di2 = aBGGame.DiceLo;
                numMoves = aBGGame.NumMoves;
                mTurns = aBGGame.mTurns;
                aBGGame.GetPlayers(players);
                aBGGame.GetP0Board(p0Board);
                aBGGame.GetP1Board(p1Board);
                aBGBoard.diceHi = di1;
                aBGBoard.diceLo = di2;
                aBGBoard.SetBlackBoard(p0Board);
                aBGBoard.SetRedBoard(p1Board);
                aBGBoard.Show();
                aBGBoard.DrawBoard();
                if (endGame == false)
                {
                    HaveAGoC0H1(false);
                }
                else
                {
                    Cout(FinishGame() + " won.");
                    DisplayBoard(); // HumanPrepV0H1(); // display incl. dice in prep for Human go
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnPlayC0H1_Click(object sender, RoutedEventArgs e)
        {
            // Human player 1 C# player 0
            txtMessage.Text = "";
            txtNotation.Text = ""; // Clear Cout window
            Cout("B A C K G A M M O N" + vbCrLf);
            InitialiseC0H1();
            Cout(players[0].Name + "; Black, 0, C#, From bottom (" + players[0].TemperStr() + ") vs ");
            Cout(players[1].Name + "; Red, 1, Human, From top" + Environment.NewLine);

            DisplayBoard();
            aBGBoard.Show();

            string playerStr = players[player].Name.Substring(0, 2) + ": ";
            Cout(playerStr);
            Cout(di1.ToString() + "-" + di2.ToString() + ": ");
            HaveAGoC0H1(false);
        }

        private void BtnPlayC0C1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!inThread && !exitThread)
                {
                    Thread thread = new Thread(PlayGamesC0C1); // this routine runs in a separate thread so board moves can be shown
                    thread.IsBackground = true; //<-- Set the thread to work in background https://stackoverflow.com/questions/2688923/how-to-exit-all-running-threads
                    thread.SetApartmentState(ApartmentState.STA); // https://stackoverflow.com/questions/4154429/apartmentstate-for-dummies
                    DisplayBoard();
                    aBGBoard.Show();
                    thread.Start(); // play games in another thread and then ask this one to update the screen sometimes
                }
                else
                {
                    exitThread = true; // stop c vs c play // set to false on exit of PlayGamesC0C1
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("btnPlayC0C1_Click: " + ex.Message);
            }
        }

        private void BtnPlayH0H1_Click(object sender, RoutedEventArgs e)
        {
            // Human player 0 Human player 1
            txtMessage.Text = "";
            txtNotation.Text = ""; // Clear Cout window
            Cout("B A C K G A M M O N" + vbCrLf);
            InitialiseH0H1(); // initialise the data objects: board, dice, players
            Cout(players[0].Name + "; Black, 0, Human, BotToTop vs ");
            Cout(players[1].Name + "; Red, 1, Human, TopToBot" + Environment.NewLine);

            DisplayBoard();
            aBGBoard.Show();

            string playerStr = players[player].Name.Substring(0, 2) + ": ";
            Cout(playerStr);
            Cout(di1.ToString() + "-" + di2.ToString() + ": ");
            HaveAGoH0H1(false);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveGame(FileName);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            clsSettings dlg = new clsSettings();
            dlg.Owner = this; //https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/dialog-boxes-overview
            dlg.ShowDialog();

            // Process data entered by user if dialog box is accepted
            if (dlg.DialogResult == true)
            {
                // Update fonts
                try
                {
                    Properties.Settings.Default.CVsCgameNumber = int.Parse(dlg.txtAutoGames.Text);
                    Properties.Settings.Default.CVsCSecPause = int.Parse(dlg.txtMovePause.Text);
                    Properties.Settings.Default.CVsCshowMoves = bool.Parse(dlg.txtShowMoves.Text);
                    Properties.Settings.Default.CVsCshowGames = bool.Parse(dlg.txtShowGames.Text);
                    Properties.Settings.Default.UseProgrammedTemp0 = int.Parse(dlg.txtTemp1.Text);
                    Properties.Settings.Default.UseProgrammedTemp1 = int.Parse(dlg.txtTemp2.Text);
                    Properties.Settings.Default.Save();
                    cVsCgameNumber = Properties.Settings.Default.CVsCgameNumber;
                    cVsCSecPause = Properties.Settings.Default.CVsCSecPause;
                    cVsCshowMoves = Properties.Settings.Default.CVsCshowMoves;
                    cVsCshowGames = Properties.Settings.Default.CVsCshowGames;
                    useProgrammedTemp0 = Properties.Settings.Default.UseProgrammedTemp0;
                    useProgrammedTemp1 = Properties.Settings.Default.UseProgrammedTemp1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // We should tell the player to learn at this point. The memory should be a property of the player
            try
            {
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SqlCeConnection conn = new SqlCeConnection(@"Data Source=" + AppPath + "Database1.sdf");
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE BGMemories SET Score = 1"; // Can't be 0 or the InitTemper thing breaks.
                int numberOfRecords = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Diagnostics.Process.Start(AppPath + "BGHelp.htm");
        }


        // Event handler (see BGBoard2 event handler MoveMad

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add an event to the board object that is raised at the end of the human's go
            aBGBoard.Owner = this; // this is effectively saying we want to subscribe to the event notifications
            aBGBoard.MoveMade += new EventHandler(BGBoard_MoveMade); //register the handler for the move made event
            aBGBoard.Left = this.Left + this.ActualWidth;
            aBGBoard.Top = this.Top;

        }

        void BGBoard_MoveMade(object sender, EventArgs e)
        {
            // After a human has moved a piece this event is raised.
            // handle event (defined in Window_Loaded for event raised by aBGBoard2 object)
            if (players[opponent].IWin || players[player].IWin)
            {
                endGame = true;
            }
            aBGBoard = (BGBoard2)sender;
            aBGBoard.GetRedBoard(p1Board); // populate p1Board (red) array from the board object
            aBGBoard.GetBlackBoard(p0Board); // populate p0Board (black) array from the board object
            if (useProgrammedMove1 == true) // i.e.for debugging: regardless of what was done on the board use the programmed move
            {
                aBGBoard.fMove1 = aMovePlay1[m1Next++];
                aBGBoard.tMove1 = aBGBoard.fMove1 - aMovePlay1[m1Next++];
            }
            if (endGame == false)
            {
                if (players[opponent].Human == true)
                {
                    HaveAGoH0H1(true); // it's either a human vs human game
                }
                else
                {
                    HaveAGoC0H1(true); // or a human vs computer game
                }
            }
            else
            {
                Cout(FinishGame() + " won.");
                DisplayBoard();
            }
        }

        // Methods order of appearance / structure / logic

        // Pre-game
        private void InitialiseC0H1()
        {
            // Human player 1 vs C# player 0
            endGame = false; // let the game begin
            numMoves = 0;
            InitBoard();
            InitDice();
            InitPlayersC0H1(); // throws dice and decides who'll go first
        }

        private void InitialiseC0C1()
        {
            // for c# v c# 
            endGame = false; // 'False means we're in a game
            numMoves = 0;
            InitBoard();
            InitDice();
            InitPlayersC0C1(); // throws dice and decides who'll go first
        }

        private void InitialiseH0H1()
        {
            // Human player 0 vs Human player 1
            endGame = false; // let the game begin
            numMoves = 0;
            InitBoard();
            InitDice();
            InitPlayersH0H1(); // throws dice and decides who'll go first
        }

        private void InitBoard()
        {
            // The visual representation of the board (with pieces and dice) is done in BGBoard2.xaml / cs
            int[] init0Board = { 0, 0, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0 }; // computer: see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819
            int[] init1Board = { 0, 0, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0 }; // each player has an array of positions and moves from high points to low (home)
            //int[] init0Board = { 0, 1, 1, 0, 0, 2, 3, 0, 0, 0, 0, 0, 0, 4, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 1 }; // test computer on bar
            //int[] init1Board = { 0, 0, 0, 2, 1, 0, 2, 0, 3, 0, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1 }; // 
            //int[] init0Board = { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // test computer bearing off
            //int[] init1Board = { 0, 1, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 

            Array.Copy(init1Board, p1Board, 26); // each player has an array of positions and moves from high points to low (home)
            Array.Copy(init0Board, p0Board, 26); //
            Array.Copy(init1Board, p1Board1, 26);
            Array.Copy(init0Board, p0Board1, 26);
        }

        private void InitDice()
        {
            if (useProgrammedDice == false)
            {
                aDiceList.Clear();
            }
            dlNext = 0;
        }

        private void InitPlayersC0H1()
        {
            // C# player 0
            players[0].Reset();
            players[0].PlayerZero = true;
            players[0].Human = false; //Computer
            //players[0].CompVB = false; //C#
            players[0].Name = "Corman0";
            //players[0].GameOver = false;
            if (useProgrammedTemp0 == 0)
            {
                players[0].InitTemper();
            }
            else
            {
                players[0].Temper = useProgrammedTemp0;
            }
            // Human player 1
            players[1].Reset();
            players[1].PlayerZero = false;
            players[1].Human = true; //not Computer
            players[1].Name = "Halter1";
            //players[1].GameOver = false;
            aMoveRec1.Clear();

            // Determine who goes first
            ThrowDice();
            player = (di1 > di2) ? 0 : 1; //aDice.Next(2); // '0 Norman or 1 Walter
            opponent = System.Math.Abs(player - 1); //'not player
        }

        private void InitPlayersC0C1()
        {
            // C player 0
            players[0].Reset();
            players[0].PlayerZero = true;
            players[0].Human = false; //Computer
            players[0].Name = "Corman0";
            players[0].GameOver = false;
            if (useProgrammedTemp0 == 0)
            {
                players[0].InitTemper();
            }
            else
            {
                players[0].Temper = useProgrammedTemp0;
                players[0].SpeedFactor = 4.00; // Normally 5.00 -- Maybe 4.00
                players[0].SafetyFactor = 1.65; // Normally 0.75 -- Maybe 1.625
                players[0].AgroFactor = .25; // Normally 0.50 -- Maybe 0.25
                players[0].RiskFactor = 5.000; // Normally 4.0 -- Maybe 5.00
                players[0].ClumpFactor = -.5; // Normally .125 -- Maybe -.5
                players[0].HomeFactor = 2; // Normally 1 -- Maybe 2
                players[0].ChangeMultiplier = 1; // Normally 5 
            }
            // C# player 1
            players[1].Reset();
            players[1].PlayerZero = false;
            players[1].Human = false; //Computer
            //players[1].CompVB = false; //C#
            players[1].Name = "Calter1";
            players[1].GameOver = false;
            if (useProgrammedTemp1 == 0)
            {
                players[1].InitTemper();
                //If InitTemper fail to talk to db it returns a set value
                int tries = 0;
                while (players[1].Temper == players[0].Temper && allowSameTemp == false)
                {
                    tries++;
                    if (tries > 10)
                    {
                        players[1].Temper = 6;
                        break;
                    }
                    else
                    {
                        players[1].InitTemper();
                    }
                }
            }
            else
            {
                players[1].Temper = useProgrammedTemp1;
            }

            // Determine who goes first
            ThrowDice();
            player = (di1 > di2) ? 0 : 1; //aDice.Next(2); // '0 Vorman or 1 Caltec
            opponent = System.Math.Abs(player - 1); //'i.e. not the player
        }

        private void InitPlayersH0H1()
        {
            // Human player 0
            players[0].PlayerZero = true;
            players[0].Human = true; //Computer
            players[0].Name = "Horman0";
            players[0].GameOver = false;
            // Human player 1
            players[1].PlayerZero = false;
            players[1].Human = true; //not Computer
            players[1].Name = "Halter1";
            players[1].GameOver = false;
            aMoveRec1.Clear();

            // Determine who goes first
            ThrowDice();
            player = (di1 > di2) ? 0 : 1; //aDice.Next(2); // '0 Norman or 1 Walter
            opponent = System.Math.Abs(player - 1); //'not player
        }

        // Have turns for comp v comp or 1 round for comp v human
        private void HaveAGoC0H1(bool moveMade)
        {
            // This version supports a human playing against the C# opponent (Player 0)
            // called from the play button initially and then from the moveMade event handler
            if (players[player].Human == false) //player == PLAYER0) // this should only be true the first time
            {
                Cout(CompMoveC0());
                if (!endGame) // the game may have ended now
                {
                    Cout(PrepForNext()); // if it hasn't then it must be human's go
                }
                else
                {
                    Cout(FinishGame() + " won.");
                }
                DisplayBoard(); // HumanPrepV0H1(); // display incl. dice in prep for Human go
            }
            else // it's a human's go
            {
                if (moveMade) // i.e. not the first time after pressing play if it's a human go
                {
                    if (CheckMove()) // Move is good
                    {
                        BoardUpdate(p1Board, p0Board, aBGBoard.fMove1, aBGBoard.tMove1); // i.e the arrays in main
                        PrintMove(aBGBoard.fMove1, aBGBoard.tMove1, 0, 0, 0, 0, 0, 0); // i.e. Human move notation
                        txtMessage.Text = "";
                        aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                        aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);
                        numMoves--; // they may have 2 or 4 to start

                        if (FindLastPiece(p1Board) == 0) // the game may have ended now
                        {
                            UserWin(); // admit defeat
                            Cout(FinishGame() + " won.");
                        }
                    }
                    DisplayBoard(); // whether its final board pos or human move prep you will want to see the board
                    if (numMoves <= 0 && !endGame)
                    {
                        Cout(PrepForNext());
                        Cout(CompMoveC0()); // Computer go

                        if (!endGame) // the game may have ended now
                        {
                            Cout(PrepForNext()); // if it hasn't then it must be human's go
                        }
                        else // if it has the computer's won
                        {
                            Cout(FinishGame() + " won.");
                        }
                        DisplayBoard(); // whether its final board pos or human move prep you will want to see the board
                    }
                    else // there is more human moving to be done in this turn
                    {
                        if (!aBGRules.IsPossible(p1Board, p0Board, di1, di2) && numMoves > 0) // but they can't
                        {
                            numMoves = 0;
                            PrintMove(0, 0, 0, 0, 0, 0, 0, 0);
                            txtMessage.Text = "There is no possible 2nd move"; // tell them
                            aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                            aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);

                            Cout(PrepForNext()); // and it's the computer's turn
                            Cout(CompMoveC0()); // Computer go
                            //txtMessage.Text = message;

                            if (!endGame) // the game may have ended now
                            {
                                Cout(PrepForNext()); // if it hasn't then it must be human's go
                            }
                            else
                            {
                                Cout(FinishGame() + " won.");
                            }
                            DisplayBoard(); // whether its final board pos or human move prep you will want to see the board
                        }
                    }
                }
                // about to wait for human... check they've something they can do
                while (!aBGRules.IsPossible(p1Board, p0Board, di1, di2) && !endGame)
                {
                    numMoves = 0; // while we're waiting for the human to be able to move 
                    PrintMove(0, 0, 0, 0, 0, 0, 0, 0); // indicate they can't move
                    txtMessage.Text = "There is no possible human move";
                    aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                    aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);

                    Cout(PrepForNext());
                    Cout(CompMoveC0()); // Computer go
                    //txtMessage.Text = message;

                    if (!endGame) // the game may have ended now
                    {
                        Cout(PrepForNext()); // if it hasn't then it must be human's go
                    }
                    else
                    {
                        Cout(FinishGame() + " won.");
                    }
                    DisplayBoard(); // whether its final board pos or human move prep you will want to see the board
                }
            }
        }

        private void HaveAGoH0H1(bool moveMade)
        {
            if (moveMade) // i.e. not the first time after pressing play if it's a human go
            {
                if (CheckMove()) // Move is good
                {
                    int lastPiece = 25;
                    PrintMove(aBGBoard.fMove1, aBGBoard.tMove1, 0, 0, 0, 0, 0, 0); // i.e. Human move notation
                    txtMessage.Text = "";
                    aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                    aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);
                    numMoves--; // they may have 2 or 4 to start
                    if (player == 1) // maybe we should have an array of boards to avoid this...
                    {
                        BoardUpdate(p1Board, p0Board, aBGBoard.fMove1, aBGBoard.tMove1); // i.e the arrays in main
                        lastPiece = FindLastPiece(p1Board);
                    }
                    else
                    {
                        BoardUpdate(p0Board, p1Board, aBGBoard.fMove1, aBGBoard.tMove1); // i.e the arrays in main
                        lastPiece = FindLastPiece(p0Board);
                    }

                    if (lastPiece == 0) // the game may have ended now
                    {
                        UserWin(); // admit defeat
                        Cout(FinishGame() + " won.");
                        endGame = true;
                        numMoves = 0;
                    }
                }
                if (endGame != true)
                {
                    if (numMoves <= 0) // this turn has ended
                    {
                        Cout(PrepForNext());
                    }
                    else // there is possibly more moving to be done in this person's turn
                    {
                        if ((!aBGRules.IsPossible(p1Board, p0Board, di1, di2) && player == 1) || (!aBGRules.IsPossible(p0Board, p1Board, di1, di2) && player == 0)) // but they can't
                        {
                            numMoves = 0;
                            PrintMove(0, 0, 0, 0, 0, 0, 0, 0);
                            txtMessage.Text = "There is no possible 2nd move"; // tell them
                            aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                            aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);

                            Cout(PrepForNext()); // and it's the computer's turn
                        }
                    }
                    // about to wait for a human... check they've something they can do
                    int stalemate = 100; // give them 100 dice throws to work it out
                    while (((!aBGRules.IsPossible(p1Board, p0Board, di1, di2) && player == 1) || (!aBGRules.IsPossible(p0Board, p1Board, di1, di2) && player == 0)) && !endGame)
                    {
                        stalemate--;
                        numMoves = 0; // while we're waiting for the human to be able to move 
                        PrintMove(0, 0, 0, 0, 0, 0, 0, 0); // indicate they can't move
                        txtMessage.Text = "There is no possible human move";
                        aMoveRec1.Add(aBGBoard.fMove1); // record human moves for replay / undo
                        aMoveRec1.Add(aBGBoard.fMove1 - aBGBoard.tMove1);

                        if (stalemate > 0)
                        {
                            Cout(PrepForNext());
                        }
                        else { endGame = true; }
                    }
                }
            }
            DisplayBoard(); // whether its final board pos or human move prep you will want to see the board
        }

        private void PlayGamesC0C1()
        {
            // Run this is run in a new thread so that the screen will update as determined below
            try
            {
                inThread = true;
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    btnPlayC0C1.Visibility = Visibility.Visible;
                    btnPlayC0C1.Content = "Interrupt";
                }
                );
                string mWinner;
                C0Wins = 0; //Count C0 win
                for (int game = 0; game < cVsCgameNumber; game++)
                {
                    string gameRecord = "";
                    gameRecord += "B A C K G A M M O N" + Environment.NewLine;
                    InitialiseC0C1(); // throws dice to work out who'll go first whose go it is
                    gameRecord += players[0].Name + "; Black, 0, C#, from bottom p (" + players[0].TemperStr() + ") vs " + Environment.NewLine;
                    gameRecord += players[1].Name + "; Red, 1, C#, to top (" + players[1].TemperStr() + ")." + Environment.NewLine;
                    gameRecord += players[player].Name.Substring(0, 2) + ": ";
                    gameRecord += di1.ToString() + "-" + di2.ToString() + ": ";

                    while (!endGame && !exitThread) //game has started
                    {
                        if (player == 0)
                        {
                            gameRecord += CompMoveC0(); // Computer go s1/d1 s2/d2 (possible s3/d3 and s4/d4)
                        }
                        else
                        {
                            gameRecord += CompMoveC1();
                        }
                        if (endGame)
                        {
                            // Show stuff at the end of each game: this is optional if you want to operate in 'quiet' mode
                            mWinner = FinishGame(); // if you don't do this here it can get the wrong board info...
                            if (cVsCshowGames)
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                    {
                                        gameRecord += mWinner + " won";
                                        txtMessage.Text = "Corman0: " + C0Wins.ToString() + " vs Calter1: " + (game - C0Wins).ToString(); // players[player].Message;
                                        txtNotation.Text = gameRecord;
                                        txtNotation.ScrollToEnd();
                                        DisplayBoard();
                                    }
                                );
                                Thread.Sleep(2 * cVsCSecPause);
                            }
                        }
                        else
                        {
                            // Show stuff at the end of each move: this is optional if you want to operate in 'quiet' mode
                            if (cVsCshowMoves && !exitThread)
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                    {
                                        txtMessage.Text = players[player].Message;
                                        txtNotation.Text = gameRecord;
                                        txtNotation.ScrollToEnd();
                                        DisplayBoard();
                                    }
                                );
                                Thread.Sleep(cVsCSecPause * 1000);
                            }
                            gameRecord += PrepForNext();
                        }
                    }
                }
                inThread = false; // reset
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    btnPlayC0C1.Visibility = Visibility.Hidden;
                    btnPlayC0C1.Content = "Auto";
                }
                );
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Source + " " + ex.Message;
            }
        }

        private string PrepForNext()
        {
            // Change player, throw dice and return information for user.
            string result;

            ChangePlayer();
            ThrowDice();
            result = players[player].Name.Substring(0, 2) + ": ";
            result += di1.ToString() + "-" + di2.ToString() + ": ";
            return result;
        }

        private void DisplayBoard()
        {
            try
            {
                aBGBoard.diceHi = di1;
                aBGBoard.diceLo = di2;
                aBGBoard.player = player;
                aBGBoard.SetBlackBoard(p0Board);
                aBGBoard.SetRedBoard(p1Board);
                aBGBoard.DrawBoard();
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Source + " " + ex.Message;
            }

        }

        // Player move calc move 
        private string CompMoveC0()
        {
            //Sets up C# Player playing as player 0 and calls its CompMove method to come up with a move
            players[player].dice1 = di1;
            players[player].dice2 = di2;
            players[player].SetMyBoard(p0Board); // Note: p0Board
            players[player].SetTheirBoard(p1Board);
            players[player].CompMove(); //sets notation, message, gameover
            players[player].GetMyBoard(p0Board);
            players[player].GetTheirBoard(p1Board);

            //txtMessage.Text = players[player].Message;
            endGame = players[player].GameOver;

            return players[player].Notation;
        }

        private string CompMoveC1()
        {
            //Sets up C# Player playing as player 1 and calls its CompMove method to come up with a move
            players[player].dice1 = di1;
            players[player].dice2 = di2;
            players[player].SetMyBoard(p1Board); // Note: p1Board
            players[player].SetTheirBoard(p0Board);
            players[player].CompMove(); //sets notation, message, gameover
            players[player].GetMyBoard(p1Board);
            players[player].GetTheirBoard(p0Board);

            //txtMessage.Text = players[player].Message;
            endGame = players[player].GameOver;

            return players[player].Notation;
        }

        // Post move
        private bool CheckMove()
        {
            //drag drop event -> on board raises a 'human move' event in the main game which calls this.
            //check legality: see if proposed move is legal with either dice 1 or dice 2
            bool result = false;

            if (players[player].Human != true || endGame)
            {
                Couterr("It's the computer's turn.");
            }
            else
            { //it is your go
                if (player == 1)
                {
                    if (aBGRules.IsLegal(aBGBoard.fMove1, aBGBoard.tMove1, p1Board, p0Board, di1))
                    {
                        result = true;
                        di1 = (di1 != di2) ? 0 : di1; // don't change it when double thrown
                    }
                    else
                    {
                        if (aBGRules.IsLegal(aBGBoard.fMove1, aBGBoard.tMove1, p1Board, p0Board, di2))
                        {
                            result = true;
                            di2 = (di1 != di2) ? 0 : di2; // don't change it when double thrown
                        }
                        else
                        {
                            Couterr("Illegal move: " + aBGRules.MessageOut);
                        }
                    }
                }
                else
                { // player 0
                    if (aBGRules.IsLegal(aBGBoard.fMove1, aBGBoard.tMove1, p0Board, p1Board, di1))
                    {
                        result = true;
                        di1 = (di1 != di2) ? 0 : di1; // don't change it when double thrown
                    }
                    else
                    {
                        if (aBGRules.IsLegal(aBGBoard.fMove1, aBGBoard.tMove1, p0Board, p1Board, di2))
                        {
                            result = true;
                            di2 = (di1 != di2) ? 0 : di2; // don't change it when double thrown
                        }
                        else
                        {
                            Couterr("Illegal move: " + aBGRules.MessageOut);
                        }
                    }
                }
            }
            return result;
        }

        private void BoardUpdate(int[] myBoard, int[] theirBoard, int fMove1, int tMove1)
        {
            checkerHit = false; // for use in printing move with *
            if (tMove1 > 0) // normal moves
            {
                myBoard[fMove1]--;
                myBoard[tMove1]++;
                if (theirBoard[25 - tMove1] == 1)
                {
                    theirBoard[25 - tMove1] = 0;
                    theirBoard[25]++;
                    checkerHit = true;
                }
            }
            else // bearing off
            {
                myBoard[fMove1]--;
            }
        }

        private void PrintMove(int f1, int t1, int f2, int t2, int f3, int t3, int f4, int t4)
        {
            // cf https://en.wikipedia.org/wiki/Backgammon_notation

            string moveText = "";
            t1 = (t1 < 0) ? 0 : t1;
            if (f1 > 0)
            {
                moveText = f1.ToString().Replace("25", "bar") + "/" + t1.ToString();
                moveText = moveText.Replace("/0", "/off");
                moveText = (checkerHit) ? moveText += "*" : moveText;
            }
            else
            {
                moveText += " (no play)";
            }
            moveText = (numMoves <= 1) ? moveText += vbCrLf : moveText += " ";
            Cout(moveText);
        }

        private int FindLastPiece(int[] aBoard)
        {
            //return position of piece furthest from home
            int result = 0;
            int checkSum = 0;

            for (int bPos = 0; bPos <= 25; bPos++)
            { //while(aBoard[bPos] > 0 
                if (aBoard[bPos] > 0)
                {
                    result = bPos;
                    checkSum += aBoard[bPos];
                }
            }
            return result;
        }

        private void ChangePlayer()
        {
            mTurns = mTurns + 1;
            opponent = player;
            player = System.Math.Abs(player - 1); // 'change players
        }

        private void ThrowDice()
        {

            if (useProgrammedDice)
            {
                // programmed dice moves
                di1 = aDiceList[dlNext++];
                di2 = aDiceList[dlNext++];
            }
            else
            {
                // random dice moves
                di1 = aDice.Next(6) + 1;
                if (mTurns == 0)
                { //you can't get a double on the first throw
                    do
                    {
                        di2 = aDice.Next(6) + 1;
                    } while (di2 == di1);
                }
                else
                {
                    di2 = aDice.Next(6) + 1;
                }
                aDiceList.Add(di2);
                aDiceList.Add(di1);
            }
            // don't comment this out
            numMoves = aBGRules.GetNumMoves(di1, di2);
        }

        // Post game
        private void UserWin()
        {
            txtMessage.Text = "Jolly good show old man, you win."; // Couterr("Jolly good show old man, you win.");
            endGame = true;
            numMoves = 0; // so we don't keep trying to move but can't and then computer moves.
            players[player].IWin = true;
        }

        private string FinishGame()
        {
            string winnerName = "";
            if (players[player].IWin == true)
            {
                GameHist(players[player].Temper, players[opponent].Temper);
                if (player == 0)
                {
                    if (FindLastPiece(p0Board) != 0)
                    {
                        throw new System.Exception("This can't be right. p0Board has a piece on it.");
                    }
                    players[player].Learn(p1Board);
                    C0Wins++;
                }
                else
                {
                    if (FindLastPiece(p1Board) != 0)
                    {
                        throw new System.Exception("This can't be right. p1Board has a piece on it.");
                    }
                    players[player].Learn(p0Board);
                }
                winnerName = players[player].Name;
            }
            else // opponent won
            {
                GameHist(players[opponent].Temper, players[player].Temper);
                if (opponent == 0)
                {
                    if (FindLastPiece(p0Board) != 0)
                    {
                        throw new System.Exception("This can't be right p0Board b");
                    }
                    players[opponent].Learn(p1Board);
                    C0Wins++;
                }
                else
                {
                    if (FindLastPiece(p1Board) != 0)
                    {
                        throw new System.Exception("This can't be right p1Board b");
                    }
                    players[opponent].Learn(p0Board);
                }
                winnerName = players[opponent].Name;
            }
            return winnerName;
        }

        private void GameHist(int winnerTemp, int loserTemp)
        {
            // use 0 for human
            string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmm");
            string gameType = (players[player].Human == true) ? "H" : "C";// (players[player].CompVB == true) ? "V" : "C";
            gameType += (players[opponent].Human == true) ? "-H" : "-C";// (players[opponent].CompVB == true) ? "-V" : "-C";
            File.AppendAllText(AppPath + GameHistFile, timeStamp
                + ", " + winnerTemp.ToString()
                + ", " + loserTemp.ToString()
                + ", " + gameType
                + Environment.NewLine
                );
        }

        private void SaveGame(string aFileName)
        {
            try
            {
                aBGGame.Notation = txtNotation.Text;
                aBGGame.EndGame = endGame;
                aBGGame.Player = player;
                aBGGame.SetPlayers(players);
                aBGGame.SetP0Board(p0Board);
                aBGGame.SetP1Board(p1Board);
                aBGGame.DiceHi = di1;
                aBGGame.DiceLo = di2;
                aBGGame.NumMoves = numMoves;
                aBGGame.mTurns = mTurns;

                Stream aStream = File.Create(aFileName);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(aStream, aBGGame);
                aStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        // General purpose
        private void Cout(string myText)
        {
            txtNotation.Focus();
            txtNotation.AppendText(myText);
            txtNotation.SelectionStart = txtNotation.Text.Length;
        }

        private void Couterr(string myText)
        {
            MessageBox.Show(myText);
        }

    }
}
