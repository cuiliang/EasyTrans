using System;
using System.Collections.Generic;
using System.Text;

namespace Img2PC.SocketServer
{
    public class PacketHeader
    {
        private static int _nextObjectSerial = 0;


        public static uint START_FLAG = 0xA1B2C3D4;
        public static uint END_FLAG = 0xFFEEDDBB;

        public static int PACKET_TYPE_FILE_INFO = 1;  	//文件信息
        public static int PACKET_TYPE_FILE_CONTENT = 2; //文件内容

        public static long LENGTH_UNKNOWEN = -1; //未知的长度

        public static int HEADER_LEN = 32; //包头长度，包含起始标志

        /** 生成一个新的对象序号.同一个对象的多个包，使用相同的序号 */
        public static int GetNextObjectSerial()
        {
            return _nextObjectSerial++;
        }

        /** 对象序号  4B*/
        public int ObjectSerial;

        /** 同一对象的报文序号  4B*/
        public int Index;

        /** 报文类型 4B */
        public int PacketType;

        /** 报文子类型 4B */
        public int SubPacketType;

        /** 响应的哪个报文 */
        public int AnswerObjectSerial;

        /** 内容长度 */
        public long ContentLength;

        //内容
        //public byte[] Content;

        //从接收的数据中读取包
        public static PacketHeader ReadFromPacket(byte[] data, int startPos)
        {
            if (data.Length - startPos < HEADER_LEN)
            {
                //数据长度不够读取一个包头
                return null;
            }


            for(int i=0; i<4; i++)
            {
                if (data[startPos + i] != ((START_FLAG >> ((4-i-1) * 8)) & 0xFF))
                {
                    //起始标志不对，返回
                    return null;
                }
            }

            PacketHeader header = new PacketHeader();

            //ignore header
            startPos += 4;
            // ObjectSerial
            header.ObjectSerial = GetIntFromByte(data, startPos);
            startPos += 4;

            //Index
            header.Index = GetIntFromByte(data, startPos);
            startPos += 4;

            //PacketType
            header.PacketType = GetIntFromByte(data, startPos);
            startPos += 4;

            //
            header.SubPacketType = GetIntFromByte(data, startPos);
            startPos += 4;

            //
            header.AnswerObjectSerial = GetIntFromByte(data, startPos);
            startPos += 4;

            //content length
            header.ContentLength = GetLongFromByte(data, startPos);
            startPos += 8;


            return header;
        }

        public static int GetIntFromByte(byte[] data, int startPos)
        {
            return data[startPos] << 24
                     | data[startPos + 1] << 16
                     | data[startPos + 2] << 8
                     | data[startPos + 3] ;
            
        }

        public static long GetLongFromByte(byte[] data, int startPos)
        {
            long value = 0;
            for(int i=0; i<8; i++)
            {
                value = value | data[startPos +8 - i -1] << (8*i);
            }

            return value;
        }
    }
}
