package leal.easytrans;

import java.io.File;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.Queue;

import leal.easytrans.misc.ConfigHelper;
import leal.easytrans.misc.FileHelper;
import leal.easytrans.misc.FileItem;
import leal.easytrans.misc.NetHelper;
import leal.easytrans.misc.ParamKeys;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.ServiceConnection;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.os.Message;
import android.os.Messenger;
import android.provider.MediaStore;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {

	private static final String TAG = "MainActivity";

	static final int REQUEST_SET_CONNECTION = 1;
	static final int REQUEST_TAKE_PHOTO = 2; // ����Ƭ������
	static final int REQUEST_SELECT_PHOTO = 3;

	private TextView textViewFileCount = null;
	private TextView textViewConnectionState = null;
	private Button btnDisconnect = null;
	private ProgressBar progressBarTrans = null;
	private ImageButton btnTakePhoto = null;
	private ImageView imageViewConnectionState = null;

	// ��̨���ͷ���
	private TransService _transService = null;
	private boolean _isServiceBinded = false;

	// �����ݸ�Service���ļ�����
	private Queue<FileItem> _pendingFiles = new LinkedList<FileItem>();

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {

		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);

		textViewConnectionState = (TextView) findViewById(R.id.textViewConnectionState);
		textViewConnectionState.setClickable(true);
		textViewConnectionState
				.setOnClickListener(new ConnectionClickListener());

		btnDisconnect = (Button) findViewById(R.id.btn_disconnect);
		btnDisconnect.setOnClickListener(new DisconnectClickListener());
		btnDisconnect.setVisibility(View.GONE);

		textViewFileCount = (TextView) findViewById(R.id.textViewFileCount);
		textViewFileCount.setText("�������ļ�n��" + _isServiceBinded);

		progressBarTrans = (ProgressBar) findViewById(R.id.progressBarTrans);
		progressBarTrans.setMax(100);

		btnTakePhoto = (ImageButton) findViewById(R.id.btn_takephoto);
		btnTakePhoto.setOnClickListener(new BtnTakePhotoClickListener());

		imageViewConnectionState = (ImageView) findViewById(R.id.imageViewConnectionState);
		
		((ImageButton) findViewById(R.id.btn_selectphoto)).setOnClickListener(new SelectPhotoClickListener());
		

		// ���������ӷ���
		connectToService();

		// ����Intent
		Intent intent = getIntent();
		Log.d(TAG, "MainActivity on create. intent:" + intent);

		//
		processIntent(getIntent());

		// Toast.makeText(this, "MainActivity onCreate. intent:" + intent,
		// Toast.LENGTH_LONG).show();
	}

	/** ����MainActivity����ʱ���ݹ�����Intent */
	private void processIntent(Intent intent) {
		Bundle extras = intent.getExtras();
		String action = intent.getAction();
		String type = intent.getType();

		if (Intent.ACTION_SEND.equals(action) && type != null) {
			if ("text/plain".equals(type)) {
				// TODO: do nothing when send text
				// handleSendText(intent); // Handle text being sent
				// } else if (type.startsWith("image/")) {
			} else {
				handleSendImage(intent); // Handle single image being sent
			}
		} else if (Intent.ACTION_SEND_MULTIPLE.equals(action) && type != null) {
			// if (type.startsWith("image/")) {
			handleSendMultipleImages(intent); // Handle multiple images being
												// sent
			// }
		} else {
			// Handle other intents, such as being started from the home screen
		}
	}

	void handleSendText(Intent intent) {
		String sharedText = intent.getStringExtra(Intent.EXTRA_TEXT);
		if (sharedText != null) {
			// Update UI to reflect text being shared
		}
	}

	/** ����1��ͼƬ�ļ� */
	void handleSendImage(Intent intent) {
		Uri imageUri = (Uri) intent.getParcelableExtra(Intent.EXTRA_STREAM);
		if (imageUri != null) {
			// Update UI to reflect image being shared

			String mime = intent.getType();

			handleSendFile(imageUri, mime);
		}
	}

	private void handleSendFile(Uri uri, String mime) {
		// Update UI to reflect image being shared

		String filePath = FileHelper.getFilePathFromUri(this, uri);

		FileItem item = new FileItem();
		item.filePathName = filePath;
		item.uri = uri;
		item.mime = mime;

		if (_transService != null) {
			_transService.sendFile(item);
		} else {
			_pendingFiles.add(item);
			Log.e(TAG, "Service is not connected, can not send file");
		}
	}

	void handleSendMultipleImages(Intent intent) {
		ArrayList<Uri> imageUris = intent
				.getParcelableArrayListExtra(Intent.EXTRA_STREAM);
		if (imageUris != null) {
			String mime = intent.getType();

			for (int i = 0; i < imageUris.size(); i++) {
				Uri uri = imageUris.get(i);

				handleSendFile(uri, mime);
			}
		}
	}

	/** ���������״̬ */
	private class ConnectionClickListener implements OnClickListener {
		public void onClick(View v) {
			// ����Ѿ����ӣ�����Ҫ�޸�
			if (_transService != null
					&& _transService.getPcConnectionState() != ParamKeys.PC_CONN_NOTCONNECTED) {
				return;
			}
			
			// ���Wifiû�����ӣ�����ʾ
			if (!NetHelper.isWifiConnected(MainActivity.this))
			{
				Toast.makeText(MainActivity.this, "Wifi����û������!", Toast.LENGTH_LONG).show();
				return;
			}

			Intent intent = new Intent(MainActivity.this,
					ConnectionActivity.class);

			if (_transService != null) {
				intent.putExtra(ParamKeys.PC_IP, _transService.getPcIP());
				intent.putExtra(ParamKeys.PC_PORT, _transService.getPcPort());
			}

			startActivityForResult(intent, REQUEST_SET_CONNECTION);
		}
	}

	/** ����˶Ͽ����� */
	private class DisconnectClickListener implements OnClickListener {
		public void onClick(View v) {
			if (_transService != null) {
				_transService.disconnect(true);
			} else {
				Log.e(TAG, "Service object is null!");
			}
		}
	}

	/** ��������հ�ť */
	private class BtnTakePhotoClickListener implements OnClickListener {

		public void onClick(View v) {
			Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
			startActivityForResult(intent, REQUEST_TAKE_PHOTO);
		}

	}
	
	private class SelectPhotoClickListener implements OnClickListener{
		
		public void onClick(View v)
		{
			Intent i = new Intent(Intent.ACTION_PICK,
		               android.provider.MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
			startActivityForResult(i, REQUEST_SELECT_PHOTO); 
		}
	}

	/** Activity���ؽ������ */
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_SET_CONNECTION) {
			// �����ӽ�����ѡ����Ҫ���ӵ�IP�Ͷ˿�
			if (resultCode == RESULT_OK) {
				String pcIP = data.getStringExtra("PcIp");
				int pcPort = data.getIntExtra("PcPort", 666);

				if (this._transService != null) {
					this._transService.connect(pcIP, pcPort);
				} else {
					Log.e(TAG, "Service is null !");
				}
			} else {
				//Toast.makeText(this, "Canceled", Toast.LENGTH_LONG).show();
			}
		} else if (requestCode == REQUEST_TAKE_PHOTO) {
			// ���շ���
			Log.d(TAG, "resultCode:" + resultCode + " intent=" + data);
			if (resultCode == RESULT_OK) {
				CharSequence path = FileHelper.getLastImagePathName(this);

				if (path.length() > 0) {
					FileItem item = new FileItem();
					item.filePathName = path.toString();
					item.uri = Uri.fromFile(new File(path.toString()));
					item.mime = "image/jpeg";

					if (_transService != null) {
						_transService.sendFile(item);
					} else {
						_pendingFiles.add(item);
						Log.e(TAG,
								"Service is not connected, can not send file");
					}
				} else {
					Toast.makeText(this, "�޷���ȡ���ĵ���Ƭ", Toast.LENGTH_SHORT)
							.show();
				}
			}

		}else if (requestCode == REQUEST_SELECT_PHOTO)
		{
			if(resultCode == RESULT_OK){  
	            Uri selectedImage = data.getData();
	            handleSendFile(selectedImage, "image/jpeg");
	            
//	            String[] filePathColumn = {MediaStore.Images.Media.DATA};
//
//	            Cursor cursor = getContentResolver().query(selectedImage, filePathColumn, null, null, null);
//	            cursor.moveToFirst();
//
//	            int columnIndex = cursor.getColumnIndex(filePathColumn[0]);
//	            String filePath = cursor.getString(columnIndex);
//	            cursor.close();
//
//
//	            Bitmap yourSelectedImage = BitmapFactory.decodeFile(filePath);
	        }
		}

		super.onActivityResult(requestCode, resultCode, data);
	}

	private ServiceConnection _connection = new ServiceConnection() {

		public void onServiceConnected(ComponentName name, IBinder service) {
			_transService = ((TransService.LocalBinder) service).getService();
			_isServiceBinded = true;

			_transService.setMainActivityMessenger(mMessenger);

			// ����д�������ļ������ݸ�Service
			if (_pendingFiles.size() > 0) {
				FileItem item = _pendingFiles.poll();
				while (item != null) {
					_transService.sendFile(item);

					item = _pendingFiles.poll();
				}
			}

			// ����ǵ�һ�����������Ҳ��������������Զ����ӣ���ô���н�������
			ConfigHelper config = new ConfigHelper();
			if (_transService.canConnect()) {

				ConfigHelper.PcInfo pcInfo = config
						.getLastPcInfo(MainActivity.this);
				if (pcInfo.ip.length() > 0) {
					if (config.canAutoConnect(MainActivity.this)  
							&& NetHelper.isWifiConnected(MainActivity.this)) {
						_transService.connect(pcInfo.ip, pcInfo.port);
					} else {
						_transService.setPcAddr(pcInfo.ip, pcInfo.port);
					}
				}
			}

			// update ui
			// MainActivity.this.UpdateConnectionState();

			Log.d(TAG, "Connected to service, Service=" + _transService);
		}

		public void onServiceDisconnected(ComponentName name) {
			// TODO Auto-generated method stub
			_transService = null;
			_isServiceBinded = false;

			Log.d(TAG, "Service disconnected");
		}

	};

	@Override
	protected void onDestroy() {
		//
		disconnectService();

		super.onDestroy();
	}

	/** �ͷ��������� */
	private void connectToService() {
		// ��ʼ����
		Intent intent = new Intent(MainActivity.this, TransService.class);
		startService(intent);

		bindService(intent, _connection, Context.BIND_AUTO_CREATE);
	}

	/** �˿ڷ������� */
	private void disconnectService() {
		Log.e(TAG, "Service disconnected");
		if (_isServiceBinded) {
			_transService.setMainActivityMessenger(null);

			unbindService(_connection);
			_isServiceBinded = false;
			_transService = null;
		}
	}

	final Messenger mMessenger = new Messenger(new IncomingHandler());

	class IncomingHandler extends Handler {
		@Override
		public void handleMessage(Message msg) {

			switch (msg.what) {
			case ParamKeys.MSG_PC_CONN_STATE_CHANGED:
				MainActivity.this.UpdateConnectionState(msg.arg1);

				break;
			case ParamKeys.MSG_FILE_COUNT_CHANGED:
				MainActivity.this.updateFileCount(msg.arg1, msg.arg2);
				break;
			case ParamKeys.MSG_FILE_SEND_PROGRESS:
				MainActivity.this.updateFileSendProgress(msg.arg1);
				break;
			case ParamKeys.MSG_BEGIN_SEND_FILE:
				MainActivity.this.progressBarTrans.setVisibility(View.VISIBLE);
				imageViewConnectionState
						.setImageResource(R.drawable.ic_state_sending);
				break;
			case ParamKeys.MSG_END_SEND_FILE:
				MainActivity.this.progressBarTrans.setVisibility(View.GONE);
				imageViewConnectionState
						.setImageResource(R.drawable.ic_state_connected);
				break;
			default:
				super.handleMessage(msg);
			}
		}
	}

	/** ��������״̬��ʾ */
	private void UpdateConnectionState(int state) {
		String stateStr = "";
		switch (state) {
		case ParamKeys.PC_CONN_NOTCONNECTED:
			if (!NetHelper.isWifiConnected(this))
			{
				stateStr = "WIFIδ����!";
			}else
			{
				stateStr = "δ����PC,������ʼ���ӡ�";
			}
			btnDisconnect.setVisibility(View.GONE);
			imageViewConnectionState
					.setImageResource(R.drawable.ic_state_disconnected);
			break;
		case ParamKeys.PC_CONN_CONNECTING:
			stateStr = "��������" + _transService.getPcIP() + ":"
					+ _transService.getPcPort();
			btnDisconnect.setVisibility(View.GONE);
			imageViewConnectionState
					.setImageResource(R.drawable.ic_state_connecting);
			break;
		case ParamKeys.PC_CONN_CONNECTED:
			stateStr = "��������" + _transService.getPcIP() + ":"
					+ _transService.getPcPort();
			btnDisconnect.setVisibility(View.VISIBLE);
			imageViewConnectionState
					.setImageResource(R.drawable.ic_state_connected);

			// ��¼������ӳɹ���IP�Ͷ˿�
			ConfigHelper config = new ConfigHelper();
			config.saveLastPcInfo(this, "last pc", _transService.getPcIP(),
					_transService.getPcPort());
			break;
		}

		MainActivity.this.textViewConnectionState.setText(stateStr);
	}

	/** �����ļ����� */
	private void updateFileCount(int pending, int sended) {
		textViewFileCount.setText("������:" + pending + "     �ѷ���:" + sended);
	}

	private void updateFileSendProgress(int percent) {
		progressBarTrans.setProgress(percent);
	}

	/** �����˵� */
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.main_activity_options, menu);
		return true;
	}

	/** �˵����� */
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.menu_exit:
			// �رշ���
			Log.d(TAG, "menu_exit clicked");

			if (this._transService != null) {
				// ����δ���͵��ļ�
				int pendingFiles = _transService.getPendingFileCount();
				if (pendingFiles > 0) {
					askExitProgram(pendingFiles);
				} else {
					exitProgram();
				}
			} else {
				// û�д�������ļ���ֱ���˳�
				exitProgram();
			}

			break;
		case R.id.menu_settings: // ϵͳ����
			Intent intent = new Intent(this, AppPreferenceActivity.class);
			startActivity(intent);

			break;
		default:
			return super.onOptionsItemSelected(item);
		}

		return true;
	}

	/** ѯ���Ƿ�Ҫ�˳� */
	private void askExitProgram(int pendingCount) {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		// builder.setIcon(icon);
		builder.setTitle("�˳�");
		builder.setMessage("����" + pendingCount + "��δ���͵��ļ�����ȷ��Ҫ�˳�ô��");
		builder.setPositiveButton("�˳�", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				exitProgram();
			}
		});
		builder.setNegativeButton("ȡ��", new DialogInterface.OnClickListener() {

			public void onClick(DialogInterface dialog, int which) {
				//
			}
		});
		builder.setNegativeButton("�رս���",
				new DialogInterface.OnClickListener() {

					public void onClick(DialogInterface dialog, int which) {
						//
						finish();
					}
				});
		builder.show();
	}

	/** �˳����� */
	private void exitProgram() {
		Log.d(TAG, "Exiting...");
		Intent intent = new Intent(MainActivity.this, TransService.class);
		boolean result = stopService(intent);
		Log.d(TAG, "Stop service returned:" + result);

		this.finish();
	}
}