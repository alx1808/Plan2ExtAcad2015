namespace Plan2Ext.Configuration
{
    partial class SetConfigForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbnSalk = new System.Windows.Forms.RadioButton();
            this.rbnNorm = new System.Windows.Forms.RadioButton();
            this.rbnBig = new System.Windows.Forms.RadioButton();
            this.rbnPlFm = new System.Windows.Forms.RadioButton();
            this.rbnFM = new System.Windows.Forms.RadioButton();
            this.rbnPlan2 = new System.Windows.Forms.RadioButton();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbnSalk);
            this.groupBox1.Controls.Add(this.rbnNorm);
            this.groupBox1.Controls.Add(this.rbnBig);
            this.groupBox1.Controls.Add(this.rbnPlFm);
            this.groupBox1.Controls.Add(this.rbnFM);
            this.groupBox1.Controls.Add(this.rbnPlan2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(156, 178);
            this.groupBox1.TabIndex = 40;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Konfigurationen";
            // 
            // rbnSalk
            // 
            this.rbnSalk.AutoSize = true;
            this.rbnSalk.Enabled = false;
            this.rbnSalk.Location = new System.Drawing.Point(6, 134);
            this.rbnSalk.Name = "rbnSalk";
            this.rbnSalk.Size = new System.Drawing.Size(52, 17);
            this.rbnSalk.TabIndex = 5;
            this.rbnSalk.TabStop = true;
            this.rbnSalk.Text = "SALK";
            this.rbnSalk.UseVisualStyleBackColor = true;
            this.rbnSalk.CheckedChanged += new System.EventHandler(this.rbnSalk_CheckedChanged);
            // 
            // rbnNorm
            // 
            this.rbnNorm.AutoSize = true;
            this.rbnNorm.Enabled = false;
            this.rbnNorm.Location = new System.Drawing.Point(6, 111);
            this.rbnNorm.Name = "rbnNorm";
            this.rbnNorm.Size = new System.Drawing.Size(58, 17);
            this.rbnNorm.TabIndex = 4;
            this.rbnNorm.TabStop = true;
            this.rbnNorm.Text = "NORM";
            this.rbnNorm.UseVisualStyleBackColor = true;
            this.rbnNorm.CheckedChanged += new System.EventHandler(this.rbnNorm_CheckedChanged);
            // 
            // rbnBig
            // 
            this.rbnBig.AutoSize = true;
            this.rbnBig.Enabled = false;
            this.rbnBig.Location = new System.Drawing.Point(6, 88);
            this.rbnBig.Name = "rbnBig";
            this.rbnBig.Size = new System.Drawing.Size(43, 17);
            this.rbnBig.TabIndex = 3;
            this.rbnBig.TabStop = true;
            this.rbnBig.Text = "BIG";
            this.rbnBig.UseVisualStyleBackColor = true;
            this.rbnBig.CheckedChanged += new System.EventHandler(this.rbnBig_CheckedChanged);
            // 
            // rbnPlFm
            // 
            this.rbnPlFm.AutoSize = true;
            this.rbnPlFm.Enabled = false;
            this.rbnPlFm.Location = new System.Drawing.Point(6, 65);
            this.rbnPlFm.Name = "rbnPlFm";
            this.rbnPlFm.Size = new System.Drawing.Size(70, 17);
            this.rbnPlFm.TabIndex = 2;
            this.rbnPlFm.TabStop = true;
            this.rbnPlFm.Text = "Plan2-FM";
            this.rbnPlFm.UseVisualStyleBackColor = true;
            this.rbnPlFm.CheckedChanged += new System.EventHandler(this.rbnPlFm_CheckedChanged);
            // 
            // rbnFM
            // 
            this.rbnFM.AutoSize = true;
            this.rbnFM.Enabled = false;
            this.rbnFM.Location = new System.Drawing.Point(6, 42);
            this.rbnFM.Name = "rbnFM";
            this.rbnFM.Size = new System.Drawing.Size(40, 17);
            this.rbnFM.TabIndex = 1;
            this.rbnFM.TabStop = true;
            this.rbnFM.Text = "FM";
            this.rbnFM.UseVisualStyleBackColor = true;
            this.rbnFM.CheckedChanged += new System.EventHandler(this.rbnFM_CheckedChanged);
            // 
            // rbnPlan2
            // 
            this.rbnPlan2.AutoSize = true;
            this.rbnPlan2.Enabled = false;
            this.rbnPlan2.Location = new System.Drawing.Point(6, 19);
            this.rbnPlan2.Name = "rbnPlan2";
            this.rbnPlan2.Size = new System.Drawing.Size(52, 17);
            this.rbnPlan2.TabIndex = 0;
            this.rbnPlan2.TabStop = true;
            this.rbnPlan2.Text = "Plan2";
            this.rbnPlan2.UseVisualStyleBackColor = true;
            this.rbnPlan2.CheckedChanged += new System.EventHandler(this.rbnPlan2_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(12, 196);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 41;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(93, 196);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 42;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SetConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(180, 231);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(196, 270);
            this.Name = "SetConfigForm";
            this.ShowIcon = false;
            this.Text = "Konfiguration setzen";
            this.Load += new System.EventHandler(this.SetConfigForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SetConfigForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbnSalk;
        private System.Windows.Forms.RadioButton rbnNorm;
        private System.Windows.Forms.RadioButton rbnBig;
        private System.Windows.Forms.RadioButton rbnPlFm;
        private System.Windows.Forms.RadioButton rbnFM;
        private System.Windows.Forms.RadioButton rbnPlan2;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}