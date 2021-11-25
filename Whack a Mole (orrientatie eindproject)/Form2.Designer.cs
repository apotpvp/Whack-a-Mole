
namespace Whack_a_Mole__orrientatie_eindproject_
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
            this.form2label1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbScoredisplay = new System.Windows.Forms.Label();
            this.btnRestart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // form2label1
            // 
            this.form2label1.AutoSize = true;
            this.form2label1.Location = new System.Drawing.Point(336, 127);
            this.form2label1.Name = "form2label1";
            this.form2label1.Size = new System.Drawing.Size(84, 17);
            this.form2label1.TabIndex = 0;
            this.form2label1.Text = "Game Over!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 213);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Score:";
            // 
            // lbScoredisplay
            // 
            this.lbScoredisplay.AutoSize = true;
            this.lbScoredisplay.Location = new System.Drawing.Point(373, 213);
            this.lbScoredisplay.Name = "lbScoredisplay";
            this.lbScoredisplay.Size = new System.Drawing.Size(16, 17);
            this.lbScoredisplay.TabIndex = 2;
            this.lbScoredisplay.Text = "0";
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(345, 328);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(75, 23);
            this.btnRestart.TabIndex = 4;
            this.btnRestart.Text = "restart";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.lbScoredisplay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.form2label1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label form2label1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbScoredisplay;
        private System.Windows.Forms.Button btnRestart;
    }
}