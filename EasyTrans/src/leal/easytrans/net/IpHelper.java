package leal.easytrans.net;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class IpHelper {
	private static final Pattern IP_ADDRESS
    = Pattern.compile(
        "((25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}|[1-9][0-9]|[1-9])\\.(25[0-5]|2[0-4]"
        + "[0-9]|[0-1][0-9]{2}|[1-9][0-9]|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1]"
        + "[0-9]{2}|[1-9][0-9]|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1][0-9]{2}"
        + "|[1-9][0-9]|[0-9]))");
	
	/** 判断是否是一个合法的IP地址 */
	public static boolean isValidIpAddr(String ip)
	{
		Matcher matcher = IP_ADDRESS.matcher(ip);
		return matcher.matches();
	}
}
