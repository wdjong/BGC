using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC
{
    [Serializable()] //https://msdn.microsoft.com/en-us/library/et91as27(v=vs.110).aspx
    public class BGRules // this class is meant to be a static class but I hit problems...
    {
        public string MessageOut { get; set; }

        //private int[] myBoard = { 0, 0, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0 }; // each player has an array of positions and moves from high points to low (home)
        //private int[] theirBoard = { 0, 0, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0 }; // see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819

        //public int fMove1;
        //public int tMove1;
        //public int diceHi;
        //public int diceLo;

        //public int Di1 { get; set; }
        //public int Di2 { get; set; }

        public List<List<int>> GetDiceSeqPermut(int di1, int di2)
        {
            List<List<int>> diceSeqPermut = new List<List<int>>();// a list of dice throw sequences;
            diceSeqPermut.Clear();
            if (di1 == 0 && di2 == 0)
            {
                return diceSeqPermut;
            }
            if (di1 == 0 || di2 == 0)
            {
                diceSeqPermut.Add(new List<int> { di1 + di2 });
                return diceSeqPermut;
            }
            if (di1 == di2)
            {
                diceSeqPermut.Add(new List<int> { di1, di2, di1, di2 });
                return diceSeqPermut; // no way to know if you've used 1 of 4 just from dice
            }
            diceSeqPermut.Add(new List<int> { di1, di2 });
            diceSeqPermut.Add(new List<int> { di2, di1 });
            return diceSeqPermut;
        }

        public int GetNumMoves(int di1, int di2)
        {
            // general purpose routine for determining number of moves 
            List<List<int>> diceSeqPermut = new List<List<int>>();// a list of dice throw sequences;
            diceSeqPermut = GetDiceSeqPermut(di1, di2); // populate the list
            return diceSeqPermut.First().Count;
        }

        public bool IsPossible(int[] myBoard, int[] theirBoard, int thrownDi1, int thrownDi2)
        {
            // Just do a quick check to see if there's any possible move
            bool result = false;
            for (int f = 1; f <= 25 && result == false; f++) // check each position on the board
            {
                if (IsLegal(f, f - thrownDi1, myBoard, theirBoard, thrownDi1))
                {
                    result = true;
                }
                if (IsLegal(f, f - thrownDi2, myBoard, theirBoard, thrownDi2))
                {
                    result = true;
                }
            }
            return result;
        }

        public bool IsLegal(int fMove1, int tMove1, int[] myBoard, int[] theirBoard, int thrownDi)
        {
            // see BGLegality.vsd            // returns true if the move is legal
            bool result = false;
            bool validFrom = false;
            bool validTo = false;

            if (thrownDi > 0 && thrownDi < 7)
            {
                if (myBoard[25] > 0) // on bar
                {
                    if (fMove1 == 25)
                    {
                        validFrom = true;// that's good
                    }
                    else
                    {
                        MessageOut = "There is a piece on the bar.";
                    }
                }
                else //not on bar
                {
                    if (fMove1 >= 1 && fMove1 <= 25) // valid source point
                    {
                        if (myBoard[fMove1] > 0)  // valid source piece
                        {
                            validFrom = true;
                        }
                        else
                        {
                            MessageOut = "There is no piece on " + fMove1.ToString() + " to move.";
                        }
                    }
                    else
                    {
                        MessageOut = "From " + fMove1.ToString() + " is not a valid.";
                    }
                }
                if (validFrom) // so far so good
                {
                    if (tMove1 >= 1 && tMove1 <= 24) // valid dest point
                    {
                        if (theirBoard[25 - tMove1] < 2)  // dest not blocked by opponents
                        {
                            int diceUsed = fMove1 - tMove1;
                            if (diceUsed == thrownDi)
                            {
                                validTo = true; // on the assumption we 0 a dice after it's used...
                            }
                            else // invalid dice
                            {
                                MessageOut = "Di doesn't match move";
                            }
                        }
                        else // blocked
                        {
                            MessageOut = "Move is blocked by opponent";
                        }
                    }
                    else // possibly bearing off
                    {
                        if (CanBearOff(myBoard, fMove1, thrownDi))
                        {
                            validTo = true;
                        }
                        {
                            MessageOut = "Can't bear off";
                        }
                    }
                    if (validTo) // home and hosed
                    {
                        result = true;
                    }
                    else
                    {
                        MessageOut = MessageOut + " (" + thrownDi + ": " + fMove1 + "/" + tMove1 + ")";
                    }
                }
            }
            else
            {
                MessageOut = "Dice used is illegal";
            }
            return result;
        }

        private bool CanBearOff(int[] aBoard, int fMove1, int thrownDi)
        {
            int yLast = FindLastPiece(aBoard);

            bool result = false;
            if (yLast <= 6) // all home
            {
                if (fMove1 == thrownDi) // dice matches pos
                {
                    result = true;
                }
                else
                {
                    if (yLast == fMove1 && thrownDi > fMove1) // dice greater than furthest piece so can move that piece
                    {
                        result = true;
                    }
                    else
                    {
                        MessageOut = "You can't bear off from " + fMove1.ToString();
                    }
                }
            }
            else
            {
                MessageOut = "You can't bear off yet.";
            }
            return result;
        }

        public int FindLastPiece(int[] aBoard)
        {
            //return position of piece furthest from home
            int result = 0;

            for (int bPos = 0; bPos <= 25; bPos++)
            { //while(aBoard[bPos] > 0 
                if (aBoard[bPos] > 0)
                {
                    result = bPos;
                }
            }
            return result;
        }

        public double ProbBlot(int[] myBoard, int[] theirBoard, int p)
        {
            //Calculate the total probability of a blot being landed on by an opponent
            double ttlProb; //total probability
            double result = 0;
            bool[] t = new bool[12]; //Boolean //take table: fill with whether that dice throw would allow taking
            int p1; //Int320
            int range = 12;

            ttlProb = 0;
            if (myBoard[p] == 1)  // a potential victim is here
            {
                range = (p >= 12) ? 12 : p;
                for (p1 = 1; p1 < range; p1++) // consider 12 positions adjacent to the current position
                {
                    if (theirBoard[25 - (p - p1)] > 0) // a potential aggressor has been found
                    {
                        ttlProb = ttlProb + ProbDice(p1);
                    } // if (
                    result = ttlProb;
                } //Next p1
                //message "probability of " & p & "being landed on is: " & blotProb)
            }
            return result;
        }

        public double ProbDice(int n)
        {
            //Chances of throwing a particular number with a combination of 2 (or more via double) (i.e. exactly 2)
            double result = 0.0;

            switch (n) //Select Case n
            {
                case 1: // 1, 1
                    result = 2 / 36;
                    break;

                case 2: // 11, 2, 2
                    result = 3 / 36;
                    break;

                case 3: // 111, 3, 3, 12, 21
                    result = 5 / 36;
                    break;

                case 4: // 1111, 4, 4, 22, 13, 31
                    result = 6 / 36;
                    break;

                case 5: // 5, 5, 41, 14, 23, 32
                    result = 6 / 36;
                    break;

                case 6: // 6, 6, 51, 15, 42, 24, 33, 222
                    result = 8 / 36;
                    break;

                case 7: // 61, 16, 25, 52, 34, 43
                    result = 6 / 36;
                    break;

                case 8: // 62, 26, 53, 35, 44, 2222
                    result = 6 / 36;
                    break;

                case 9: // 63, 36, 54, 45, 333
                    result = 5 / 36;
                    break;

                case 10: // 64, 46, 55
                    result = 3 / 36;
                    break;

                case 11: // 65, 56
                    result = 2 / 36;
                    break;

                case 12: // 66, 444, 3333
                    result = 3 / 36;
                    break;

                case 15: // 555
                    result = 1 / 36;
                    break;

                case 16: // 4444
                    result = 1 / 36;
                    break;

                case 18: // 666
                    result = 1 / 36;
                    break;

                case 20: // 5555
                    result = 1 / 36;
                    break;

                case 24: // 6666
                    result = 1 / 36;
                    break;

            } // Select
            return result;
        }
    }
}
