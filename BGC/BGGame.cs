using System;
using System.Collections.Generic;

namespace BGC
{
    [Serializable()] //https://msdn.microsoft.com/en-us/library/et91as27(v=vs.110).aspx
    class BGGame
    {
        // This is currently just used for saving a game.
        private int[] p_p0Board = new int[26]; // see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819
        private int[] p_p1Board = new int[26]; // each player has an array of positions and moves from high points to low (home)
        private List<BGPlayer> p_players = new List<BGPlayer> { new BGPlayer(), new BGPlayer() }; // a list of players
        public bool EndGame { get; set; }
        public string Notation { get; set; }
        public int Player { get; set; }
        public int DiceHi { get; set; }
        public int DiceLo { get; set; }
        public int NumMoves { get; set; }
        public int mTurns { get; set; }
        public string Message { get; set; }

        public List<BGPlayer> GetPlayers(List<BGPlayer> players)
        {
            int i = 0;
            foreach (BGPlayer p in p_players)
            {
                players[i++] = p;
            }
            return players;
        }

        public void SetPlayers(List<BGPlayer> players)
        {
            int i = 0;
            foreach (BGPlayer p in players)
            {
                p_players[i++] = p;
            }
        }

        public void GetP0Board(int[] aBoard) // give them a copy don't return a reference to a private array. Copy the private contents to their array (or give them a new array)
        {
            Array.Copy(p_p0Board, aBoard, 26);
        }

        public void SetP0Board(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, p_p0Board, 26);
        }

        public void GetP1Board(int[] aBoard) // give them a copy 
        {
            Array.Copy(p_p1Board, aBoard, 26);
        }

        public void SetP1Board(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, p_p1Board, 26);
        }

        // Constructor/s
        public BGGame()
        {
        }
    }
}
