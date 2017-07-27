namespace Plan2Ext.Nummerierung
{
    partial class NrControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpManually = new System.Windows.Forms.GroupBox();
            this.btnHBlock = new System.Windows.Forms.Button();
            this.txtHBlockname = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAttName = new System.Windows.Forms.TextBox();
            this.btnSelectBlock = new System.Windows.Forms.Button();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpFbHoehe = new System.Windows.Forms.GroupBox();
            this.btnFbhWithoutNr = new System.Windows.Forms.Button();
            this.btnFbhWithNr = new System.Windows.Forms.Button();
            this.grNumber = new System.Windows.Forms.GroupBox();
            this.btnSelectTop = new System.Windows.Forms.Button();
            this.chkAutoCorr = new System.Windows.Forms.CheckBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.lblNumber = new System.Windows.Forms.Label();
            this.txtSeparator = new System.Windows.Forms.TextBox();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.txtTop = new System.Windows.Forms.TextBox();
            this.lblTop = new System.Windows.Forms.Label();
            this.grpManually.SuspendLayout();
            this.grpFbHoehe.SuspendLayout();
            this.grNumber.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpManually
            // 
            this.grpManually.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpManually.Controls.Add(this.btnHBlock);
            this.grpManually.Controls.Add(this.txtHBlockname);
            this.grpManually.Controls.Add(this.label3);
            this.grpManually.Controls.Add(this.label2);
            this.grpManually.Controls.Add(this.txtAttName);
            this.grpManually.Controls.Add(this.btnSelectBlock);
            this.grpManually.Controls.Add(this.txtBlockname);
            this.grpManually.Controls.Add(this.label1);
            this.grpManually.Location = new System.Drawing.Point(3, 3);
            this.grpManually.Name = "grpManually";
            this.grpManually.Size = new System.Drawing.Size(178, 106);
            this.grpManually.TabIndex = 11;
            this.grpManually.TabStop = false;
            this.grpManually.Text = "Manuell";
            // 
            // btnHBlock
            // 
            this.btnHBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHBlock.Location = new System.Drawing.Point(148, 71);
            this.btnHBlock.Name = "btnHBlock";
            this.btnHBlock.Size = new System.Drawing.Size(24, 20);
            this.btnHBlock.TabIndex = 35;
            this.btnHBlock.Text = "...";
            this.btnHBlock.UseVisualStyleBackColor = true;
            this.btnHBlock.Click += new System.EventHandler(this.btnSelectHBlock_Click);
            // 
            // txtHBlockname
            // 
            this.txtHBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHBlockname.Enabled = false;
            this.txtHBlockname.Location = new System.Drawing.Point(72, 71);
            this.txtHBlockname.Name = "txtHBlockname";
            this.txtHBlockname.Size = new System.Drawing.Size(70, 20);
            this.txtHBlockname.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Höhenblock";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Attributname";
            // 
            // txtAttName
            // 
            this.txtAttName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAttName.Enabled = false;
            this.txtAttName.Location = new System.Drawing.Point(72, 45);
            this.txtAttName.Name = "txtAttName";
            this.txtAttName.Size = new System.Drawing.Size(70, 20);
            this.txtAttName.TabIndex = 31;
            // 
            // btnSelectBlock
            // 
            this.btnSelectBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBlock.Location = new System.Drawing.Point(148, 19);
            this.btnSelectBlock.Name = "btnSelectBlock";
            this.btnSelectBlock.Size = new System.Drawing.Size(24, 20);
            this.btnSelectBlock.TabIndex = 30;
            this.btnSelectBlock.Text = "...";
            this.btnSelectBlock.UseVisualStyleBackColor = true;
            this.btnSelectBlock.Click += new System.EventHandler(this.btnSelectBlock_Click);
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Enabled = false;
            this.txtBlockname.Location = new System.Drawing.Point(72, 19);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(70, 20);
            this.txtBlockname.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Blockname";
            // 
            // grpFbHoehe
            // 
            this.grpFbHoehe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFbHoehe.Controls.Add(this.btnFbhWithoutNr);
            this.grpFbHoehe.Controls.Add(this.btnFbhWithNr);
            this.grpFbHoehe.Location = new System.Drawing.Point(3, 115);
            this.grpFbHoehe.Name = "grpFbHoehe";
            this.grpFbHoehe.Size = new System.Drawing.Size(178, 85);
            this.grpFbHoehe.TabIndex = 10;
            this.grpFbHoehe.TabStop = false;
            this.grpFbHoehe.Text = "Fußbodenhöhenblock";
            // 
            // btnFbhWithoutNr
            // 
            this.btnFbhWithoutNr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFbhWithoutNr.Location = new System.Drawing.Point(6, 48);
            this.btnFbhWithoutNr.Name = "btnFbhWithoutNr";
            this.btnFbhWithoutNr.Size = new System.Drawing.Size(166, 23);
            this.btnFbhWithoutNr.TabIndex = 9;
            this.btnFbhWithoutNr.Text = "Ohne Nummer";
            this.btnFbhWithoutNr.UseVisualStyleBackColor = true;
            this.btnFbhWithoutNr.Click += new System.EventHandler(this.btnFbhWithoutNr_Click);
            // 
            // btnFbhWithNr
            // 
            this.btnFbhWithNr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFbhWithNr.Location = new System.Drawing.Point(6, 19);
            this.btnFbhWithNr.Name = "btnFbhWithNr";
            this.btnFbhWithNr.Size = new System.Drawing.Size(166, 23);
            this.btnFbhWithNr.TabIndex = 8;
            this.btnFbhWithNr.Text = "Mit Nummer";
            this.btnFbhWithNr.UseVisualStyleBackColor = true;
            this.btnFbhWithNr.Click += new System.EventHandler(this.btnFbhWithNr_Click);
            // 
            // grNumber
            // 
            this.grNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grNumber.Controls.Add(this.btnSelectTop);
            this.grNumber.Controls.Add(this.chkAutoCorr);
            this.grNumber.Controls.Add(this.btnStart);
            this.grNumber.Controls.Add(this.txtNumber);
            this.grNumber.Controls.Add(this.lblNumber);
            this.grNumber.Controls.Add(this.txtSeparator);
            this.grNumber.Controls.Add(this.lblSeparator);
            this.grNumber.Controls.Add(this.txtTop);
            this.grNumber.Controls.Add(this.lblTop);
            this.grNumber.Location = new System.Drawing.Point(0, 202);
            this.grNumber.Name = "grNumber";
            this.grNumber.Size = new System.Drawing.Size(181, 155);
            this.grNumber.TabIndex = 9;
            this.grNumber.TabStop = false;
            this.grNumber.Text = "Zuordnen";
            // 
            // btnSelectTop
            // 
            this.btnSelectTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectTop.Location = new System.Drawing.Point(151, 19);
            this.btnSelectTop.Name = "btnSelectTop";
            this.btnSelectTop.Size = new System.Drawing.Size(24, 20);
            this.btnSelectTop.TabIndex = 31;
            this.btnSelectTop.Text = "...";
            this.btnSelectTop.UseVisualStyleBackColor = true;
            this.btnSelectTop.Click += new System.EventHandler(this.btnSelectTop_Click);
            // 
            // chkAutoCorr
            // 
            this.chkAutoCorr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoCorr.AutoSize = true;
            this.chkAutoCorr.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAutoCorr.Checked = true;
            this.chkAutoCorr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoCorr.Location = new System.Drawing.Point(85, 98);
            this.chkAutoCorr.Name = "chkAutoCorr";
            this.chkAutoCorr.Size = new System.Drawing.Size(90, 17);
            this.chkAutoCorr.TabIndex = 7;
            this.chkAutoCorr.Text = "Autokorrektur";
            this.chkAutoCorr.UseVisualStyleBackColor = true;
            this.chkAutoCorr.CheckedChanged += new System.EventHandler(this.chkAutoCorr_CheckedChanged);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(6, 121);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(169, 23);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtNumber
            // 
            this.txtNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNumber.Location = new System.Drawing.Point(110, 72);
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(35, 20);
            this.txtNumber.TabIndex = 6;
            this.txtNumber.TextChanged += new System.EventHandler(this.txtNumber_TextChanged);
            this.txtNumber.Validating += new System.ComponentModel.CancelEventHandler(this.txtNumber_Validating);
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(6, 75);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(46, 13);
            this.lblNumber.TabIndex = 5;
            this.lblNumber.Text = "Nummer";
            // 
            // txtSeparator
            // 
            this.txtSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSeparator.Location = new System.Drawing.Point(110, 46);
            this.txtSeparator.Name = "txtSeparator";
            this.txtSeparator.Size = new System.Drawing.Size(35, 20);
            this.txtSeparator.TabIndex = 4;
            this.txtSeparator.TextChanged += new System.EventHandler(this.txtSeparator_TextChanged);
            // 
            // lblSeparator
            // 
            this.lblSeparator.AutoSize = true;
            this.lblSeparator.Location = new System.Drawing.Point(6, 49);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Size = new System.Drawing.Size(72, 13);
            this.lblSeparator.TabIndex = 3;
            this.lblSeparator.Text = "Trennzeichen";
            // 
            // txtTop
            // 
            this.txtTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTop.Location = new System.Drawing.Point(80, 19);
            this.txtTop.Name = "txtTop";
            this.txtTop.Size = new System.Drawing.Size(65, 20);
            this.txtTop.TabIndex = 2;
            this.txtTop.TextChanged += new System.EventHandler(this.txtTop_TextChanged);
            // 
            // lblTop
            // 
            this.lblTop.AutoSize = true;
            this.lblTop.Location = new System.Drawing.Point(6, 22);
            this.lblTop.Name = "lblTop";
            this.lblTop.Size = new System.Drawing.Size(26, 13);
            this.lblTop.TabIndex = 1;
            this.lblTop.Text = "Top";
            // 
            // NrControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpManually);
            this.Controls.Add(this.grpFbHoehe);
            this.Controls.Add(this.grNumber);
            this.Name = "NrControl";
            this.Size = new System.Drawing.Size(184, 712);
            this.Load += new System.EventHandler(this.NrControl_Load);
            this.grpManually.ResumeLayout(false);
            this.grpManually.PerformLayout();
            this.grpFbHoehe.ResumeLayout(false);
            this.grNumber.ResumeLayout(false);
            this.grNumber.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpManually;
        private System.Windows.Forms.Button btnHBlock;
        internal System.Windows.Forms.TextBox txtHBlockname;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txtAttName;
        private System.Windows.Forms.Button btnSelectBlock;
        internal System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpFbHoehe;
        private System.Windows.Forms.Button btnFbhWithoutNr;
        private System.Windows.Forms.Button btnFbhWithNr;
        private System.Windows.Forms.GroupBox grNumber;
        private System.Windows.Forms.Button btnSelectTop;
        internal System.Windows.Forms.CheckBox chkAutoCorr;
        private System.Windows.Forms.Button btnStart;
        internal System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Label lblNumber;
        internal System.Windows.Forms.TextBox txtSeparator;
        private System.Windows.Forms.Label lblSeparator;
        internal System.Windows.Forms.TextBox txtTop;
        private System.Windows.Forms.Label lblTop;
    }
}
