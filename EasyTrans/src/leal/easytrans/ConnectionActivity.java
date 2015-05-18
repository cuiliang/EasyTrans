package leal.easytrans;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

import leal.easytrans.misc.ParamKeys;
import leal.easytrans.net.IpHelper;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class ConnectionActivity extends Activity {
	
	private Button btnConnect = null;
	private EditText txtPcIp = null;
	private EditText txtPcPort = null;
	
	/** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.connection);
        
        btnConnect = (Button)findViewById(R.id.btnConnect);
        btnConnect.setOnClickListener(new BtnConnectListener());
        
        txtPcIp = (EditText)findViewById(R.id.txtPcIP);
        txtPcPort = (EditText)findViewById(R.id.txtPcPort);
        
        Intent intent = getIntent();
        txtPcIp.setText(intent.getStringExtra(ParamKeys.PC_IP));
        txtPcPort.setText("" + intent.getIntExtra(ParamKeys.PC_PORT, ParamKeys.DEFAULT_SERVER_PORT));
    }
    
    
    private class BtnConnectListener implements OnClickListener
    {
    	

		public void onClick(View v) {
			
			
			// 判断输入的IP地址是否合法
			String ip = txtPcIp.getText().toString();
			if  (IpHelper.isValidIpAddr(ip))
			{				
				//Toast.makeText(ConnectionActivity.this, "IP is valid", Toast.LENGTH_LONG).show();
			}else
			{				
				Toast.makeText(ConnectionActivity.this, "非法的IP地址", Toast.LENGTH_LONG).show();
				return;
			}
			
			String portStr = txtPcPort.getText().toString();
			int port = Integer.parseInt(portStr);
			
			if (port < 10 || port > 65535)
			{
				Toast.makeText(ConnectionActivity.this, "非法的端口号", Toast.LENGTH_LONG).show();
				return;
			}
			
			//
			Intent intent = new Intent();
			Bundle bundle = new Bundle();
			bundle.putString(ParamKeys.PC_IP, ip);
			bundle.putInt(ParamKeys.PC_PORT, port);
			intent.putExtras(bundle);
			
			ConnectionActivity.this.setResult(RESULT_OK, intent);
			ConnectionActivity.this.finish();
		}
    	
    }
}
