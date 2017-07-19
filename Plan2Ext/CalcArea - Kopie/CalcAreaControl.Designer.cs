namespace Plan2Ext.CalcArea
{
    partial class CalcAreaControl
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
            this.btnCalcArea = new System.Windows.Forms.Button();
            this.txtAttribute = new System.Windows.Forms.TextBox();
            this.lblAttribute = new System.Windows.Forms.Label();
            this.btnAG = new System.Windows.Forms.Button();
            this.txtAG = new System.Windows.Forms.TextBox();
            this.lblAG = new System.Windows.Forms.Label();
            this.btnFG = new System.Windows.Forms.Button();
            this.txtFG = new System.Windows.Forms.TextBox();
            this.lblFG = new System.Windows.Forms.Label();
            this.btnSelectBlock = new System.Windows.Forms.Button();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.lblBlock = new System.Windows.Forms.Label();
            this.typeTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCalcArea
            // 
            this.btnCalcArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalcArea.Location = new System.Drawing.Point(3, 106);
            this.btnCalcArea.Name = "btnCalcArea";
            this.btnCalcArea.Size = new System.Drawing.Size(346, 23);
            this.btnCalcArea.TabIndex = 25;
            this.btnCalcArea.Text = "Fläche rechnen";
            this.btnCalcArea.UseVisualStyleBackColor = true;
            this.btnCalcArea.Click += new System.EventHandler(this.btnCalcArea_Click);
            // 
            // txtAttribute
            // 
            this.txtAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAttribute.Enabled = false;
            this.txtAttribute.Location = new System.Drawing.Point(88, 27);
            this.txtAttribute.Name = "txtAttribute";
            this.txtAttribute.Size = new System.Drawing.Size(231, 20);
            this.txtAttribute.TabIndex = 24;
            this.txtAttribute.TextChanged += new System.EventHandler(this.txtAttribute_TextChanged);
            // 
            // lblAttribute
            // 
            this.lblAttribute.AutoSize = true;
            this.lblAttribute.Location = new System.Drawing.Point(0, 30);
            this.lblAttribute.Name = "lblAttribute";
            this.lblAttribute.Size = new System.Drawing.Size(66, 13);
            this.lblAttribute.TabIndex = 23;
            this.lblAttribute.Text = "Attributname";
            // 
            // btnAG
            // 
            this.btnAG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAG.Location = new System.Drawing.Point(325, 78);
            this.btnAG.Name = "btnAG";
            this.btnAG.Size = new System.Drawing.Size(24, 22);
            this.btnAG.TabIndex = 22;
            this.btnAG.Text = "...";
            this.btnAG.UseVisualStyleBackColor = true;
            this.btnAG.Click += new System.EventHandler(this.btnAG_Click);
            // 
            // txtAG
            // 
            this.txtAG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAG.Enabled = false;
            this.txtAG.Location = new System.Drawing.Point(88, 80);
            this.txtAG.Name = "txtAG";
            this.txtAG.Size = new System.Drawing.Size(231, 20);
            this.txtAG.TabIndex = 21;
            this.txtAG.TextChanged += new System.EventHandler(this.txtAG_TextChanged);
            // 
            // lblAG
            // 
            this.lblAG.AutoSize = true;
            this.lblAG.Location = new System.Drawing.Point(0, 83);
            this.lblAG.Name = "lblAG";
            this.lblAG.Size = new System.Drawing.Size(71, 13);
            this.lblAG.TabIndex = 20;
            this.lblAG.Text = "Abzugsfläche";
            // 
            // btnFG
            // 
            this.btnFG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFG.Location = new System.Drawing.Point(325, 52);
            this.btnFG.Name = "btnFG";
            this.btnFG.Size = new System.Drawing.Size(24, 22);
            this.btnFG.TabIndex = 19;
            this.btnFG.Text = "...";
            this.btnFG.UseVisualStyleBackColor = true;
            this.btnFG.Click += new System.EventHandler(this.btnFG_Click);
            // 
            // txtFG
            // 
            this.txtFG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFG.Enabled = false;
            this.txtFG.Location = new System.Drawing.Point(88, 54);
            this.txtFG.Name = "txtFG";
            this.txtFG.Size = new System.Drawing.Size(231, 20);
            this.txtFG.TabIndex = 18;
            this.txtFG.TextChanged += new System.EventHandler(this.txtFG_TextChanged);
            // 
            // lblFG
            // 
            this.lblFG.AutoSize = true;
            this.lblFG.Location = new System.Drawing.Point(0, 57);
            this.lblFG.Name = "lblFG";
            this.lblFG.Size = new System.Drawing.Size(77, 13);
            this.lblFG.TabIndex = 17;
            this.lblFG.Text = "Flächengrenze";
            // 
            // btnSelectBlock
            // 
            this.btnSelectBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBlock.Location = new System.Drawing.Point(325, 2);
            this.btnSelectBlock.Name = "btnSelectBlock";
            this.btnSelectBlock.Size = new System.Drawing.Size(24, 22);
            this.btnSelectBlock.TabIndex = 16;
            this.btnSelectBlock.Text = "...";
            this.btnSelectBlock.UseVisualStyleBackColor = true;
            this.btnSelectBlock.Click += new System.EventHandler(this.btnSelectBlock_Click);
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Enabled = false;
            this.txtBlockname.Location = new System.Drawing.Point(88, 4);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(231, 20);
            this.txtBlockname.TabIndex = 15;
            this.txtBlockname.TextChanged += new System.EventHandler(this.txtBlockname_TextChanged);
            // 
            // lblBlock
            // 
            this.lblBlock.AutoSize = true;
            this.lblBlock.Location = new System.Drawing.Point(0, 7);
            this.lblBlock.Name = "lblBlock";
            this.lblBlock.Size = new System.Drawing.Size(60, 13);
            this.lblBlock.TabIndex = 14;
            this.lblBlock.Text = "Blockname";
            // 
            // typeTextBox
            // 
            this.typeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typeTextBox.Location = new System.Drawing.Point(5, 135);
            this.typeTextBox.Multiline = true;
            this.typeTextBox.Name = "typeTextBox";
            this.typeTextBox.ReadOnly = true;
            this.typeTextBox.Size = new System.Drawing.Size(344, 231);
            this.typeTextBox.TabIndex = 13;
            // 
            // CalcAreaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCalcArea);
            this.Controls.Add(this.txtAttribute);
            this.Controls.Add(this.lblAttribute);
            this.Controls.Add(this.btnAG);
            this.Controls.Add(this.txtAG);
            this.Controls.Add(this.lblAG);
            this.Controls.Add(this.btnFG);
            this.Controls.Add(this.txtFG);
            this.Controls.Add(this.lblFG);
            this.Controls.Add(this.btnSelectBlock);
            this.Controls.Add(this.txtBlockname);
            this.Controls.Add(this.lblBlock);
            this.Controls.Add(this.typeTextBox);
            this.Name = "CalcAreaControl";
            this.Size = new System.Drawing.Size(354, 368);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCalcArea;
        public System.Windows.Forms.TextBox txtAttribute;
        private System.Windows.Forms.Label lblAttribute;
        private System.Windows.Forms.Button btnAG;
        public System.Windows.Forms.TextBox txtAG;
        private System.Windows.Forms.Label lblAG;
        private System.Windows.Forms.Button btnFG;
        public System.Windows.Forms.TextBox txtFG;
        private System.Windows.Forms.Label lblFG;
        private System.Windows.Forms.Button btnSelectBlock;
        public System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label lblBlock;
        public System.Windows.Forms.TextBox typeTextBox;
    }
}
