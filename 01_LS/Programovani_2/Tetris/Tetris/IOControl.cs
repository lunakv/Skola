using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Tetris
{
    public partial class IOControl : Form
    {

        //TODO Fix deleting rows
        //TODO Add kicks
        //TODO Add next button
        //TODO Look at visual bugs

        private GameBoard g;

        public IOControl()
        {
            InitializeComponent();
            KeyDown += KeyPressed;
            KeyUp += KeyReleased;
            InitGraphics();
            g = new GameBoard(this);
            g.Start();

        }

        
        #region Input Events

        private System.Timers.Timer keyTimer = new System.Timers.Timer()
        {
            Interval = 100,
            AutoReset = false,
        };
        private bool longPress, multiPress;

        public void KeyPressed(object sender, KeyEventArgs e)
        {
            if (longPress || keyTimer.Enabled) return;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    longPress = true;
                    g.RotatePiece();
                    break;
                case Keys.Down:
                    longPress = true;
                    g.SetFastFall();
                    break;
                case Keys.Left:
                    g.ShiftPiece(Direction.Left);
                    break;
                case Keys.Right:
                    g.ShiftPiece(Direction.Right);
                    break;
                case Keys.Space:
                    g.Stop();
                    break;
                case Keys.X:
                    g.Start();
                    break;
            }

            if (!longPress && !multiPress)
            {
                keyTimer.Elapsed += MultiPressEvent;
                keyTimer.Start();
            }
        }

        void MultiPressEvent(object sender, EventArgs e)
        {
            multiPress = true;
        }
        

        public void KeyReleased(object sender, KeyEventArgs e)
        {
            longPress = false;
            multiPress = false;
            keyTimer.Elapsed -= MultiPressEvent;
            if (e.KeyCode == Keys.Down)
                g.SetSlowFall();
        }
        #endregion

        #region Visual Control
        private Bitmap _picture;
        private Graphics gr;

        private int _blockWidth;
        private int _blockHeight;

        private int _firstInsideWidth;
        private int _lastInsideWidth;
        private int _firstInsideHeight;
        private int _lastInsideHeight;

        private readonly Color _baseColor = Color.Blue;
        private Color _backColor;
        private Pen linePen = new Pen(Color.Gray, 3);
        private SolidBrush b = new SolidBrush(Color.Blue);


        void InitGraphics()
        {
            _backColor = gameBox.BackColor;
            _firstInsideWidth = (int) linePen.Width;
            _firstInsideHeight = (int) linePen.Width;
            _lastInsideWidth = gameBox.Width - 1 - (int) linePen.Width;
            _lastInsideHeight = gameBox.Height - 1 - (int) linePen.Width;
            _blockWidth =  (_lastInsideWidth - _firstInsideWidth + 1) / 10;
            _blockHeight = (_lastInsideHeight - _firstInsideHeight + 1) / 20;

            _picture = new Bitmap(gameBox.Width, gameBox.Height);
            gr = Graphics.FromImage(_picture);
            gameBox.Image = _picture;
            _backColor = Color.Gray;
            gr.FillRectangle(Brushes.Gray, _firstInsideWidth, _firstInsideHeight, _lastInsideWidth -_firstInsideWidth + 1, _lastInsideHeight - _firstInsideHeight + 1);

            
            linePen.Color = Color.Black;
            int x_0 = (int) linePen.Width / 2;
            int y_0 = x_0;
            int x_max = gameBox.Width - 1 - x_0;
            int y_max = gameBox.Height - 1 - y_0;

            gr.DrawLine(linePen, x_0, y_0, x_0, y_max);
            gr.DrawLine(linePen, x_0, y_0, x_max, y_0);
            gr.DrawLine(linePen, x_0, y_max, x_max, y_max);
            gr.DrawLine(linePen, x_max, y_0, x_max, y_max);
        }

        public void FillSpot(int x, int y)
        {
            FillSpot(_baseColor, x, y);
        }

        

        public void DeleteSpot(int x, int y)
        {
            FillSpot(_backColor, x, y);
        }

        public void DeleteRow(int y)
        {
            for (int i = 0; i < 10; i++)
            {
                DeleteSpot(i, y);
            }
        }
        
        public void Refresh()
        {
            gameBox.Invalidate();
            
        }
        
        public void MoveSpot(int fromX, int fromY, int toX, int toY)
        {
            Color c = GetSpotColor(fromX, fromY);
            FillSpot(c, toX, toY);
            DeleteSpot(fromX, fromY);
        }

        public void ClearScreen()
        {
            Brush tmp = new SolidBrush(_backColor);
            gr.FillRectangle(tmp, _firstInsideWidth, _firstInsideHeight, _lastInsideWidth-_firstInsideWidth, _lastInsideHeight-_firstInsideHeight);
        }


        public delegate Color GSCD(int x, int y);

        public Color GetSpotColor(int x, int y)
        {
            if (InvokeRequired)
            {
                
                return (Color) Invoke(new GSCD(GetSpotColor), x, y);
            }
            else
            {
                y -= 2;
                int pixelX = x * _blockWidth + _firstInsideWidth + 1;
                int pixelY = y * _blockHeight + _firstInsideHeight + 1;
                return _picture.GetPixel(pixelX, pixelY);
            }

            
        }

        public delegate void USD(int score);
        
        public void UpdateScore(int score)
        {
            if (InvokeRequired)
            {
                Invoke(new USD(UpdateScore), score);
            }
            else
            {
                scoreLabel.Text = score.ToString();
            }
        }

        public delegate void FSD(Color c, int x, int y);

        public void FillSpot(Color color, int x, int y)
        {
            if (InvokeRequired)
            {
                Invoke(new FSD(FillSpot), color, x, y);
            }
            else
            {
                y -= 2;
                if (x < 0 || x >= 10 || y < 0 || y >= 20) return;

                b.Color = color;
                gr.FillRectangle(b, x * _blockWidth + _firstInsideWidth, y * _blockHeight + _firstInsideHeight, _blockWidth, _blockHeight);
            }
        }
        #endregion
    }
}
