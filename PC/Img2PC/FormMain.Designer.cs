namespace Img2PC
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.lblServiceState = new System.Windows.Forms.Label();
            this.lblLocalIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblIPValue = new System.Windows.Forms.Label();
            this.lblPortValue = new System.Windows.Forms.Label();
            this.lbReceivedFiles = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnFolder = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSaveAs = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblServiceState
            // 
            this.lblServiceState.AutoSize = true;
            this.lblServiceState.ForeColor = System.Drawing.Color.Red;
            this.lblServiceState.Location = new System.Drawing.Point(16, 49);
            this.lblServiceState.Name = "lblServiceState";
            this.lblServiceState.Size = new System.Drawing.Size(65, 12);
            this.lblServiceState.TabIndex = 4;
            this.lblServiceState.Text = "服务未启动";
            // 
            // lblLocalIP
            // 
            this.lblLocalIP.AutoSize = true;
            this.lblLocalIP.Location = new System.Drawing.Point(15, 9);
            this.lblLocalIP.Name = "lblLocalIP";
            this.lblLocalIP.Size = new System.Drawing.Size(77, 12);
            this.lblLocalIP.TabIndex = 0;
            this.lblLocalIP.Text = "本机IP地址：";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(247, 9);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(65, 12);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "服务端口：";
            // 
            // lblIPValue
            // 
            this.lblIPValue.AutoSize = true;
            this.lblIPValue.ForeColor = System.Drawing.Color.Red;
            this.lblIPValue.Location = new System.Drawing.Point(99, 9);
            this.lblIPValue.Name = "lblIPValue";
            this.lblIPValue.Size = new System.Drawing.Size(47, 12);
            this.lblIPValue.TabIndex = 1;
            this.lblIPValue.Text = "0.0.0.0";
            // 
            // lblPortValue
            // 
            this.lblPortValue.AutoSize = true;
            this.lblPortValue.ForeColor = System.Drawing.Color.Red;
            this.lblPortValue.Location = new System.Drawing.Point(331, 9);
            this.lblPortValue.Name = "lblPortValue";
            this.lblPortValue.Size = new System.Drawing.Size(29, 12);
            this.lblPortValue.TabIndex = 3;
            this.lblPortValue.Text = "6666";
            // 
            // lbReceivedFiles
            // 
            this.lbReceivedFiles.FormattingEnabled = true;
            this.lbReceivedFiles.ItemHeight = 12;
            this.lbReceivedFiles.Location = new System.Drawing.Point(6, 20);
            this.lbReceivedFiles.Name = "lbReceivedFiles";
            this.lbReceivedFiles.Size = new System.Drawing.Size(225, 208);
            this.lbReceivedFiles.TabIndex = 0;
            this.lbReceivedFiles.SelectedIndexChanged += new System.EventHandler(this.lbReceivedFiles_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(237, 39);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(221, 140);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(239, 185);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(47, 43);
            this.btnCopy.TabIndex = 2;
            this.btnCopy.Text = "复制";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(413, 185);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(45, 43);
            this.btnFolder.TabIndex = 5;
            this.btnFolder.Text = "打开目录";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(345, 185);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(46, 43);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "打开文件";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(237, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "预览图片：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSaveAs);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lbReceivedFiles);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.btnOpen);
            this.groupBox1.Controls.Add(this.btnCopy);
            this.groupBox1.Controls.Add(this.btnFolder);
            this.groupBox1.Location = new System.Drawing.Point(12, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(471, 242);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "接收到的文件";
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Location = new System.Drawing.Point(292, 185);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(47, 43);
            this.btnSaveAs.TabIndex = 3;
            this.btnSaveAs.Text = "另存";
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 332);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblPortValue);
            this.Controls.Add(this.lblIPValue);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblLocalIP);
            this.Controls.Add(this.lblServiceState);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Img2PC";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServiceState;
        private System.Windows.Forms.Label lblLocalIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblIPValue;
        private System.Windows.Forms.Label lblPortValue;
        private System.Windows.Forms.ListBox lbReceivedFiles;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSaveAs;
    }
}

