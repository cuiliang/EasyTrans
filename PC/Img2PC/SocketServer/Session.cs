using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;
using Img2PC.SocketServer;

namespace TcpCSFramework
{

    /// <summary> 
    /// 客户端与服务器之间的会话类 
    /// 
    /// 版本: 1.1 
    /// 替换版本: 1.0 
    /// 
    /// 说明: 
    /// 会话类包含远程通讯端的状态,这些状态包括Socket,报文内容, 
    /// 客户端退出的类型(正常关闭,强制退出两种类型) 
    /// </summary> 
    public class Session
    {
        //缓存
        private byte[] _recvDataBuffer = new byte[1024];

        public byte[] RecvBuffer
        {
            get { return _recvDataBuffer; }
        }

        #region 字段

        /// <summary> 
        /// 会话ID 
        /// </summary> 
        private SessionId _id;



        /// <summary> 
        /// 客户端的Socket 
        /// </summary> 
        private Socket _cliSock;

        /// <summary> 
        /// 客户端的退出类型 
        /// </summary> 
        private ExitType _exitType;

        /// <summary> 
        /// 退出类型枚举 
        /// </summary> 
        public enum ExitType
        {
            NormalExit,
            ExceptionExit
        };

        #endregion

        #region 属性

        /// <summary> 
        /// 返回会话的ID 
        /// </summary> 
        public SessionId ID
        {
            get
            {
                return _id;
            }
        }



        /// <summary> 
        /// 获得与客户端会话关联的Socket对象 
        /// </summary> 
        public Socket ClientSocket
        {
            get
            {
                return _cliSock;
            }
        }

        /// <summary> 
        /// 存取客户端的退出方式 
        /// </summary> 
        public ExitType TypeOfExit
        {
            get
            {
                return _exitType;
            }

            set
            {
                _exitType = value;
            }
        }

        #endregion

        // TCP服务对象
        private TcpSvr _tcpSvr = null;

        // 数据处理器
        private DataProcessor _processor = null;

        #region 方法

        /// <summary> 
        /// 使用Socket对象的Handle值作为HashCode,它具有良好的线性特征. 
        /// </summary> 
        /// <returns></returns> 
        public override int GetHashCode()
        {
            return (int)_cliSock.Handle;
        }

        /// <summary> 
        /// 重载ToString()方法,返回Session对象的特征 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            string result = string.Format("Session:{0},IP:{1}",
            _id, _cliSock.RemoteEndPoint.ToString());

            //result.C 
            return result;
        }

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="cliSock">会话使用的Socket连接</param> 
        public Session(Socket cliSock, TcpSvr tcpSvr)
        {
            Debug.Assert(cliSock != null);

            _cliSock = cliSock;
            _tcpSvr = tcpSvr;

            _id = new SessionId((int)cliSock.Handle);

            _processor = new DataProcessor(this);
            _processor.FileReceived += OnFileReceived;
            _processor.NetPacketReceived += OnNetPacketReceived;
        }

        /// <summary> 
        /// 关闭会话 
        /// </summary> 
        public void Close()
        {
            Debug.Assert(_cliSock != null);

            //关闭数据的接受和发送 
            _cliSock.Shutdown(SocketShutdown.Both);

            //清理资源 
            _cliSock.Close();
        }

        #endregion



        // 处理接收到的数据
        public void ProcessIncomingData(byte[] data, int length)
        {
            _processor.ProcessData(data, 0, length);

        }

        /// <summary>
        ///  收到了完整的数据包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnNetPacketReceived(object sender, NetPacketEventArgs e)
        {
            if (GlobalParams.OnPacketReceived != null)
            {
                GlobalParams.OnPacketReceived(this, e);
            }


        }

        /// <summary>
        /// 接收到了文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnFileReceived(object sender, NetPacketEventArgs e)
        {
            if (GlobalParams.OnFileReceived != null)
            {
                GlobalParams.OnFileReceived(this, e);
            }
        }
    }


    /// <summary> 
    /// 唯一的标志一个Session,辅助Session对象在Hash表中完成特定功能 
    /// </summary> 
    public class SessionId
    {
        /// <summary> 
        /// 与Session对象的Socket对象的Handle值相同,必须用这个值来初始化它 
        /// </summary> 
        private int _id;

        /// <summary> 
        /// 返回ID值 
        /// </summary> 
        public int ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="id">Socket的Handle值</param> 
        public SessionId(int id)
        {
            _id = id;
        }

        /// <summary> 
        /// 重载.为了符合Hashtable键值特征 
        /// </summary> 
        /// <param name="obj"></param> 
        /// <returns></returns> 
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                SessionId right = (SessionId)obj;

                return _id == right._id;
            }
            else if (this == null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary> 
        /// 重载.为了符合Hashtable键值特征 
        /// </summary> 
        /// <returns></returns> 
        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary> 
        /// 重载,为了方便显示输出 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            return _id.ToString();
        }

    }


    /// <summary> 
    /// 服务器程序的事件参数,包含了激发该事件的会话对象 
    /// </summary> 
    public class NetEventArgs : EventArgs
    {

        #region 字段

        /// <summary> 
        /// 客户端与服务器之间的会话 
        /// </summary> 
        private Session _client;



        #endregion

        #region 构造函数
        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="client">客户端会话</param> 
        public NetEventArgs(Session client)
        {
            if (null == client)
            {
                throw (new ArgumentNullException());
            }

            _client = client;
        }
        #endregion

        #region 属性

        /// <summary> 
        /// 获得激发该事件的会话对象 
        /// </summary> 
        public Session Client
        {
            get
            {
                return _client;
            }

        }

        /// <summary>
        /// 接收到网络包的包头
        /// </summary>
        public PacketHeader PacketHeader { get; set; }

        /// <summary>
        /// 接收到网络包的文本内容
        /// </summary>
        public string StringContent { get; set; }

        /// <summary>
        /// 接收到的文件路径
        /// </summary>
        public string FilePathName { get; set; }
        #endregion

    }
}
