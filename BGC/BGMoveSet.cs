using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC
{
    [Serializable()] //https://msdn.microsoft.com/en-us/library/et91as27(v=vs.110).aspx
    class BGMoveSet // is for communicating moves between entities
    {
        //public string Message { get; set; }
        //public bool IsPossible { get; set; }
        private List<BGMove> bgmoves = new List<BGMove>();
        public List<BGMove> BGMoves
        {
            get { return bgmoves; }
            set { bgmoves = value; }
        }
        public int Di1 { get; set; }
        public int Di2 { get; set; }
        public double Score { get; set; }

        public string GetNotation(bool verbose)
        {
            // 
            // cf https://en.wikipedia.org/wiki/Backgammon_notation
            string result = "";// Di1.ToString() + "-" + Di2.ToString() + Environment.NewLine;
            string checkerHit;
            foreach (var move in BGMoves) //.OrderBy(m => m.SeqOrder))
            {
                checkerHit = (move.CheckerHit == true) ? "*" : "";
                if (verbose)
                {
                    result += move.PieceID.ToString()
                    + ":" + move.SeqOrder.ToString()
                    + ":" + move.Di.ToString()
                    + " " + move.FMove.ToString()
                    + "/" + move.TMove.ToString()
                    + checkerHit
                    + Environment.NewLine;
                }
                else
                {
                    move.TMove = (move.TMove < 0) ? 0 : move.TMove;
                    if (move.FMove > 0)
                    {
                        result += move.FMove.ToString().Replace("25", "bar")
                        + "/" + move.TMove.ToString()
                        + checkerHit;
                        result = result.Replace("/0", "/off");
                    }
                    else
                    {
                        result += "(no play)";
                    }
                    // If there's only 1 move add (no play)
                    result = (BGMoves.Count == 1) ? result += " (no play)" : result;
                    // If it's the last move in the sequence add CR LF
                    result = (move.SeqOrder == BGMoves.Last().SeqOrder) ? result += Environment.NewLine : result += " ";
                }
            }
            return result;
        }

        public string GetMessage(bool gameOver)
        {
            string result;

            if (gameOver == true)
            {
                result = "Game over.";
            }
            else
            {
                if (bgmoves.Count == 0)
                {
                    result = "No move possible.";
                }
                else
                {
                    if (Di1 != Di2)
                    {
                        result = (bgmoves.Count == 1) ? "No more moves possible." : "";
                    }
                    else // double
                    {
                        result = (bgmoves.Count < 3) ? "No more moves possible." : "";
                    }
                }
            }
            return result;
        }

        public void Update(BGMoveSet moveSet)
        {
            BGMoves.Clear();
            Di1 = moveSet.Di1;
            Di2 = moveSet.Di2;
            Score = moveSet.Score;
            foreach (BGMove move in moveSet.BGMoves)
            {
                BGMove bestMove = new BGMove(move);
                BGMoves.Add(bestMove);
            }
        }

        public void Clear()
        {
            bgmoves.Clear();
            Di1 = 0;
            Di2 = 0;
            Score = 0;
        }
    }
}
