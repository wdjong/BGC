using System;

namespace BGC
{
    [Serializable()] //https://msdn.microsoft.com/en-us/library/et91as27(v=vs.110).aspx
    public class BGMove
    {
        public BGMove() { }

        public BGMove(BGMove move)
        {
            MoveID = move.MoveID;
            PieceID = move.PieceID;
            FMove = move.FMove;
            TMove = move.TMove;
            Di = move.Di;
            CheckerHit = move.CheckerHit;
            SeqOrder = move.SeqOrder;
        }

        public int MoveID { get; set; } // uniqueID
        public int PieceID { get; set; } // 0 to 14
        public int FMove { get; set; } // between 25 and 1
        public int TMove { get; set; } // between 24 and 0
        public int Di { get; set; } // between 1 and 6
        public bool CheckerHit { get; set; } // if the to move results a piece being taken
        public int SeqOrder { get; set; } // between 1 and 4. the nth move in a sequence for a piece


    }
}
