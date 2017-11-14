namespace Plan2Ext.Vorauswahl
{
    partial class VorauswahlControl
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
            this.grpBlocknamen = new System.Windows.Forms.GroupBox();
            this.btnSelBlocknamen = new System.Windows.Forms.Button();
            this.lstBlocknamen = new System.Windows.Forms.ListBox();
            this.grpLayer = new System.Windows.Forms.GroupBox();
            this.btnSelLayer = new System.Windows.Forms.Button();
            this.lstLayer = new System.Windows.Forms.ListBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.grpBlocknamen.SuspendLayout();
            this.grpLayer.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBlocknamen
            // 
            this.grpBlocknamen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBlocknamen.Controls.Add(this.btnSelBlocknamen);
            this.grpBlocknamen.Controls.Add(this.lstBlocknamen);
            this.grpBlocknamen.Location = new System.Drawing.Point(0, 0);
            this.grpBlocknamen.Name = "grpBlocknamen";
            this.grpBlocknamen.Size = new System.Drawing.Size(151, 111);
            this.grpBlocknamen.TabIndex = 0;
            this.grpBlocknamen.TabStop = false;
            this.grpBlocknamen.Text = "Blocknamen";
            // 
            // btnSelBlocknamen
            // 
            this.btnSelBlocknamen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelBlocknamen.Location = new System.Drawing.Point(70, 81);
            this.btnSelBlocknamen.Name = "btnSelBlocknamen";
            this.btnSelBlocknamen.Size = new System.Drawing.Size(75, 23);
            this.btnSelBlocknamen.TabIndex = 1;
            this.btnSelBlocknamen.Text = "Auswählen...";
            this.btnSelBlocknamen.UseVisualStyleBackColor = true;
            this.btnSelBlocknamen.Click += new System.EventHandler(this.btnSelBlocknamen_Click);
            // 
            // lstBlocknamen
            // 
            this.lstBlocknamen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBlocknamen.FormattingEnabled = true;
            this.lstBlocknamen.Location = new System.Drawing.Point(6, 19);
            this.lstBlocknamen.Name = "lstBlocknamen";
            this.lstBlocknamen.Size = new System.Drawing.Size(139, 56);
            this.lstBlocknamen.TabIndex = 0;
            this.lstBlocknamen.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstBlocknamen_KeyDown);
            // 
            // grpLayer
            // 
            this.grpLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLayer.Controls.Add(this.btnSelLayer);
            this.grpLayer.Controls.Add(this.lstLayer);
            this.grpLayer.Location = new System.Drawing.Point(0, 117);
            this.grpLayer.Name = "grpLayer";
            this.grpLayer.Size = new System.Drawing.Size(151, 111);
            this.grpLayer.TabIndex = 2;
            this.grpLayer.TabStop = false;
            this.grpLayer.Text = "Layer";
            // 
            // btnSelLayer
            // 
            this.btnSelLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelLayer.Location = new System.Drawing.Point(70, 81);
            this.btnSelLayer.Name = "btnSelLayer";
            this.btnSelLayer.Size = new System.Drawing.Size(75, 23);
            this.btnSelLayer.TabIndex = 1;
            this.btnSelLayer.Text = "Auswählen...";
            this.btnSelLayer.UseVisualStyleBackColor = true;
            this.btnSelLayer.Click += new System.EventHandler(this.btnSelLayer_Click);
            // 
            // lstLayer
            // 
            this.lstLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLayer.FormattingEnabled = true;
            this.lstLayer.Location = new System.Drawing.Point(6, 19);
            this.lstLayer.Name = "lstLayer";
            this.lstLayer.Size = new System.Drawing.Size(139, 56);
            this.lstLayer.TabIndex = 0;
            this.lstLayer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstLayer_KeyDown);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(76, 234);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Vorauswahl";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // VorauswahlControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.grpLayer);
            this.Controls.Add(this.grpBlocknamen);
            this.Name = "VorauswahlControl";
            this.Size = new System.Drawing.Size(154, 268);
            this.grpBlocknamen.ResumeLayout(false);
            this.grpLayer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpBlocknamen;
        private System.Windows.Forms.Button btnSelBlocknamen;
        private System.Windows.Forms.GroupBox grpLayer;
        private System.Windows.Forms.Button btnSelLayer;
        internal System.Windows.Forms.ListBox lstBlocknamen;
        internal System.Windows.Forms.ListBox lstLayer;
        private System.Windows.Forms.Button btnSelect;
    }
}
