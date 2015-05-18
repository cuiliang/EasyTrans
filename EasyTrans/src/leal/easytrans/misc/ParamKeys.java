package leal.easytrans.misc;

/** 定义参数键值,常量等 */
public class ParamKeys {
	
	final public static String PC_IP = "PcIp";
	
	final public static String PC_PORT = "PcPort";
	
	final public static int DEFAULT_SERVER_PORT = 666;
	
	//PC连接状态
	final public static int PC_CONN_NOTCONNECTED = 1; 
	final public static int PC_CONN_CONNECTING = 2; //正在连接
	final public static int PC_CONN_CONNECTED = 3; //已连接
	
	
	final public static int MSG_PC_CONN_STATE_CHANGED = 1;	
	final public static int MSG_FILE_COUNT_CHANGED = 3; // 待传输和已传输的文件数量有变化
	final public static int MSG_FILE_SEND_PROGRESS = 4; //文件传输进度
	
	final public static int MSG_BEGIN_SEND_FILE = 5; //开始传送文件
	final public static int MSG_END_SEND_FILE = 6; //完成传送文件
	
	
	
}
