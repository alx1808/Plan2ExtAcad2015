namespace Plan2Ext.AutoIdVergabe
{
    partial class AutoIdControl
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
            this.grpZuordnen = new System.Windows.Forms.GroupBox();
            this.btnZuordnen = new System.Windows.Forms.Button();
            this.txtArial = new System.Windows.Forms.TextBox();
            this.lblArial = new System.Windows.Forms.Label();
            this.txtGeschoss = new System.Windows.Forms.TextBox();
            this.lblGeschoss = new System.Windows.Forms.Label();
            this.txtObjekt = new System.Windows.Forms.TextBox();
            this.lblObjekt = new System.Windows.Forms.Label();
            this.txtLiegenschaft = new System.Windows.Forms.TextBox();
            this.lblLiegenschaft = new System.Windows.Forms.Label();
            this.grpRaumblock = new System.Windows.Forms.GroupBox();
            this.txtBisStelle = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStelle = new System.Windows.Forms.TextBox();
            this.lblStelle = new System.Windows.Forms.Label();
            this.btnSelBlockAndAtt = new System.Windows.Forms.Button();
            this.txtIdNummer = new System.Windows.Forms.TextBox();
            this.lblIdNummer = new System.Windows.Forms.Label();
            this.txtTuerschildnummer = new System.Windows.Forms.TextBox();
            this.lblTuerschildnummer = new System.Windows.Forms.Label();
            this.txtBlockname = new System.Windows.Forms.TextBox();
            this.lblBlockname = new System.Windows.Forms.Label();
            this.grpExamine = new System.Windows.Forms.GroupBox();
            this.btnEindeutigkeit = new System.Windows.Forms.Button();
            this.grpAssignToRID = new System.Windows.Forms.GroupBox();
            this.btnZuRaumIdVergabeAttribut = new System.Windows.Forms.Button();
            this.lvZuweisungen = new System.Windows.Forms.ListView();
            this.btnZuRaumIdVergabe = new System.Windows.Forms.Button();
            this.btnSelToRaumIdAtt = new System.Windows.Forms.Button();
            this.btnSelPolygonLayer = new System.Windows.Forms.Button();
            this.txtPolygonLayer = new System.Windows.Forms.TextBox();
            this.lblPolygonLayer = new System.Windows.Forms.Label();
            this.txtZuRaumIdAtt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpExport = new System.Windows.Forms.GroupBox();
            this.btnExcelImport = new System.Windows.Forms.Button();
            this.btnExcelExport = new System.Windows.Forms.Button();
            this.grpZuordnen.SuspendLayout();
            this.grpRaumblock.SuspendLayout();
            this.grpExamine.SuspendLayout();
            this.grpAssignToRID.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpZuordnen
            // 
            this.grpZuordnen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpZuordnen.Controls.Add(this.btnZuordnen);
            this.grpZuordnen.Controls.Add(this.txtArial);
            this.grpZuordnen.Controls.Add(this.lblArial);
            this.grpZuordnen.Controls.Add(this.txtGeschoss);
            this.grpZuordnen.Controls.Add(this.lblGeschoss);
            this.grpZuordnen.Controls.Add(this.txtObjekt);
            this.grpZuordnen.Controls.Add(this.lblObjekt);
            this.grpZuordnen.Controls.Add(this.txtLiegenschaft);
            this.grpZuordnen.Controls.Add(this.lblLiegenschaft);
            this.grpZuordnen.Location = new System.Drawing.Point(3, 164);
            this.grpZuordnen.Name = "grpZuordnen";
            this.grpZuordnen.Size = new System.Drawing.Size(186, 156);
            this.grpZuordnen.TabIndex = 0;
            this.grpZuordnen.TabStop = false;
            this.grpZuordnen.Text = "Raum-ID zuordnen";
            // 
            // btnZuordnen
            // 
            this.btnZuordnen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZuordnen.Location = new System.Drawing.Point(6, 123);
            this.btnZuordnen.Name = "btnZuordnen";
            this.btnZuordnen.Size = new System.Drawing.Size(174, 23);
            this.btnZuordnen.TabIndex = 8;
            this.btnZuordnen.Text = "ID-Vergabe";
            this.btnZuordnen.UseVisualStyleBackColor = true;
            this.btnZuordnen.Click += new System.EventHandler(this.btnZuordnen_Click);
            // 
            // txtArial
            // 
            this.txtArial.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArial.Location = new System.Drawing.Point(80, 97);
            this.txtArial.Name = "txtArial";
            this.txtArial.Size = new System.Drawing.Size(100, 20);
            this.txtArial.TabIndex = 7;
            this.txtArial.TextChanged += new System.EventHandler(this.txtArial_TextChanged);
            // 
            // lblArial
            // 
            this.lblArial.AutoSize = true;
            this.lblArial.Location = new System.Drawing.Point(6, 100);
            this.lblArial.Name = "lblArial";
            this.lblArial.Size = new System.Drawing.Size(27, 13);
            this.lblArial.TabIndex = 6;
            this.lblArial.Text = "Arial";
            // 
            // txtGeschoss
            // 
            this.txtGeschoss.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGeschoss.Location = new System.Drawing.Point(80, 71);
            this.txtGeschoss.Name = "txtGeschoss";
            this.txtGeschoss.Size = new System.Drawing.Size(100, 20);
            this.txtGeschoss.TabIndex = 5;
            this.txtGeschoss.TextChanged += new System.EventHandler(this.txtGeschoss_TextChanged);
            // 
            // lblGeschoss
            // 
            this.lblGeschoss.AutoSize = true;
            this.lblGeschoss.Location = new System.Drawing.Point(6, 74);
            this.lblGeschoss.Name = "lblGeschoss";
            this.lblGeschoss.Size = new System.Drawing.Size(54, 13);
            this.lblGeschoss.TabIndex = 4;
            this.lblGeschoss.Text = "Geschoss";
            // 
            // txtObjekt
            // 
            this.txtObjekt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtObjekt.Location = new System.Drawing.Point(80, 45);
            this.txtObjekt.Name = "txtObjekt";
            this.txtObjekt.Size = new System.Drawing.Size(100, 20);
            this.txtObjekt.TabIndex = 3;
            this.txtObjekt.TextChanged += new System.EventHandler(this.txtObjekt_TextChanged);
            // 
            // lblObjekt
            // 
            this.lblObjekt.AutoSize = true;
            this.lblObjekt.Location = new System.Drawing.Point(6, 48);
            this.lblObjekt.Name = "lblObjekt";
            this.lblObjekt.Size = new System.Drawing.Size(38, 13);
            this.lblObjekt.TabIndex = 2;
            this.lblObjekt.Text = "Objekt";
            // 
            // txtLiegenschaft
            // 
            this.txtLiegenschaft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLiegenschaft.Location = new System.Drawing.Point(80, 19);
            this.txtLiegenschaft.Name = "txtLiegenschaft";
            this.txtLiegenschaft.Size = new System.Drawing.Size(100, 20);
            this.txtLiegenschaft.TabIndex = 1;
            this.txtLiegenschaft.TextChanged += new System.EventHandler(this.txtLiegenschaft_TextChanged);
            // 
            // lblLiegenschaft
            // 
            this.lblLiegenschaft.AutoSize = true;
            this.lblLiegenschaft.Location = new System.Drawing.Point(6, 22);
            this.lblLiegenschaft.Name = "lblLiegenschaft";
            this.lblLiegenschaft.Size = new System.Drawing.Size(68, 13);
            this.lblLiegenschaft.TabIndex = 0;
            this.lblLiegenschaft.Text = "Liegenschaft";
            // 
            // grpRaumblock
            // 
            this.grpRaumblock.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpRaumblock.Controls.Add(this.txtBisStelle);
            this.grpRaumblock.Controls.Add(this.label2);
            this.grpRaumblock.Controls.Add(this.txtStelle);
            this.grpRaumblock.Controls.Add(this.lblStelle);
            this.grpRaumblock.Controls.Add(this.btnSelBlockAndAtt);
            this.grpRaumblock.Controls.Add(this.txtIdNummer);
            this.grpRaumblock.Controls.Add(this.lblIdNummer);
            this.grpRaumblock.Controls.Add(this.txtTuerschildnummer);
            this.grpRaumblock.Controls.Add(this.lblTuerschildnummer);
            this.grpRaumblock.Controls.Add(this.txtBlockname);
            this.grpRaumblock.Controls.Add(this.lblBlockname);
            this.grpRaumblock.Location = new System.Drawing.Point(3, 3);
            this.grpRaumblock.Name = "grpRaumblock";
            this.grpRaumblock.Size = new System.Drawing.Size(186, 159);
            this.grpRaumblock.TabIndex = 8;
            this.grpRaumblock.TabStop = false;
            this.grpRaumblock.Text = "Konfiguration Raumblock";
            // 
            // txtBisStelle
            // 
            this.txtBisStelle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBisStelle.Location = new System.Drawing.Point(159, 121);
            this.txtBisStelle.Name = "txtBisStelle";
            this.txtBisStelle.Size = new System.Drawing.Size(21, 20);
            this.txtBisStelle.TabIndex = 18;
            this.txtBisStelle.TextChanged += new System.EventHandler(this.txtBisStelle_TextChanged);
            this.txtBisStelle.Validating += new System.ComponentModel.CancelEventHandler(this.txtBisStelle_Validating);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(143, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "-";
            // 
            // txtStelle
            // 
            this.txtStelle.Location = new System.Drawing.Point(80, 121);
            this.txtStelle.Name = "txtStelle";
            this.txtStelle.Size = new System.Drawing.Size(21, 20);
            this.txtStelle.TabIndex = 16;
            this.txtStelle.TextChanged += new System.EventHandler(this.txtStelle_TextChanged);
            this.txtStelle.Validating += new System.ComponentModel.CancelEventHandler(this.txtStelle_Validating);
            // 
            // lblStelle
            // 
            this.lblStelle.AutoSize = true;
            this.lblStelle.Location = new System.Drawing.Point(6, 124);
            this.lblStelle.Name = "lblStelle";
            this.lblStelle.Size = new System.Drawing.Size(68, 13);
            this.lblStelle.TabIndex = 15;
            this.lblStelle.Text = "Raumnr. von";
            this.lblStelle.Click += new System.EventHandler(this.lblStelle_Click);
            // 
            // btnSelBlockAndAtt
            // 
            this.btnSelBlockAndAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelBlockAndAtt.Location = new System.Drawing.Point(9, 92);
            this.btnSelBlockAndAtt.Name = "btnSelBlockAndAtt";
            this.btnSelBlockAndAtt.Size = new System.Drawing.Size(171, 23);
            this.btnSelBlockAndAtt.TabIndex = 14;
            this.btnSelBlockAndAtt.Text = "Zeigen";
            this.btnSelBlockAndAtt.UseVisualStyleBackColor = true;
            this.btnSelBlockAndAtt.Click += new System.EventHandler(this.btnSelBlockAndAtt_Click);
            // 
            // txtIdNummer
            // 
            this.txtIdNummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIdNummer.Location = new System.Drawing.Point(99, 66);
            this.txtIdNummer.Name = "txtIdNummer";
            this.txtIdNummer.Size = new System.Drawing.Size(81, 20);
            this.txtIdNummer.TabIndex = 13;
            this.txtIdNummer.TextChanged += new System.EventHandler(this.txtIdNummer_TextChanged);
            // 
            // lblIdNummer
            // 
            this.lblIdNummer.AutoSize = true;
            this.lblIdNummer.Location = new System.Drawing.Point(6, 69);
            this.lblIdNummer.Name = "lblIdNummer";
            this.lblIdNummer.Size = new System.Drawing.Size(60, 13);
            this.lblIdNummer.TabIndex = 12;
            this.lblIdNummer.Text = "ID-Nummer";
            // 
            // txtTuerschildnummer
            // 
            this.txtTuerschildnummer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTuerschildnummer.Location = new System.Drawing.Point(99, 40);
            this.txtTuerschildnummer.Name = "txtTuerschildnummer";
            this.txtTuerschildnummer.Size = new System.Drawing.Size(81, 20);
            this.txtTuerschildnummer.TabIndex = 11;
            this.txtTuerschildnummer.TextChanged += new System.EventHandler(this.txtTuerschildnummer_TextChanged);
            // 
            // lblTuerschildnummer
            // 
            this.lblTuerschildnummer.AutoSize = true;
            this.lblTuerschildnummer.Location = new System.Drawing.Point(6, 43);
            this.lblTuerschildnummer.Name = "lblTuerschildnummer";
            this.lblTuerschildnummer.Size = new System.Drawing.Size(87, 13);
            this.lblTuerschildnummer.TabIndex = 10;
            this.lblTuerschildnummer.Text = "Türschildnummer";
            // 
            // txtBlockname
            // 
            this.txtBlockname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockname.Enabled = false;
            this.txtBlockname.Location = new System.Drawing.Point(99, 14);
            this.txtBlockname.Name = "txtBlockname";
            this.txtBlockname.Size = new System.Drawing.Size(81, 20);
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
            // grpExamine
            // 
            this.grpExamine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExamine.Controls.Add(this.btnEindeutigkeit);
            this.grpExamine.Location = new System.Drawing.Point(3, 326);
            this.grpExamine.Name = "grpExamine";
            this.grpExamine.Size = new System.Drawing.Size(186, 57);
            this.grpExamine.TabIndex = 9;
            this.grpExamine.TabStop = false;
            this.grpExamine.Text = "Prüfen";
            // 
            // btnEindeutigkeit
            // 
            this.btnEindeutigkeit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEindeutigkeit.Location = new System.Drawing.Point(6, 19);
            this.btnEindeutigkeit.Name = "btnEindeutigkeit";
            this.btnEindeutigkeit.Size = new System.Drawing.Size(174, 23);
            this.btnEindeutigkeit.TabIndex = 0;
            this.btnEindeutigkeit.Text = "Eindeutigkeit";
            this.btnEindeutigkeit.UseVisualStyleBackColor = true;
            this.btnEindeutigkeit.Click += new System.EventHandler(this.btnEindeutigkeit_Click);
            // 
            // grpAssignToRID
            // 
            this.grpAssignToRID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAssignToRID.Controls.Add(this.btnZuRaumIdVergabeAttribut);
            this.grpAssignToRID.Controls.Add(this.lvZuweisungen);
            this.grpAssignToRID.Controls.Add(this.btnZuRaumIdVergabe);
            this.grpAssignToRID.Controls.Add(this.btnSelToRaumIdAtt);
            this.grpAssignToRID.Controls.Add(this.btnSelPolygonLayer);
            this.grpAssignToRID.Controls.Add(this.txtPolygonLayer);
            this.grpAssignToRID.Controls.Add(this.lblPolygonLayer);
            this.grpAssignToRID.Location = new System.Drawing.Point(3, 389);
            this.grpAssignToRID.Name = "grpAssignToRID";
            this.grpAssignToRID.Size = new System.Drawing.Size(186, 229);
            this.grpAssignToRID.TabIndex = 10;
            this.grpAssignToRID.TabStop = false;
            this.grpAssignToRID.Text = "Zu Raum-ID zuordnen";
            // 
            // btnZuRaumIdVergabeAttribut
            // 
            this.btnZuRaumIdVergabeAttribut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZuRaumIdVergabeAttribut.Location = new System.Drawing.Point(6, 103);
            this.btnZuRaumIdVergabeAttribut.Name = "btnZuRaumIdVergabeAttribut";
            this.btnZuRaumIdVergabeAttribut.Size = new System.Drawing.Size(174, 23);
            this.btnZuRaumIdVergabeAttribut.TabIndex = 37;
            this.btnZuRaumIdVergabeAttribut.Text = "Zu-Raum-ID Vergabe Attribut";
            this.btnZuRaumIdVergabeAttribut.UseVisualStyleBackColor = true;
            this.btnZuRaumIdVergabeAttribut.Click += new System.EventHandler(this.btnZuRaumIdVergabeAttribut_Click);
            // 
            // lvZuweisungen
            // 
            this.lvZuweisungen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvZuweisungen.FullRowSelect = true;
            this.lvZuweisungen.GridLines = true;
            this.lvZuweisungen.Location = new System.Drawing.Point(6, 133);
            this.lvZuweisungen.Name = "lvZuweisungen";
            this.lvZuweisungen.Size = new System.Drawing.Size(174, 86);
            this.lvZuweisungen.TabIndex = 36;
            this.lvZuweisungen.UseCompatibleStateImageBehavior = false;
            this.lvZuweisungen.View = System.Windows.Forms.View.Details;
            // 
            // btnZuRaumIdVergabe
            // 
            this.btnZuRaumIdVergabe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZuRaumIdVergabe.Location = new System.Drawing.Point(6, 75);
            this.btnZuRaumIdVergabe.Name = "btnZuRaumIdVergabe";
            this.btnZuRaumIdVergabe.Size = new System.Drawing.Size(174, 23);
            this.btnZuRaumIdVergabe.TabIndex = 35;
            this.btnZuRaumIdVergabe.Text = "Zu-Raum-ID Vergabe Block";
            this.btnZuRaumIdVergabe.UseVisualStyleBackColor = true;
            this.btnZuRaumIdVergabe.Click += new System.EventHandler(this.btnZuRaumIdVergabe_Click);
            // 
            // btnSelToRaumIdAtt
            // 
            this.btnSelToRaumIdAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelToRaumIdAtt.Location = new System.Drawing.Point(6, 49);
            this.btnSelToRaumIdAtt.Name = "btnSelToRaumIdAtt";
            this.btnSelToRaumIdAtt.Size = new System.Drawing.Size(174, 20);
            this.btnSelToRaumIdAtt.TabIndex = 34;
            this.btnSelToRaumIdAtt.Text = "Zuweisungen ...";
            this.btnSelToRaumIdAtt.UseVisualStyleBackColor = true;
            this.btnSelToRaumIdAtt.Click += new System.EventHandler(this.btnSelToRaumIdAtt_Click);
            // 
            // btnSelPolygonLayer
            // 
            this.btnSelPolygonLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPolygonLayer.Location = new System.Drawing.Point(156, 19);
            this.btnSelPolygonLayer.Name = "btnSelPolygonLayer";
            this.btnSelPolygonLayer.Size = new System.Drawing.Size(24, 20);
            this.btnSelPolygonLayer.TabIndex = 31;
            this.btnSelPolygonLayer.Text = "...";
            this.btnSelPolygonLayer.UseVisualStyleBackColor = true;
            this.btnSelPolygonLayer.Click += new System.EventHandler(this.btnSelPolygonLayer_Click);
            // 
            // txtPolygonLayer
            // 
            this.txtPolygonLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPolygonLayer.Location = new System.Drawing.Point(80, 19);
            this.txtPolygonLayer.Name = "txtPolygonLayer";
            this.txtPolygonLayer.Size = new System.Drawing.Size(70, 20);
            this.txtPolygonLayer.TabIndex = 10;
            this.txtPolygonLayer.TextChanged += new System.EventHandler(this.txtPolygonLayer_TextChanged);
            // 
            // lblPolygonLayer
            // 
            this.lblPolygonLayer.AutoSize = true;
            this.lblPolygonLayer.Location = new System.Drawing.Point(6, 22);
            this.lblPolygonLayer.Name = "lblPolygonLayer";
            this.lblPolygonLayer.Size = new System.Drawing.Size(67, 13);
            this.lblPolygonLayer.TabIndex = 1;
            this.lblPolygonLayer.Text = "Polygonlayer";
            this.lblPolygonLayer.Click += new System.EventHandler(this.lblPolygonLayer_Click);
            // 
            // txtZuRaumIdAtt
            // 
            this.txtZuRaumIdAtt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtZuRaumIdAtt.Location = new System.Drawing.Point(102, 710);
            this.txtZuRaumIdAtt.Name = "txtZuRaumIdAtt";
            this.txtZuRaumIdAtt.Size = new System.Drawing.Size(70, 20);
            this.txtZuRaumIdAtt.TabIndex = 33;
            this.txtZuRaumIdAtt.Visible = false;
            this.txtZuRaumIdAtt.TextChanged += new System.EventHandler(this.txtZuRaumIdAtt_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 713);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Zu-ID-Attribut";
            this.label1.Visible = false;
            // 
            // grpExport
            // 
            this.grpExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExport.Controls.Add(this.btnExcelImport);
            this.grpExport.Controls.Add(this.btnExcelExport);
            this.grpExport.Location = new System.Drawing.Point(9, 618);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(180, 84);
            this.grpExport.TabIndex = 34;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export";
            // 
            // btnExcelImport
            // 
            this.btnExcelImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcelImport.Location = new System.Drawing.Point(6, 48);
            this.btnExcelImport.Name = "btnExcelImport";
            this.btnExcelImport.Size = new System.Drawing.Size(168, 23);
            this.btnExcelImport.TabIndex = 1;
            this.btnExcelImport.Text = "Import";
            this.btnExcelImport.UseVisualStyleBackColor = true;
            this.btnExcelImport.Click += new System.EventHandler(this.btnExcelImport_Click);
            // 
            // btnExcelExport
            // 
            this.btnExcelExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcelExport.Location = new System.Drawing.Point(6, 19);
            this.btnExcelExport.Name = "btnExcelExport";
            this.btnExcelExport.Size = new System.Drawing.Size(168, 23);
            this.btnExcelExport.TabIndex = 0;
            this.btnExcelExport.Text = "Export";
            this.btnExcelExport.UseVisualStyleBackColor = true;
            this.btnExcelExport.Click += new System.EventHandler(this.btnExcelExport_Click);
            // 
            // AutoIdControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpExport);
            this.Controls.Add(this.grpAssignToRID);
            this.Controls.Add(this.grpExamine);
            this.Controls.Add(this.grpRaumblock);
            this.Controls.Add(this.txtZuRaumIdAtt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.grpZuordnen);
            this.Name = "AutoIdControl";
            this.Size = new System.Drawing.Size(192, 751);
            this.Load += new System.EventHandler(this.AutoIdControl_Load);
            this.grpZuordnen.ResumeLayout(false);
            this.grpZuordnen.PerformLayout();
            this.grpRaumblock.ResumeLayout(false);
            this.grpRaumblock.PerformLayout();
            this.grpExamine.ResumeLayout(false);
            this.grpAssignToRID.ResumeLayout(false);
            this.grpAssignToRID.PerformLayout();
            this.grpExport.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpZuordnen;
        public System.Windows.Forms.TextBox txtObjekt;
        private System.Windows.Forms.Label lblObjekt;
        public System.Windows.Forms.TextBox txtLiegenschaft;
        private System.Windows.Forms.Label lblLiegenschaft;
        public System.Windows.Forms.TextBox txtArial;
        private System.Windows.Forms.Label lblArial;
        public System.Windows.Forms.TextBox txtGeschoss;
        private System.Windows.Forms.Label lblGeschoss;
        private System.Windows.Forms.GroupBox grpRaumblock;
        public System.Windows.Forms.TextBox txtIdNummer;
        private System.Windows.Forms.Label lblIdNummer;
        public System.Windows.Forms.TextBox txtTuerschildnummer;
        private System.Windows.Forms.Label lblTuerschildnummer;
        public  System.Windows.Forms.TextBox txtBlockname;
        private System.Windows.Forms.Label lblBlockname;
        private System.Windows.Forms.Button btnZuordnen;
        private System.Windows.Forms.Button btnSelBlockAndAtt;
        public System.Windows.Forms.TextBox txtStelle;
        private System.Windows.Forms.Label lblStelle;
        private System.Windows.Forms.GroupBox grpExamine;
        private System.Windows.Forms.Button btnEindeutigkeit;
        private System.Windows.Forms.GroupBox grpAssignToRID;
        public System.Windows.Forms.TextBox txtPolygonLayer;
        private System.Windows.Forms.Label lblPolygonLayer;
        private System.Windows.Forms.Button btnSelToRaumIdAtt;
        public System.Windows.Forms.TextBox txtZuRaumIdAtt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelPolygonLayer;
        private System.Windows.Forms.Button btnZuRaumIdVergabe;
        private System.Windows.Forms.ListView lvZuweisungen;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Button btnExcelExport;
        private System.Windows.Forms.Button btnExcelImport;
        public System.Windows.Forms.TextBox txtBisStelle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnZuRaumIdVergabeAttribut;
    }
}
