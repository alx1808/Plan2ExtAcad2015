namespace Plan2Ext.LayerKontrolle
{
    partial class LayerNamesLengthFrm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstDifferingLayerNames = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLayerNameLength = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLayerName = new System.Windows.Forms.TextBox();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDifferingInList = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstDifferingLayerNames
            // 
            this.lstDifferingLayerNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDifferingLayerNames.FormattingEnabled = true;
            this.lstDifferingLayerNames.Location = new System.Drawing.Point(12, 53);
            this.lstDifferingLayerNames.Name = "lstDifferingLayerNames";
            this.lstDifferingLayerNames.Size = new System.Drawing.Size(319, 160);
            this.lstDifferingLayerNames.TabIndex = 4;
            this.lstDifferingLayerNames.SelectedIndexChanged += new System.EventHandler(this.lstDifferingLayerNames_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Länge der Layernamen";
            // 
            // txtLayerNameLength
            // 
            this.txtLayerNameLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLayerNameLength.Location = new System.Drawing.Point(134, 6);
            this.txtLayerNameLength.Name = "txtLayerNameLength";
            this.txtLayerNameLength.Size = new System.Drawing.Size(62, 20);
            this.txtLayerNameLength.TabIndex = 2;
            this.txtLayerNameLength.TextChanged += new System.EventHandler(this.txtLayerNameLength_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Abweichende Layer";
            // 
            // txtLayerName
            // 
            this.txtLayerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLayerName.Location = new System.Drawing.Point(12, 228);
            this.txtLayerName.Name = "txtLayerName";
            this.txtLayerName.Size = new System.Drawing.Size(220, 20);
            this.txtLayerName.TabIndex = 5;
            this.txtLayerName.TextChanged += new System.EventHandler(this.txtLayerName_TextChanged);
            // 
            // btnRename
            // 
            this.btnRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRename.Location = new System.Drawing.Point(238, 228);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(93, 23);
            this.btnRename.TabIndex = 6;
            this.btnRename.Text = "Umbenennen";
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(256, 260);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Beenden";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDifferingInList
            // 
            this.btnDifferingInList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDifferingInList.Location = new System.Drawing.Point(202, 4);
            this.btnDifferingInList.Name = "btnDifferingInList";
            this.btnDifferingInList.Size = new System.Drawing.Size(129, 23);
            this.btnDifferingInList.TabIndex = 3;
            this.btnDifferingInList.Text = "Abweichende anzeigen";
            this.btnDifferingInList.UseVisualStyleBackColor = true;
            this.btnDifferingInList.Click += new System.EventHandler(this.btnDifferingInList_Click);
            // 
            // LayerNamesLengthFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 295);
            this.Controls.Add(this.btnDifferingInList);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.txtLayerName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLayerNameLength);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstDifferingLayerNames);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(359, 334);
            this.Name = "LayerNamesLengthFrm";
            this.Text = "Layernamen Längen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstDifferingLayerNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLayerNameLength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLayerName;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDifferingInList;
    }
}