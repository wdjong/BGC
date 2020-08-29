using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;

namespace BGC
{
    [Serializable()] //https://msdn.microsoft.com/en-us/library/et91as27(v=vs.110).aspx // to support saving state
    class BGPlayer
    {
        const int USERBAR = 25;

        private BGMoveSet bestMoveSet = new BGMoveSet(); // the best move set containing a sequence of moves
        private BGRules aBGRules = new BGRules();
        private int[] myBoard = new int[26]; // each player has an array of positions and moves from high points to low (home)
        private int[] theirBoard = new int[26]; // see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819
        private int aTemper = 0;

        // characteristics
        public string Name { get; set; }
        public bool Human { get; set; } // is the player human
        public bool PlayerZero { get; set; } // is the player player 0 or player 1 -- I don't think this is really used.
        public int Temper
        {
            get
            {
                return aTemper;
            }
            set
            {
                // If you set Temper you need to recalculate values
                aTemper = value;
                SpeedFactor = 4.00; //'Speed factor 1 --5.000 (old values)
                SafetyFactor = 1.65; //'Safety factor 2 -- 0.750
                AgroFactor = .25; //'Agression factor -- 0.500
                RiskFactor = 5.000; //'Risk  factor 4 -- 4.000
                ClumpFactor = -.5; //'Clumping factor 5 -- 0.125
                HomeFactor = 2; //'Bearing off factor 6 -- 1.000

                ChangeMultiplier = 2; //'Degree of change due to selecting temperament

                switch (aTemper)
                {
                    case 1:
                        SpeedFactor *= ChangeMultiplier;                //'Cout(" (hurry)" & vbCrLf)
                        break;
                    case 2:
                        SafetyFactor *= ChangeMultiplier;                //'Cout(" (safe)" & vbCrLf)
                        break;
                    case 3:
                        AgroFactor *= ChangeMultiplier;                // aka taking 'Cout(" (mean)" & vbCrLf)
                        break;
                    case 4:
                        RiskFactor *= ChangeMultiplier;                //'Cout(" (lucky)" & vbCrLf)
                        break;
                    case 5:
                        ClumpFactor *= ChangeMultiplier;                //' Cout(" (defensive)" & vbCrLf)
                        break;
                    case 6:
                        HomeFactor *= ChangeMultiplier;                //' Cout(" (defensive)" & vbCrLf)
                        break;
                    case 7:
                        break;
                    default:
                        aTemper = 7;
                        break;
                }
            }
        }
        public double SpeedFactor { get; set; } //Speed
        public double SafetyFactor { get; set; } //Safety
        public double AgroFactor { get; set; } //Agression
        public double RiskFactor { get; set; } //Risk establishing new points
        public double ClumpFactor { get; set; } //Clumping urge
        public double HomeFactor { get; set; } //Bearing off 
        public double ChangeMultiplier { get; set; } //Degree of effect of temperament selection

        // turn information
        public int dice1 { get; set; }
        public int dice2 { get; set; }

