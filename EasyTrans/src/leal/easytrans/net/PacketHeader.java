package leal.easytrans.net;

import java.io.DataOutputStream;
import java.io.IOException;


/** 网络报文 
 * 	生成步骤：
 * 		对于文件。首先，使用GetNextObjectSerial()得到一个新的ObjectID；
 * 		然后，创建一个Index为0的，类型为PACKET_TYPE_FILE_INFO的包，将文件内容写入ＸＭＬ格式字符串中，发送。
 * 		然后，创建一个Index为1的，类型为PACKET_TYPE_FILE_CONTENT的包，将文件内容写入，发送。】
 * 
 * 报文格式：
 * 	[START FLAG][OBJECT ID][INDEX][PACKET TYPE][SUB PACKET TYPE][ANSWER OBJECT TYPE][CONTENT LENGTH][CONTENT BYTES][END FLAG]
 *  4           4          4      4            4                4                   8               n              4
 *  32个字节的头，加上内容，加上4字节的尾。
 * */
public class PacketHeader {
	private static int _nextObjectSerial = 0;
	
	public static int START_FLAG = 0xA1B2C3D4;
	public static int END_FLAG = 0xFFEEDDBB;
	
	public static int PACKET_TYPE_FILE_INFO = 1;  	//文件信息
	public static int PACKET_TYPE_FILE_CONTENT = 2; //文件内容
	
	public static long LENGTH_UNKNOWEN = -1; //未知的长度
	
	/** 生成一个新的对象序号.同一个对象的多个包，使用相同的序号 */
	public static int GetNextObjectSerial()
	{
		return _nextObjectSerial ++;
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
	
	/** 将报文头写入输出流中 */
	public void WriteHeader(DataOutputStream os)
	{
		try {
			os.writeInt(START_FLAG);
			os.writeInt(ObjectSerial);
			os.writeInt(Index);
			os.writeInt(PacketType);
			os.writeInt(SubPacketType);
			os.writeInt(AnswerObjectSerial);
			os.writeLong(ContentLength);
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	/** 写入报文结尾. 只有当消息长度不固定的时候，才使用此函数 */
	public void WriteEnd(DataOutputStream os)
	{
		try {
			os.writeInt(END_FLAG);
		} catch (IOException e) {			
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
