////////////////////////////////////////////////////////////////////////////////////////// 
/* 

标题：在C#中使用异步Socket编程实现TCP网络服务的C/S的通讯构架(一)----基础类库部分 

当看到.NET中TcpListener和TcpClient的时候,我非常高兴,那就是我想要的通讯模式 
但是使用之后发现它们的力量太单薄了,我们需要一个更好的类库来替代它们. 

下面提供了一些类,可以很好的完成Tcp的C/S通讯模式.在本文的第二部分,我将为大家介绍怎么使用它们 

主要通过事件来现实整个的功能: 
服务器的事件包括: 

服务器满 
新客户端连接 
客户端关闭 
接收到数据 

客户端使用的事件包括: 

已连接服务器 
接收到数据 
连接关闭 

另外为了灵活的处理需求变化,还提供了编码器和报文解析器的实现方法. 
注意:该类库没有经过严格的测试,如出现Bug,请发送给我,我会觉得你的整个行为是对我的鼓励和支持. 

*/
////////////////////////////////////////////////////////////////////////////////////////// 

/// <summary> 
/// (C)2003-2005 C2217 Studio 
/// 保留所有权利 
/// 
/// 文件名称: TcpCSFramework.cs 
/// 文件ID: 
/// 编程语言: C# 
/// 文件说明: 提供TCP网络服务的C/S的通讯构架基础类 
/// (使用异步Socket编程实现) 
/// 
/// 当前版本: 1.1 
/// 替换版本: 1.0 
/// 
/// 作者: 邓杨均 
/// EMail: dyj057@gmail.com 
/// 创建日期: 2005-3-9 
/// 最后修改日期: 2005-3-17 
/// 
/// 历史修改记录: 
/// 
/// 时间: 2005-3-14 
/// 修改内容: 
/// 1.创建Ibms.Net.TcpCSFramework命名空间和添加Session对象. 
/// 2.修改NetEventArgs类,以适应新添加对象. 
/// 3.添加了会话退出类型,更适合实际的情况. 
/// 注意: 
/// * 强制退出类型是应用程序直接结束,比如通过任务管理器结束 
/// 程序或者程序异常退出等,没有执行正常的退出方法而产生的. 
/// * 正常的退出类型是应用程序执行正常的退出的方法关键在于 
/// 需要调用Socket.Shutdown( SocketShutdown.Both )后才调用 
/// Socket.Close()方法,而不是直接的调用Socket.Close()方法, 
/// 如果那样调用将产生强制退出类型. 
/// 
/// 时间: 2005-3-16 
/// 修改内容: 
/// 1.创建TcpCli,Coder,DatagramResover对象,把抽象和实现部分分离 
/// 2.文件版本修改为1.1,1.0版本仍然保留,更名为: 
/// TcpCSFramework_v1.0.cs 
/// 3.在TcpServer中修改自定义的hashtable为系统Hashtable类型 
/// 
/// </summary> 

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
    /// 网络通讯事件模型委托 
    /// </summary> 
    public delegate void NetEvent(object sender, NetEventArgs e);

    /// <summary> 
    /// 提供TCP连接服务的服务器类 
    /// 
    /// 版本: 1.1 
    /// 替换版本: 1.0 
    /// 
    /// 特点: 
    /// 1.使用hash表保存所有已连接客户端的状态，收到数据时能实现快速查找.每当 
    /// 有一个新的客户端连接就会产生一个新的会话(Session).该Session代表了客 
    /// 户端对象. 
    /// 2.使用异步的Socket事件作为基础，完成网络通讯功能. 
    /// 3.支持带标记的数据报文格式的识别,以完成大数据报文的传输和适应恶劣的网 
    /// 络环境.初步规定该类支持的最大数据报文为640K(即一个数据包的大小不能大于 
    /// 640K,否则服务器程序会自动删除报文数据,认为是非法数据),防止因为数据报文 
    /// 无限制的增长而倒是服务器崩溃 
    /// 4.通讯格式默认使用Encoding.Default格式这样就可以和以前32位程序的客户端 
    /// 通讯.也可以使用U-16和U-8的的通讯方式进行.可以在该DatagramResolver类的 
    /// 继承类中重载编码和解码函数,自定义加密格式进行通讯.总之确保客户端与服务 
    /// 器端使用相同的通讯格式 
    /// 5.使用C# native code,将来出于效率的考虑可以将C++代码写成的32位dll来代替 
    /// C#核心代码, 但这样做缺乏可移植性,而且是Unsafe代码(该类的C++代码也存在) 
    /// 6.可以限制服务器的最大登陆客户端数目 
    /// 7.比使用TcpListener提供更加精细的控制和更加强大异步数据传输的功能,可作为 
    /// TcpListener的替代类 
    /// 8.使用异步通讯模式,完全不用担心通讯阻塞和线程问题,无须考虑通讯的细节 
    /// 
    /// 注意: 
    /// 1.部分的代码由Rational XDE生成,可能与编码规范不符 
    /// 
    /// 原理: 
    /// 
    /// 
    /// 使用用法: 
    /// 
    /// 例子: 
    /// 
    /// </summary> 
    public class TcpSvr
    {
        #region 定义字段

        /// <summary> 
        /// 默认的服务器最大连接客户端端数据 
        /// </summary> 
        public const int DefaultMaxClient = 100;


        /// <summary> 
        /// 服务器程序使用的端口 
        /// </summary> 
        private ushort _port;

        /// <summary> 
        /// 服务器程序允许的最大客户端连接数 
        /// </summary> 
        private ushort _maxClient;

        /// <summary> 
        /// 服务器的运行状态 
        /// </summary> 
        private bool _isRun;


        /// <summary> 
        /// 服务器使用的异步Socket类, 
        /// </summary> 
        private Socket _svrSock;

        /// <summary> 
        /// 保存所有客户端会话的哈希表 
        /// </summary> 
        private Hashtable _sessionTable;

        /// <summary> 
        /// 当前的连接的客户端数 
        /// </summary> 
        private ushort _clientCount;


        #endregion

        #region 事件定义

        /// <summary> 
        /// 客户端建立连接事件 
        /// </summary> 
        public event NetEvent ClientConn;

        /// <summary> 
        /// 客户端关闭事件 
        /// </summary> 
        public event NetEvent ClientClose;

        /// <summary> 
        /// 服务器已经满事件 
        /// </summary> 
        public event NetEvent ServerFull;

        /// <summary> 
        /// 服务器接收到数据事件 
        /// </summary> 
        public event NetEvent RecvData;

        /// <summary>
        /// 服务器接收到文件
        /// </summary>
        public event NetEvent RecvFile;

        #endregion

        #region 构造函数

        


        /// <summary> 
        /// 构造函数(默认使用Default编码方式) 
        /// </summary> 
        /// <param name="port">服务器端监听的端口号</param> 
        /// <param name="maxClient">服务器能容纳客户端的最大能力</param> 
        public TcpSvr(ushort port, ushort maxClient)
        {
            _port = port;
            _maxClient = maxClient;
        }


        // <summary> 
        /// 构造函数(默认使用Default编码方式和DefaultMaxClient(100)个客户端的容量) 
        /// </summary> 
        /// <param name="port">服务器端监听的端口号</param> 
        public TcpSvr(ushort port)
            : this(port, DefaultMaxClient)
        {

        }

        #endregion

        #region 属性

        /// <summary> 
        /// 服务器的Socket对象 
        /// </summary> 
        public Socket ServerSocket
        {
            get { return _svrSock; }
        }



        /// <summary> 
        /// 客户端会话数组,保存所有的客户端,不允许对该数组的内容进行修改 
        /// </summary> 
        public Hashtable SessionTable
        {
            get { return _sessionTable; }
        }

        /// <summary> 
        /// 服务器可以容纳客户端的最大能力 
        /// </summary> 
        public int Capacity
        {
            get { return _maxClient; }
        }

        /// <summary> 
        /// 当前的客户端连接数 
        /// </summary> 
        public int SessionCount
        {
            get { return _clientCount; }
        }

        /// <summary> 
        /// 服务器运行状态 
        /// </summary> 
        public bool IsRun
        {
            get { return _isRun; }

        }

        #endregion

        #region 公有方法

        /// <summary> 
        /// 启动服务器程序,开始监听客户端请求 
        /// </summary> 
        public virtual void Start()
        {
            if (_isRun)
            {
                throw (new ApplicationException("TcpSvr已经在运行."));
            }

            _sessionTable = new Hashtable(53);


            //初始化socket 
            _svrSock = new Socket(AddressFamily.InterNetwork,
                                  SocketType.Stream, ProtocolType.Tcp);

            //绑定端口 
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _port);
            _svrSock.Bind(iep);

            //开始监听 
            _svrSock.Listen(5);

            //设置异步方法接受客户端连接 
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

            _isRun = true;

        }

        /// <summary> 
        /// 停止服务器程序,所有与客户端的连接将关闭 
        /// </summary> 
        public virtual void Stop()
        {
            if (!_isRun)
            {
                throw (new ApplicationException("TcpSvr已经停止"));
            }

            //这个条件语句，一定要在关闭所有客户端以前调用 
            //否则在EndConn会出现错误 
            _isRun = false;

            //关闭数据连接,负责客户端会认为是强制关闭连接 
            if (_svrSock.Connected)
            {
                _svrSock.Shutdown(SocketShutdown.Both);
            }

            CloseAllClient();

            //清理资源 
            _svrSock.Close();

            _sessionTable = null;

        }


        /// <summary> 
        /// 关闭所有的客户端会话,与所有的客户端连接会断开 
        /// </summary> 
        public virtual void CloseAllClient()
        {
            foreach (Session client in _sessionTable.Values)
            {
                client.Close();
            }

            _sessionTable.Clear();
        }


        /// <summary> 
        /// 关闭一个与客户端之间的会话 
        /// </summary> 
        /// <param name="closeClient">需要关闭的客户端会话对象</param> 
        public virtual void CloseSession(Session closeClient)
        {
            Debug.Assert(closeClient != null);

            if (closeClient != null)
            {


                _sessionTable.Remove(closeClient.ID);

                _clientCount--;

                //客户端强制关闭链接 
                if (ClientClose != null)
                {
                    ClientClose(this, new NetEventArgs(closeClient));
                }

                closeClient.Close();
            }
        }


        /// <summary> 
        /// 发送数据 
        /// </summary> 
        /// <param name="recvDataClient">接收数据的客户端会话</param> 
        /// <param name="datagram">数据报文</param> 
        public virtual void Send(Session recvDataClient, byte[] data)
        {
            //获得数据编码 
           // byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
                                                  new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);

        }

        #endregion

        #region 受保护方法

        /// <summary> 
        /// 关闭一个客户端Socket,首先需要关闭Session 
        /// </summary> 
        /// <param name="client">目标Socket对象</param> 
        /// <param name="exitType">客户端退出的类型</param> 
        protected virtual void CloseClient(Socket client, Session.ExitType exitType)
        {
            Debug.Assert(client != null);

            //查找该客户端是否存在,如果不存在,抛出异常 
            Session closeClient = FindSession(client);

            closeClient.TypeOfExit = exitType;

            if (closeClient != null)
            {
                CloseSession(closeClient);
            }
            else
            {
                throw (new ApplicationException("需要关闭的Socket对象不存在"));
            }
        }


        /// <summary> 
        /// 客户端连接处理函数 
        /// </summary> 
        /// <param name="iar">欲建立服务器连接的Socket对象</param> 
        protected virtual void AcceptConn(IAsyncResult iar)
        {
            //如果服务器停止了服务,就不能再接收新的客户端 
            if (!_isRun)
            {
                return;
            }

            //接受一个客户端的连接请求 
            Socket oldserver = (Socket)iar.AsyncState;

            Socket client = oldserver.EndAccept(iar);

            //检查是否达到最大的允许的客户端数目 
            if (_clientCount == _maxClient)
            {
                //服务器已满,发出通知 
                if (ServerFull != null)
                {
                    ServerFull(this, new NetEventArgs(new Session(client, this)));
                }

            }
            else
            {

                Session newSession = new Session(client, this);

                _sessionTable.Add(newSession.ID, newSession);

                //客户端引用计数+1 
                _clientCount++;

                //开始接受来自该客户端的数据 
                client.BeginReceive(newSession.RecvBuffer, 0, newSession.RecvBuffer.Length, SocketFlags.None,
                                    new AsyncCallback(ReceiveData), newSession);

                //新的客户段连接,发出通知 
                if (ClientConn != null)
                {
                    ClientConn(this, new NetEventArgs(newSession));
                }
            }

            //继续接受客户端 
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
        }


        /// <summary> 
        /// 通过Socket对象查找Session对象 
        /// </summary> 
        /// <param name="client"></param> 
        /// <returns>找到的Session对象,如果为null,说明并不存在该回话</returns> 
        private Session FindSession(Socket client)
        {
            SessionId id = new SessionId((int)client.Handle);

            return (Session)_sessionTable[id];
        }


        /// <summary> 
        /// 接受数据完成处理函数，异步的特性就体现在这个函数中， 
        /// 收到数据后，会自动解析为字符串报文 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Session client = (Session)iar.AsyncState;

            try
            {
                //如果两次开始了异步的接收,所以当客户端退出的时候 
                //会两次执行EndReceive 

                int recv = client.ClientSocket.EndReceive(iar);

                if (recv == 0)
                {
                    //正常的关闭 
                    CloseClient(client.ClientSocket, Session.ExitType.NormalExit);
                    return;
                }

                //string receivedData = _coder.GetEncodingString(_recvDataBuffer, recv);

                
                


                //发布收到数据的事件 
                if (RecvData != null)
                {

                    client.ProcessIncomingData(client.RecvBuffer, recv);

                    RecvData(this, new NetEventArgs(client));
                    

                } //end of if(RecvData!=null) 

                //继续接收来自来客户端的数据 
                client.ClientSocket.BeginReceive(client.RecvBuffer, 0, client.RecvBuffer.Length, SocketFlags.None,
                                    new AsyncCallback(ReceiveData), client);

            }
            catch (SocketException ex)
            {
                //客户端退出 
                if (10054 == ex.ErrorCode)
                {
                    //客户端强制关闭 
                    CloseClient(client.ClientSocket, Session.ExitType.ExceptionExit);
                }

            }
            catch (ObjectDisposedException ex)
            {
                //这里的实现不够优雅 
                //当调用CloseSession()时,会结束数据接收,但是数据接收 
                //处理中会调用int recv = client.EndReceive(iar); 
                //就访问了CloseSession()已经处置的对象 
                //我想这样的实现方法也是无伤大雅的. 
                if (ex != null)
                {
                    ex = null;
                    //DoNothing; 
                }
            }

        }


        /// <summary> 
        /// 发送数据完成处理函数 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            int sent = client.EndSend(iar);
        }

        #endregion

    }
    
    
}
