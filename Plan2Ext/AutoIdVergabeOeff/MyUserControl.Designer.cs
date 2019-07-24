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
            this.btnStart = new System.Windows.Forms.Button();
            this.grpTuer = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTuerNummer = new System.Windows.Forms.TextBox();
            this.btnStartTueren = new System.Windows.Forms.Button();
            this.btnStartAlle = new System.Windows.Forms.Button();
            this.grpExamine = new System.Windows.Forms.GroupBox();
            this.btnEindeutigkeit = new System.Windows.Forms.Button();
            this.btnEindeutigkeitTuer = new System.Windows.Forms.Button();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.grpFenster.SuspendLayout();
            this.grpTuer.SuspendLayout();
            this.grpExamine.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpFenster
            // 
            this.grpFenster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFenster.Controls.Add(this.label2);
            this.grpFenster.Controls.Add(this.txtFenNummer);
            this.grpFenster.Location = new System.Drawing.Point(3, 3);
            this.grpFenster.Name = "grpFenster";
            this.grpFenster.Size = new System.Drawing.Size(242, 53);
            this.grpFenster.TabIndex = 0;
            this.grpFenster.TabStop = false;
            this.grpFenster.Text = "Fenster";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Nummer";
            // 
            // txtFenNummer
            // 
            this.txtFenNummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFenNummer.Location = new System.Drawing.Point(73, 18);
            this.txtFenNummer.Name = "txtFenNummer";
            this.txtFenNummer.Size = new System.Drawing.Size(163, 20);
            this.txtFenNummer.TabIndex = 1;
            this.txtFenNummer.Text = "1";
            this.txtFenNummer.TextChanged += new System.EventHandler(this.txtFenNummer_TextChanged);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(2, 160);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start Fenster";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // grpTuer
            // 
            this.grpTuer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTuer.Controls.Add(this.label3);
            this.grpTuer.Controls.Add(this.txtTuerNummer);
            this.grpTuer.Location = new System.Drawing.Point(3, 62);
            this.grpTuer.Name = "grpTuer";
            this.grpTuer.Size = new System.Drawing.Size(242, 53);
            this.grpTuer.TabIndex = 4;
            this.grpTuer.TabStop = false;
            this.grpTuer.Text = "Türen";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Nummer";
            // 
            // txtTuerNummer
            // 
            this.txtTuerNummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTuerNummer.Location = new System.Drawing.Point(73, 19);
            this.txtTuerNummer.Name = "txtTuerNummer";
            this.txtTuerNummer.Size = new System.Drawing.Size(163, 20);
            this.txtTuerNummer.TabIndex = 3;
            this.txtTuerNummer.Text = "1";
            // 
            // btnStartTueren
            // 
            this.btnStartTueren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartTueren.Location = new System.Drawing.Point(83, 160);
            this.btnStartTueren.Name = "btnStartTueren";
            this.btnStartTueren.Size = new System.Drawing.Size(75, 23);
            this.btnStartTueren.TabIndex = 7;
            this.btnStartTueren.Text = "Start Türen";
            this.btnStartTueren.UseVisualStyleBackColor = true;
            this.btnStartTueren.Click += new System.EventHandler(this.btnStartTueren_Click);
            // 
            // btnStartAlle
            // 
            this.btnStartAlle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartAlle.Location = new System.Drawing.Point(164, 160);
            this.btnStartAlle.Name = "btnStartAlle";
            this.btnStartAlle.Size = new System.Drawing.Size(75, 23);
            this.btnStartAlle.TabIndex = 8;
            this.btnStartAlle.Text = "Start alle";
            this.btnStartAlle.UseVisualStyleBackColor = true;
            this.btnStartAlle.Click += new System.EventHandler(this.btnStartAlle_Click);
            // 
            // grpExamine
            // 
            this.grpExamine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExamine.Controls.Add(this.btnEindeutigkeitTuer);
            this.grpExamine.Controls.Add(this.btnEindeutigkeit);
            this.grpExamine.Location = new System.Drawing.Point(3, 194);
            this.grpExamine.Name = "grpExamine";
            this.grpExamine.Size = new System.Drawing.Size(242, 52);
            this.grpExamine.TabIndex = 13;
            this.grpExamine.TabStop = false;
            this.grpExamine.Text = "Eindeutigkeit prüfen";
            // 
            // btnEindeutigkeit
            // 
            this.btnEindeutigkeit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEindeutigkeit.Location = new System.Drawing.Point(80, 19);
            this.btnEindeutigkeit.Name = "btnEindeutigkeit";
            this.btnEindeutigkeit.Size = new System.Drawing.Size(75, 23);
            this.btnEindeutigkeit.TabIndex = 9;
            this.btnEindeutigkeit.Text = "Fenster";
            this.btnEindeutigkeit.UseVisualStyleBackColor = true;
            this.btnEindeutigkeit.Click += new System.EventHandler(this.btnEindeutigkeit_Click);
            // 
            // btnEindeutigkeitTuer
            // 
            this.btnEindeutigkeitTuer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEindeutigkeitTuer.Location = new System.Drawing.Point(161, 19);
            this.btnEindeutigkeitTuer.Name = "btnEindeutigkeitTuer";
            this.btnEindeutigkeitTuer.Size = new System.Drawing.Size(75, 23);
            this.btnEindeutigkeitTuer.TabIndex = 10;
            this.btnEindeutigkeitTuer.Text = "Türen";
            this.btnEindeutigkeitTuer.UseVisualStyleBackColor = true;
            this.btnEindeutigkeitTuer.Click += new System.EventHandler(this.btnEindeutigkeitTuer_Click);
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(9, 126);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(33, 13);
            this.lblPrefix.TabIndex = 14;
            this.lblPrefix.Text = "Prefix";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPrefix.Location = new System.Drawing.Point(76, 123);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(163, 20);
            this.txtPrefix.TabIndex = 5;
            // 
            // MyUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblPrefix);
            this.Controls.Add(this.txtPrefix);
            this.Controls.Add(this.grpExamine);
            this.Controls.Add(this.btnStartAlle);
            this.Controls.Add(this.btnStartTueren);
            this.Controls.Add(this.grpTuer);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.grpFenster);
            this.MinimumSize = new System.Drawing.Size(248, 306);
            this.Name = "MyUserControl";
            this.Size = new System.Drawing.Size(248, 306);
            this.grpFenster.ResumeLayout(false);
            this.grpFenster.PerformLayout();
            this.grpTuer.ResumeLayout(false);
            this.grpTuer.PerformLayout();
            this.grpExamine.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpFenster;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txtFenNummer;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox grpTuer;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.TextBox txtTuerNummer;
        private System.Windows.Forms.Button btnStartTueren;
        private System.Windows.Forms.Button btnStartAlle;
        private System.Windows.Forms.GroupBox grpExamine;
        private System.Windows.Forms.Button btnEindeutigkeit;
        private System.Windows.Forms.Button btnEindeutigkeitTuer;
        private System.Windows.Forms.Label lblPrefix;
        internal System.Windows.Forms.TextBox txtPrefix;
    }
}
