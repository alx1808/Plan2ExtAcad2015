namespace Plan2Ext.Tuer
{
    partial class TuerOptionsControl
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
            this.grpTuer = new System.Windows.Forms.GroupBox();
            this.rbnEck = new System.Windows.Forms.RadioButton();
            this.rbnBlock = new System.Windows.Forms.RadioButton();
            this.rbnUmfassung = new System.Windows.Forms.RadioButton();
            this.grpFluegel = new System.Windows.Forms.GroupBox();
            this.rbnZwei = new System.Windows.Forms.RadioButton();
            this.rbnEins = new System.Windows.Forms.RadioButton();
            this.butSelHeight = new System.Windows.Forms.Button();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.btnSelWidth = new System.Windows.Forms.Button();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.lblWidth = new System.Windows.Forms.Label();
            this.grpTextBlock = new System.Windows.Forms.GroupBox();
            this.rbnTb4 = new System.Windows.Forms.RadioButton();
            this.rbnTbStandard = new System.Windows.Forms.RadioButton();
            this.txtStockStaerke = new System.Windows.Forms.TextBox();
            this.lblStockStaerke = new System.Windows.Forms.Label();
            this.grpTuer.SuspendLayout();
            this.grpFluegel.SuspendLayout();
            this.grpTextBlock.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTuer
            // 
            this.grpTuer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTuer.Controls.Add(this.rbnEck);
            this.grpTuer.Controls.Add(this.rbnBlock);
            this.grpTuer.Controls.Add(this.rbnUmfassung);
            this.grpTuer.Location = new System.Drawing.Point(3, 3);
            this.grpTuer.Name = "grpTuer";
            this.grpTuer.Size = new System.Drawing.Size(245, 96);
            this.grpTuer.TabIndex = 27;
            this.grpTuer.TabStop = false;
            this.grpTuer.Text = "Türtyp";
            // 
            // rbnEck
            // 
            this.rbnEck.AutoSize = true;
            this.rbnEck.Location = new System.Drawing.Point(6, 65);
            this.rbnEck.Name = "rbnEck";
            this.rbnEck.Size = new System.Drawing.Size(70, 17);
            this.rbnEck.TabIndex = 2;
            this.rbnEck.TabStop = true;
            this.rbnEck.Text = "Eckzarge";
            this.rbnEck.UseVisualStyleBackColor = true;
            this.rbnEck.CheckedChanged += new System.EventHandler(this.rbnEck_CheckedChanged);
            // 
            // rbnBlock
            // 
            this.rbnBlock.AutoSize = true;
            this.rbnBlock.Location = new System.Drawing.Point(6, 42);
            this.rbnBlock.Name = "rbnBlock";
            this.rbnBlock.Size = new System.Drawing.Size(78, 17);
            this.rbnBlock.TabIndex = 1;
            this.rbnBlock.TabStop = true;
            this.rbnBlock.Text = "Blockzarge";
            this.rbnBlock.UseVisualStyleBackColor = true;
            this.rbnBlock.CheckedChanged += new System.EventHandler(this.rbnBlock_CheckedChanged);
            // 
            // rbnUmfassung
            // 
            this.rbnUmfassung.AutoSize = true;
            this.rbnUmfassung.Location = new System.Drawing.Point(6, 19);
            this.rbnUmfassung.Name = "rbnUmfassung";
            this.rbnUmfassung.Size = new System.Drawing.Size(109, 17);
            this.rbnUmfassung.TabIndex = 0;
            this.rbnUmfassung.TabStop = true;
            this.rbnUmfassung.Text = "Umfassungszarge";
            this.rbnUmfassung.UseVisualStyleBackColor = true;
            this.rbnUmfassung.CheckedChanged += new System.EventHandler(this.rbnUmfassung_CheckedChanged);
            // 
            // grpFluegel
            // 
            this.grpFluegel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFluegel.Controls.Add(this.rbnZwei);
            this.grpFluegel.Controls.Add(this.rbnEins);
            this.grpFluegel.Location = new System.Drawing.Point(3, 105);
            this.grpFluegel.Name = "grpFluegel";
            this.grpFluegel.Size = new System.Drawing.Size(245, 75);
            this.grpFluegel.TabIndex = 28;
            this.grpFluegel.TabStop = false;
            // 
            // rbnZwei
            // 
            this.rbnZwei.AutoSize = true;
            this.rbnZwei.Location = new System.Drawing.Point(6, 42);
            this.rbnZwei.Name = "rbnZwei";
            this.rbnZwei.Size = new System.Drawing.Size(81, 17);
            this.rbnZwei.TabIndex = 1;
            this.rbnZwei.TabStop = true;
            this.rbnZwei.Text = "Zweiflügelig";
            this.rbnZwei.UseVisualStyleBackColor = true;
            this.rbnZwei.CheckedChanged += new System.EventHandler(this.rbnZwei_CheckedChanged);
            // 
            // rbnEins
            // 
            this.rbnEins.AutoSize = true;
            this.rbnEins.Location = new System.Drawing.Point(6, 19);
            this.rbnEins.Name = "rbnEins";
            this.rbnEins.Size = new System.Drawing.Size(73, 17);
            this.rbnEins.TabIndex = 0;
            this.rbnEins.TabStop = true;
            this.rbnEins.Text = "Einflügelig";
            this.rbnEins.UseVisualStyleBackColor = true;
            this.rbnEins.CheckedChanged += new System.EventHandler(this.rbnEins_CheckedChanged);
            // 
            // butSelHeight
            // 
            this.butSelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelHeight.Location = new System.Drawing.Point(224, 216);
            this.butSelHeight.Name = "butSelHeight";
            this.butSelHeight.Size = new System.Drawing.Size(24, 22);
            this.butSelHeight.TabIndex = 34;
            this.butSelHeight.Text = "...";
            this.butSelHeight.UseVisualStyleBackColor = true;
            this.butSelHeight.Click += new System.EventHandler(this.butSelHeight_Click);
            // 
            // txtHeight
            // 
            this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeight.Location = new System.Drawing.Point(80, 218);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(138, 20);
            this.txtHeight.TabIndex = 33;
            this.txtHeight.Validating += new System.ComponentModel.CancelEventHandler(this.txtHeight_Validating);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(10, 221);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(33, 13);
            this.lblHeight.TabIndex = 32;
            this.lblHeight.Text = "Höhe";
            // 
            // btnSelWidth
            // 
            this.btnSelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelWidth.Location = new System.Drawing.Point(224, 190);
            this.btnSelWidth.Name = "btnSelWidth";
            this.btnSelWidth.Size = new System.Drawing.Size(24, 22);
            this.btnSelWidth.TabIndex = 31;
            this.btnSelWidth.Text = "...";
            this.btnSelWidth.UseVisualStyleBackColor = true;
            this.btnSelWidth.Click += new System.EventHandler(this.btnSelWidth_Click);
            // 
            // txtWidth
            // 
            this.txtWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWidth.Location = new System.Drawing.Point(80, 192);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(138, 20);
            this.txtWidth.TabIndex = 30;
            this.txtWidth.Validating += new System.ComponentModel.CancelEventHandler(this.txtWidth_Validating);
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(10, 195);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(34, 13);
            this.lblWidth.TabIndex = 29;
            this.lblWidth.Text = "Breite";
            // 
            // grpTextBlock
            // 
            this.grpTextBlock.Controls.Add(this.rbnTb4);
            this.grpTextBlock.Controls.Add(this.rbnTbStandard);
            this.grpTextBlock.Location = new System.Drawing.Point(3, 286);
            this.grpTextBlock.Name = "grpTextBlock";
            this.grpTextBlock.Size = new System.Drawing.Size(156, 82);
            this.grpTextBlock.TabIndex = 35;
            this.grpTextBlock.TabStop = false;
            this.grpTextBlock.Text = "Textblock";
            this.grpTextBlock.Visible = false;
            // 
            // rbnTb4
            // 
            this.rbnTb4.AutoSize = true;
            this.rbnTb4.Location = new System.Drawing.Point(6, 42);
            this.rbnTb4.Name = "rbnTb4";
            this.rbnTb4.Size = new System.Drawing.Size(67, 17);
            this.rbnTb4.TabIndex = 1;
            this.rbnTb4.TabStop = true;
            this.rbnTb4.Text = "4 Blöcke";
            this.rbnTb4.UseVisualStyleBackColor = true;
            this.rbnTb4.CheckedChanged += new System.EventHandler(this.rbnTb4_CheckedChanged);
            // 
            // rbnTbStandard
            // 
            this.rbnTbStandard.AutoSize = true;
            this.rbnTbStandard.Location = new System.Drawing.Point(6, 19);
            this.rbnTbStandard.Name = "rbnTbStandard";
            this.rbnTbStandard.Size = new System.Drawing.Size(68, 17);
            this.rbnTbStandard.TabIndex = 0;
            this.rbnTbStandard.TabStop = true;
            this.rbnTbStandard.Text = "Standard";
            this.rbnTbStandard.UseVisualStyleBackColor = true;
            this.rbnTbStandard.CheckedChanged += new System.EventHandler(this.rbnTbStandard_CheckedChanged);
            // 
            // txtStockStaerke
            // 
            this.txtStockStaerke.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStockStaerke.Location = new System.Drawing.Point(80, 244);
            this.txtStockStaerke.Name = "txtStockStaerke";
            this.txtStockStaerke.Size = new System.Drawing.Size(138, 20);
            this.txtStockStaerke.TabIndex = 37;
            this.txtStockStaerke.Validating += new System.ComponentModel.CancelEventHandler(this.txtStockStaerke_Validating);
            // 
            // lblStockStaerke
            // 
            this.lblStockStaerke.AutoSize = true;
            this.lblStockStaerke.Location = new System.Drawing.Point(10, 247);
            this.lblStockStaerke.Name = "lblStockStaerke";
            this.lblStockStaerke.Size = new System.Drawing.Size(64, 13);
            this.lblStockStaerke.TabIndex = 36;
            this.lblStockStaerke.Text = "Stockstärke";
            // 
            // TuerOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtStockStaerke);
            this.Controls.Add(this.lblStockStaerke);
            this.Controls.Add(this.grpTextBlock);
            this.Controls.Add(this.butSelHeight);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.btnSelWidth);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblWidth);
            this.Controls.Add(this.grpFluegel);
            this.Controls.Add(this.grpTuer);
            this.Name = "TuerOptionsControl";
            this.Size = new System.Drawing.Size(251, 420);
            this.grpTuer.ResumeLayout(false);
            this.grpTuer.PerformLayout();
            this.grpFluegel.ResumeLayout(false);
            this.grpFluegel.PerformLayout();
            this.grpTextBlock.ResumeLayout(false);
            this.grpTextBlock.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTuer;
        private System.Windows.Forms.RadioButton rbnEck;
        private System.Windows.Forms.RadioButton rbnBlock;
        private System.Windows.Forms.RadioButton rbnUmfassung;
        private System.Windows.Forms.GroupBox grpFluegel;
        private System.Windows.Forms.RadioButton rbnZwei;
        private System.Windows.Forms.RadioButton rbnEins;
        private System.Windows.Forms.Button butSelHeight;
        internal System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Button btnSelWidth;
        internal System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.GroupBox grpTextBlock;
        private System.Windows.Forms.RadioButton rbnTb4;
        private System.Windows.Forms.RadioButton rbnTbStandard;
        internal System.Windows.Forms.TextBox txtStockStaerke;
        private System.Windows.Forms.Label lblStockStaerke;
    }
}
