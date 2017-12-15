namespace Plan2Ext.Fenster
{
    partial class FensterOptionsControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblWidth = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.btnSelWidth = new System.Windows.Forms.Button();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.butSelHeight = new System.Windows.Forms.Button();
            this.lblParapet = new System.Windows.Forms.Label();
            this.txtParapet = new System.Windows.Forms.TextBox();
            this.btnSelParapet = new System.Windows.Forms.Button();
            this.lblOberlichte = new System.Windows.Forms.Label();
            this.txtOlAb = new System.Windows.Forms.TextBox();
            this.lblStaerke = new System.Windows.Forms.Label();
            this.txtStaerke = new System.Windows.Forms.TextBox();
            this.lblStock = new System.Windows.Forms.Label();
            this.txtStock = new System.Windows.Forms.TextBox();
            this.txtSprossenBreite = new System.Windows.Forms.TextBox();
            this.lblSprossenBreite = new System.Windows.Forms.Label();
            this.btnSelAbstand = new System.Windows.Forms.Button();
            this.txtAbstand = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFluegelStaerke = new System.Windows.Forms.TextBox();
            this.lblFluegelStaerke = new System.Windows.Forms.Label();
            this.grpFenster = new System.Windows.Forms.GroupBox();
            this.rbnKasten = new System.Windows.Forms.RadioButton();
            this.rbnStandard = new System.Windows.Forms.RadioButton();
            this.grpSprossen = new System.Windows.Forms.GroupBox();
            this.rbnSprosse2 = new System.Windows.Forms.RadioButton();
            this.rbnSprosse1 = new System.Windows.Forms.RadioButton();
            this.rbnSprosse0 = new System.Windows.Forms.RadioButton();
            this.btnFenster = new System.Windows.Forms.Button();
            this.btnExamine = new System.Windows.Forms.Button();
            this.txtWeiteTol = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.grpFenster.SuspendLayout();
            this.grpSprossen.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(3, 86);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(34, 13);
            this.lblWidth.TabIndex = 0;
            this.lblWidth.Text = "Breite";
            // 
            // txtWidth
            // 
            this.txtWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWidth.Location = new System.Drawing.Point(95, 83);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(101, 20);
            this.txtWidth.TabIndex = 1;
            this.txtWidth.Validating += new System.ComponentModel.CancelEventHandler(this.txtWidth_Validating);
            // 
            // btnSelWidth
            // 
            this.btnSelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelWidth.Location = new System.Drawing.Point(202, 81);
            this.btnSelWidth.Name = "btnSelWidth";
            this.btnSelWidth.Size = new System.Drawing.Size(24, 22);
            this.btnSelWidth.TabIndex = 2;
            this.btnSelWidth.Text = "...";
            this.btnSelWidth.UseVisualStyleBackColor = true;
            this.btnSelWidth.Click += new System.EventHandler(this.btnSelWidth_Click);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(3, 112);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(33, 13);
            this.lblHeight.TabIndex = 3;
            this.lblHeight.Text = "Höhe";
            // 
            // txtHeight
            // 
            this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeight.Location = new System.Drawing.Point(95, 109);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(101, 20);
            this.txtHeight.TabIndex = 4;
            this.txtHeight.Validating += new System.ComponentModel.CancelEventHandler(this.txtHeight_Validating);
            // 
            // butSelHeight
            // 
            this.butSelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelHeight.Location = new System.Drawing.Point(202, 107);
            this.butSelHeight.Name = "butSelHeight";
            this.butSelHeight.Size = new System.Drawing.Size(24, 22);
            this.butSelHeight.TabIndex = 5;
            this.butSelHeight.Text = "...";
            this.butSelHeight.UseVisualStyleBackColor = true;
            this.butSelHeight.Click += new System.EventHandler(this.butSelHeight_Click);
            // 
            // lblParapet
            // 
            this.lblParapet.AutoSize = true;
            this.lblParapet.Location = new System.Drawing.Point(3, 138);
            this.lblParapet.Name = "lblParapet";
            this.lblParapet.Size = new System.Drawing.Size(44, 13);
            this.lblParapet.TabIndex = 6;
            this.lblParapet.Text = "Parapet";
            // 
            // txtParapet
            // 
            this.txtParapet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParapet.Location = new System.Drawing.Point(95, 135);
            this.txtParapet.Name = "txtParapet";
            this.txtParapet.Size = new System.Drawing.Size(101, 20);
            this.txtParapet.TabIndex = 7;
            this.txtParapet.Validating += new System.ComponentModel.CancelEventHandler(this.txtParapet_Validating);
            // 
            // btnSelParapet
            // 
            this.btnSelParapet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelParapet.Location = new System.Drawing.Point(202, 133);
            this.btnSelParapet.Name = "btnSelParapet";
            this.btnSelParapet.Size = new System.Drawing.Size(24, 22);
            this.btnSelParapet.TabIndex = 8;
            this.btnSelParapet.Text = "...";
            this.btnSelParapet.UseVisualStyleBackColor = true;
            this.btnSelParapet.Click += new System.EventHandler(this.btnSelParapet_Click);
            // 
            // lblOberlichte
            // 
            this.lblOberlichte.AutoSize = true;
            this.lblOberlichte.Location = new System.Drawing.Point(3, 380);
            this.lblOberlichte.Name = "lblOberlichte";
            this.lblOberlichte.Size = new System.Drawing.Size(77, 13);
            this.lblOberlichte.TabIndex = 17;
            this.lblOberlichte.Text = "Anzeige OL ab";
            // 
            // txtOlAb
            // 
            this.txtOlAb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOlAb.Location = new System.Drawing.Point(95, 377);
            this.txtOlAb.Name = "txtOlAb";
            this.txtOlAb.Size = new System.Drawing.Size(101, 20);
            this.txtOlAb.TabIndex = 18;
            this.txtOlAb.Validating += new System.ComponentModel.CancelEventHandler(this.txtOlAb_Validating);
            // 
            // lblStaerke
            // 
            this.lblStaerke.AutoSize = true;
            this.lblStaerke.Location = new System.Drawing.Point(3, 264);
            this.lblStaerke.Name = "lblStaerke";
            this.lblStaerke.Size = new System.Drawing.Size(71, 13);
            this.lblStaerke.TabIndex = 11;
            this.lblStaerke.Text = "Fensterstärke";
            // 
            // txtStaerke
            // 
            this.txtStaerke.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStaerke.Location = new System.Drawing.Point(95, 261);
            this.txtStaerke.Name = "txtStaerke";
            this.txtStaerke.Size = new System.Drawing.Size(101, 20);
            this.txtStaerke.TabIndex = 12;
            this.txtStaerke.Validating += new System.ComponentModel.CancelEventHandler(this.txtStaerke_Validating);
            // 
            // lblStock
            // 
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(3, 290);
            this.lblStock.Name = "lblStock";
            this.lblStock.Size = new System.Drawing.Size(35, 13);
            this.lblStock.TabIndex = 13;
            this.lblStock.Text = "Stock";
            // 
            // txtStock
            // 
            this.txtStock.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStock.Location = new System.Drawing.Point(95, 287);
            this.txtStock.Name = "txtStock";
            this.txtStock.Size = new System.Drawing.Size(101, 20);
            this.txtStock.TabIndex = 14;
            this.txtStock.Validating += new System.ComponentModel.CancelEventHandler(this.txtStock_Validating);
            // 
            // txtSprossenBreite
            // 
            this.txtSprossenBreite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSprossenBreite.Location = new System.Drawing.Point(95, 339);
            this.txtSprossenBreite.Name = "txtSprossenBreite";
            this.txtSprossenBreite.Size = new System.Drawing.Size(101, 20);
            this.txtSprossenBreite.TabIndex = 16;
            this.txtSprossenBreite.Validating += new System.ComponentModel.CancelEventHandler(this.txtSprossenBreite_Validating);
            // 
            // lblSprossenBreite
            // 
            this.lblSprossenBreite.AutoSize = true;
            this.lblSprossenBreite.Location = new System.Drawing.Point(3, 342);
            this.lblSprossenBreite.Name = "lblSprossenBreite";
            this.lblSprossenBreite.Size = new System.Drawing.Size(42, 13);
            this.lblSprossenBreite.TabIndex = 15;
            this.lblSprossenBreite.Text = "Spr. Br.";
            // 
            // btnSelAbstand
            // 
            this.btnSelAbstand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelAbstand.Location = new System.Drawing.Point(202, 401);
            this.btnSelAbstand.Name = "btnSelAbstand";
            this.btnSelAbstand.Size = new System.Drawing.Size(24, 22);
            this.btnSelAbstand.TabIndex = 21;
            this.btnSelAbstand.Text = "...";
            this.btnSelAbstand.UseVisualStyleBackColor = true;
            this.btnSelAbstand.Click += new System.EventHandler(this.btnSelAbstand_Click);
            // 
            // txtAbstand
            // 
            this.txtAbstand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAbstand.Location = new System.Drawing.Point(95, 403);
            this.txtAbstand.Name = "txtAbstand";
            this.txtAbstand.Size = new System.Drawing.Size(101, 20);
            this.txtAbstand.TabIndex = 20;
            this.txtAbstand.Validating += new System.ComponentModel.CancelEventHandler(this.txtAbstand_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 406);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Textabstand";
            // 
            // txtFluegelStaerke
            // 
            this.txtFluegelStaerke.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFluegelStaerke.Location = new System.Drawing.Point(95, 313);
            this.txtFluegelStaerke.Name = "txtFluegelStaerke";
            this.txtFluegelStaerke.Size = new System.Drawing.Size(101, 20);
            this.txtFluegelStaerke.TabIndex = 23;
            this.txtFluegelStaerke.Validating += new System.ComponentModel.CancelEventHandler(this.txtFluegelStaerke_Validating);
            // 
            // lblFluegelStaerke
            // 
            this.lblFluegelStaerke.AutoSize = true;
            this.lblFluegelStaerke.Location = new System.Drawing.Point(3, 316);
            this.lblFluegelStaerke.Name = "lblFluegelStaerke";
            this.lblFluegelStaerke.Size = new System.Drawing.Size(64, 13);
            this.lblFluegelStaerke.TabIndex = 22;
            this.lblFluegelStaerke.Text = "Flügelstärke";
            // 
            // grpFenster
            // 
            this.grpFenster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFenster.Controls.Add(this.rbnKasten);
            this.grpFenster.Controls.Add(this.rbnStandard);
            this.grpFenster.Location = new System.Drawing.Point(0, 7);
            this.grpFenster.Name = "grpFenster";
            this.grpFenster.Size = new System.Drawing.Size(226, 70);
            this.grpFenster.TabIndex = 26;
            this.grpFenster.TabStop = false;
            this.grpFenster.Text = "Fenstertyp";
            // 
            // rbnKasten
            // 
            this.rbnKasten.AutoSize = true;
            this.rbnKasten.Location = new System.Drawing.Point(6, 42);
            this.rbnKasten.Name = "rbnKasten";
            this.rbnKasten.Size = new System.Drawing.Size(90, 17);
            this.rbnKasten.TabIndex = 1;
            this.rbnKasten.TabStop = true;
            this.rbnKasten.Text = "Kastenfenster";
            this.rbnKasten.UseVisualStyleBackColor = true;
            this.rbnKasten.CheckedChanged += new System.EventHandler(this.rbnKasten_CheckedChanged);
            // 
            // rbnStandard
            // 
            this.rbnStandard.AutoSize = true;
            this.rbnStandard.Location = new System.Drawing.Point(6, 19);
            this.rbnStandard.Name = "rbnStandard";
            this.rbnStandard.Size = new System.Drawing.Size(68, 17);
            this.rbnStandard.TabIndex = 0;
            this.rbnStandard.TabStop = true;
            this.rbnStandard.Text = "Standard";
            this.rbnStandard.UseVisualStyleBackColor = true;
            this.rbnStandard.CheckedChanged += new System.EventHandler(this.rbnStandard_CheckedChanged);
            // 
            // grpSprossen
            // 
            this.grpSprossen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSprossen.Controls.Add(this.rbnSprosse2);
            this.grpSprossen.Controls.Add(this.rbnSprosse1);
            this.grpSprossen.Controls.Add(this.rbnSprosse0);
            this.grpSprossen.Location = new System.Drawing.Point(0, 165);
            this.grpSprossen.Name = "grpSprossen";
            this.grpSprossen.Size = new System.Drawing.Size(226, 84);
            this.grpSprossen.TabIndex = 2;
            this.grpSprossen.TabStop = false;
            this.grpSprossen.Text = "Sprossen";
            // 
            // rbnSprosse2
            // 
            this.rbnSprosse2.AutoSize = true;
            this.rbnSprosse2.Location = new System.Drawing.Point(6, 65);
            this.rbnSprosse2.Name = "rbnSprosse2";
            this.rbnSprosse2.Size = new System.Drawing.Size(95, 17);
            this.rbnSprosse2.TabIndex = 3;
            this.rbnSprosse2.TabStop = true;
            this.rbnSprosse2.Text = "Zwei Sprossen";
            this.rbnSprosse2.UseVisualStyleBackColor = true;
            this.rbnSprosse2.CheckedChanged += new System.EventHandler(this.rbnSprosse2_CheckedChanged);
            // 
            // rbnSprosse1
            // 
            this.rbnSprosse1.AutoSize = true;
            this.rbnSprosse1.Location = new System.Drawing.Point(6, 42);
            this.rbnSprosse1.Name = "rbnSprosse1";
            this.rbnSprosse1.Size = new System.Drawing.Size(87, 17);
            this.rbnSprosse1.TabIndex = 2;
            this.rbnSprosse1.TabStop = true;
            this.rbnSprosse1.Text = "Eine Sprosse";
            this.rbnSprosse1.UseVisualStyleBackColor = true;
            this.rbnSprosse1.CheckedChanged += new System.EventHandler(this.rbnSprosse1_CheckedChanged);
            // 
            // rbnSprosse0
            // 
            this.rbnSprosse0.AutoSize = true;
            this.rbnSprosse0.Location = new System.Drawing.Point(6, 19);
            this.rbnSprosse0.Name = "rbnSprosse0";
            this.rbnSprosse0.Size = new System.Drawing.Size(93, 17);
            this.rbnSprosse0.TabIndex = 1;
            this.rbnSprosse0.TabStop = true;
            this.rbnSprosse0.Text = "Keine Sprosse";
            this.rbnSprosse0.UseVisualStyleBackColor = true;
            this.rbnSprosse0.CheckedChanged += new System.EventHandler(this.rbnSprosse0_CheckedChanged);
            // 
            // btnFenster
            // 
            this.btnFenster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFenster.Location = new System.Drawing.Point(3, 538);
            this.btnFenster.Name = "btnFenster";
            this.btnFenster.Size = new System.Drawing.Size(220, 23);
            this.btnFenster.TabIndex = 27;
            this.btnFenster.Text = "Fenster";
            this.btnFenster.UseVisualStyleBackColor = true;
            this.btnFenster.Visible = false;
            this.btnFenster.Click += new System.EventHandler(this.btnFenster_Click);
            // 
            // btnExamine
            // 
            this.btnExamine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExamine.Location = new System.Drawing.Point(3, 441);
            this.btnExamine.Name = "btnExamine";
            this.btnExamine.Size = new System.Drawing.Size(193, 23);
            this.btnExamine.TabIndex = 28;
            this.btnExamine.Text = "Prüfen";
            this.btnExamine.UseVisualStyleBackColor = true;
            this.btnExamine.Click += new System.EventHandler(this.btnExamine_Click);
            // 
            // txtWeiteTol
            // 
            this.txtWeiteTol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWeiteTol.Location = new System.Drawing.Point(95, 470);
            this.txtWeiteTol.Name = "txtWeiteTol";
            this.txtWeiteTol.Size = new System.Drawing.Size(101, 20);
            this.txtWeiteTol.TabIndex = 30;
            this.txtWeiteTol.Validating += new System.ComponentModel.CancelEventHandler(this.txtWeiteTol_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 473);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Breite Tol. (cm)";
            // 
            // FensterOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtWeiteTol);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnExamine);
            this.Controls.Add(this.btnFenster);
            this.Controls.Add(this.grpSprossen);
            this.Controls.Add(this.grpFenster);
            this.Controls.Add(this.txtFluegelStaerke);
            this.Controls.Add(this.lblFluegelStaerke);
            this.Controls.Add(this.btnSelAbstand);
            this.Controls.Add(this.txtAbstand);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSprossenBreite);
            this.Controls.Add(this.lblSprossenBreite);
            this.Controls.Add(this.txtStock);
            this.Controls.Add(this.lblStock);
            this.Controls.Add(this.txtStaerke);
            this.Controls.Add(this.lblStaerke);
            this.Controls.Add(this.txtOlAb);
            this.Controls.Add(this.lblOberlichte);
            this.Controls.Add(this.btnSelParapet);
            this.Controls.Add(this.txtParapet);
            this.Controls.Add(this.lblParapet);
            this.Controls.Add(this.butSelHeight);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.btnSelWidth);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblWidth);
            this.MinimumSize = new System.Drawing.Size(143, 164);
            this.Name = "FensterOptionsControl";
            this.Size = new System.Drawing.Size(229, 613);
            this.grpFenster.ResumeLayout(false);
            this.grpFenster.PerformLayout();
            this.grpSprossen.ResumeLayout(false);
            this.grpSprossen.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Button btnSelWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Button butSelHeight;
        private System.Windows.Forms.Label lblParapet;
        private System.Windows.Forms.Button btnSelParapet;
        private System.Windows.Forms.Label lblOberlichte;
        private System.Windows.Forms.Label lblStaerke;
        private System.Windows.Forms.Label lblStock;
        internal System.Windows.Forms.TextBox txtWidth;
        internal System.Windows.Forms.TextBox txtHeight;
        internal System.Windows.Forms.TextBox txtParapet;
        internal System.Windows.Forms.TextBox txtOlAb;
        internal System.Windows.Forms.TextBox txtStaerke;
        internal System.Windows.Forms.TextBox txtStock;
        internal System.Windows.Forms.TextBox txtSprossenBreite;
        private System.Windows.Forms.Label lblSprossenBreite;
        private System.Windows.Forms.Button btnSelAbstand;
        internal System.Windows.Forms.TextBox txtAbstand;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox txtFluegelStaerke;
        private System.Windows.Forms.Label lblFluegelStaerke;
        private System.Windows.Forms.GroupBox grpFenster;
        private System.Windows.Forms.RadioButton rbnKasten;
        private System.Windows.Forms.RadioButton rbnStandard;
        private System.Windows.Forms.GroupBox grpSprossen;
        private System.Windows.Forms.RadioButton rbnSprosse2;
        private System.Windows.Forms.RadioButton rbnSprosse1;
        private System.Windows.Forms.RadioButton rbnSprosse0;
        private System.Windows.Forms.Button btnFenster;
        private System.Windows.Forms.Button btnExamine;
        internal System.Windows.Forms.TextBox txtWeiteTol;
        private System.Windows.Forms.Label label2;
    }
}
