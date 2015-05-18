using System;
using System.Collections.Generic;
using System.Text;

namespace Img2PC.SocketServer
{
    class GlobalParams
    {
        /// <summary>
        /// 接收文件的保存路径
        /// </summary>
        public static string RecvFileFolder { get; set; }

        /// <summary>
        /// 接收到文件的事件
        /// </summary>
        public static NetPacketEventHandler OnFileReceived;


        public static NetPacketEventHandler OnPacketReceived;
    }
}
