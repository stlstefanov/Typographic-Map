namespace XMLOutputTester
{
    partial class MapBuilderDlg
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
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtFactor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.cbDebug = new System.Windows.Forms.CheckBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(229, 419);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(94, 24);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save to .svg";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSaveSvg_Click);
            // 
            // txtFactor
            // 
            this.txtFactor.Location = new System.Drawing.Point(77, 10);
            this.txtFactor.Margin = new System.Windows.Forms.Padding(1);
            this.txtFactor.Name = "txtFactor";
            this.txtFactor.Size = new System.Drawing.Size(131, 20);
            this.txtFactor.TabIndex = 5;
            this.txtFactor.Text = "0.65";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Text factor:";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRun.Location = new System.Drawing.Point(129, 419);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(94, 24);
            this.btnRun.TabIndex = 7;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.BtnRun_Click);
            // 
            // cbDebug
            // 
            this.cbDebug.AutoSize = true;
            this.cbDebug.Location = new System.Drawing.Point(77, 34);
            this.cbDebug.Name = "cbDebug";
            this.cbDebug.Size = new System.Drawing.Size(58, 17);
            this.cbDebug.TabIndex = 8;
            this.cbDebug.Text = "Debug";
            this.cbDebug.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 390);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(311, 23);
            this.progressBar.TabIndex = 9;
            // 
            // MapBuilderDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 455);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.cbDebug);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtFactor);
            this.Name = "MapBuilderDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Typographic Map Builder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtFactor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.CheckBox cbDebug;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

