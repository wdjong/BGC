using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BGC
{
    /// <summary>
    /// Interaction logic for BGBoard2.xaml
    /// </summary>
    public partial class BGBoard2 : Window
    {
        // Board
        private readonly int[] redBoard = new int[26]; // each player has an array of positions and moves from high points to low (home)
        private readonly int[] blackBoard = new int[26]; // see https://msdn.microsoft.com/en-us/library/0fss9skc.aspx CA1819
        private double boardHeight;
        private double boardWidth;
        // Pieces
        private readonly int nPieces = 15;
        private readonly List<Image> redPieces; // list of piece images
        private readonly List<Image> blackPieces; // list of piece images
        private Point pieceMouseOffset;
        private double pieceHeight;
        private Image dragImage;
        private DateTime mouseDownTime;
        private DateTime mouseUpTime;
        // Dice
        private readonly int nFaces = 6;
        private readonly List<Image> diImg1; // list of dice face images
        private readonly List<Image> diImg2; // list of dice face images
        private double diHeight;

        // Interface properties
        public int diceHi { get; set; }
        public int diceLo { get; set; }
        public int fMove1 { get; set; }
        public int tMove1 { get; set; }
        public int player { get; set; }

        public string message { get; }

        // public methods
        public BGBoard2() // constructor
        {
            InitializeComponent();
            redPieces = CreateRedPieces();
            blackPieces = CreateBlackPieces();
            diImg1 = CreateDice();
            diImg2 = CreateDice();
        }

        public void GetRedBoard(int[] aBoard) // return copy not reference to private array. Copy contents to array (or give them a new array)
        {
            Array.Copy(redBoard, aBoard, 26);
        }

        public void SetRedBoard(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, redBoard, 26);
        }

        public void GetBlackBoard(int[] aBoard) // give them a copy 
        {
            Array.Copy(blackBoard, aBoard, 26);
        }

        public void SetBlackBoard(int[] aBoard) // get a copy
        {
            Array.Copy(aBoard, blackBoard, 26);
        }

        public void DrawBoard() //async Task 
        {
            //Visual representation of board, with pieces and dice
            //Player 0 (black) 0 Norman
            //moves from bottom to top (clockwise)
            //Player 1 (red) 1 Walter
            //moves from top to bottom (anti-clockwise) (bridge on left)
            //bar board(25) and home board(0)

            DrawPieces();
            DrawDice();
        }

        private void DrawDice()
        {
            int aDi; // for each di
            int aDiceFace; // for each dice face
            double diceLeft = 6.5 * pieceHeight;
            double diceTop1 = 7.0 * pieceHeight + player * -2 * pieceHeight; // 0 or 1 --> 6 / 14 or 8 / 14 vertically
            double diceTop2 = -1 + 6.0 * pieceHeight + player * -2 * pieceHeight; // 0 or 1 --> 6 / 14 or 8 / 14 vertically

            for (aDi = 1; aDi <= 2; aDi++)
            {
                if (aDi == 1) // Di1 = top di
                {

                    for (aDiceFace = 1; aDiceFace <= nFaces; aDiceFace++) // each dice face
                    {
                        Image q = diImg1[aDiceFace];
                        q.Width = diHeight;
                        q.Height = diHeight;
                        q.Visibility = (aDiceFace == diceHi) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        Canvas.SetLeft(q, diceLeft);
                        Canvas.SetTop(q, diceTop1);
                    }
                }
                if (aDi == 2) // Di2 = top di
                {
                    for (aDiceFace = 1; aDiceFace <= nFaces; aDiceFace++) // each dice face
                    {
                        Image q = diImg2[aDiceFace];
                        q.Width = diHeight;
                        q.Height = diHeight;
                        q.Visibility = (aDiceFace == diceLo) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        Canvas.SetLeft(q, diceLeft);
                        Canvas.SetTop(q, diceTop2);
                    }
                }
            }
        }

        private void DrawPieces()
        {
            int aPlayer; // for each player
            int aBGPoint; // for each point
            int aPiece; //  for each piece on a point
            int piecesOnPoint;

            for (aPlayer = 0; aPlayer <= 1; aPlayer++)
            {
                if (aPlayer == 0) // Player0 = black top right = 1
                {
                    int iB = 0;
                    for (aBGPoint = 0; aBGPoint <= 25; aBGPoint++)  // Bottom right = 24
                    {
                        piecesOnPoint = blackBoard[aBGPoint]; // comp: 0 12-1 on top, 14-24 on bott
                        for (aPiece = 0; aPiece < piecesOnPoint && iB < 15; aPiece++)
                        {
                            Image q = blackPieces[iB++];
                            q.Width = pieceHeight;
                            q.Height = pieceHeight;
                            q.Visibility = System.Windows.Visibility.Visible;
                            Canvas.SetLeft(q, PtoM(aBGPoint, aPlayer).X);
                            int towerDirection = (aBGPoint < 13) ? 1 : -1;
                            int bottomOffset = (aBGPoint < 13) ? 0 : 1; // because we're setting the top of the piece
                            Canvas.SetTop(q, PtoM(aBGPoint, aPlayer).Y + towerDirection * (aPiece + bottomOffset) * pieceHeight);
                        }
                    }
                    while (iB < 15) // make the remaining ones invisible
                    {
                        blackPieces[iB++].Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else // player1 = red bottom right  = 1
                {
                    int iR = 0;
                    for (aBGPoint = 0; aBGPoint <= 25; aBGPoint++)  // each point BotR to Top right = 24 clock wise
                    {
                        piecesOnPoint = redBoard[aBGPoint]; // human:1, 13-24 on top, 12-1 on bottom
                        for (aPiece = 0; aPiece < piecesOnPoint; aPiece++) // each piece on a point
                        {
                            Point aPoint = PtoM(aBGPoint, aPlayer);
                            Image q = redPieces[iR++];
                            q.Width = pieceHeight;
                            q.Height = pieceHeight;
                            q.Visibility = System.Windows.Visibility.Visible;
                            Canvas.SetLeft(q, aPoint.X);
                            int towerDirection = (aBGPoint < 13) ? -1 : 1;
                            int bottomOffset = (aBGPoint < 13) ? 1 : 0; // because we're setting the top of the piece
                            Canvas.SetTop(q, aPoint.Y + towerDirection * (aPiece + bottomOffset) * pieceHeight);
                        }
                    }
                    while (iR < 15)
                    {
                        redPieces[iR++].Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
        }

        // public event
        public event EventHandler MoveMade; // the actions relating to this event declare are specified in Main

        protected void RaiseMoveMade()
        {
            if (this.MoveMade != null)
                this.MoveMade(this, EventArgs.Empty);
        }  // this is called to raise the event

        // events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            boardWidth = (int)MyCanvas.ActualWidth; // ((int)MyCanvas.ActualWidth > (int)MyCanvas.ActualHeight + 23)  ? (int)MyCanvas.ActualHeight : (int)MyCanvas.ActualWidth; // 284 = 261 + 23
            boardHeight = (int)MyCanvas.ActualHeight; //boardWidth;
            pieceHeight = boardWidth / 14;
            //pointWidth = pieceHeight;
            diHeight = boardWidth / 14;

            DrawBoard();
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            // This event fires as we move the mouse over board
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (dragImage != null)
                {
                    Canvas.SetLeft(dragImage, Mouse.GetPosition(MyCanvas).X + pieceMouseOffset.X);
                    Canvas.SetTop(dragImage, Mouse.GetPosition(MyCanvas).Y + pieceMouseOffset.Y);
                }
            }
        }

        private void Board_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Window1.ActualWidth > Window1.ActualHeight - 23)
            {
                boardWidth = Window1.ActualHeight - 23 - 16; // ((int)MyCanvas.ActualWidth > (int)MyCanvas.ActualHeight + 23)  ? (int)MyCanvas.ActualHeight : (int)MyCanvas.ActualWidth; // 284 = 261 + 23
                boardHeight = boardWidth; // (int)MyCanvas.ActualHeight; //boardWidth;
                MyCanvas.Height = boardWidth;
                MyCanvas.Width = boardWidth;
                pieceHeight = boardWidth / 14;
                diHeight = boardWidth / 14;
            }
            else
            {
                boardWidth = Window1.ActualWidth - 16; // ((int)MyCanvas.ActualWidth > (int)MyCanvas.ActualHeight + 23)  ? (int)MyCanvas.ActualHeight : (int)MyCanvas.ActualWidth; // 284 = 261 + 23
                boardHeight = boardWidth; // (int)MyCanvas.ActualHeight; //boardWidth;
                MyCanvas.Height = boardWidth;
                MyCanvas.Width = boardWidth;
                pieceHeight = boardWidth / 14;
                diHeight = boardWidth / 14;
            }

            boardWidth = (int)MyCanvas.ActualWidth; // ((int)MyCanvas.ActualWidth > (int)MyCanvas.ActualHeight + 23)  ? (int)MyCanvas.ActualHeight : (int)MyCanvas.ActualWidth; // 284 = 261 + 23
            boardHeight = boardWidth; // (int)MyCanvas.ActualHeight; //boardWidth;
            MyCanvas.Height = MyCanvas.ActualWidth;
            pieceHeight = boardWidth / 14;
            //pointWidth = pieceHeight;
            diHeight = boardWidth / 14;
            System.Diagnostics.Debug.Write(Window1.ActualWidth.ToString() + ", ");
            System.Diagnostics.Debug.Write(Grid1.ActualWidth.ToString() + ", ");
            System.Diagnostics.Debug.Write(MyCanvas.ActualWidth.ToString() + ": ");
            System.Diagnostics.Debug.Write(Window1.ActualHeight.ToString() + ", ");
            System.Diagnostics.Debug.Write(Grid1.ActualHeight.ToString() + ", ");
            System.Diagnostics.Debug.Write(MyCanvas.ActualHeight.ToString());

            DrawBoard();
        }

        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            // This event fires when we click on a piece
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                mouseDownTime = DateTime.Now;
                dragImage = (Image)sender;
                pieceMouseOffset.X = Canvas.GetLeft(dragImage) - Mouse.GetPosition(MyCanvas).X;
                pieceMouseOffset.Y = Canvas.GetTop(dragImage) - Mouse.GetPosition(MyCanvas).Y;
                fMove1 = MtoP(Mouse.GetPosition(MyCanvas).X, Mouse.GetPosition(MyCanvas).Y, player);
            }
        }

        private void Piece_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseUpTime = DateTime.Now;
            TimeSpan span = mouseUpTime - mouseDownTime;
            int ms = (int)span.TotalMilliseconds;
            tMove1 = MtoP(Mouse.GetPosition(MyCanvas).X, Mouse.GetPosition(MyCanvas).Y, player);
            // handles tall towers
            // automatic moves
            if (tMove1 == fMove1) // i.e. just clicked on a piece without dragging 
            {
                if (ms < 500) // but allow them to drag back when they change their mind
                {
                    if (AutoMove())
                    {
                        dragImage = null;
                        this.RaiseMoveMade();
                    }
                    else
                    {
                        System.Media.SystemSounds.Beep.Play();
                    }
                }
            }
            else // they moved the piece
            {
                // bearing off (moving to the bar = bearing off)
                tMove1 = (tMove1 == 25) ? 0 : tMove1;
                dragImage = null;
                this.RaiseMoveMade();
            }
        }

        private void Dice_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // This event fires when we click on a dice
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                mouseDownTime = DateTime.Now;
            }
        }

        private void Dice_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                mouseUpTime = DateTime.Now;
            }
        }

        // methods (alphabetical)
        private bool AutoMove() // try to find a legal automove if they just clicked on a piece. Give up if not found
        {
            bool result = false;
            int toPoss = 0;
            BGRules aBGRules = new BGRules();
            if (diceHi != 0) // there is a diceHi throw available (used by default)(diceHi is zero after a successful diceHi move (if not a double))
            {
                toPoss = ((fMove1 - diceHi) < 0) ? 0 : (fMove1 - diceHi); // when bearing off avoid negatives
                if (player == 1)
                {
                    if (aBGRules.IsLegal(fMove1, toPoss, redBoard, blackBoard, diceHi))
                    {
                        tMove1 = fMove1 - diceHi; // use the diceHi
                        result = true;
                    }
                    else // diceHI move blocked
                    {
                        toPoss = ((fMove1 - diceLo) < 0) ? 0 : (fMove1 - diceLo); // when bearing off avoid negatives
                        if (aBGRules.IsLegal(fMove1, toPoss, redBoard, blackBoard, diceLo))
                        {
                            tMove1 = fMove1 - diceLo;
                            result = true;
                        }
                    }
                }
                else
                {
                    if (aBGRules.IsLegal(fMove1, toPoss, blackBoard, redBoard, diceHi))
                    {
                        tMove1 = fMove1 - diceHi; // use the diceHi
                        result = true;
                    }
                    else // diceHI move blocked
                    {
                        toPoss = ((fMove1 - diceLo) < 0) ? 0 : (fMove1 - diceLo); // when bearing off avoid negatives
                        if (aBGRules.IsLegal(fMove1, toPoss, blackBoard, redBoard, diceLo))
                        {
                            tMove1 = fMove1 - diceLo;
                            result = true;
                        }
                    }
                }
            }
            else // diceHi was used already 
            {
                if (player == 1)
                {
                    toPoss = ((fMove1 - diceLo) < 0) ? 0 : (fMove1 - diceLo); // when bearing off avoid negatives
                    if (aBGRules.IsLegal(fMove1, toPoss, redBoard, blackBoard, diceLo))
                    {
                        tMove1 = fMove1 - diceLo;
                        result = true;
                    }
                }
                else
                {
                    toPoss = ((fMove1 - diceLo) < 0) ? 0 : (fMove1 - diceLo); // when bearing off avoid negatives
                    if (aBGRules.IsLegal(fMove1, toPoss, blackBoard, redBoard, diceLo))
                    {
                        tMove1 = fMove1 - diceLo;
                        result = true;
                    }
                }

            }
            return result;
        }

        private List<Image> CreateBlackPieces()
        {
            List<Image> result = new List<Image>();   // Create the List
            BitmapImage bm =             // Fetch the bitmap that we're going to use
                 new BitmapImage(new Uri("pack://application:,,,/Assets/black.gif"));

            for (int i = 0; i < nPieces; i++)     // Build the controls and the list.
            {
                Image im = new Image();     // Create the WPF control
                im.Source = bm;             // Tell it what image to display
                im.MouseDown += new MouseButtonEventHandler(Piece_MouseDown);
                im.MouseUp += new MouseButtonEventHandler(Piece_MouseUp);
                MyCanvas.Children.Add(im);   // Add the control to the canvas
                result.Add(im);             // And remember a reference to it.
            }
            return result;
        }

        private List<Image> CreateRedPieces()
        {
            List<Image> result = new List<Image>();   // Create the List
            BitmapImage bm =             // Fetch the bitmap that we're going to use
                 new BitmapImage(new Uri("pack://application:,,,/Assets/red.gif"));

            for (int i = 1; i <= nPieces; i++)     // Build the controls and the list.
            {
                Image im = new Image();     // Create the WPF control
                im.Source = bm;             // Tell it what image to display
                im.MouseDown += new MouseButtonEventHandler(Piece_MouseDown);
                im.MouseUp += new MouseButtonEventHandler(Piece_MouseUp);
                MyCanvas.Children.Add(im);   // Add the control to the canvas
                result.Add(im);             // And remember a reference to it.
            }
            return result;
        }

        private List<Image> CreateDice()
        {
            // Create a list of dice face images
            List<Image> result = new List<Image>();   // Create the List

            for (int i = 0; i <= nFaces; i++)     // Build the controls and the list // 0 base means index 1 = Dice face 1
            {
                Image im = new Image();
                BitmapImage bm =             // Fetch the bitmap that we're going to use
                    new BitmapImage(new Uri("pack://application:,,,/Assets/dice" + i.ToString() + ".png"));
                im.Source = bm;
                im.MouseDown += new MouseButtonEventHandler(Dice_MouseDown);
                im.MouseUp += new MouseButtonEventHandler(Dice_MouseUp);
                im.Visibility = System.Windows.Visibility.Hidden;
                MyCanvas.Children.Add(im);   // Add the control to the canvas
                result.Add(im);             // And remember a reference to it.
            }
            return result;
        }

        private int MtoP(double X, double Y, int Player)
        {
            // translates (M=Mouse) co-ordinates to backgammon 'points')
            // converts mouse co-ord to point 1-24 or bar
            int intY; // top or bottom of board
            int intX; // 1 bottom left and 24 top left for player 
            int intPoint = 25; // the point that the player is on (bar by default) (from their point of view)

            intY = (int)(Y / (MyCanvas.ActualHeight / 2.0)); // either 0 top or 1 bottom
            intX = (int)(X / (MyCanvas.ActualWidth / 14.0)); // from 0 -> 13 (allowing for a bar (double width) between
            if (intX != 6 && intX != 7) // not bar (if bar... leave as 25)
            {
                intX = (intX > 7) ? intX - 2 : intX; // to right of bar: remove the bar
                intPoint = (intY == 0) ? 13 + intX : 12 - intX; // 13 ... 24 on top, 12 ... 1 on bot
                intPoint = (Player == 0) ? 25 - intPoint : intPoint; // 'Red is player 1
            }
            return intPoint;
        }

        private Point PtoM(int intPoint, int Player)
        {
            // convert backgammon point (P) (for a player) to co-ordinate reference (M=Mouse)
            int intY; // top or bottom of board
            int intX; // 1 bottom left and 24 top left for player 
            Point MCoOrd = new Point(0, 0);

            if (intPoint == 25) // when they're on bar it's just a matter of decided which side they're on black / computer on the bottom
            {
                MCoOrd.X = 6.5 * pieceHeight;
                MCoOrd.Y = (Player == 1) ? 0.0 : boardHeight;
            }
            else
            {
                intPoint = (Player == 0) ? 25 - intPoint : intPoint; // 'Norman: moving from bottom to top (clockwise) my 1 is his 24
                intX = (intPoint > 12) ? intPoint - 13 : 12 - intPoint; // top line 24 becomes 11  bottom line 1 becomes 11
                intX = (intX > 5) ? intX + 2 : intX; // add in bar
                intY = (intPoint > 12) ? 0 : 1; // 13 ... 24 on top, 12 ... 1 on bottom
                MCoOrd.X = (double)intX * pieceHeight;
                MCoOrd.Y = (double)intY * boardHeight;
            }

            return MCoOrd;
        }

    }
}
