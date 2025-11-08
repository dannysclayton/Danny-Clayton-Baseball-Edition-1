namespace TeamMenuSelection.Forms
{
    partial class MainMenuForm
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

        private void InitializeComponent()
        {
            this.btnExhibition = new System.Windows.Forms.Button();
            this.btnSeason = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnExhibition
            // 
            this.btnExhibition.Location = new System.Drawing.Point(50, 30);
            this.btnExhibition.Name = "btnExhibition";
            this.btnExhibition.Size = new System.Drawing.Size(200, 40);
            this.btnExhibition.TabIndex = 0;
            this.btnExhibition.Text = "Start Exhibition Game";
            this.btnExhibition.UseVisualStyleBackColor = true;
            this.btnExhibition.Click += new System.EventHandler(this.btnExhibition_Click);
            // 
            // btnSeason
            // 
            this.btnSeason.Location = new System.Drawing.Point(50, 80);
            this.btnSeason.Name = "btnSeason";
            this.btnSeason.Size = new System.Drawing.Size(200, 40);
            this.btnSeason.TabIndex = 1;
            this.btnSeason.Text = "Season";
            this.btnSeason.UseVisualStyleBackColor = true;
            this.btnSeason.Click += new System.EventHandler(this.btnSeason_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(50, 130);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(200, 40);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(50, 180);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(200, 40);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // MainMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 260);
            this.Controls.Add(this.btnExhibition);
            this.Controls.Add(this.btnSeason);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnExit);
            this.Name = "MainMenuForm";
            this.Text = "Baseball Sim - Main Menu";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnExhibition;
        private System.Windows.Forms.Button btnSeason;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnExit;
    }
}
