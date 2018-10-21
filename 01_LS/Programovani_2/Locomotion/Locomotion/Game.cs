using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Locomotion
{
    public class Game
    {
        public const int GAME_HEIGHT = 560; //Výška herní plochy
        public const int GAME_WIDTH = 630;  //Šířka herní plochy (bez zatáčky vpravo dole)
        public const int PIECE_SIZE = 70;   //Rozměr políčka (včetně mezery)
        public const int START_Y = 7;       //Počáteční souřadnice lokomotivy
        public const int START_X = 5;
        public const int END_Y = 7;         //Koncové souřadnice lokomotivy
        public const int END_X = 6;

        private Panel container;            //Panel se všemi prvky, ke kterým má hra přístup
        private Label stationLabel, scoreLabel, stopTimeLabel;   //Stavové labely     
        private Label[] lifeLabels;         //Labely značící životy
        private Track[,] gameBoard;         //Herní plocha, indexovaná [y,x]
        public Random rand;
        public Train T;                     //Současný vlak
        private int freeX, freeY;           //Pozice volného políčka
        private int lives, level, score;    //Stavové proměnné
        private Timer timer, stopTimer;     //Časovače
        
        public Game(Panel board)
        {
            //Nastavení panelu
            container = board;
            container.Height = GAME_HEIGHT;
            container.Width = 870;
            
            CreateLabels();

            //Inicializace herní plochy
            gameBoard = new Track[8, 10];
            rand = new Random();
            CreateFixedFields();
            
            //Nastavení časovačů
            timer = new Timer {Interval = 700};
            timer.Tick += Iterate;
            stopTimer = new Timer {Interval = 200};
            stopTimer.Tick += TickPause;
        }

        //Začátek hry
        public void Start()
        {
            lives = 3;
            UpdateLifeLabels();
            level = 1;
            score = 0;
            stopTimeLabel.Text = "60:00";
            stopTimeLabel.Tag = 60000;
            StartLevel();
            container.Show();
        }

        //Jedna iterace herní smyčky
        void Iterate(object sender, EventArgs e)
        {
            int res = T.MoveOnce();
            if (res == -1)
                Crash();
            else if (res == +1)
                GoToNextLevel();
        }

        //Jeden tick stopTimeru
        void TickPause(object sender, EventArgs e)
        {
            int msLeft = (int) stopTimeLabel.Tag;

            if (msLeft <= 0)
            {
                stopTimeLabel.Text = "00:00";
                PauseTrain();
                return;
            }

            stopTimeLabel.Tag = msLeft - 200;
            stopTimeLabel.Text = $"{msLeft / 1000:D2}:{(msLeft % 1000) / 10:D2}";
        }

        //Procedura při nárazu vlaku
        void Crash()
        {
            timer.Stop();
            lives--;
            UpdateLifeLabels();
            System.Threading.Thread.Sleep(1000);

            if (lives <= 0)
                EndTheGame(false);
            else
            {
                DisposeLevel();
                StartLevel();
            }
        }

        //Začátek úrovně *level*
        void StartLevel()
        {
            freeX = 1;
            freeY = 0;
            CreateBoard(Maps.GetMap(level));
            T = new Train(this, gameBoard[START_Y, START_X]);

            stationLabel.Text = Maps.GetMapName(level);
            scoreLabel.Text = score.ToString();
            container.Refresh();
            System.Threading.Thread.Sleep(1000);
            timer.Start();
        }

        //Zahození polí z plánu při skončení úrovně
        void DisposeLevel()
        {
            T.Dispose();
            for (int i = 0; i < 7; i++)
            {
                for (int j = 1; j < 9; j++)
                {
                    gameBoard[i, j]?.Dispose();
                }
            }
        }

        //Přechod na další úroveň
        void GoToNextLevel()
        {
            timer.Stop();
            if (level >= 9)
            {
                EndTheGame(true);
            }
            else
            {
                level++;
                gameBoard[END_Y, END_X + 1].Refresh(); //Když tu není, vlak se objevuje na dvou místech současně
                AddScore();
                DisposeLevel();
                StartLevel();
            }
        }

        //Přičtení skóre po skončení úrovně
        void AddScore()
        {
            foreach (var policko in gameBoard)
            {
                if (policko != null && policko.Visited)
                {
                    policko.BackColor = Color.Green;
                    policko.Refresh();
                    UpdateScore(+1);
                    System.Threading.Thread.Sleep(300);
                }
            }
        }

        //Úprava skóre o danou hodnotu
        public void UpdateScore(int difference)
        {
            score += difference;
            scoreLabel.Text = score.ToString();
            scoreLabel.Refresh();
        }

        //Vytvoří všechny informační labely pro hru
        void CreateLabels()
        {
            Label l1 = new Label
            {
                Parent = container,
                Top = 20,
                Left = GAME_WIDTH + 35,
                AutoSize = true,
                Font = new Font(FontFamily.GenericMonospace, 13),
                TextAlign = ContentAlignment.BottomLeft,
                Text = "Station:",
                Visible = true
            };
            stationLabel = new Label
            {
                Parent = container,
                Top = 40,
                Left = GAME_WIDTH + 35,
                AutoSize = false,
                Height = 40,
                Width = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(FontFamily.GenericMonospace, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = true
            };
            Label l2 = new Label
            {
                Parent = container,
                Top = 370,
                Left = GAME_WIDTH + 35,
                AutoSize = true,
                Font = new Font(FontFamily.GenericMonospace, 13),
                TextAlign = ContentAlignment.BottomLeft,
                Text = "Score:",
                Visible = true
            };
            scoreLabel = new Label
            {
                Parent = container,
                Top = 390,
                Left = GAME_WIDTH + 35,
                AutoSize = false,
                Height = 35,
                Width = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(FontFamily.GenericMonospace, 18),
                TextAlign = ContentAlignment.MiddleRight,
                Visible = true
            };
            Label l3 = new Label
            {
                Parent = container,
                Top = 450,
                Left = GAME_WIDTH + 120,
                AutoSize = true,
                Font = new Font(FontFamily.GenericMonospace, 10),
                TextAlign = ContentAlignment.BottomLeft,
                Text = "Break time:",
                Visible = true
            };
            stopTimeLabel = new Label
            {
                Parent = container,
                Top = 470,
                Left = GAME_WIDTH + 120,
                AutoSize = false,
                Height = 32,
                Width = 115,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(FontFamily.GenericMonospace, 16),
                TextAlign = ContentAlignment.MiddleRight,
                Text = "60:00",
                Tag = 60000
            };
            Label tab = new Label
            {
                Parent = container,
                Top = 500,
                Left = GAME_WIDTH + 200,
                AutoSize = true,
                Font = new Font(FontFamily.GenericMonospace, 8),
                TextAlign = ContentAlignment.BottomLeft,
                Text = "[Tab]",
                Visible = true
            };
            CreateLifeLabels();
        }

        //Vytvoření labelů reprezentujících životy
        void CreateLifeLabels()
        {
            lifeLabels = new Label[3];
            for (int i = 0; i < 3; i++)
            {
                lifeLabels[i] = new Label()
                {
                    Parent = container,
                    AutoSize = false,
                    Height = 70,
                    Width = 200,
                    Top = 130 + i * 75,
                    Left = GAME_WIDTH + 35, 
                    BackColor = Color.Transparent,
                    BackgroundImage = Properties.Resources.Life,
                    Visible = true
                };
            }
        }

        //Aktualizace labelů životů
        void UpdateLifeLabels()
        {
            for (int i = 0; i < 3; i++)
            {
                if (i >= lives)
                    lifeLabels[2 - i].Hide(); //Skrývají se shora
                else
                    lifeLabels[2 - i].Show();
            }
        }

        //Konec hry
        public void EndTheGame(bool won)
        {
            DisposeLevel();
            container.Hide();

            //Vytvoří event pro formulář symbolizující konec hry.
            Interface.GameEnded(this, new EndGameEventArgs{Score = score, Win = won});
        }

        //Vytvoří herní plán dle zadané mapy
        void CreateBoard(Type[,] Map)
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 1; j < 9; j++)
                if (i != 0 || j != 1)   //První pole je prázdné
                {
                    gameBoard[i, j] = new Track(this, Map[i, j-1], true)
                    {
                        Parent = container,
                        Top = i * PIECE_SIZE,
                        Left = j * PIECE_SIZE
                    };
                }
            }
            
        }

        //Vytvoří pevné koleje stejné pro všechny úrovně
        void CreateFixedFields()
        {
                gameBoard[0, 0] = new Track(this, Type.DR, false){Parent = container, Top = 0, Left = 0};
            for (int i = 1; i < 7; i++)
            {
                gameBoard[i, 0] = new Track(this, Type.V,  false){Parent = container, Top = i * PIECE_SIZE, Left = 0};
            }
                gameBoard[7, 0] = new Track(this, Type.UR, false){Parent = container, Top = 7 * PIECE_SIZE, Left = 0};
            for (int j = 1; j < 9; j++)
            { 
                gameBoard[7, j] = new Track(this, Type.H,  false){Parent = container, Top = 7 * PIECE_SIZE, Left = j * PIECE_SIZE};
            }
                gameBoard[7, 9] = new Track(this, Type.UL, false){Parent = container, Top = 7 * PIECE_SIZE, Left = 9 * PIECE_SIZE};
                gameBoard[6, 9] = new Track(this, Type.DL, false){Parent = container, Top = 6 * PIECE_SIZE, Left = 9 * PIECE_SIZE};

            DrawStation();
        }

        //Kreslí pruhy stanice
        void DrawStation()
        {
            Graphics g = Graphics.FromImage(gameBoard[START_Y, START_X].BackgroundImage);
            Pen p = new Pen(Color.DarkSlateGray)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Square,
                Width = 6
            };

            g.DrawLine(p, 5, 10, 69, 10);
            g.DrawLine(p, 5, 59, 69, 59);

            gameBoard[END_Y, END_X].BackgroundImage = (Image) gameBoard[START_Y, START_X].BackgroundImage.Clone();
            gameBoard[END_Y, END_X].BackgroundImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

        }

        //Přesune kolej daným směrem
        public void MoveTrack(int direction)
        {
                    //Pravá šipka posouvá kolej nalevo
            int x = freeX - Directions.DirToX(direction);
            int y = freeY - Directions.DirToY(direction);

            if (x < 1 || x > 8 || y < 0 || y > 6 || !gameBoard[y, x].Movable) return;

            gameBoard[freeY, freeX] = gameBoard[y, x];
            gameBoard[freeY, freeX].Redraw(freeY * PIECE_SIZE, freeX * PIECE_SIZE);
            if (T.Current == gameBoard[freeY, freeX])
            {
                T.X = freeX;
                T.Y = freeY;
            }

            freeX = x;
            freeY = y;
            gameBoard[freeY, freeX] = null;
        }

        //Vrátí, jestli se vlak může pohnout daným směrem
        public bool CanTrainMove(int direction)
        {
            int x = T.X + Directions.DirToX(direction);
            int y = T.Y + Directions.DirToY(direction);

            return y >= 0 && Directions.CanBeAccessed(direction, gameBoard[y, x]);
        }

        //Zastaví vlak a spustí odpočet
        public void PauseTrain()
        {
            timer.Enabled = !timer.Enabled;
            stopTimer.Enabled = !stopTimer.Enabled;
        }

        //Vrátí další políčko vlaku v daném směru
        public Track GetNextPolicko(int direction)
        {
            int x = T.X + Directions.DirToX(direction);
            int y = T.Y + Directions.DirToY(direction);

            return gameBoard[y, x];
        }



        //Úplně normální podvod
        public void CheatPause()
        {
            timer.Enabled = !timer.Enabled;
        }
    }
}