        public BGPlayer() // Constructor: Set default
        {
            Name = "Walt" + DateTime.Now.ToShortTimeString();
        }
        public void SetMyBoard(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, myBoard, 26);
        }
        public void SetTheirBoard(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, theirBoard, 26);
        }
        public void GetMyBoard(int[] aBoard) // give them a copy don't return a reference to a private array. Copy the private contents to their array (or give them a new array)
        {
            Array.Copy(myBoard, aBoard, 26);
        }
        public void GetTheirBoard(int[] aBoard) // give them a copy 
        {
            Array.Copy(theirBoard, aBoard, 26);
        }
        public string Notation { get; set; }
        public string Message { get; set; }
        // game information
        public bool GameOver { get; set; }
        public bool IWin { get; set; }

        // public methods
        public void Reset()
        {
            GameOver = false;
            IWin = false;
        }

        public void InitTemper() //'Temperament
        {
            Random rnd = new Random(PlayerZero ? DateTime.Now.Millisecond : (int)DateTime.UtcNow.Ticks); //seed Random
            try
            {
                int[] tempRanges = new int[8]; // array of 8 ranges used for preferring more successful strategies.
                // There is a dependecy on SQL Server Compact 4.0 SP1 https://www.microsoft.com/en-au/download/details.aspx?id=30709
                // Installed SQLLite/SQL Server Compact Toolbox
                // Get to the point where it's showing up in the data connections
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SqlCeConnection conn = new SqlCeConnection(@"Data Source=" + AppPath + "Database1.sdf");
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Sum(Score) as tR0" +
                    ",  Sum(CASE WHEN TemperID = 1 THEN Score ELSE 0 END) as tR1" +
                    ",  Sum(CASE WHEN TemperID = 2 THEN Score ELSE 0 END) as tR2" +
                    ",  Sum(CASE WHEN TemperID = 3 THEN Score ELSE 0 END) as tR3" +
                    ",  Sum(CASE WHEN TemperID = 4 THEN Score ELSE 0 END) as tR4" +
                    ",  Sum(CASE WHEN TemperID = 5 THEN Score ELSE 0 END) as tR5" +
                    ",  Sum(CASE WHEN TemperID = 6 THEN Score ELSE 0 END) as tR6" +
                    ",  Sum(CASE WHEN TemperID = 7 THEN Score ELSE 0 END) as tR7" +
                    " FROM bgmemories";
                SqlCeDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    // Pick a random number between 1 and the sum of all the scores
                    // each temper has a range of number proportional to it's score as part of the total of all scores i.e. if scores were t1 = 1, t2 = 1, t3 = 2, t4 = 1, t5 = 1, t6 = 1, t7 = 1 then t3 would have twice the chance of being picked
                    tempRanges[0] = (int)dr.GetValue(0); //db.BGMemories.Sum(m => m.Score); // find the sum of all
                    tempRanges[1] = (int)dr.GetValue(1); // The score of the first temperment is the upper bound of the first range
                    tempRanges[2] = tempRanges[1] + (int)dr.GetValue(2); // the value of a tempRange represents the upper bound of a range of numbers within which a random number might fall.
                    tempRanges[3] = tempRanges[2] + (int)dr.GetValue(3);
                    tempRanges[4] = tempRanges[3] + (int)dr.GetValue(4);
                    tempRanges[5] = tempRanges[4] + (int)dr.GetValue(5);
                    tempRanges[6] = tempRanges[5] + (int)dr.GetValue(6);
                    tempRanges[7] = tempRanges[6] + (int)dr.GetValue(7);
                }
                conn.Close();
                int tIndex = rnd.Next(1, tempRanges[0]); // between 1 and the sum of all scores
                SpeedFactor = 4.00; // 5; //'Speed factor
                SafetyFactor = 1.65; // 0.75; //'Safety factor
                AgroFactor = .25; // ; //'Agression factor
                RiskFactor = 5.000; // 4; //'Risk  factor
                ClumpFactor = -.5; // 0.125; //'Clumping factor
                HomeFactor = 2; // 1; //'Bearing off factor
                ChangeMultiplier = 1; //'Degree of change

                if (tIndex >= 1 && tIndex < tempRanges[1]) // the range is as big as the score for this temperament.
                {
                    SpeedFactor *= ChangeMultiplier;                //'Cout(" (hurry)" & vbCrLf)
                    Temper = 1;
                }
                else if (tIndex >= tempRanges[1] && tIndex < tempRanges[2])
                {
                    SafetyFactor *= ChangeMultiplier;                //'Cout(" (safe)" & vbCrLf)
                    Temper = 2;
                }
                else if (tIndex >= tempRanges[2] && tIndex < tempRanges[3])
                {
                    AgroFactor *= ChangeMultiplier;                // aka taking 'Cout(" (mean)" & vbCrLf)
                    Temper = 3;
                }
                else if (tIndex >= tempRanges[3] && tIndex < tempRanges[4])
                {
                    RiskFactor *= ChangeMultiplier;                //'Cout(" (lucky)" & vbCrLf)
                    Temper = 4;
                }
                else if (tIndex >= tempRanges[4] && tIndex < tempRanges[5])
                {
                    ClumpFactor *= ChangeMultiplier;                //' Cout(" (defensive)" & vbCrLf)
                    Temper = 5;
                }
                else if (tIndex >= tempRanges[5] && tIndex < tempRanges[6])
                {
                    HomeFactor *= ChangeMultiplier;                //' Cout(" (defensive)" & vbCrLf)
                    Temper = 6;
                }
                else
                {
                    Temper = 7;
                }
            }
            catch (Exception ex)
            {
                Temper = rnd.Next(1, 8);
                Message = ex.Message;
            }
        }

        public void CompMove()
        {
            // For each combination of dice (because it can happen that using them in a different order has a different result
            // e.g. when a piece is taken with the first throw or only one dice is usable on the first throw for a piece
            List<List<int>> diceSeqPermut = new List<List<int>>();// a list of sequences possible from the throw of the dice e.g. diceHi, diceLo or diceLo, diceHi;
            BGMoveSet thisMoveSet = new BGMoveSet(); // a moveSet to work with to find all the combinations
            bestMoveSet.Clear();
            if (aBGRules.IsPossible(myBoard, theirBoard, dice1, dice2)) // check if there's at least one move
            {
                diceSeqPermut = aBGRules.GetDiceSeqPermut(dice1, dice2); // get dice sequences
                foreach (var diceSeq in diceSeqPermut)
                {
                    GetPieceRecurse(thisMoveSet, diceSeq, myBoard, theirBoard);
                }
                foreach (BGMove move in bestMoveSet.BGMoves)
                {
                    BoardUpdate(myBoard, theirBoard, move.FMove, move.TMove); //     update board
                }
            }
            else
            {
                bestMoveSet.BGMoves.Add(new BGMove { FMove = 0, TMove = 0, PieceID = 0, SeqOrder = 0, Di = 0, MoveID = 0 });
            }
            bestMoveSet.Di1 = dice1;
            bestMoveSet.Di2 = dice2;
            Message = bestMoveSet.GetMessage(GameOver);
            Notation = bestMoveSet.GetNotation(false);
            IWin = GameOver; // if the games ending now then this player must be the winner
        }

        private void GetPieceRecurse(BGMoveSet moveset, List<int> diceSeq, int[] myBoard, int[] theirBoard)
        {
            //This routine is called recursively for each di in a sequence 2 or 4 (if doubles)
            //each piece is tried and the highest scoring sequence is remembered 
            List<BGPiece> pieces = new List<BGPiece>(); // a list of pieces each piece has an id a position
            int[] myBoard1 = new int[26]; //  declare an instance of the board
            int[] theirBoard1 = new int[26];
            int i = moveset.BGMoves.Count(); // move index
            int f = 0; // piece.Position
            int t = 0; // to
            int d = 0; // dice to use

            Array.Copy(myBoard, myBoard1, 26);
            Array.Copy(theirBoard, theirBoard1, 26); //  work with a copy of the board
            pieces = GetPieces(myBoard); // create alist of pieces
            if (i < diceSeq.Count) //there is an unused di in the sequence
            {
                bool moveFound = false;
                foreach (var piece in pieces)
                {
                    f = piece.Position;
                    d = diceSeq[i];
                    t = (f - d) < 0 ? 0 : f - d; // 0 represents bearing off; less than zero indicates you can bear off
                    if (aBGRules.IsLegal(f, t, myBoard, theirBoard, d)) // if legal move
                    {
                        BGMove aBGMove = new BGMove(); //              add the move to the list of moves
                        aBGMove.PieceID = piece.PieceID;
                        aBGMove.FMove = f;
                        aBGMove.TMove = t;
                        aBGMove.Di = d;
                        aBGMove.SeqOrder = i;
                        aBGMove.CheckerHit = (theirBoard[25 - t] == 1 && t != 0) ? true : false;
                        moveset.BGMoves.Add(aBGMove);
                        BoardUpdate(myBoard1, theirBoard1, f, t); //     update board
                        GetPieceRecurse(moveset, diceSeq, myBoard1, theirBoard1); // get another move
                        Array.Copy(myBoard, myBoard1, 26);
                        Array.Copy(theirBoard, theirBoard1, 26); //  restore the board
                        moveset.BGMoves.RemoveAt(i);
                        moveFound = true;
                    }
                }
                if (moveFound == false) // if no move is found it could be the first dice is unusable and the second may be
                {
                    moveset.Score = ScoreBoard(myBoard1, theirBoard1); // nevertheless score the move so far and record it
                    if ((moveset.Score > bestMoveSet.Score && moveset.BGMoves.Count == bestMoveSet.BGMoves.Count) || moveset.BGMoves.Count > bestMoveSet.BGMoves.Count)
                    { // either the score is better or there's more  move.
                        bestMoveSet.Update(moveset);
                    }
                }
            }
            else
            {
                moveset.Score = ScoreBoard(myBoard1, theirBoard1); // at the end of each set, score the moves
                if (moveset.Score > bestMoveSet.Score)
                {
                    bestMoveSet.Update(moveset);
                }
            }
        }

        public string TemperStr()
        {
            return TemperStr(Temper);
        }

        public string TemperStr(int temper = 0)
        {
            string result = "";
            if (temper == 0)
            {
                temper = Temper;
            }

            switch (Temper)
            {
                case 1: // first (over ~3K) -- favouring hurrying
                    result = "furthest away first";
                    break;
                case 2: // last -- favouring >=2 on a point
                    result = "making points"; // blocking sort of the same as not blotting...             
                    break;
                case 3: // second last -- favouring taking
                    result = "blitz"; // aka mean aggressive taking where possible            
                    break;
                case 4: // fourth -- 
                    result = "not blotting";  // safe avoiding blots with some consideration of probability          
                    break;
                case 5: // second -- favours making points next to others (should be exponential)
                    result = "clumping";    // adjacent points favoured             
                    break;
                case 6: // fifth -- good end game practice of getting into the 4th quadrant
                    result = "bearing in";  // home quadrant/ bearing off favoured            
                    break;
                case 7: // third -- good general balance of 
                    result = "normal";  // no weighting           
                    break;

                default:
                    break;
            }

            return result;
        }

        // private methods (logically arranged (in the process of working out a move) with sub routines below)
        private List<BGPiece> GetPieces(int[] myBoard)
        {
            // Create a list of pieces on the board search each position for a piece
            List<BGPiece> result = new List<BGPiece>();
            int pieceID = 0;
            for (int f = 0; f < 26; f++) //each board position
            {
                for (int p = 0; p < myBoard[f]; p++) // each piece on position
                {
                    BGPiece bgPiece = new BGPiece();
                    bgPiece.PieceID = pieceID++;
                    bgPiece.Position = f;
                    result.Add(bgPiece);
                }
            }
            if (pieceID == 0)
            {
                GameOver = true;
            }
            return result;
        }

        private bool BoardUpdate(int[] myBoard, int[] theirBoard, int fMove1, int tMove1)
        {
            bool result = false; // for use in printing move with *
            if (tMove1 > 0) // normal moves
            {
                myBoard[fMove1]--;
                myBoard[tMove1]++;
                if (theirBoard[25 - tMove1] == 1)
                {
                    theirBoard[25 - tMove1] = 0;
                    theirBoard[25]++;
                    result = true;
                }
            }
            else // bearing off
            {
                myBoard[fMove1]--;
            }
            if (myBoard[fMove1] < 0)
            {
                Message = "Help! " + fMove1.ToString() + " has negative pieces";
                GameOver = true;
            }
            return result;
        }

        private double ScoreBoard(int[] myBoard, int[] theirBoard)
        {
            ////b3 is a board (an array 0..25 of integers)
            int n = 15; //Dim n As Int32 //number of pieces off the board = home (borne off)
            double s = 0; //Dim s As Double //speed
            double b = 0; //Dim b As Double //blots: risk
            int db = 0; //Dim db As Int32 //doubles: safety
            int c = 0; //Dim c As Int32 //clump
            int a = 0; //Dim a As Int32 //agro
            bool cFlag = false; //Dim cFlag As Boolean

            var yLast = aBGRules.FindLastPiece(theirBoard);
            //speed s should be a positive number with each piece yielding a number between 0 and 1
            // 24     18      12      6
            //.04    .29     .54     .79
            //.085   .522    .835    1.023
            for (int p = 1; p < 26; p++)    //To 24 //check each board position
            {
                if (myBoard[p] > 0) //Then //there is a piece here
                {
                    n -= myBoard[p]; //n is what's not on the board i.e. borne off: big n is good 
                    // Max value would be 15 : 625 = 25^2 // old s = s + (24 ^ 2 - p ^ 2) / 24 ^ 2 * b3(p) 
                    s += (625 - Math.Pow(p, 2.0)) / 625 * myBoard[p]; //speed: smaller p -> bigger s
                    if (p > (25 - yLast))   // these things only matter if there is the possibility of interacting with the opponent
                    {
                        if (myBoard[p] == 1) //Max Value would be 15 if all all single and capable of being hit: vulnerable: check danger // 15625 = 25^3 weights the position on the board
                        {
                            b += (15625 - Math.Pow(p, 3.0)) / 15625 * aBGRules.ProbBlot(myBoard, theirBoard, p); //blots: danger taking into account probability of being hit and position on board
                        }
                        if (myBoard[p] > 1 && p != 25) //safety
                        {
                            db++; //doubles
                        }
                        if (cFlag == true && p != 25) //keep blocks together
                        {
                            c++; //adjacent pieces have been found
                        }
                        else
                        {
                            cFlag = true; //set clumping flag
                        }
                    }
                }
                else
                { //no piece so turn off adjacency flag
                    cFlag = false; //clear clumping flag
                }
            }

            a = theirBoard[USERBAR]; //aggression encourage a high number on opponent bar
            s = s + n; // pieces that are home should be counted in speed otherwise they have a negative effect.
            var result = 5 + (s * SpeedFactor) - (b * RiskFactor) + (db * SafetyFactor) + (c * ClumpFactor) + (n * HomeFactor) + (a * AgroFactor);

            //if (PlayerZero == true) { 
            //    Debug.Print "borne:  "; n
            //    System.Diagnostics.Debug.Print ("speed:  " + s.ToString() + "* " + SpeedFactor.ToString());
            //    Debug.Print "risk:   "; -b, -b * RIFAC
            //    Debug.Print "safety: "; db, db * SAFAC
            //    Debug.Print "clumpi: "; c, c * CLFAC
            //    Debug.Print "agro:   "; a, a * AGFAC
            //    Debug.Print "------------    "
            //    Debug.Print ScoreBoard
            //    Debug.Print
            //}
            return result;
        }

        private int GameScore(int[] theirBoard)
        {
            // This is called at the end of a game to give a measure of the size of the victory
            double s = 0; //score

            foreach (int p in theirBoard) // check each board position
            {
                s = s + (myBoard[p] * (p / 4.0)); // there is a piece here & it should take roughly p / 4 throws to get home for each
            }
            return (int)s;
        }

        public void Learn(int[] theirBoard)
        {
            // We should tell the player to learn at this point. The memory should be a property of the player
            try
            {
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SqlCeConnection conn = new SqlCeConnection(@"Data Source=" + AppPath + "Database1.sdf");
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE BGMemories " +
                    "SET Score = Score + 1 " +
                    "WHERE TemperID = " + Temper;
                //Console.WriteLine(cmd.CommandText);
                int numberOfRecords = cmd.ExecuteNonQuery();
                if (numberOfRecords == 0)
                {
                    cmd.CommandText = "INSERT INTO BGMemories (Temper, Score) " +
                        "VALUES (" + Temper + ", 1)";
                    //Console.WriteLine(cmd.CommandText);
                    numberOfRecords = cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Message = ex.Message; // This message is passed back when the thread delegates controll
            }
        }

    }
}
