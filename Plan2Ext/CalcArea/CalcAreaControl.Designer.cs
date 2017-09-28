namespace Plan2Ext.CalcArea
{
    partial class CalcAreaControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtHeightAtt = new System.Windows.Forms.TextBox();
            this.btnSelVolAttribs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVolAtt = new System.Windows.Forms.TextBox();
            this.btnCalcVol = new System.Windows.Forms.Button();
            this.txtPeriAtt = new System.Windows.Forms.TextBox();
            this.lblPeriAtt = new System.Windows.Forms.Label();
            this.btnFlaBereinig = new System.Windows.Forms.Button();
            this.btnLayerRestore = new System.Windows.Forms.Button();
            this.chkLayerSchaltung = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCalcArea
            // 
            this.btnCalcArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalcArea.Location = new System.Drawing.Point(6, 132);
            this.btnCalcArea.Name = "btnCalcArea";
            this.btnCalcArea.Size = new System.Drawing.Size(114, 23);
            this.btnCalcArea.TabIndex = 38;
            this.btnCalcArea.Text = "Fläche rechnen";
            this.btnCalcArea.UseVisualStyleBackColor = true;
            this.btnCalcArea.Click += new System.EventHandler(this.btnCalcArea_Click);
            // 
            // txtAttribute
            // 
            this.txtAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAttribute.Enabled = false;
            this.txtAttribute.Location = new System.Drawing.Point(91, 28);
            this.txtAttribute.Name = "txtAttribute";
            this.txtAttribute.Size = new System.Drawing.Size(130, 20);
            this.txtAttribute.TabIndex = 37;
            this.txtAttribute.TextChanged += new System.EventHandler(this.txtAttribute_TextChanged);
            // 
            // lblAttribute
            // 
            this.lblAttribute.AutoSize = true;
            this.lblAttribute.Location = new System.Drawing.Point(3, 31);
            this.lblAttribute.Name = "lblAttribute";
            this.lblAttribute.Size = new System.Drawing.Size(77, 13);
            this.lblAttribute.TabIndex = 36;
            this.lblAttribute.Text = "Flächenattribut";
            // 
            // btnAG
            // 
            this.btnAG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAG.Location = new System.Drawing.Point(227, 105);
            this.btnAG.Name = "btnAG";
            this.btnAG.Size = new System.Drawing.Size(24, 22);
            this.btnAG.TabIndex = 35;
            this.btnAG.Text = "...";
            this.btnAG.UseVisualStyleBackColor = true;
            this.btnAG.Click += new System.EventHandler(this.btnAG_Click);
            // 
            // txtAG
            // 
            this.txtAG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAG.Enabled = false;
            this.txtAG.Location = new System.Drawing.Point(91, 107);
            this.txtAG.Name = "txtAG";
            this.txtAG.Size = new System.Drawing.Size(130, 20);
            this.txtAG.TabIndex = 34;
            this.txtAG.TextChanged += new System.EventHandler(this.txtAG_TextChanged);
            // 
            // lblAG
            // 
            this.lblAG.AutoSize = true;
            this.lblAG.Location = new System.Drawing.Point(3, 110);
            this.lblAG.Name = "lblAG";
            this.lblAG.Size = new System.Drawing.Size(71, 13);
            this.lblAG.TabIndex = 33;
            this.lblAG.Text = "Abzugsfläche";
            // 
            // btnFG
            // 
            this.btnFG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFG.Location = new System.Drawing.Point(227, 79);
            this.btnFG.Name = "btnFG";
            this.btnFG.Size = new System.Drawing.Size(24, 22);
            this.btnFG.TabIndex = 32;
            this.btnFG.Text = "...";
            this.btnFG.UseVisualStyleBackColor = true;
            this.btnFG.Click += new System.EventHandler(this.btnFG_Click);
            // 
            // txtFG
            // 
            this.txtFG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFG.Enabled = false;
            this.txtFG.Location = new System.Drawing.Point(91, 81);
            this.txtFG.Name = "txtFG";
            this.txtFG.Size = new System.Drawing.Size(130, 20);
            this.txtFG.TabIndex = 31;
            this.txtFG.TextChanged += new System.EventHandler(this.txtFG_TextChanged);
            // 
            // lblFG
            // 
            this.lblFG.AutoSize = true;
            this.lblFG.Location = new System.Drawing.Point(3, 84);
            this.lblFG.Name = "lblFG";
            this.lblFG.Size = new System.Drawing.Size(77, 13);
            this.lblFG.TabIndex = 30;
            this.lblFG.Text = "Flächengrenze";
            // 
            // btnSelectBlock
            // 
            this.btnSelectBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBlock.Location = new System.Drawing.Point(227, 2);
            this.btnSelectBlock.Name = "btnSelectBlock";
            this.btnSelectBlock.Size = new System.Drawing.Size(24, 22);
            this.btnSelectBlock.TabIndex = 29;
            this.btnSelectBlock.Text = "...";
            this.btnSelectBlock.UseVisualStyleBackColor = true;
            this.btnSelectBlock.Click += new System.EventHandler(this.btnSelectBlock_Click);
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Enabled = false;
            this.txtBlockname.Location = new System.Drawing.Point(91, 4);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(130, 20);
            this.txtBlockname.TabIndex = 28;
            this.txtBlockname.TextChanged += new System.EventHandler(this.txtBlockname_TextChanged);
            // 
            // lblBlock
            // 
            this.lblBlock.AutoSize = true;
            this.lblBlock.Location = new System.Drawing.Point(3, 7);
            this.lblBlock.Name = "lblBlock";
            this.lblBlock.Size = new System.Drawing.Size(60, 13);
            this.lblBlock.TabIndex = 27;
            this.lblBlock.Text = "Blockname";
            // 
            // typeTextBox
            // 
            this.typeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typeTextBox.Location = new System.Drawing.Point(7, 273);
            this.typeTextBox.Multiline = true;
            this.typeTextBox.Name = "typeTextBox";
            this.typeTextBox.ReadOnly = true;
            this.typeTextBox.Size = new System.Drawing.Size(243, 148);
            this.typeTextBox.TabIndex = 40;
            this.typeTextBox.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 41;
            this.label1.Text = "Höhenattribut";
            // 
            // txtHeightAtt
            // 
            this.txtHeightAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeightAtt.Enabled = false;
            this.txtHeightAtt.Location = new System.Drawing.Point(91, 185);
            this.txtHeightAtt.Name = "txtHeightAtt";
            this.txtHeightAtt.Size = new System.Drawing.Size(130, 20);
            this.txtHeightAtt.TabIndex = 42;
            // 
            // btnSelVolAttribs
            // 
            this.btnSelVolAttribs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelVolAttribs.Location = new System.Drawing.Point(226, 183);
            this.btnSelVolAttribs.Name = "btnSelVolAttribs";
            this.btnSelVolAttribs.Size = new System.Drawing.Size(24, 22);
            this.btnSelVolAttribs.TabIndex = 43;
            this.btnSelVolAttribs.Text = "...";
            this.btnSelVolAttribs.UseVisualStyleBackColor = true;
            this.btnSelVolAttribs.Click += new System.EventHandler(this.btnSelVolAttribs_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 214);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 44;
            this.label2.Text = "Volumsattribut";
            // 
            // txtVolAtt
            // 
            this.txtVolAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVolAtt.Enabled = false;
            this.txtVolAtt.Location = new System.Drawing.Point(91, 211);
            this.txtVolAtt.Name = "txtVolAtt";
            this.txtVolAtt.Size = new System.Drawing.Size(130, 20);
            this.txtVolAtt.TabIndex = 45;
            // 
            // btnCalcVol
            // 
            this.btnCalcVol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalcVol.Location = new System.Drawing.Point(6, 237);
            this.btnCalcVol.Name = "btnCalcVol";
            this.btnCalcVol.Size = new System.Drawing.Size(243, 23);
            this.btnCalcVol.TabIndex = 46;
            this.btnCalcVol.Text = "Volumen berechnen";
            this.btnCalcVol.UseVisualStyleBackColor = true;
            this.btnCalcVol.Click += new System.EventHandler(this.btnCalcVol_Click);
            // 
            // txtPeriAtt
            // 
            this.txtPeriAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPeriAtt.Enabled = false;
            this.txtPeriAtt.Location = new System.Drawing.Point(91, 53);
            this.txtPeriAtt.Name = "txtPeriAtt";
            this.txtPeriAtt.Size = new System.Drawing.Size(130, 20);
            this.txtPeriAtt.TabIndex = 48;
            // 
            // lblPeriAtt
            // 
            this.lblPeriAtt.AutoSize = true;
            this.lblPeriAtt.Location = new System.Drawing.Point(3, 56);
            this.lblPeriAtt.Name = "lblPeriAtt";
            this.lblPeriAtt.Size = new System.Drawing.Size(76, 13);
            this.lblPeriAtt.TabIndex = 47;
            this.lblPeriAtt.Text = "Umfangattribut";
            // 
            // btnFlaBereinig
            // 
            this.btnFlaBereinig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFlaBereinig.Location = new System.Drawing.Point(126, 132);
            this.btnFlaBereinig.Name = "btnFlaBereinig";
            this.btnFlaBereinig.Size = new System.Drawing.Size(64, 23);
            this.btnFlaBereinig.TabIndex = 49;
            this.btnFlaBereinig.Text = "Bereinig";
            this.btnFlaBereinig.UseVisualStyleBackColor = true;
            this.btnFlaBereinig.Click += new System.EventHandler(this.btnFlaBereinig_Click);
            // 
            // btnLayerRestore
            // 
            this.btnLayerRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLayerRestore.Location = new System.Drawing.Point(196, 132);
            this.btnLayerRestore.Name = "btnLayerRestore";
            this.btnLayerRestore.Size = new System.Drawing.Size(53, 23);
            this.btnLayerRestore.TabIndex = 53;
            this.btnLayerRestore.Text = "Layer";
            this.btnLayerRestore.UseVisualStyleBackColor = true;
            this.btnLayerRestore.Click += new System.EventHandler(this.btnLayerRestore_Click);
            // 
            // chkLayerSchaltung
            // 
            this.chkLayerSchaltung.AutoSize = true;
            this.chkLayerSchaltung.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkLayerSchaltung.Checked = true;
            this.chkLayerSchaltung.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLayerSchaltung.Location = new System.Drawing.Point(91, 162);
            this.chkLayerSchaltung.Name = "chkLayerSchaltung";
            this.chkLayerSchaltung.Size = new System.Drawing.Size(15, 14);
            this.chkLayerSchaltung.TabIndex = 54;
            this.chkLayerSchaltung.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 55;
            this.label3.Text = "Layerschaltung";
            // 
            // CalcAreaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkLayerSchaltung);
            this.Controls.Add(this.btnLayerRestore);
            this.Controls.Add(this.btnFlaBereinig);
            this.Controls.Add(this.txtPeriAtt);
            this.Controls.Add(this.lblPeriAtt);
            this.Controls.Add(this.btnCalcVol);
            this.Controls.Add(this.txtVolAtt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelVolAttribs);
            this.Controls.Add(this.txtHeightAtt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.typeTextBox);
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
            this.Name = "CalcAreaControl";
            this.Size = new System.Drawing.Size(253, 424);
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
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txtHeightAtt;
        private System.Windows.Forms.Button btnSelVolAttribs;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtVolAtt;
        private System.Windows.Forms.Button btnCalcVol;
        public System.Windows.Forms.TextBox txtPeriAtt;
        private System.Windows.Forms.Label lblPeriAtt;
        private System.Windows.Forms.Button btnFlaBereinig;
        private System.Windows.Forms.Button btnLayerRestore;
        public System.Windows.Forms.CheckBox chkLayerSchaltung;
        private System.Windows.Forms.Label label3;
    }
}
