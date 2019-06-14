namespace Plan2Ext.AutoIdVergabeOeff
{
    partial class MyUserControl
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
            this.grpFenster = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFenNummer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFenPrefix = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.grpTuer = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTuerNummer = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTuerPrefix = new System.Windows.Forms.TextBox();
            this.grpFenster.SuspendLayout();
            this.grpTuer.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpFenster
            // 
            this.grpFenster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFenster.Controls.Add(this.label2);
            this.grpFenster.Controls.Add(this.txtFenNummer);
            this.grpFenster.Controls.Add(this.label1);
            this.grpFenster.Controls.Add(this.txtFenPrefix);
            this.grpFenster.Location = new System.Drawing.Point(3, 3);
            this.grpFenster.Name = "grpFenster";
            this.grpFenster.Size = new System.Drawing.Size(209, 81);
            this.grpFenster.TabIndex = 0;
            this.grpFenster.TabStop = false;
            this.grpFenster.Text = "Fenster";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Nummer";
            // 
            // txtFenNummer
            // 
            this.txtFenNummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFenNummer.Location = new System.Drawing.Point(73, 45);
            this.txtFenNummer.Name = "txtFenNummer";
            this.txtFenNummer.Size = new System.Drawing.Size(130, 20);
            this.txtFenNummer.TabIndex = 3;
            this.txtFenNummer.Text = "1";
            this.txtFenNummer.TextChanged += new System.EventHandler(this.txtFenNummer_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Prefix";
            // 
            // txtFenPrefix
            // 
            this.txtFenPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFenPrefix.Location = new System.Drawing.Point(73, 19);
            this.txtFenPrefix.Name = "txtFenPrefix";
            this.txtFenPrefix.Size = new System.Drawing.Size(130, 20);
            this.txtFenPrefix.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(137, 177);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // grpTuer
            // 
            this.grpTuer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTuer.Controls.Add(this.label3);
            this.grpTuer.Controls.Add(this.txtTuerNummer);
            this.grpTuer.Controls.Add(this.label4);
            this.grpTuer.Controls.Add(this.txtTuerPrefix);
            this.grpTuer.Location = new System.Drawing.Point(3, 90);
            this.grpTuer.Name = "grpTuer";
            this.grpTuer.Size = new System.Drawing.Size(209, 81);
            this.grpTuer.TabIndex = 4;
            this.grpTuer.TabStop = false;
            this.grpTuer.Text = "Türen";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Nummer";
            // 
            // txtTuerNummer
            // 
            this.txtTuerNummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTuerNummer.Location = new System.Drawing.Point(73, 45);
            this.txtTuerNummer.Name = "txtTuerNummer";
            this.txtTuerNummer.Size = new System.Drawing.Size(130, 20);
            this.txtTuerNummer.TabIndex = 3;
            this.txtTuerNummer.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Prefix";
            // 
            // txtTuerPrefix
            // 
            this.txtTuerPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTuerPrefix.Location = new System.Drawing.Point(73, 19);
            this.txtTuerPrefix.Name = "txtTuerPrefix";
            this.txtTuerPrefix.Size = new System.Drawing.Size(130, 20);
            this.txtTuerPrefix.TabIndex = 1;
            // 
            // MyUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpTuer);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.grpFenster);
            this.Name = "MyUserControl";
            this.Size = new System.Drawing.Size(215, 449);
            this.grpFenster.ResumeLayout(false);
            this.grpFenster.PerformLayout();
            this.grpTuer.ResumeLayout(false);
            this.grpTuer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpFenster;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txtFenNummer;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox txtFenPrefix;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox grpTuer;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.TextBox txtTuerNummer;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.TextBox txtTuerPrefix;
    }
}
