namespace Plan2Ext.Raumnummern
{
    partial class RnControl
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
            this.grNumber = new System.Windows.Forms.GroupBox();
            this.btnRemoveRaum = new System.Windows.Forms.Button();
            this.btnSelectTop = new System.Windows.Forms.Button();
            this.chkAutoCorr = new System.Windows.Forms.CheckBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.lblNumber = new System.Windows.Forms.Label();
            this.txtSeparator = new System.Windows.Forms.TextBox();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.txtTop = new System.Windows.Forms.TextBox();
            this.lblTop = new System.Windows.Forms.Label();
            this.grpFbHoehe = new System.Windows.Forms.GroupBox();
            this.btnFbhWithoutNr = new System.Windows.Forms.Button();
            this.btnFbhWithNr = new System.Windows.Forms.Button();
            this.grpManually = new System.Windows.Forms.GroupBox();
            this.btnAbzFlaechenGrenzeLayerName = new System.Windows.Forms.Button();
            this.txtAbzFlaechenGrenzeLayerName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnFlaechenGrenzeLayerName = new System.Windows.Forms.Button();
            this.txtFlaechenGrenzeLayerName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFlaechenAttributName = new System.Windows.Forms.TextBox();
            this.btnHBlock = new System.Windows.Forms.Button();
            this.txtHBlockname = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAttName = new System.Windows.Forms.TextBox();
            this.btnSelectBlock = new System.Windows.Forms.Button();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCalcArea = new System.Windows.Forms.Button();
            this.btnFlaBereinig = new System.Windows.Forms.Button();
            this.btnInsertTop = new System.Windows.Forms.Button();
            this.txtTopNr = new System.Windows.Forms.TextBox();
            this.btnSum = new System.Windows.Forms.Button();
            this.grNumber.SuspendLayout();
            this.grpFbHoehe.SuspendLayout();
            this.grpManually.SuspendLayout();
            this.SuspendLayout();
            // 
            // grNumber
            // 
            this.grNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grNumber.Controls.Add(this.btnRemoveRaum);
            this.grNumber.Controls.Add(this.btnSelectTop);
            this.grNumber.Controls.Add(this.chkAutoCorr);
            this.grNumber.Controls.Add(this.btnStart);
            this.grNumber.Controls.Add(this.txtNumber);
            this.grNumber.Controls.Add(this.lblNumber);
            this.grNumber.Controls.Add(this.txtSeparator);
            this.grNumber.Controls.Add(this.lblSeparator);
            this.grNumber.Controls.Add(this.txtTop);
            this.grNumber.Controls.Add(this.lblTop);
            this.grNumber.Location = new System.Drawing.Point(0, 162);
            this.grNumber.Name = "grNumber";
            this.grNumber.Size = new System.Drawing.Size(228, 182);
            this.grNumber.TabIndex = 0;
            this.grNumber.TabStop = false;
            this.grNumber.Text = "Zuordnen";
            // 
            // btnRemoveRaum
            // 
            this.btnRemoveRaum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveRaum.Location = new System.Drawing.Point(6, 150);
            this.btnRemoveRaum.Name = "btnRemoveRaum";
            this.btnRemoveRaum.Size = new System.Drawing.Size(216, 23);
            this.btnRemoveRaum.TabIndex = 32;
            this.btnRemoveRaum.Text = "Raumfläche entfernen";
            this.btnRemoveRaum.UseVisualStyleBackColor = true;
            this.btnRemoveRaum.Click += new System.EventHandler(this.btnRemoveRaum_Click);
            // 
            // btnSelectTop
            // 
            this.btnSelectTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectTop.Location = new System.Drawing.Point(198, 19);
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
            this.chkAutoCorr.Location = new System.Drawing.Point(132, 98);
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
            this.btnStart.Size = new System.Drawing.Size(216, 23);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtNumber
            // 
            this.txtNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNumber.Location = new System.Drawing.Point(157, 72);
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
            this.txtSeparator.Location = new System.Drawing.Point(157, 46);
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
            this.txtTop.Location = new System.Drawing.Point(127, 19);
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
            // grpFbHoehe
            // 
            this.grpFbHoehe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFbHoehe.Controls.Add(this.btnFbhWithoutNr);
            this.grpFbHoehe.Controls.Add(this.btnFbhWithNr);
            this.grpFbHoehe.Location = new System.Drawing.Point(3, 71);
            this.grpFbHoehe.Name = "grpFbHoehe";
            this.grpFbHoehe.Size = new System.Drawing.Size(225, 85);
            this.grpFbHoehe.TabIndex = 8;
            this.grpFbHoehe.TabStop = false;
            this.grpFbHoehe.Text = "Fußbodenhöhenblock";
            // 
            // btnFbhWithoutNr
            // 
            this.btnFbhWithoutNr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFbhWithoutNr.Location = new System.Drawing.Point(6, 48);
            this.btnFbhWithoutNr.Name = "btnFbhWithoutNr";
            this.btnFbhWithoutNr.Size = new System.Drawing.Size(213, 23);
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
            this.btnFbhWithNr.Size = new System.Drawing.Size(213, 23);
            this.btnFbhWithNr.TabIndex = 8;
            this.btnFbhWithNr.Text = "Mit Nummer";
            this.btnFbhWithNr.UseVisualStyleBackColor = true;
            this.btnFbhWithNr.Click += new System.EventHandler(this.btnFbhWithNr_Click);
            // 
            // grpManually
            // 
            this.grpManually.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpManually.Controls.Add(this.btnAbzFlaechenGrenzeLayerName);
            this.grpManually.Controls.Add(this.txtAbzFlaechenGrenzeLayerName);
            this.grpManually.Controls.Add(this.label6);
            this.grpManually.Controls.Add(this.btnFlaechenGrenzeLayerName);
            this.grpManually.Controls.Add(this.txtFlaechenGrenzeLayerName);
            this.grpManually.Controls.Add(this.label5);
            this.grpManually.Controls.Add(this.label4);
            this.grpManually.Controls.Add(this.txtFlaechenAttributName);
            this.grpManually.Controls.Add(this.btnHBlock);
            this.grpManually.Controls.Add(this.txtHBlockname);
            this.grpManually.Controls.Add(this.label3);
            this.grpManually.Controls.Add(this.label2);
            this.grpManually.Controls.Add(this.txtAttName);
            this.grpManually.Controls.Add(this.btnSelectBlock);
            this.grpManually.Controls.Add(this.txtBlockname);
            this.grpManually.Controls.Add(this.label1);
            this.grpManually.Location = new System.Drawing.Point(3, 446);
            this.grpManually.Name = "grpManually";
            this.grpManually.Size = new System.Drawing.Size(225, 178);
            this.grpManually.TabIndex = 8;
            this.grpManually.TabStop = false;
            this.grpManually.Text = "Blöcke und Attribute";
            // 
            // btnAbzFlaechenGrenzeLayerName
            // 
            this.btnAbzFlaechenGrenzeLayerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbzFlaechenGrenzeLayerName.Location = new System.Drawing.Point(195, 149);
            this.btnAbzFlaechenGrenzeLayerName.Name = "btnAbzFlaechenGrenzeLayerName";
            this.btnAbzFlaechenGrenzeLayerName.Size = new System.Drawing.Size(24, 20);
            this.btnAbzFlaechenGrenzeLayerName.TabIndex = 43;
            this.btnAbzFlaechenGrenzeLayerName.Text = "...";
            this.btnAbzFlaechenGrenzeLayerName.UseVisualStyleBackColor = true;
            this.btnAbzFlaechenGrenzeLayerName.Click += new System.EventHandler(this.btnAbzFlaechenGrenzeLayerName_Click);
            // 
            // txtAbzFlaechenGrenzeLayerName
            // 
            this.txtAbzFlaechenGrenzeLayerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAbzFlaechenGrenzeLayerName.Enabled = false;
            this.txtAbzFlaechenGrenzeLayerName.Location = new System.Drawing.Point(89, 149);
            this.txtAbzFlaechenGrenzeLayerName.Name = "txtAbzFlaechenGrenzeLayerName";
            this.txtAbzFlaechenGrenzeLayerName.Size = new System.Drawing.Size(100, 20);
            this.txtAbzFlaechenGrenzeLayerName.TabIndex = 42;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 41;
            this.label6.Text = "Abzugsfläche";
            // 
            // btnFlaechenGrenzeLayerName
            // 
            this.btnFlaechenGrenzeLayerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFlaechenGrenzeLayerName.Location = new System.Drawing.Point(195, 123);
            this.btnFlaechenGrenzeLayerName.Name = "btnFlaechenGrenzeLayerName";
            this.btnFlaechenGrenzeLayerName.Size = new System.Drawing.Size(24, 20);
            this.btnFlaechenGrenzeLayerName.TabIndex = 40;
            this.btnFlaechenGrenzeLayerName.Text = "...";
            this.btnFlaechenGrenzeLayerName.UseVisualStyleBackColor = true;
            this.btnFlaechenGrenzeLayerName.Click += new System.EventHandler(this.btnFlaechenGrenzeLayerName_Click);
            // 
            // txtFlaechenGrenzeLayerName
            // 
            this.txtFlaechenGrenzeLayerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFlaechenGrenzeLayerName.Enabled = false;
            this.txtFlaechenGrenzeLayerName.Location = new System.Drawing.Point(89, 123);
            this.txtFlaechenGrenzeLayerName.Name = "txtFlaechenGrenzeLayerName";
            this.txtFlaechenGrenzeLayerName.Size = new System.Drawing.Size(100, 20);
            this.txtFlaechenGrenzeLayerName.TabIndex = 39;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "Flächengrenze";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Fläche";
            // 
            // txtFlaechenAttributName
            // 
            this.txtFlaechenAttributName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFlaechenAttributName.Enabled = false;
            this.txtFlaechenAttributName.Location = new System.Drawing.Point(89, 71);
            this.txtFlaechenAttributName.Name = "txtFlaechenAttributName";
            this.txtFlaechenAttributName.Size = new System.Drawing.Size(100, 20);
            this.txtFlaechenAttributName.TabIndex = 36;
            // 
            // btnHBlock
            // 
            this.btnHBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHBlock.Location = new System.Drawing.Point(195, 97);
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
            this.txtHBlockname.Location = new System.Drawing.Point(89, 97);
            this.txtHBlockname.Name = "txtHBlockname";
            this.txtHBlockname.Size = new System.Drawing.Size(100, 20);
            this.txtHBlockname.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 100);
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
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Nummer";
            // 
            // txtAttName
            // 
            this.txtAttName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAttName.Enabled = false;
            this.txtAttName.Location = new System.Drawing.Point(89, 45);
            this.txtAttName.Name = "txtAttName";
            this.txtAttName.Size = new System.Drawing.Size(100, 20);
            this.txtAttName.TabIndex = 31;
            // 
            // btnSelectBlock
            // 
            this.btnSelectBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBlock.Location = new System.Drawing.Point(195, 19);
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
            this.txtBlockname.Location = new System.Drawing.Point(89, 19);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(100, 20);
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
            // btnCalcArea
            // 
            this.btnCalcArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCalcArea.Location = new System.Drawing.Point(12, 3);
            this.btnCalcArea.Name = "btnCalcArea";
            this.btnCalcArea.Size = new System.Drawing.Size(143, 23);
            this.btnCalcArea.TabIndex = 9;
            this.btnCalcArea.Text = "Fläche rechnen";
            this.btnCalcArea.UseVisualStyleBackColor = true;
            this.btnCalcArea.Click += new System.EventHandler(this.btnCalcArea_Click);
            // 
            // btnFlaBereinig
            // 
            this.btnFlaBereinig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFlaBereinig.Location = new System.Drawing.Point(161, 3);
            this.btnFlaBereinig.Name = "btnFlaBereinig";
            this.btnFlaBereinig.Size = new System.Drawing.Size(64, 23);
            this.btnFlaBereinig.TabIndex = 50;
            this.btnFlaBereinig.Text = "Bereinig";
            this.btnFlaBereinig.UseVisualStyleBackColor = true;
            this.btnFlaBereinig.Click += new System.EventHandler(this.btnFlaBereinig_Click);
            // 
            // btnInsertTop
            // 
            this.btnInsertTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInsertTop.Location = new System.Drawing.Point(12, 32);
            this.btnInsertTop.Name = "btnInsertTop";
            this.btnInsertTop.Size = new System.Drawing.Size(142, 23);
            this.btnInsertTop.TabIndex = 32;
            this.btnInsertTop.Text = "Top einfügen";
            this.btnInsertTop.UseVisualStyleBackColor = true;
            this.btnInsertTop.Click += new System.EventHandler(this.btnInsertTop_Click);
            // 
            // txtTopNr
            // 
            this.txtTopNr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTopNr.Location = new System.Drawing.Point(160, 34);
            this.txtTopNr.Name = "txtTopNr";
            this.txtTopNr.Size = new System.Drawing.Size(62, 20);
            this.txtTopNr.TabIndex = 51;
            this.txtTopNr.TextChanged += new System.EventHandler(this.txtTopNr_TextChanged);
            // 
            // btnSum
            // 
            this.btnSum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSum.Location = new System.Drawing.Point(6, 350);
            this.btnSum.Name = "btnSum";
            this.btnSum.Size = new System.Drawing.Size(216, 23);
            this.btnSum.TabIndex = 33;
            this.btnSum.Text = "Summieren";
            this.btnSum.UseVisualStyleBackColor = true;
            this.btnSum.Click += new System.EventHandler(this.btnSum_Click);
            // 
            // RnControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSum);
            this.Controls.Add(this.txtTopNr);
            this.Controls.Add(this.btnInsertTop);
            this.Controls.Add(this.btnFlaBereinig);
            this.Controls.Add(this.btnCalcArea);
            this.Controls.Add(this.grpManually);
            this.Controls.Add(this.grpFbHoehe);
            this.Controls.Add(this.grNumber);
            this.Name = "RnControl";
            this.Size = new System.Drawing.Size(231, 627);
            this.grNumber.ResumeLayout(false);
            this.grNumber.PerformLayout();
            this.grpFbHoehe.ResumeLayout(false);
            this.grpManually.ResumeLayout(false);
            this.grpManually.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grNumber;
        private System.Windows.Forms.Label lblTop;
        internal System.Windows.Forms.TextBox txtTop;
        internal System.Windows.Forms.TextBox txtSeparator;
        private System.Windows.Forms.Label lblSeparator;
        internal System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Button btnStart;
        internal System.Windows.Forms.CheckBox chkAutoCorr;
        private System.Windows.Forms.GroupBox grpFbHoehe;
        private System.Windows.Forms.Button btnFbhWithoutNr;
        private System.Windows.Forms.Button btnFbhWithNr;
        private System.Windows.Forms.GroupBox grpManually;
        internal System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectBlock;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txtAttName;
        private System.Windows.Forms.Button btnHBlock;
        internal System.Windows.Forms.TextBox txtHBlockname;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelectTop;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.TextBox txtFlaechenAttributName;
        private System.Windows.Forms.Button btnFlaechenGrenzeLayerName;
        internal System.Windows.Forms.TextBox txtFlaechenGrenzeLayerName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnAbzFlaechenGrenzeLayerName;
        internal System.Windows.Forms.TextBox txtAbzFlaechenGrenzeLayerName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCalcArea;
        private System.Windows.Forms.Button btnFlaBereinig;
        private System.Windows.Forms.Button btnInsertTop;
        internal System.Windows.Forms.TextBox txtTopNr;
        private System.Windows.Forms.Button btnRemoveRaum;
        private System.Windows.Forms.Button btnSum;
    }
}
