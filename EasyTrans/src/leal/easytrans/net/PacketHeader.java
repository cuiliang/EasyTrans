package leal.easytrans.net;

import java.io.DataOutputStream;
import java.io.IOException;


/** ���籨�� 
 * 	���ɲ��裺
 * 		�����ļ������ȣ�ʹ��GetNextObjectSerial()�õ�һ���µ�ObjectID��
 * 		Ȼ�󣬴���һ��IndexΪ0�ģ�����ΪPACKET_TYPE_FILE_INFO�İ������ļ�����д��أ̸ͣ�ʽ�ַ����У����͡�
 * 		Ȼ�󣬴���һ��IndexΪ1�ģ�����ΪPACKET_TYPE_FILE_CONTENT�İ������ļ�����д�룬���͡���
 * 
 * ���ĸ�ʽ��
 * 	[START FLAG][OBJECT ID][INDEX][PACKET TYPE][SUB PACKET TYPE][ANSWER OBJECT TYPE][CONTENT LENGTH][CONTENT BYTES][END FLAG]
 *  4           4          4      4            4                4                   8               n              4
 *  32���ֽڵ�ͷ���������ݣ�����4�ֽڵ�β��
 * */
public class PacketHeader {
	private static int _nextObjectSerial = 0;
	
	public static int START_FLAG = 0xA1B2C3D4;
	public static int END_FLAG = 0xFFEEDDBB;
	
	public static int PACKET_TYPE_FILE_INFO = 1;  	//�ļ���Ϣ
	public static int PACKET_TYPE_FILE_CONTENT = 2; //�ļ�����
	
	public static long LENGTH_UNKNOWEN = -1; //δ֪�ĳ���
	
	/** ����һ���µĶ������.ͬһ������Ķ������ʹ����ͬ����� */
	public static int GetNextObjectSerial()
	{
		return _nextObjectSerial ++;
	}
	
	/** �������  4B*/
	public int ObjectSerial;
	
	/** ͬһ����ı������  4B*/
	public int Index;
	
	/** �������� 4B */
	public int PacketType;
	
	/** ���������� 4B */
	public int SubPacketType;
	
	/** ��Ӧ���ĸ����� */
	public int AnswerObjectSerial;
	
	/** ���ݳ��� */
	public long ContentLength;	
	
	/** ������ͷд��������� */
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
	
	/** д�뱨�Ľ�β. ֻ�е���Ϣ���Ȳ��̶���ʱ�򣬲�ʹ�ô˺��� */
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
