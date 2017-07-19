namespace Plan2Ext.CenterBlock
{
    partial class CenterBlockControl
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
            this.grpMain = new System.Windows.Forms.GroupBox();
            this.btnDelErrorSyms = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSelBlock = new System.Windows.Forms.Button();
            this.txtLayer = new System.Windows.Forms.TextBox();
            this.lblLayer = new System.Windows.Forms.Label();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.lblBlockname = new System.Windows.Forms.Label();
            this.chkUseXrefs = new System.Windows.Forms.CheckBox();
            this.grpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpMain
            // 
            this.grpMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMain.Controls.Add(this.chkUseXrefs);
            this.grpMain.Controls.Add(this.btnDelErrorSyms);
            this.grpMain.Controls.Add(this.btnStart);
            this.grpMain.Controls.Add(this.btnSelBlock);
            this.grpMain.Controls.Add(this.txtLayer);
            this.grpMain.Controls.Add(this.lblLayer);
            this.grpMain.Controls.Add(this.txtBlockname);
            this.grpMain.Controls.Add(this.lblBlockname);
            this.grpMain.Location = new System.Drawing.Point(3, 3);
            this.grpMain.Name = "grpMain";
            this.grpMain.Size = new System.Drawing.Size(282, 154);
            this.grpMain.TabIndex = 10;
            this.grpMain.TabStop = false;
            this.grpMain.Text = "Block zentrieren";
            // 
            // btnDelErrorSyms
            // 
            this.btnDelErrorSyms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelErrorSyms.Location = new System.Drawing.Point(9, 89);
            this.btnDelErrorSyms.Name = "btnDelErrorSyms";
            this.btnDelErrorSyms.Size = new System.Drawing.Size(267, 23);
            this.btnDelErrorSyms.TabIndex = 20;
            this.btnDelErrorSyms.Text = "Fehlersymbole löschen";
            this.btnDelErrorSyms.UseVisualStyleBackColor = true;
            this.btnDelErrorSyms.Click += new System.EventHandler(this.btnDelErrorSyms_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(9, 118);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(267, 23);
            this.btnStart.TabIndex = 19;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSelBlock
            // 
            this.btnSelBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelBlock.Location = new System.Drawing.Point(252, 13);
            this.btnSelBlock.Name = "btnSelBlock";
            this.btnSelBlock.Size = new System.Drawing.Size(24, 20);
            this.btnSelBlock.TabIndex = 14;
            this.btnSelBlock.Text = "...";
            this.btnSelBlock.UseVisualStyleBackColor = true;
            this.btnSelBlock.Click += new System.EventHandler(this.btnSelBlock_Click);
            // 
            // txtLayer
            // 
            this.txtLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLayer.Location = new System.Drawing.Point(83, 40);
            this.txtLayer.Name = "txtLayer";
            this.txtLayer.Size = new System.Drawing.Size(193, 20);
            this.txtLayer.TabIndex = 11;
            this.txtLayer.TextChanged += new System.EventHandler(this.txtLayer_TextChanged);
            // 
            // lblLayer
            // 
            this.lblLayer.AutoSize = true;
            this.lblLayer.Location = new System.Drawing.Point(6, 43);
            this.lblLayer.Name = "lblLayer";
            this.lblLayer.Size = new System.Drawing.Size(73, 13);
            this.lblLayer.TabIndex = 10;
            this.lblLayer.Text = "Polylinienlayer";
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Location = new System.Drawing.Point(83, 13);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(163, 20);
            this.txtBlockname.TabIndex = 9;
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
            // chkUseXrefs
            // 
            this.chkUseXrefs.AutoSize = true;
            this.chkUseXrefs.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkUseXrefs.Location = new System.Drawing.Point(9, 66);
            this.chkUseXrefs.Name = "chkUseXrefs";
            this.chkUseXrefs.Size = new System.Drawing.Size(110, 17);
            this.chkUseXrefs.TabIndex = 21;
            this.chkUseXrefs.Text = "XREF verwenden";
            this.chkUseXrefs.UseVisualStyleBackColor = true;
            this.chkUseXrefs.CheckedChanged += new System.EventHandler(this.chkUseXrefs_CheckedChanged);
            // 
            // CenterBlockControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpMain);
            this.Name = "CenterBlockControl";
            this.Size = new System.Drawing.Size(288, 252);
            this.grpMain.ResumeLayout(false);
            this.grpMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpMain;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSelBlock;
        public System.Windows.Forms.TextBox txtLayer;
        private System.Windows.Forms.Label lblLayer;
        public System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label lblBlockname;
        private System.Windows.Forms.Button btnDelErrorSyms;
        private System.Windows.Forms.CheckBox chkUseXrefs;
    }
}
