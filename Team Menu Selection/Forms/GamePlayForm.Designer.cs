namespace TeamMenuSelection.Forms
{
    partial class GamePlayForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblMatchup;

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
            this.lblMatchup = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblMatchup
            // 
            this.lblMatchup.AutoSize = true;
            this.lblMatchup.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblMatchup.Location = new System.Drawing.Point(30, 30);
            this.lblMatchup.Name = "lblMatchup";
            this.lblMatchup.Size = new System.Drawing.Size(144, 32);
            this.lblMatchup.TabIndex = 0;
            this.lblMatchup.Text = "Matchup ...";
            // 
            // GamePlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.lblMatchup);
            this.Name = "GamePlayForm";
            this.Text = "Game Play";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}
