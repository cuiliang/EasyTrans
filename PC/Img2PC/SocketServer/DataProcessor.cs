using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TcpCSFramework;

namespace Img2PC.SocketServer
{
    /// <summary>
    /// 网络包事件参数
    /// </summary>
    public class NetPacketEventArgs:EventArgs
    {
        private PacketHeader _header;
        private string _strContent = String.Empty;
        private string _filePathName = String.Empty;

        public NetPacketEventArgs(PacketHeader packetHeader)
        {
            _header = packetHeader;
        }

        /// <summary>
        /// 命令的文本内容
        /// </summary>
        public string StringContent { get; set; }

        /// <summary>
        /// 接收到文件的保存路径
        /// </summary>
        public string FilePathName { get; set; }

        /// <summary>
        /// 消息头
        /// </summary>
        public PacketHeader PacketHeader{get { return _header; }}
    }

    public delegate void NetPacketEventHandler(object sender, NetPacketEventArgs e);
    

    /// <summary>
    /// 数据处理类
    /// </summary>
    public class DataProcessor
    {
        /// <summary>
        /// 接收到了一个数据包
        /// </summary>
        public event NetPacketEventHandler NetPacketReceived;

        public event NetPacketEventHandler FileReceived;

        /// <summary>
        /// 客户端Socket对象
        /// </summary>
        private Session _session;

        /// <summary>
        /// 接收文件的保存路径
        /// </summary>
        private string _recvPath;

        public DataProcessor(Session session)
        {
            _session = session;
        }

        public const int BUFF_LEN = 3 * 1024 * 1024; //缓存大小

        private byte[] _buff = new byte[BUFF_LEN];
        
        private int _currBuffPos = 0; //当前缓存中的数据长度

        //private bool _isRecvingHead = true;

        private PacketHeader _currHeader = null;

        private FileStream _recvFile = null; // 文件

        private long _recvedFileLen = 0; //已接收的文件长度

