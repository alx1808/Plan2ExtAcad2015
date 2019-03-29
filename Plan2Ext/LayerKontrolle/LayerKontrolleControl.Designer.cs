namespace Plan2Ext.LayerKontrolle
{
    partial class LayerKontrolleControl
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
            this.lstAllLayers = new System.Windows.Forms.ListBox();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAllLayerOn = new System.Windows.Forms.Button();
            this.btnGetAlwaysOn = new System.Windows.Forms.Button();
            this.lstAlwaysOn = new System.Windows.Forms.ListBox();
            this.grpBoxEntityTypes = new System.Windows.Forms.GroupBox();
            this.lstEntityTypes = new System.Windows.Forms.ListBox();
            this.lblColorPropertyMode = new System.Windows.Forms.Label();
            this.lblLineTypePropertyMode = new System.Windows.Forms.Label();
            this.lblLineWeightPropertyMode = new System.Windows.Forms.Label();
            this.btnCheckVonlayer = new System.Windows.Forms.Button();
            this.lstCheckLayernameLength = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.grpBoxEntityTypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstAllLayers
            // 
            this.lstAllLayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAllLayers.FormattingEnabled = true;
            this.lstAllLayers.Location = new System.Drawing.Point(3, 3);
            this.lstAllLayers.Name = "lstAllLayers";
            this.lstAllLayers.Size = new System.Drawing.Size(254, 82);
            this.lstAllLayers.TabIndex = 0;
            this.lstAllLayers.SelectedIndexChanged += new System.EventHandler(this.lstAllLayers_SelectedIndexChanged);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrevious.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnPrevious.Location = new System.Drawing.Point(198, 91);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(25, 23);
            this.btnPrevious.TabIndex = 1;
            this.btnPrevious.Text = "";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnNext.Location = new System.Drawing.Point(229, 91);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(25, 23);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnAllLayerOn);
            this.groupBox1.Controls.Add(this.btnGetAlwaysOn);
            this.groupBox1.Controls.Add(this.lstAlwaysOn);
            this.groupBox1.Location = new System.Drawing.Point(3, 120);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(254, 108);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Immer ein";
            // 
            // btnAllLayerOn
            // 
            this.btnAllLayerOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllLayerOn.Location = new System.Drawing.Point(92, 81);
            this.btnAllLayerOn.Name = "btnAllLayerOn";
            this.btnAllLayerOn.Size = new System.Drawing.Size(75, 23);
            this.btnAllLayerOn.TabIndex = 4;
            this.btnAllLayerOn.Text = "Alle Ein";
            this.btnAllLayerOn.UseVisualStyleBackColor = true;
            this.btnAllLayerOn.Click += new System.EventHandler(this.btnAllLayerOn_Click);
            // 
            // btnGetAlwaysOn
            // 
            this.btnGetAlwaysOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetAlwaysOn.Location = new System.Drawing.Point(173, 81);
            this.btnGetAlwaysOn.Name = "btnGetAlwaysOn";
            this.btnGetAlwaysOn.Size = new System.Drawing.Size(75, 23);
            this.btnGetAlwaysOn.TabIndex = 1;
            this.btnGetAlwaysOn.Text = "...";
            this.btnGetAlwaysOn.UseVisualStyleBackColor = true;
            this.btnGetAlwaysOn.Click += new System.EventHandler(this.btnGetAlwaysOn_Click);
            // 
            // lstAlwaysOn
            // 
            this.lstAlwaysOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAlwaysOn.FormattingEnabled = true;
            this.lstAlwaysOn.Location = new System.Drawing.Point(6, 19);
            this.lstAlwaysOn.Name = "lstAlwaysOn";
            this.lstAlwaysOn.Size = new System.Drawing.Size(242, 56);
            this.lstAlwaysOn.TabIndex = 0;
            this.lstAlwaysOn.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstAlwaysOn_KeyUp);
            // 
            // grpBoxEntityTypes
            // 
            this.grpBoxEntityTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBoxEntityTypes.Controls.Add(this.lstEntityTypes);
            this.grpBoxEntityTypes.Location = new System.Drawing.Point(3, 234);
            this.grpBoxEntityTypes.Name = "grpBoxEntityTypes";
            this.grpBoxEntityTypes.Size = new System.Drawing.Size(254, 152);
            this.grpBoxEntityTypes.TabIndex = 5;
            this.grpBoxEntityTypes.TabStop = false;
            this.grpBoxEntityTypes.Text = "Elementtypen";
            // 
            // lstEntityTypes
            // 
            this.lstEntityTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstEntityTypes.FormattingEnabled = true;
            this.lstEntityTypes.Location = new System.Drawing.Point(3, 16);
            this.lstEntityTypes.Name = "lstEntityTypes";
            this.lstEntityTypes.Size = new System.Drawing.Size(248, 133);
            this.lstEntityTypes.TabIndex = 0;
            // 
            // lblColorPropertyMode
            // 
            this.lblColorPropertyMode.AutoSize = true;
            this.lblColorPropertyMode.Location = new System.Drawing.Point(6, 389);
            this.lblColorPropertyMode.Name = "lblColorPropertyMode";
            this.lblColorPropertyMode.Size = new System.Drawing.Size(0, 13);
            this.lblColorPropertyMode.TabIndex = 6;
            // 
            // lblLineTypePropertyMode
            // 
            this.lblLineTypePropertyMode.AutoSize = true;
            this.lblLineTypePropertyMode.Location = new System.Drawing.Point(6, 402);
            this.lblLineTypePropertyMode.Name = "lblLineTypePropertyMode";
            this.lblLineTypePropertyMode.Size = new System.Drawing.Size(0, 13);
            this.lblLineTypePropertyMode.TabIndex = 7;
            // 
            // lblLineWeightPropertyMode
            // 
            this.lblLineWeightPropertyMode.AutoSize = true;
            this.lblLineWeightPropertyMode.Location = new System.Drawing.Point(6, 415);
            this.lblLineWeightPropertyMode.Name = "lblLineWeightPropertyMode";
            this.lblLineWeightPropertyMode.Size = new System.Drawing.Size(0, 13);
            this.lblLineWeightPropertyMode.TabIndex = 8;
            // 
            // btnCheckVonlayer
            // 
            this.btnCheckVonlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckVonlayer.Location = new System.Drawing.Point(145, 392);
            this.btnCheckVonlayer.Name = "btnCheckVonlayer";
            this.btnCheckVonlayer.Size = new System.Drawing.Size(112, 23);
            this.btnCheckVonlayer.TabIndex = 9;
            this.btnCheckVonlayer.Text = "Vonlayer prüfen";
            this.btnCheckVonlayer.UseVisualStyleBackColor = true;
            this.btnCheckVonlayer.Click += new System.EventHandler(this.btnCheckVonlayer_Click);
            // 
            // lstCheckLayernameLength
            // 
            this.lstCheckLayernameLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCheckLayernameLength.Location = new System.Drawing.Point(145, 418);
            this.lstCheckLayernameLength.Name = "lstCheckLayernameLength";
            this.lstCheckLayernameLength.Size = new System.Drawing.Size(112, 23);
            this.lstCheckLayernameLength.TabIndex = 10;
            this.lstCheckLayernameLength.Text = "Layerlängenprüfung";
            this.lstCheckLayernameLength.UseVisualStyleBackColor = true;
            this.lstCheckLayernameLength.Click += new System.EventHandler(this.lstCheckLayernameLength_Click);
            // 
            // LayerKontrolleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstCheckLayernameLength);
            this.Controls.Add(this.btnCheckVonlayer);
            this.Controls.Add(this.lblLineWeightPropertyMode);
            this.Controls.Add(this.lblLineTypePropertyMode);
            this.Controls.Add(this.lblColorPropertyMode);
            this.Controls.Add(this.grpBoxEntityTypes);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.lstAllLayers);
            this.Name = "LayerKontrolleControl";
            this.Size = new System.Drawing.Size(260, 550);
            this.groupBox1.ResumeLayout(false);
            this.grpBoxEntityTypes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstAllLayers;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGetAlwaysOn;
        private System.Windows.Forms.ListBox lstAlwaysOn;
        private System.Windows.Forms.Button btnAllLayerOn;
        private System.Windows.Forms.GroupBox grpBoxEntityTypes;
        private System.Windows.Forms.ListBox lstEntityTypes;
        private System.Windows.Forms.Label lblColorPropertyMode;
        private System.Windows.Forms.Label lblLineTypePropertyMode;
        private System.Windows.Forms.Label lblLineWeightPropertyMode;
        private System.Windows.Forms.Button btnCheckVonlayer;
        private System.Windows.Forms.Button lstCheckLayernameLength;
    }
}
