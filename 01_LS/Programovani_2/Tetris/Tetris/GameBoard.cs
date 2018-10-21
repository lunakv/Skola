using System;
using System.Windows.Forms;

namespace Tetris
{
    class GameBoard
    {
        static readonly int[] multipliers = { 0, 40, 100, 300, 1200 };
        const byte MAX_Y = 22;
        const byte MAX_X = 10;

        private Random r = new Random();
        private IOControl gui;
        private Piece currentPiece;
        bool[,] occupied = new bool[MAX_X, MAX_Y];
        private int occupiedTopY = MAX_Y;
        private System.Timers.Timer t;
        private int currentInterval = 800;
        private bool stuck;
        private int level = 1;
        private int totalCleared = 0;
        private int score = 0;

        public GameBoard(IOControl gui)
        {
            this.gui = gui;
            CreateNewPiece();
            t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Interval = currentInterval;
            t.Elapsed += Iterate;
        }

        public void NewGame()
        {
            occupied = new bool[MAX_X, MAX_Y];
            occupiedTopY = MAX_Y;
            currentInterval = 800;
            t.Interval = currentInterval;
            score = 0;
            totalCleared = 0;
            gui.ClearScreen();
            gui.UpdateScore(score);
            Start();
        }

        void LoseTheGame()
        {
            var res = MessageBox.Show("You Lose!","You Lose", MessageBoxButtons.YesNo);
            if (res == DialogResult.No) NewGame();
        }


        public void Iterate(object Sender, EventArgs e)
        {
            Stop();
            if (occupiedTopY < 2)
            {
                Stop();
                LoseTheGame();
                return;
            }

            DeletePiece();
            currentPiece.Fall();
            RenderPiece();
            
            if (stuck)
            {
                OccupyPiece();
                DeleteFullRows();
                CreateNewPiece();
            }
            
            gui.Refresh();
            Start();
        }

        void DeleteFullRows()
        {
            int deleted = 0;
            int highestDeleted = -1;
            for (int i = 0; i < currentPiece.boxSize; i++)
            {
                int y = currentPiece.ulY + i;
                if (IsRowFull(y))
                {
                    deleted++;
                    gui.DeleteRow(y);
                    if (deleted == 1)
                        highestDeleted = y;
                } 
            }
            gui.Refresh();

            for (int i = highestDeleted - 1; i >= occupiedTopY; i--)
            {
                DropRow(i, deleted);
            }

            occupiedTopY += deleted;
            AddScore(deleted);
            gui.Refresh();
        }

        void AddScore(int deleted)
        {
            score += (level + 1) * multipliers[deleted];
            
            if (totalCleared >= 10 * level)
            {
                level++;
                currentInterval -= 50;
                t.Interval = currentInterval;
            }
            
            gui.UpdateScore(score);
        }

        void DropRow(int row, int height)
        {
            if (row + height >= MAX_Y || row < 2) return;

            for (int i = 0; i < MAX_X; i++)
            {
                occupied[i, row + height] = occupied[i, row];
                occupied[i, row] = false;
                gui.MoveSpot(i, row, i, row + height);
            }
        }

        bool IsRowFull(int row)
        {
            if (row >= MAX_Y) return false;

            for (int i = 0; i < MAX_X; i++)
            {
                if (!occupied[i, row]) return false;
            }

            return true;
        }

        void CreateNewPiece()
        {
            int type = r.Next(7);
            currentPiece = new Piece((PieceType) type, this);
        }

        void OccupyPiece()
        {
            for (int i = 0; i < currentPiece.boxSize; i++)
            for (int j = 0; j < currentPiece.boxSize; j++)
            {
                int x = currentPiece.ulX + i;
                int y = currentPiece.ulY + j;

                if (x < 0 || x >= MAX_X || y < 0 || y >= MAX_Y) continue;

                if (currentPiece.myBox[i, j] == 1)
                {
                    occupied[x, y] = true;
                    if (y < occupiedTopY) occupiedTopY = y;
                }
            }
        }

        void RenderPiece()
        {
            for (int i = 0; i < currentPiece.boxSize; i++)
            {
                for (int j = 0; j < currentPiece.boxSize; j++)
                {
                    if (currentPiece.myBox[i, j] == 1)
                        gui.FillSpot(currentPiece.MyColor, currentPiece.ulX + i, currentPiece.ulY + j);
                }
            }
        }

        void DeletePiece()
        {
            for (int i = 0; i < currentPiece.boxSize; i++)
            {
                for (int j = 0; j < currentPiece.boxSize; j++)
                {
                    if (currentPiece.myBox[i, j] == 1)
                        gui.DeleteSpot(currentPiece.ulX + i, currentPiece.ulY + j);
                }
            }
        }

        public bool CanFall()
        {
            stuck = !IsSpaceEmpty(currentPiece.myBox, currentPiece.ulX, currentPiece.ulY + 1);
            return !stuck;
        }

        public bool CanRotate()
        {
            return IsSpaceEmpty(PieceData.Rotate(currentPiece.myBox),
                                currentPiece.ulX, currentPiece.ulY);
        }

        public bool CanShift(Direction d)
        {
            int shift = (byte) d * 2 - 1;
            return IsSpaceEmpty(currentPiece.myBox, currentPiece.ulX + shift, currentPiece.ulY);
        }

        bool IsSpaceEmpty(byte[,] box, int ulX, int ulY)
        {
            for (int i = 0; i < currentPiece.boxSize; i++)
            {
                for (int j = 0; j < currentPiece.boxSize; j++)
                {
                    if (box[i, j] != 1) continue;
                     
                    int x = ulX + i;
                    int y = ulY + j;

                    if (x < 0 || x >= MAX_X || y < 0 || y >= MAX_Y ||
                        occupied[x, y])
                        return false;

                }
            }

            return true;
        }

        public void Start()
        {
            t.Start();
        }

        public void Stop()
        {
            t.Stop();
        }

        public void RotatePiece()
        {
            //t.Stop();
            DeletePiece();
            currentPiece.Rotate();
            RenderPiece();
            gui.Refresh();
            //t.Start();
        }

        public void ShiftPiece(Direction dir)
        {
            DeletePiece();
            currentPiece.Shift(dir);
            RenderPiece();
            gui.Refresh();
        }

        public void SetFastFall()
        {
            t.Interval = 30;
        }

        public void SetSlowFall()
        {
            t.Interval = currentInterval;
        }

        public void Pixel(int x, int y)
        {
            gui.GetSpotColor(x, y);
        }
    }
}
