package leal.easytrans.misc;

/** ���������ֵ,������ */
public class ParamKeys {
	
	final public static String PC_IP = "PcIp";
	
	final public static String PC_PORT = "PcPort";
	
	final public static int DEFAULT_SERVER_PORT = 666;
	
	//PC����״̬
	final public static int PC_CONN_NOTCONNECTED = 1; 
	final public static int PC_CONN_CONNECTING = 2; //��������
	final public static int PC_CONN_CONNECTED = 3; //������
	
	
	final public static int MSG_PC_CONN_STATE_CHANGED = 1;	
	final public static int MSG_FILE_COUNT_CHANGED = 3; // ��������Ѵ�����ļ������б仯
	final public static int MSG_FILE_SEND_PROGRESS = 4; //�ļ��������
	
	final public static int MSG_BEGIN_SEND_FILE = 5; //��ʼ�����ļ�
	final public static int MSG_END_SEND_FILE = 6; //��ɴ����ļ�
	
	
	
}
