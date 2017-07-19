namespace Plan2Ext.HoehenPruefung
{
    partial class HoePrControl
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
            this.grpFussboden = new System.Windows.Forms.GroupBox();
            this.btnResetCheck = new System.Windows.Forms.Button();
            this.lblPlan2HoePrReset = new System.Windows.Forms.Label();
            this.btnCheckFb = new System.Windows.Forms.Button();
            this.lblFbToleranz = new System.Windows.Forms.Label();
            this.txtFbToleranz = new System.Windows.Forms.TextBox();
            this.btnSelBlockAndAtt = new System.Windows.Forms.Button();
            this.txtHoehenAtt = new System.Windows.Forms.TextBox();
            this.lblHoehe = new System.Windows.Forms.Label();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.lblBlockname = new System.Windows.Forms.Label();
            this.grpAllgemein = new System.Windows.Forms.GroupBox();
            this.btnSelPolygonLayer = new System.Windows.Forms.Button();
            this.txtPolygonLayer = new System.Windows.Forms.TextBox();
            this.lblPolygonLayer = new System.Windows.Forms.Label();
            this.grpFussboden.SuspendLayout();
            this.grpAllgemein.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpFussboden
            // 
            this.grpFussboden.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFussboden.Controls.Add(this.btnResetCheck);
            this.grpFussboden.Controls.Add(this.lblPlan2HoePrReset);
            this.grpFussboden.Controls.Add(this.btnCheckFb);
            this.grpFussboden.Controls.Add(this.lblFbToleranz);
            this.grpFussboden.Controls.Add(this.txtFbToleranz);
            this.grpFussboden.Controls.Add(this.btnSelBlockAndAtt);
            this.grpFussboden.Controls.Add(this.txtHoehenAtt);
            this.grpFussboden.Controls.Add(this.lblHoehe);
            this.grpFussboden.Controls.Add(this.txtBlockname);
            this.grpFussboden.Controls.Add(this.lblBlockname);
            this.grpFussboden.Location = new System.Drawing.Point(3, 55);
            this.grpFussboden.Name = "grpFussboden";
            this.grpFussboden.Size = new System.Drawing.Size(237, 161);
            this.grpFussboden.TabIndex = 9;
            this.grpFussboden.TabStop = false;
            this.grpFussboden.Text = "Fußboden";
            // 
            // btnResetCheck
            // 
            this.btnResetCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetCheck.Location = new System.Drawing.Point(207, 92);
            this.btnResetCheck.Name = "btnResetCheck";
            this.btnResetCheck.Size = new System.Drawing.Size(24, 20);
            this.btnResetCheck.TabIndex = 21;
            this.btnResetCheck.Text = "...";
            this.btnResetCheck.UseVisualStyleBackColor = true;
            this.btnResetCheck.Click += new System.EventHandler(this.btnResetCheck_Click);
            // 
            // lblPlan2HoePrReset
            // 
            this.lblPlan2HoePrReset.AutoSize = true;
            this.lblPlan2HoePrReset.Location = new System.Drawing.Point(6, 96);
            this.lblPlan2HoePrReset.Name = "lblPlan2HoePrReset";
            this.lblPlan2HoePrReset.Size = new System.Drawing.Size(136, 13);
            this.lblPlan2HoePrReset.TabIndex = 20;
            this.lblPlan2HoePrReset.Text = "Kontrollstatus zurücksetzen";
            // 
            // btnCheckFb
            // 
            this.btnCheckFb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckFb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCheckFb.Location = new System.Drawing.Point(9, 121);
            this.btnCheckFb.Name = "btnCheckFb";
            this.btnCheckFb.Size = new System.Drawing.Size(222, 23);
            this.btnCheckFb.TabIndex = 19;
            this.btnCheckFb.Text = "Prüfen";
            this.btnCheckFb.UseVisualStyleBackColor = true;
            this.btnCheckFb.Click += new System.EventHandler(this.btnCheckFb_Click);
            // 
            // lblFbToleranz
            // 
            this.lblFbToleranz.AutoSize = true;
            this.lblFbToleranz.Location = new System.Drawing.Point(6, 69);
            this.lblFbToleranz.Name = "lblFbToleranz";
            this.lblFbToleranz.Size = new System.Drawing.Size(76, 13);
            this.lblFbToleranz.TabIndex = 18;
            this.lblFbToleranz.Text = "Toleranz in cm";
            // 
            // txtFbToleranz
            // 
            this.txtFbToleranz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFbToleranz.Location = new System.Drawing.Point(207, 66);
            this.txtFbToleranz.Name = "txtFbToleranz";
            this.txtFbToleranz.Size = new System.Drawing.Size(24, 20);
            this.txtFbToleranz.TabIndex = 17;
            this.txtFbToleranz.TextChanged += new System.EventHandler(this.txtFbToleranz_TextChanged);
            // 
            // btnSelBlockAndAtt
            // 
            this.btnSelBlockAndAtt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelBlockAndAtt.Location = new System.Drawing.Point(207, 13);
            this.btnSelBlockAndAtt.Name = "btnSelBlockAndAtt";
            this.btnSelBlockAndAtt.Size = new System.Drawing.Size(24, 20);
            this.btnSelBlockAndAtt.TabIndex = 14;
            this.btnSelBlockAndAtt.Text = "...";
            this.btnSelBlockAndAtt.UseVisualStyleBackColor = true;
            this.btnSelBlockAndAtt.Click += new System.EventHandler(this.btnSelBlockAndAtt_Click);
            // 
            // txtHoehenAtt
            // 
            this.txtHoehenAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHoehenAtt.Location = new System.Drawing.Point(83, 40);
            this.txtHoehenAtt.Name = "txtHoehenAtt";
            this.txtHoehenAtt.Size = new System.Drawing.Size(148, 20);
            this.txtHoehenAtt.TabIndex = 11;
            this.txtHoehenAtt.Text = "HOEHE";
            this.txtHoehenAtt.TextChanged += new System.EventHandler(this.txtHoehenAtt_TextChanged);
            // 
            // lblHoehe
            // 
            this.lblHoehe.AutoSize = true;
            this.lblHoehe.Location = new System.Drawing.Point(6, 43);
            this.lblHoehe.Name = "lblHoehe";
            this.lblHoehe.Size = new System.Drawing.Size(71, 13);
            this.lblHoehe.TabIndex = 10;
            this.lblHoehe.Text = "Höhenattribut";
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Location = new System.Drawing.Point(83, 14);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(118, 20);
            this.txtBlockname.TabIndex = 9;
            this.txtBlockname.Text = "HÖHENKOTE";
            this.txtBlockname.TextChanged += new System.EventHandler(this.txtBlockname_TextChanged);
            // 
            // lblBlockname
            // 
            this.lblBlockname.AutoSize = true;
            this.lblBlockname.Location = new System.Drawing.Point(6, 17);
            this.lblBlockname.Name = "lblBlockname";
            this.lblBlockname.Size = new System.Drawing.Size(60, 13);
            this.lblBlockname.TabIndex = 0;
            this.lblBlockname.Text = "Blockname";
            // 
            // grpAllgemein
            // 
            this.grpAllgemein.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAllgemein.Controls.Add(this.btnSelPolygonLayer);
            this.grpAllgemein.Controls.Add(this.txtPolygonLayer);
            this.grpAllgemein.Controls.Add(this.lblPolygonLayer);
            this.grpAllgemein.Location = new System.Drawing.Point(3, 3);
            this.grpAllgemein.Name = "grpAllgemein";
            this.grpAllgemein.Size = new System.Drawing.Size(237, 48);
            this.grpAllgemein.TabIndex = 20;
            this.grpAllgemein.TabStop = false;
            this.grpAllgemein.Text = "Allgemein";
            // 
            // btnSelPolygonLayer
            // 
            this.btnSelPolygonLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPolygonLayer.Location = new System.Drawing.Point(207, 13);
            this.btnSelPolygonLayer.Name = "btnSelPolygonLayer";
            this.btnSelPolygonLayer.Size = new System.Drawing.Size(24, 20);
            this.btnSelPolygonLayer.TabIndex = 34;
            this.btnSelPolygonLayer.Text = "...";
            this.btnSelPolygonLayer.UseVisualStyleBackColor = true;
            this.btnSelPolygonLayer.Click += new System.EventHandler(this.btnSelPolygonLayer_Click);
            // 
            // txtPolygonLayer
            // 
            this.txtPolygonLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPolygonLayer.Location = new System.Drawing.Point(74, 13);
            this.txtPolygonLayer.Name = "txtPolygonLayer";
            this.txtPolygonLayer.Size = new System.Drawing.Size(127, 20);
            this.txtPolygonLayer.TabIndex = 33;
            this.txtPolygonLayer.TextChanged += new System.EventHandler(this.txtPolygonLayer_TextChanged);
            // 
            // lblPolygonLayer
            // 
            this.lblPolygonLayer.AutoSize = true;
            this.lblPolygonLayer.Location = new System.Drawing.Point(6, 16);
            this.lblPolygonLayer.Name = "lblPolygonLayer";
            this.lblPolygonLayer.Size = new System.Drawing.Size(67, 13);
            this.lblPolygonLayer.TabIndex = 32;
            this.lblPolygonLayer.Text = "Polygonlayer";
            // 
            // HoePrControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpAllgemein);
            this.Controls.Add(this.grpFussboden);
            this.Name = "HoePrControl";
            this.Size = new System.Drawing.Size(243, 443);
            this.grpFussboden.ResumeLayout(false);
            this.grpFussboden.PerformLayout();
            this.grpAllgemein.ResumeLayout(false);
            this.grpAllgemein.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpFussboden;
        private System.Windows.Forms.Button btnSelBlockAndAtt;
        public System.Windows.Forms.TextBox txtHoehenAtt;
        private System.Windows.Forms.Label lblHoehe;
        public System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label lblBlockname;
        private System.Windows.Forms.Button btnCheckFb;
        private System.Windows.Forms.Label lblFbToleranz;
        public System.Windows.Forms.TextBox txtFbToleranz;
        private System.Windows.Forms.GroupBox grpAllgemein;
        private System.Windows.Forms.Button btnSelPolygonLayer;
        public System.Windows.Forms.TextBox txtPolygonLayer;
        private System.Windows.Forms.Label lblPolygonLayer;
        private System.Windows.Forms.Button btnResetCheck;
        private System.Windows.Forms.Label lblPlan2HoePrReset;
    }
}
