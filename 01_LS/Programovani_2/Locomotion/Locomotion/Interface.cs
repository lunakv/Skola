using System;
using System.Drawing;
using System.Windows.Forms;

namespace Locomotion
{
    
    public partial class Interface : Form
    {
        private Game g;
        public static EventHandler<EndGameEventArgs> GameEnded;

        
        
        public Interface()
        {
            //Inicializace
            InitializeComponent();
            board.BackColor = Color.LightGray;
            BackColor = Color.LightGray;
            Icon = Properties.Resources.Icon;
            
            g = new Game(board);

            //Ošetření řídících eventů
            KeyDown += KeyPressed;
            KeyUp += KeyReleased;
            GameEnded += EndGame;
        }
        

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyPressed;
            int smer = -1;

            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.D:
                case Keys.NumPad6:
                    smer = 0;
                    break;
                case Keys.Up:
                case Keys.W:
                case Keys.NumPad8:
                    smer = 1;
                    break;
                case Keys.Left:
                case Keys.A:
                case Keys.NumPad4:
                    smer = 2;
                    break;
                case Keys.Down:
                case Keys.S:
                case Keys.NumPad2:
                    smer = 3;
                    break;
                case Keys.Tab:
                    g.PauseTrain();
                    return;

                case Keys.F12:

                case Keys.NumPad5:
                    g.CheatPause();
                    return;
            }

            if (smer >= 0)
            {
                g.MoveTrack(smer);
            }
        }

        private void KeyReleased(object sender, EventArgs e)
        {
            KeyDown += KeyPressed;
        }

        //Začátek hry
        private void BStart_Click(object sender, EventArgs e)
        {
            BStart.Dispose();
            signatureLabel.Dispose();
            titleLabel.Dispose();
            g.Start();
        }

        //Konec hry
        private void EndGame(object sender, EndGameEventArgs e)
        {
            KeyUp -= KeyReleased;
            KeyDown -= KeyPressed;
            GameEnded -= EndGame;

            BAgain.Show();
            BQuit.Show();
            finalScoreLabel.Text = "Score: " + e.Score;
            finalScoreLabel.Show();

            if (e.Win)
            {
                winLabel1.Show();
                winLabel2.Show();
            }
            else
            {
                loseLabel.Show();
            }
        }

        //Nový začátek hry
        private void BAgain_Click(object sender, EventArgs e)
        {
            KeyDown += KeyPressed;
            KeyUp += KeyReleased;
            GameEnded += EndGame;
            BAgain.Hide();
            BQuit.Hide();
            loseLabel.Hide();
            winLabel1.Hide();
            winLabel2.Hide();
            finalScoreLabel.Hide();

            Focus();
            g.Start();
        }

        //Konec hry
        private void BQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    public class EndGameEventArgs : EventArgs
    {
        public int Score { get; set; }
        public bool Win { get; set; }
    }
 }
