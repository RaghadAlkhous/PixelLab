using System.Drawing;

namespace PixelLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quantizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beforeAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._channelPanel = new PixelLab.Controls.ChannelPanelControl();

            this.visualizingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visualizing2DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visualizing3DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.panelMain = new System.Windows.Forms.Panel();
            this.pictureBoxMain = new System.Windows.Forms.PictureBox();
            this.panelControls = new System.Windows.Forms.Panel();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.lblImageInfo = new System.Windows.Forms.Label();
            this.groupBoxColorSpace = new System.Windows.Forms.GroupBox();
            this.cmbColorSpace = new System.Windows.Forms.ComboBox();
            this.lblColorSpace = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
            this.panelControls.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.groupBoxColorSpace.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();


            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.visualizingToolStripMenuItem
            });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1000, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.BackColor = Color.FromArgb(45, 45, 48);
            this.menuStrip1.ForeColor = Color.White;

            // 
            // visualizingToolStripMenuItem
            // 
            this.visualizingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.visualizing2DToolStripMenuItem,
                this.visualizing3DToolStripMenuItem
            });
            this.visualizingToolStripMenuItem.Name = "visualizingToolStripMenuItem";
            this.visualizingToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.visualizingToolStripMenuItem.Text = "Visualize";
            // 
            // visualizing2DToolStripMenuItem
            // 
            this.visualizing2DToolStripMenuItem.Name = "visualizing2DToolStripMenuItem";
            this.visualizing2DToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.visualizing2DToolStripMenuItem.Text = "2D";
            this.visualizing2DToolStripMenuItem.Click += new System.EventHandler(this.Open2DColorSpaces_Click);
            // 
            // visualizing3DToolStripMenuItem
            // 
            this.visualizing3DToolStripMenuItem.Name = "visualizing3DToolStripMenuItem";
            this.visualizing3DToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.visualizing3DToolStripMenuItem.Text = "3D";
            this.visualizing3DToolStripMenuItem.Click += new System.EventHandler(this.Open3DColorSpaces_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.openToolStripMenuItem,
                this.saveToolStripMenuItem,
                this.imageInfoToolStripMenuItem,
                this.resetToolStripMenuItem,
                this.quantizeToolStripMenuItem,
                this.beforeAfterToolStripMenuItem,
                this.toolStripSeparator1,
                this.exitToolStripMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.openToolStripMenuItem.Text = "Open Image...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.saveToolStripMenuItem.Text = "Save...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // imageInfoToolStripMenuItem
            // 
            this.imageInfoToolStripMenuItem.Name = "imageInfoToolStripMenuItem";
            this.imageInfoToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.imageInfoToolStripMenuItem.Text = "Image Info...";
            this.imageInfoToolStripMenuItem.Click += new System.EventHandler(this.imageInfoToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.resetToolStripMenuItem.Text = "Reset To Original...";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);// 
            //
            // quanitizeToolStripMenuItem
            // 
            this.quantizeToolStripMenuItem.Name = "quantizeToolStripMenuItem";
            this.quantizeToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.quantizeToolStripMenuItem.Text = "Quantize Colors...";
            this.quantizeToolStripMenuItem.Click += new System.EventHandler(this.quantizeToolStripMenuItem_Click);
            //
            // beforeAfterToolStripMenuItem
            // 
            this.beforeAfterToolStripMenuItem.Name = "beforeAfterToolStripMenuItem";
            this.beforeAfterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.beforeAfterToolStripMenuItem.Text = "Before-After Comparsion";
            this.beforeAfterToolStripMenuItem.Click += new System.EventHandler(this.BtnBeforeAfter_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(163, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.exitToolStripMenuItem.Text = "Quit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.BackColor = Color.FromArgb(45, 45, 48);
            this.splitContainer1.ForeColor = Color.White;

            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelMain);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelControls);
            this.splitContainer1.Size = new System.Drawing.Size(1000, 563);
            this.splitContainer1.SplitterDistance = 699;
            this.splitContainer1.TabIndex = 1;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.pictureBoxMain);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(699, 563);
            this.panelMain.TabIndex = 0;
            // 
            // pictureBoxMain
            // 
            this.pictureBoxMain.AllowDrop = true;
            this.pictureBoxMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMain.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxMain.Name = "pictureBoxMain";
            this.pictureBoxMain.Size = new System.Drawing.Size(699, 563);
            this.pictureBoxMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxMain.TabIndex = 0;
            this.pictureBoxMain.TabStop = false;
            // 
            // panelControls
            // 
            this.panelControls.AutoScroll = true;
            this.panelControls.Controls.Add(this._channelPanel);
            this.panelControls.Controls.Add(this.groupBoxColorSpace);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControls.Location = new System.Drawing.Point(0, 0);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(297, 563);
            this.panelControls.TabIndex = 0;

            this._channelPanel.SettingsChanged += this.ChannelPanel_SettingsChanged;
            this._channelPanel.ClearPreviewRequested += this.ChannelPanel_ClearPreviewRequested;
            this._channelPanel.ApplyToWorkingRequested += this.ChannelPanel_ApplyToWorkingRequested;

            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.lblImageInfo);
            this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInfo.Location = new System.Drawing.Point(0, 60);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(297, 503);
            this.groupBoxInfo.TabIndex = 1;
            this.groupBoxInfo.TabStop = false;
            // 
            // lblImageInfo
            // 
            this.lblImageInfo.AutoSize = true;
            this.lblImageInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImageInfo.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImageInfo.Location = new System.Drawing.Point(3, 16);
            this.lblImageInfo.Name = "lblImageInfo";
            this.lblImageInfo.Size = new System.Drawing.Size(0, 13);
            this.lblImageInfo.TabIndex = 0;
            this.lblImageInfo.TabIndex = 0;

            // 
            // groupBoxColorSpace
            // 
            this.groupBoxColorSpace.Controls.Add(this.cmbColorSpace);
            this.groupBoxColorSpace.Controls.Add(this.lblColorSpace);
            this.groupBoxColorSpace.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxColorSpace.Location = new System.Drawing.Point(0, 0);
            this.groupBoxColorSpace.Name = "groupBoxColorSpace";
            this.groupBoxColorSpace.Size = new System.Drawing.Size(297, 60);
            this.groupBoxColorSpace.TabIndex = 0;
            this.groupBoxColorSpace.TabStop = false;
            this.groupBoxColorSpace.Text = "Color Space";
            this.groupBoxColorSpace.ForeColor = Color.White;
            // 
            // cmbColorSpace
            // 
            this.cmbColorSpace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColorSpace.FormattingEnabled = true;
            this.cmbColorSpace.Items.AddRange(new object[] {
            "RGB",
            "CMY",
            "HSV",
            "YUV",
            "YCbCr",
            "LAB"});
            this.cmbColorSpace.Location = new System.Drawing.Point(120, 25);
            this.cmbColorSpace.Name = "cmbColorSpace";
            this.cmbColorSpace.Size = new System.Drawing.Size(120, 21);
            this.cmbColorSpace.TabIndex = 1;
            this.cmbColorSpace.SelectedIndexChanged += new System.EventHandler(this.cmbColorSpace_SelectedIndexChanged);
            // 
            // lblColorSpace
            // 
            this.lblColorSpace.AutoSize = true;
            this.lblColorSpace.Location = new System.Drawing.Point(10, 28);
            this.lblColorSpace.Name = "lblColorSpace";
            this.lblColorSpace.Size = new System.Drawing.Size(77, 13);
            this.lblColorSpace.TabIndex = 0;
            this.lblColorSpace.Text = "Select Color Space:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 587);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ForeColor = Color.White;
            this.statusStrip1.BackColor = Color.FromArgb(0, 122, 204);
            this.statusStrip1.Height= 28;

            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(103, 17);
            this.lblStatus.Text = "Ready - Drag Iamge";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 609);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "PixelLab - مختبر الصور";
            //this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
            this.panelControls.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.groupBoxColorSpace.ResumeLayout(false);
            this.groupBoxColorSpace.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visualizingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visualizing2DToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visualizing3DToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quantizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beforeAfterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.PictureBox pictureBoxMain;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.GroupBox groupBoxColorSpace;
        private System.Windows.Forms.ComboBox cmbColorSpace;
        private System.Windows.Forms.Label lblColorSpace;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.Label lblImageInfo;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private PixelLab.Controls.ChannelPanelControl _channelPanel;
    }
}