        private bool IsRecvingHead()
        {
            return _currHeader == null;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="data">要处理的数据（从网络接收到的内容）</param>
        /// <param name="startPos">处理到的位置</param>
        /// <param name="length">数据总长度</param>
        public void ProcessData(byte[] data, int startPos, int length)
        {
            /// 如果没有缓存数据，则直接下来是一个包的包头。 如果接收长度超过包头长度，则直接读取包头。

            int currPos = startPos; //未读取的数据位置
            

            // 如果等待接收消息头
            if (IsRecvingHead())
            {
                //如果待处理缓存中含有数据
                if (_currBuffPos > PacketHeader.HEADER_LEN)
                {
                    //应该不会发生
                    throw new Exception("已经有未处理的消息头");
                }

                // 正常情况下，待处理缓存中的长度应该会少于消息头长度。

                int copyLen = PacketHeader.HEADER_LEN - _currBuffPos;
                if ( currPos + copyLen > length)
                {
                    copyLen = length - currPos;
                }

                Buffer.BlockCopy(data, currPos, _buff, _currBuffPos, copyLen);
                _currBuffPos += copyLen;
                currPos += copyLen;

                // 接收到数据的长度大于消息头长度，可以读取了。
                if (_currBuffPos >= PacketHeader.HEADER_LEN)
                {
                    _currHeader = PacketHeader.ReadFromPacket(_buff, 0);
                    

                    if (_currHeader == null)
                    {
                        throw new Exception("接收到的消息不正常！");
                    }

                    //清空buffer
                    _currBuffPos = 0;

                    //Debug.WriteLine("ContentLen:" + _currHeader.ContentLength);

                    // 开始接收消息内容
                    if ( _currHeader.ContentLength < 0)
                    {
                        throw new Exception("内容长度小于0！");
                    }

                    if (_currHeader.ContentLength > BUFF_LEN-50)
                    {
                        //消息体长度太长，或者长度未知，写入文件
                        //TODO:
                        if (_recvFile != null)
                        {
                            throw new Exception("文件句柄已存在！");
                        }

                        _recvFile = MakeFile(_currFileInfo);
                    }else if (_currHeader.ContentLength > 0)
                    {
                        //普通消息，不写文件，先放入缓存
                    }

                    //处理剩余数据
                    if (currPos < length-1)
                    {
                        ProcessData(data, currPos, length);
                    }
                }
                else
                {
                    //尚未接收完报文头，继续等待下一个消息
                    return;
                }
            }
            else
            {
                //正在接收文件消息内容。
                if (_recvFile != null)
                {
                    //正在接收文件
                    long pendingLen = _currHeader.ContentLength - _recvedFileLen;
                    //Debug.WriteLine("PendingLen:" + pendingLen + "  DataLen:" + (length-currPos));
                    //接收的数据完成了文件长度
                    if ((length - currPos) >= pendingLen)
                    {
                        Debug.WriteLine("The last packet");

                        _recvFile.Write(data, currPos, (int)pendingLen);
                        currPos += (int) pendingLen;

                        //文件已接收完。
                        try
                        {
                            
                            _recvFile.Flush();
                            _recvFile.Close();
                            Debug.WriteLine("Writed FileLength:" + _recvFile.Length);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Closing file EX:" + ex.Message);
                        }
                        
                        OnFileReceived(_currHeader, _recvFile);

                        //销毁变量
                        _recvFile = null;
                        _recvedFileLen = 0;
                        _currHeader = null;

                        //继续处理后面的数据
                        if (currPos < length-1)
                        {
                            ProcessData(data, currPos, length);
                        }

                        return;
                    }
                    else
                    {
                        // 接收的内容没有完成包
                        _recvFile.Write(data, currPos, length - currPos);
                        _recvedFileLen += (length - currPos);

                        Debug.WriteLine("已接收的长度：" + _recvedFileLen);
                        return;
                    }
                }
                else
                {
                    //接收的普通包，如果数据够整包
                    if (_currBuffPos + (length - currPos) >= _currHeader.ContentLength)
                    {
                        int copyLen = (int)_currHeader.ContentLength - _currBuffPos;
                        Buffer.BlockCopy(data, currPos, _buff, _currBuffPos, copyLen);

                        currPos += copyLen;

                        //整包接收完成
                        OnPacketReceived(_currHeader, _buff, (int)_currHeader.ContentLength);
                        _currHeader = null;
                        
                        
                        //清空缓存
                        _currBuffPos = 0;

                        //处理剩余数据
                        if (currPos < length-1)
                        {
                            ProcessData(data, currPos, length);
                        }

                        return;
                    }
                    else 
                    {
                        //接收到的内容不够整包
                        int copyLen = length - currPos;
                        Buffer.BlockCopy(data, currPos, _buff, _currBuffPos, copyLen);
                        _currBuffPos += copyLen;
                    }
                }

            }

        }


        private FileStream MakeFile(FileInfo fileInfo)
        {
            //TODO:判断有没有接收到文件头信息，如果接收到，根据文件头信息命名。
            

            string fileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss.file");
            if (_currFileInfo !=null)
            {
                if (!String.IsNullOrEmpty(_currFileInfo.FileName))
                {
                    if (!String.IsNullOrEmpty(Path.GetExtension(_currFileInfo.FileName)))
                    {
                        fileName = _currFileInfo.FileName;
                    }
                    else
                    {
                        if (_currFileInfo.Mime.Contains("image"))
                        {
                            fileName = _currFileInfo.FileName + ".jpg";
                        }
                    }
                }
            }

            string pathName = Path.Combine(GlobalParams.RecvFileFolder, fileName);

            fileInfo.PathName = pathName;

            FileStream file = File.Create(pathName, 10240);
            return file;
        }


        private FileInfo _currFileInfo = null; //正在接收的文件的信息对象

        private void OnPacketReceived(PacketHeader header, byte[] content, int contentLen)
        {
            if (header.PacketType == PacketHeader.PACKET_TYPE_FILE_INFO)
            {
                if (_currFileInfo != null)
                {
                    throw new Exception("应该为null");
                }

                _currFileInfo = new FileInfo();

                String strContent = Encoding.UTF8.GetString(content,0,contentLen);

                Debug.WriteLine("文本内容：" + strContent);

                _currFileInfo.ParseFromString(strContent);
                _currFileInfo.ObjectSerial = header.ObjectSerial;
            }else if (header.PacketType == PacketHeader.PACKET_TYPE_FILE_CONTENT)
            {
                if (_currFileInfo == null)
                {
                    //应该先有文件信息
                    throw new Exception("应该先收到FileInfo包");
                }

                FileStream file = MakeFile(_currFileInfo);
                file.Write(content, 0, contentLen);
                file.Flush();
                file.Close();

                
                
                OnFileReceived(header, file);
            }

            if (header.PacketType != PacketHeader.PACKET_TYPE_FILE_INFO
                && header.PacketType != PacketHeader.PACKET_TYPE_FILE_CONTENT)
            {
                if (NetPacketReceived != null)
                {
                    NetPacketEventArgs e = new NetPacketEventArgs(header);
                    NetPacketReceived(this, e);
                }
            }

            Debug.WriteLine("接收到包");
        }

        private void OnFileReceived(PacketHeader header, FileStream fileStream)
        {
            if (FileReceived != null)
            {
                NetPacketEventArgs e = new NetPacketEventArgs(header);
                e.FilePathName = _currFileInfo.PathName;
                FileReceived(this, e);
            }

            _currFileInfo = null;
            Debug.WriteLine("接收到文件");
        }

        
        private void OutputDebug(byte[] data, int length)
        {
            //输出到调试窗口
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("RECV:");
            for (int i = 0; i < length; i++)
            {
                if (i % 10 == 0 && i != 0)
                {
                    sb.Append("   ");
                }

                if (i % 20 == 0 && i != 0)
                {
                    sb.AppendLine();
                }

                sb.AppendFormat("{0:x2} ", data[i]);
            }
            sb.AppendLine();

            Debug.Write(sb.ToString());
        }
    }
}
