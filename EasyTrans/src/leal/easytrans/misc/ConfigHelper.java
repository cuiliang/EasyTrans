package leal.easytrans.misc;

import android.app.Activity;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;




public class ConfigHelper {
	public class PcInfo
	{
		public String name;
		public String ip;
		public int port;		
	}
	
	final static private String LAST_PC_INFO_FILE = "last_pc_info.pref";
	
	/** 保存最后连接的PC信息 */
	public void saveLastPcInfo(Activity activity, String name, String ip, int port)
	{
		SharedPreferences settings = activity.getSharedPreferences(LAST_PC_INFO_FILE, 0);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString("pc_name", name);
		editor.putString("pc_ip", ip);
		editor.putInt("pc_port", port);
		
		editor.commit();		
	}
	
	/** 最后连接的PC信息*/
	public PcInfo getLastPcInfo(Activity activity)
	{
		PcInfo pc = new PcInfo();
		
		SharedPreferences settings = activity.getSharedPreferences(LAST_PC_INFO_FILE, 0);
		pc.name = settings.getString("pc_name", "");
		pc.ip = settings.getString("pc_ip", "");
		pc.port = settings.getInt("pc_port", 666);
		
		return pc;
	}
	
	/** 是否允许自动连接 */
	public boolean canAutoConnect(Activity activity)
	{
		SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(activity);
		return prefs.getBoolean("auto_connect_last_pc", false);	
		
	}
}
