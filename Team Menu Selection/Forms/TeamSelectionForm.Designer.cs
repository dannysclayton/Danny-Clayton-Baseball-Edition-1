namespace TeamMenuSelection.Forms
{
    partial class TeamSelectionForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.listBoxTeamA = new System.Windows.Forms.ListBox();
            this.listBoxTeamB = new System.Windows.Forms.ListBox();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxTeamA
            // 
            this.listBoxTeamA.FormattingEnabled = true;
            this.listBoxTeamA.ItemHeight = 20;
            this.listBoxTeamA.Location = new System.Drawing.Point(30, 30);
            this.listBoxTeamA.Name = "listBoxTeamA";
            this.listBoxTeamA.Size = new System.Drawing.Size(120, 124);
            this.listBoxTeamA.TabIndex = 0;
            // 
            // listBoxTeamB
            // 
            this.listBoxTeamB.FormattingEnabled = true;
            this.listBoxTeamB.ItemHeight = 20;
            this.listBoxTeamB.Location = new System.Drawing.Point(170, 30);
            this.listBoxTeamB.Name = "listBoxTeamB";
            this.listBoxTeamB.Size = new System.Drawing.Size(120, 124);
            this.listBoxTeamB.TabIndex = 1;
            // 
            // btnStartGame
            // 
            this.btnStartGame.Location = new System.Drawing.Point(90, 170);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(120, 40);
            this.btnStartGame.TabIndex = 2;
            this.btnStartGame.Text = "Start Game";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // TeamSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 240);
            this.Controls.Add(this.listBoxTeamA);
            this.Controls.Add(this.listBoxTeamB);
            this.Controls.Add(this.btnStartGame);
            this.Name = "TeamSelectionForm";
            this.Text = "Select Teams";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxTeamA;
        private System.Windows.Forms.ListBox listBoxTeamB;
        private System.Windows.Forms.Button btnStartGame;
    }
}
