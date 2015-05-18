using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Img2PC.SocketServer;
using TcpCSFramework;

namespace Img2PC
{
    public delegate void MyInvoke(string path);

    public partial class FormMain : Form
    {
        private TcpSvr _server = null;

        public FormMain()
        {
            InitializeComponent();

            // 创建接收目录

            //保存到全局
            SocketServer.GlobalParams.RecvFileFolder = RecvFolder;

            try
            {
                PrepareFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "无法创建文件接收目录！" + ex.Message, "Img2PC", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Close();
            }

            //绑定事件
            GlobalParams.OnFileReceived += OnFileReceived;

            //显示本机IP
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            string myip = "";
            foreach (var item in IpEntry.AddressList)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!String.IsNullOrEmpty(myip))
                    {
                        myip += "\n";
                    }

                    myip += item.ToString();
                }
            }
            lblIPValue.Text = myip;

            //显示端口
            ushort port = 666;
            bool success = false;
            while (success == false && port < 676)
            {
                _server = new TcpSvr(port);

                _server.ClientConn += new NetEvent(OnClientConnected);
                _server.RecvData += new NetEvent(OnDataReceived);

                try
                {
                    _server.Start();
                    success = true;
                    break;
                }
                catch (Exception ex)
                {

                }

                port++;
            }

            lblPortValue.Text = port.ToString();

            if (success)
            {
                lblServiceState.Text = "服务已运行，您现在可以从手机发送图片了。";
            }
            else
            {
                lblServiceState.Text = "服务未能正常启动！";
            }


        }

        public void OnClientConnected(object sender, NetEventArgs e)
        {
            //MessageBox.Show("客户端连接了!");
        }

        private void PrepareFolder()
        {

            if (!Directory.Exists(RecvFolder))
            {
                try
                {
                    Directory.CreateDirectory(RecvFolder);
                }
                catch (Exception ex)
                {
                    //Can not create folder
                    throw;
                }
            }
        }

        public void OnDataReceived(object sender, NetEventArgs e)
        {
            
        }



        /// <summary>
        /// 保存接收文件的路径
        /// </summary>
        public string RecvFolder
        {
            get
            {
                return Path.Combine(Application.StartupPath, "Recv");
            }
        }

        public void ReceiveFile(string fileName)
        {
            lbReceivedFiles.Items.Add(fileName);
            lbReceivedFiles.SelectedItem = fileName;

            PreviewFile(fileName);
        }

        private void PreviewFile(string fileName)
        {
            if (IsImgFile(fileName))
            {
                string pathName = Path.Combine(RecvFolder, fileName);

                try
                {
                    Image image = new Bitmap(pathName);
                    pictureBox1.Image = image;

                    //Clipboard.SetImage(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "出错！" + ex.Message);
                }
            }
            else
            {
                pictureBox1.Image = null;
                pictureBox1.Text = "无法预览";
            }
        }

        public bool IsImgFile(string fileName)
        {
            string ext = Path.GetExtension(fileName.ToLower());

            if (Array.IndexOf(new string[] { ".jpg", ".png", ".bmp", ".gif" }, ext) >= 0)
            {
                return true;
            }

            return false;
        }

        

        /// <summary>
        /// 接收到网络事件，需要异步调用界面线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnFileReceived(object sender, NetPacketEventArgs e)
        {
            string fileName = Path.GetFileName(e.FilePathName);

            MyInvoke mi = new MyInvoke(ReceiveFile);
            BeginInvoke(mi, new object[] { fileName });

        }

        //复制文件到剪贴板
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (lbReceivedFiles.SelectedItem != null)
            {
                string fileName = (string) lbReceivedFiles.SelectedItem;
                string pathName = Path.Combine(GlobalParams.RecvFileFolder, fileName);
                if (IsImgFile(fileName))
                {
                    Image image = new Bitmap(pathName);
                    Clipboard.SetImage(image);
                }

                string[] file = new string[1];
                file[0] = pathName;
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, file);
                Clipboard.SetDataObject(dataObject, true);

                MessageBox.Show("文件已复制到剪切板。");
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (lbReceivedFiles.SelectedItem != null)
            {
                string fileName = (string)lbReceivedFiles.SelectedItem;
                string pathName = Path.Combine(GlobalParams.RecvFileFolder, fileName);

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "文件另存为...";
                dialog.FileName = fileName;

                dialog.Filter = Path.GetExtension(fileName) + " File|*" + Path.GetExtension(fileName) + "|任意文件|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!String.IsNullOrEmpty(dialog.FileName))
                    {
                        File.Copy(pathName, dialog.FileName);
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (lbReceivedFiles.SelectedItem != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(Path.Combine(GlobalParams.RecvFileFolder, (string)lbReceivedFiles.SelectedItem));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开文件夹出错，ex=" + ex.Message);
                }
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(GlobalParams.RecvFileFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开文件夹出错，ex=" + ex.Message);
            }
        }

        private void lbReceivedFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbReceivedFiles.SelectedItem != null)
            {
                PreviewFile((string)lbReceivedFiles.SelectedItem);
            }
        }
    }
}
