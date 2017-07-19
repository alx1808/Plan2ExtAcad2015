namespace Plan2Ext
{
    partial class ConfigForm
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
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblValues = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstVars = new System.Windows.Forms.ListBox();
            this.lstVals = new System.Windows.Forms.ListBox();
            this.txtWert = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(12, 9);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "Kategorie";
            // 
            // cmbCategory
            // 
            this.cmbCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(15, 25);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(461, 21);
            this.cmbCategory.TabIndex = 1;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            // 
            // lblValues
            // 
            this.lblValues.AutoSize = true;
            this.lblValues.Location = new System.Drawing.Point(12, 49);
            this.lblValues.Name = "lblValues";
            this.lblValues.Size = new System.Drawing.Size(36, 13);
            this.lblValues.TabIndex = 2;
            this.lblValues.Text = "Werte";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(15, 65);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstVars);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lstVals);
            this.splitContainer1.Size = new System.Drawing.Size(461, 195);
            this.splitContainer1.SplitterDistance = 297;
            this.splitContainer1.TabIndex = 4;
            this.splitContainer1.TabStop = false;
            // 
            // lstVars
            // 
            this.lstVars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVars.FormattingEnabled = true;
            this.lstVars.Location = new System.Drawing.Point(0, 0);
            this.lstVars.Name = "lstVars";
            this.lstVars.Size = new System.Drawing.Size(297, 195);
            this.lstVars.TabIndex = 4;
            this.lstVars.SelectedIndexChanged += new System.EventHandler(this.lstVars_SelectedIndexChanged);
            // 
            // lstVals
            // 
            this.lstVals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVals.Enabled = false;
            this.lstVals.FormattingEnabled = true;
            this.lstVals.Location = new System.Drawing.Point(0, 0);
            this.lstVals.Name = "lstVals";
            this.lstVals.Size = new System.Drawing.Size(160, 195);
            this.lstVals.TabIndex = 6;
            this.lstVals.TabStop = false;
            // 
            // txtWert
            // 
            this.txtWert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWert.Location = new System.Drawing.Point(15, 266);
            this.txtWert.Name = "txtWert";
            this.txtWert.Size = new System.Drawing.Size(380, 20);
            this.txtWert.TabIndex = 5;
            this.txtWert.Validating += new System.ComponentModel.CancelEventHandler(this.txtWert_Validating);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(401, 266);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 296);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtWert);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lblValues);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lblCategory);
            this.Name = "ConfigForm";
            this.ShowIcon = false;
            this.Text = "Konfiguration";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblValues;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstVars;
        private System.Windows.Forms.ListBox lstVals;
        private System.Windows.Forms.TextBox txtWert;
        private System.Windows.Forms.Button btnOk;
    }
}