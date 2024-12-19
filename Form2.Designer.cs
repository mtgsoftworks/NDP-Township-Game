namespace NDP_Township_GAME
{
    partial class Form2
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
            this.components = new System.ComponentModel.Container();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lbl_Playername = new System.Windows.Forms.Label();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.Location = new System.Drawing.Point(55, 37);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(160, 25);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "Kalan Süre (Sn):";
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(376, 37);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(64, 25);
            this.lblScore.TabIndex = 2;
            this.lblScore.Text = "Puan:";
            // 
            // lbl_Playername
            // 
            this.lbl_Playername.AutoSize = true;
            this.lbl_Playername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Playername.Location = new System.Drawing.Point(641, 37);
            this.lbl_Playername.Name = "lbl_Playername";
            this.lbl_Playername.Size = new System.Drawing.Size(87, 25);
            this.lbl_Playername.TabIndex = 3;
            this.lbl_Playername.Text = "Oyuncu:";
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 500;
            this.gameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 540);
            this.Controls.Add(this.lbl_Playername);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblTime);
            this.Name = "Form2";
            this.Text = "GAME";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lbl_Playername;
        private System.Windows.Forms.Timer gameTimer;
    }
}