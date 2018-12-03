namespace Locomotion
{
    partial class Interface
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.board = new System.Windows.Forms.Panel();
            this.BStart = new System.Windows.Forms.Button();
            this.loseLabel = new System.Windows.Forms.Label();
            this.BAgain = new System.Windows.Forms.Button();
            this.BQuit = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.signatureLabel = new System.Windows.Forms.Label();
            this.finalScoreLabel = new System.Windows.Forms.Label();
            this.winLabel1 = new System.Windows.Forms.Label();
            this.winLabel2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // board
            // 
            this.board.Location = new System.Drawing.Point(12, 12);
            this.board.Name = "board";
            this.board.Size = new System.Drawing.Size(42, 35);
            this.board.TabIndex = 0;
            this.board.Visible = false;
            // 
            // BStart
            // 
            this.BStart.Location = new System.Drawing.Point(439, 529);
            this.BStart.Name = "BStart";
            this.BStart.Size = new System.Drawing.Size(331, 84);
            this.BStart.TabIndex = 1;
            this.BStart.Text = "START";
            this.BStart.UseVisualStyleBackColor = true;
            this.BStart.Click += new System.EventHandler(this.BStart_Click);
            // 
            // loseLabel
            // 
            this.loseLabel.AutoSize = true;
            this.loseLabel.Font = new System.Drawing.Font("Courier New", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.loseLabel.Location = new System.Drawing.Point(345, 193);
            this.loseLabel.Name = "loseLabel";
            this.loseLabel.Size = new System.Drawing.Size(490, 94);
            this.loseLabel.TabIndex = 2;
            this.loseLabel.Text = "GAME OVER";
            this.loseLabel.Visible = false;
            // 
            // BAgain
            // 
            this.BAgain.Location = new System.Drawing.Point(239, 410);
            this.BAgain.Name = "BAgain";
            this.BAgain.Size = new System.Drawing.Size(270, 113);
            this.BAgain.TabIndex = 3;
            this.BAgain.Text = "Try again";
            this.BAgain.UseVisualStyleBackColor = true;
            this.BAgain.Visible = false;
            this.BAgain.Click += new System.EventHandler(this.BAgain_Click);
            // 
            // BQuit
            // 
            this.BQuit.Location = new System.Drawing.Point(702, 410);
            this.BQuit.Name = "BQuit";
            this.BQuit.Size = new System.Drawing.Size(270, 113);
            this.BQuit.TabIndex = 4;
            this.BQuit.Text = "Quit";
            this.BQuit.UseVisualStyleBackColor = true;
            this.BQuit.Visible = false;
            this.BQuit.Click += new System.EventHandler(this.BQuit_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Courier New", 80F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.titleLabel.Location = new System.Drawing.Point(151, 193);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(860, 144);
            this.titleLabel.TabIndex = 5;
            this.titleLabel.Text = "LOCOMOTION";
            // 
            // signatureLabel
            // 
            this.signatureLabel.AutoSize = true;
            this.signatureLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.signatureLabel.Location = new System.Drawing.Point(533, 350);
            this.signatureLabel.Name = "signatureLabel";
            this.signatureLabel.Size = new System.Drawing.Size(131, 18);
            this.signatureLabel.TabIndex = 6;
            this.signatureLabel.Text = "2018 Václav Luňák";
            // 
            // finalScoreLabel
            // 
            this.finalScoreLabel.AutoSize = true;
            this.finalScoreLabel.Location = new System.Drawing.Point(560, 333);
            this.finalScoreLabel.Name = "finalScoreLabel";
            this.finalScoreLabel.Size = new System.Drawing.Size(49, 17);
            this.finalScoreLabel.TabIndex = 7;
            this.finalScoreLabel.Text = "Score:";
            this.finalScoreLabel.Visible = false;
            // 
            // winLabel1
            // 
            this.winLabel1.AutoSize = true;
            this.winLabel1.Font = new System.Drawing.Font("Courier New", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.winLabel1.Location = new System.Drawing.Point(214, 141);
            this.winLabel1.Name = "winLabel1";
            this.winLabel1.Size = new System.Drawing.Size(840, 94);
            this.winLabel1.TabIndex = 8;
            this.winLabel1.Text = "CONGRATULATIONS!";
            this.winLabel1.Visible = false;
            // 
            // winLabel2
            // 
            this.winLabel2.AutoSize = true;
            this.winLabel2.Font = new System.Drawing.Font("Courier New", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.winLabel2.Location = new System.Drawing.Point(399, 233);
            this.winLabel2.Name = "winLabel2";
            this.winLabel2.Size = new System.Drawing.Size(390, 94);
            this.winLabel2.TabIndex = 9;
            this.winLabel2.Text = "YOU WIN";
            this.winLabel2.Visible = false;
            // 
            // Interface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 712);
            this.Controls.Add(this.winLabel2);
            this.Controls.Add(this.winLabel1);
            this.Controls.Add(this.finalScoreLabel);
            this.Controls.Add(this.signatureLabel);
            this.Controls.Add(this.BQuit);
            this.Controls.Add(this.BAgain);
            this.Controls.Add(this.BStart);
            this.Controls.Add(this.board);
            this.Controls.Add(this.loseLabel);
            this.Controls.Add(this.titleLabel);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1228, 759);
            this.MinimumSize = new System.Drawing.Size(1228, 759);
            this.Name = "Interface";
            this.Text = "Locomotion";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel board;
        private System.Windows.Forms.Button BStart;
        private System.Windows.Forms.Label loseLabel;
        private System.Windows.Forms.Button BAgain;
        private System.Windows.Forms.Button BQuit;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label signatureLabel;
        private System.Windows.Forms.Label finalScoreLabel;
        private System.Windows.Forms.Label winLabel1;
        private System.Windows.Forms.Label winLabel2;
    }
}

