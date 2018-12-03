using System.Drawing;
using System.Windows.Forms;

namespace Locomotion
{
    public class Track : PictureBox
    {
        public Type MyType;     //Typ koleje
        public bool Visited;    //Zda byla kolej navštívena
        public bool Movable;      //Zda se dá kolejí pohnout
        private Game myGame;
        private int visitThreshold, gainPerVisit;
        private Graphics g;
        
        public Track(Game game, Type typ, bool movable)
        {
            myGame = game;
            MyType = typ;
            Movable = movable;
            
            visitThreshold = 1;
            GenerateSpecial();
            BackgroundImage = Images.GetTrackImage(MyType);
            g = Graphics.FromImage(BackgroundImage);
            
            //Rozměry a pozice koleje
            Width = Game.PIECE_SIZE - 1;
            Height = Game.PIECE_SIZE - 1;
            Visible = true;
        }

        void GenerateSpecial()
        {
            if (!Movable)
            {
                visitThreshold = 0;
                BackColor = Color.Transparent;
                return;
            }

            int n = myGame.rand.Next(300);

            if (n == 0)
            {
                MyType = Type.Null;
                BackColor = Color.Red;
                visitThreshold = 0;
            }
            else if (n < 6)
            {
                BackColor = Color.Aqua;
                gainPerVisit = 14;
                Text = "+14";
            }
            else if (n < 20)
            {
                BackColor = Color.Green;
                visitThreshold = 3;
                gainPerVisit = 6;
                Text = "x3";
            }
            else
            {
                BackColor = Color.Green;
                gainPerVisit = 2;
            }

        }

        //Překreslí kolej do zadané pozice
        public void Redraw(int y, int x)
        {
            Top = y;
            Left = x;
            Refresh();
        }

        //Navštívení koleje vlakem
        public void Visit()
        {
            visitThreshold--;

            if (!Movable || visitThreshold < 0) return;
            
            BackColor = visitThreshold == 0 ? Color.Yellow : Color.Purple;
            g.DrawString(Text, new Font(FontFamily.GenericMonospace, 10), Brushes.Black, 45, 50);
            Visited = true;
            myGame.UpdateScore(gainPerVisit);
        }

    }

    public class Train : PictureBox
    {
        public int Direction;       //Směr, kterým vlak jede: 0>, 1^, 2<, 3v 
        public Track Current;       //Kolej, na které vlak právě stojí
        private Game myGame;          //Hra, jejíž je vlak součástí
        public int X, Y;

        public Train(Game game, Track start)
        {
            myGame = game;
            Current = start;
            Direction = 2;
            X = Game.START_X;
            Y = Game.START_Y;

            
            Parent = Current;
            Width = Current.Width;
            Height = Current.Height;
            Top = 0;
            Left = 0;
            BackColor = Color.Transparent;
            BackgroundImage = Images.GetTrainImage(Direction, Current.MyType);
            BringToFront();
            Show();
        }
        
        //Jeden krok vlaku vpřed
        public int MoveOnce()
        {
            //Vlak se může pohnout
            if (myGame.CanTrainMove(Direction))
            {
                Current = myGame.GetNextPolicko(Direction);
                Parent = Current;
                Current.Visit();
                X += Directions.DirToX(Direction);
                Y += Directions.DirToY(Direction);

                Direction = Directions.NextTrainDir(Direction, Current);
                BackgroundImage = Images.GetTrainImage(Direction, Current.MyType);
                Refresh();

                //Vlak se dostal na cílové pole
                if (X == Game.END_X && Y == Game.END_Y)
                {
                    return +1;
                }

                return 0;
            }

            //Vlak se nemůže pohnout
            BackgroundImage = Properties.Resources.Explosion;
            Refresh();
            return -1;
        }
    }
}