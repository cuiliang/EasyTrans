package leal.easytrans;

import java.io.BufferedOutputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.SocketAddress;
import java.net.UnknownHostException;

import leal.easytrans.MainActivity.IncomingHandler;
import leal.easytrans.misc.FileHelper;
import leal.easytrans.misc.FileItem;
import leal.easytrans.misc.ParamKeys;
import leal.easytrans.misc.SendFileTask;
import leal.easytrans.net.PacketHeader;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.ContentResolver;
import android.content.Intent;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.os.Message;
import android.os.Messenger;
import android.os.RemoteException;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

public class TransService extends Service {

	final private String TAG = "TransService";

	/** ֪ͨ������ */
	private NotificationManager _NM;
	private int NOTIFICATION = R.string.trans_service_started;
	private final IBinder _binder = new LocalBinder();

	/** �������Ӳ��� */
	private String _pcIP = "";
	private int _pcPort = 0;
	private int _pcConnectionState = ParamKeys.PC_CONN_NOTCONNECTED;

	Socket _socket = null;

	/** �����ļ��Ķ��� */
	private SendFileTask _sendFileTask = new SendFileTask();

	/** MainActivity ���� */
	private Messenger _mainActivityMessenger = null;

	/** ��Activity���ʵĽӿڶ��� */
	public class LocalBinder extends Binder {
		TransService getService() {
			return TransService.this;
		}
	}

	@Override
	public IBinder onBind(Intent intent) {

		Log.d(TAG, "onBind , Intent =" + intent);
		return _binder;
	}

	@Override
	public void onCreate() {
		Log.d(TAG, "onCreate");

		//
		_NM = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);

		showNotification();
	}

	private boolean _isClosing = false; //�Ƿ����ڹرճ���
	
	@Override	
	public void onDestroy() {
		_isClosing = true;

		// �رչ����߳�
		disconnect(false);

		Log.d(TAG, "onDestroy");

		// _NM.cancel(NOTIFICATION);
		_NM.cancelAll();
	}

	@Override
	public void onRebind(Intent intent) {
		Log.d(TAG, "onRebind");
		super.onRebind(intent);
	}

	@Override
	public void onStart(Intent intent, int startId) {
		Log.d(TAG, "onStart");
		super.onStart(intent, startId);
	}

	@Override
	public boolean onUnbind(Intent intent) {

		Log.d(TAG, "onBind");

		this._mainActivityMessenger = null;

		return super.onUnbind(intent);
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		Log.d(TAG, "onStartCommand, start id=" + startId + " : " + intent);

		return START_STICKY;
	}

	
	Thread _clientThread = null;

	/** ����PC */
	public boolean connect(String pcIP, int pcPort) {
		if (!canConnect())
		{
			Log.e(TAG, "����������");
			return false;
		}
		
		// �������ӱ�־
		_hasTryToConnected = true;

		//
		this._pcIP = pcIP;
		this._pcPort = pcPort;

		// �����ͻ����߳�
		if (_clientThread != null && _clientThread.isAlive()) {
			Log.e(TAG, "Thread is running!");
			return false;
		}

		_clientThread = new Thread(new ClientThread());
		_clientThread.start();

		// update ui
		// notifyPcConnectionStateChange();

		return true;
	}
	
	public boolean canConnect()
	{
		return (_clientThread == null);
	}
	
	public void setPcAddr(String pcIP, int pcPort)
	{
		Log.d(TAG, "set ip and port" + pcIP + "  " + pcPort);
		//
		this._pcIP = pcIP;
		this._pcPort = pcPort;
	}

	/**
	 * �ر�����
	 * 
	 * @param noNotify
	 *            �Ƿ���֪ͨ��Ϣ
	 */
	public void disconnect(boolean sendNotify) {
		// TODO: disconnect
		if (_clientThread != null && _clientThread.isAlive()) {
			_clientThread.interrupt();
			_clientThread = null;
		}

		if (_socket != null) {
			try {
				_socket.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				Log.e(TAG, "Close socket failed! Perhaps already closed.");
			}

			_socket = null;
		}

		// TODO:
		this._pcConnectionState = ParamKeys.PC_CONN_NOTCONNECTED;

		// update ui
		if (sendNotify) {
			notifyPcConnectionStateChange();
		}
	}

	/** �Ƿ������ӵ�PC */
	public int getPcConnectionState() {
		return _pcConnectionState;
	}

	public String getPcIP() {
		return _pcIP;
	}

	public int getPcPort() {
		return _pcPort;
	}

	/** ���������ڵ�Messenger���� */
	public void setMainActivityMessenger(Messenger messenger) {
		_mainActivityMessenger = messenger;

		// ���Ӻ�һ������״̬����Ϣ
		notifyMainActivityFileCountChange();
		notifyPcConnectionStateChange();
	}

	/** ���ļ����Ͷ������һ���ļ� */
	public void sendFile(FileItem fileItem) {
		_sendFileTask.AddFileTask(fileItem);

		notifyMainActivityFileCountChange();
	}

	/** ��MainActivity��Service������Ϣ */
	private void broadcastMessage(Message msg) {
		// ��MacinActivity ������Ϣ
		if (_mainActivityMessenger != null) {
			try {
				_mainActivityMessenger.send(msg);
			} catch (RemoteException e) {
				Log.e(TAG, "MainActivityMessenger is not valid!");
				e.printStackTrace();
			}
		}

		Message newMsg = new Message();
		newMsg.copyFrom(msg);

		// ��Service������Ϣ
		try {
			mMessenger.send(newMsg);
		} catch (RemoteException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	/** ֪ͨ����������״̬�ı� */
	private void notifyPcConnectionStateChange() {
		Message msg = Message.obtain(null, ParamKeys.MSG_PC_CONN_STATE_CHANGED);
		msg.arg1 = _pcConnectionState;

		broadcastMessage(msg);

	}

	/** ֪ͨ�������ļ������ı� */
	private void notifyMainActivityFileCountChange() {
		Message msg = Message.obtain(null, ParamKeys.MSG_FILE_COUNT_CHANGED);
		msg.arg1 = _sendFileTask.GetPendingFileCount();
		msg.arg2 = _sendFileTask.GetSendedFileCount();
		broadcastMessage(msg);
	}

	/** �õ�ʣ���ļ����� */
	public int getPendingFileCount() {
		return _sendFileTask.GetPendingFileCount();
	}

	/** ��������״̬��֪ͨ������ */
	private void changeConnectionState(int state) {
		_pcConnectionState = state;
		notifyPcConnectionStateChange();
	}

	/** �������淢���ļ����ͽ�����Ϣ */
	private void notifyFileSendProgress(int percent) {
		Message msg = Message.obtain(null, ParamKeys.MSG_FILE_SEND_PROGRESS);
		msg.arg1 = percent;
		broadcastMessage(msg);
	}

	/** �������淢���ļ���ʼ������ת�͵���Ϣ */
	private void notifySendFileBeginEnd(boolean isBegin) {
		Message msg = null;
		if (isBegin) {
			msg = Message.obtain(null, ParamKeys.MSG_BEGIN_SEND_FILE);
			broadcastMessage(msg);
		} else {
			msg = Message.obtain(null, ParamKeys.MSG_END_SEND_FILE);
			broadcastMessage(msg);
		}
	}

	/** �����̷߳���������Ϣ */
	final Messenger mMessenger = new Messenger(new IncomingHandler());

	class IncomingHandler extends Handler {
		@Override
		public void handleMessage(Message msg) {

			switch (msg.what) {
			case ParamKeys.MSG_PC_CONN_STATE_CHANGED:
				if (_pcConnectionState != ParamKeys.PC_CONN_CONNECTING) // �����г���ʱ��̣�������״̬��֪ͨ
				{
					showNotification(getConnectionStateIcon(),
							getConnectionStateString(), "�״�������",
							getNotifyContentText());
				}
				break;
			case ParamKeys.MSG_FILE_COUNT_CHANGED:
				//showNotification(R.drawable.ic_state_sending, "���ڴ���...",
				//		"�״�������", getNotifyContentText());
				break;
			case ParamKeys.MSG_FILE_SEND_PROGRESS:
				break;
			case ParamKeys.MSG_BEGIN_SEND_FILE:
				showNotification(R.drawable.ic_state_sending, "��ʼ�����ļ�...",
						"�״�������", getNotifyContentText());
				break;
			case ParamKeys.MSG_END_SEND_FILE:
				showNotification(R.drawable.ic_state_connected, "�ļ��������",
						"�״�������", getNotifyContentText());
				break;
			default:
				super.handleMessage(msg);
			}
		}
	}

	/** �õ�����״̬�ַ��� */
	private CharSequence getConnectionStateString() {
		CharSequence stateStr = "";
		switch (_pcConnectionState) {
		case ParamKeys.PC_CONN_CONNECTED:
			stateStr = "������PC";
			break;
		case ParamKeys.PC_CONN_CONNECTING:
			stateStr = "��������...";
			break;
		case ParamKeys.PC_CONN_NOTCONNECTED:
			stateStr = "δ����PC";
			break;
		}

		return stateStr;
	}

	/** �õ�����״̬ͼ�� */
	private int getConnectionStateIcon() {
		switch (_pcConnectionState) {
		case ParamKeys.PC_CONN_CONNECTED:
			return R.drawable.ic_state_connected;

		case ParamKeys.PC_CONN_CONNECTING:
			return R.drawable.ic_state_connecting;

		case ParamKeys.PC_CONN_NOTCONNECTED:
			return R.drawable.ic_state_disconnected;

		default:
			return R.drawable.ic_state_disconnected;

		}
	}

	/** �õ�֪ͨ���� */
	private CharSequence getNotifyContentText() {
		CharSequence stateStr = getConnectionStateString();

		// if (_sendFileTask.GetPendingFileCount() > 0) {
		stateStr = stateStr + "  ������:" + _sendFileTask.GetPendingFileCount()
				+ "  �Ѵ���:" + _sendFileTask.GetSendedFileCount();
		// }

		return stateStr;
	}

	/** ��ʾ״̬��֪ͨ */
	Notification _notification = null;

	private void showNotification() {
		showNotification(R.drawable.ic_state_disconnected, "�״�������", "�״�������",
				getNotifyContentText());
	}

	private void showNotification(int icon, CharSequence ticker,
			CharSequence title, CharSequence contentText) {
		if (_isClosing)
		{
			return;
		}
		Log.d(TAG, "showNotification " + icon + " ticker:" + ticker + " title:" + title + " content:" + contentText);
		

		if (_notification == null) {
			_notification = new Notification(icon, ticker,
					System.currentTimeMillis());
			_notification.flags = _notification.flags
					| Notification.FLAG_ONGOING_EVENT
					| Notification.FLAG_ONLY_ALERT_ONCE;
		} else {
			_notification.icon = icon;
			_notification.tickerText = ticker;
		}

		Intent intent = new Intent(this, MainActivity.class);
		intent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP
				| Intent.FLAG_ACTIVITY_SINGLE_TOP);
		PendingIntent contentIntent = PendingIntent.getActivity(this, 0,
				intent, PendingIntent.FLAG_UPDATE_CURRENT);

		_notification.setLatestEventInfo(this, title, contentText,
				contentIntent);
		// _notification.contentView.set

		_NM.notify(NOTIFICATION, _notification);
	}

	/** ��ʼ����־����һ������ʱ����ȡ���� */
	private boolean _hasTryToConnected = false;

	public boolean hasTryToConnected() {
		return _hasTryToConnected;
	}

	// �����߳�
	public class ClientThread implements Runnable {
		final private String TAG = "ClientThread";

		public void run() {
			try {
				Log.d("ClientThread", "start thread");
				changeConnectionState(ParamKeys.PC_CONN_CONNECTING);

				// ��ʼ����PC
				_socket = new Socket();

				SocketAddress remoteAddr = new InetSocketAddress(_pcIP, _pcPort);

				// try
				// {
				_socket.connect(remoteAddr, 3000);
				// }catch(Exception ex)
				// {
				// Toast.makeText(TransService.this, "�޷����ӵ�PC������" +
				// ex.getMessage(), Toast.LENGTH_LONG).show();
				// }

				Log.d("ClientThread", "end new thread");
				changeConnectionState(ParamKeys.PC_CONN_CONNECTED);

				// �õ�BufferWriter
				OutputStream os = _socket.getOutputStream();
				//BufferedOutputStream out = new BufferedOutputStream(os, 1024);
				DataOutputStream dos = new DataOutputStream(os);

				// BufferedWriter wr = new BufferedWriter(new
				// OutputStreamWriter(
				// os, "UTF-8"));

				// TODO ѭ�������ļ�
				boolean isSending = false; // �Ƿ����ڴ���

				while (!Thread.interrupted()) {
					if (_socket.isClosed() || _socket.isConnected() == false) {
						break;
					}

					if (TransService.this._sendFileTask.GetPendingFileCount() == 0) {
						// ����淢��ֹͣ�����ļ�����Ϣ
						if (isSending) {
							isSending = false;
							notifySendFileBeginEnd(false);
						}

						try {
							Thread.sleep(1000);
						} catch (InterruptedException e) {
							// TODO Auto-generated catch block
							break;
						}
					}

					FileItem fileItem = _sendFileTask.GetNextFile();
					if (fileItem != null) {
						// ����淢�Ϳ�ʼ�����ļ�����Ϣ
						if (!isSending) {
							isSending = true;
							notifySendFileBeginEnd(true);
						}

						// do send file
						try
						{
							boolean success = doSendFile(fileItem, dos);
						}catch (IOException ex)
						{
							Log.e(TAG, "Send file error:" + ex.getMessage());
							throw ex;													
						}

						// remove file from task
						_sendFileTask.OnFileSended(fileItem);
						
						// ֪ͨ���̴߳������ļ������ˡ�
						notifyMainActivityFileCountChange();
					}
				}

				if (_socket != null && !_socket.isClosed()) {
					_socket.close();
					_socket = null;
				}
				
				changeConnectionState(ParamKeys.PC_CONN_NOTCONNECTED);

			} catch (UnknownHostException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();

				changeConnectionState(ParamKeys.PC_CONN_NOTCONNECTED);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();

				changeConnectionState(ParamKeys.PC_CONN_NOTCONNECTED);
			}

		}

		/**
		 * ִ�з����ļ��Ĳ���
		 * 
		 * @throws IOException
		 */
		private boolean doSendFile(FileItem fileItem, DataOutputStream os)
				throws IOException {
			Log.d(TransService.this.TAG, "sending file.");

			// ֪ͨ�������ļ����ͽ���
			notifyFileSendProgress(0);

			File file = new File(fileItem.filePathName);
			String fileName = file.getName();
			long fileLen = file.length();

			ContentResolver cr = getContentResolver();
			
			Log.d(TAG, "Sending file:" + fileName + " size=" + fileLen);

			InputStream is = null;
			try {
				is = new FileInputStream(file);
			} catch (FileNotFoundException e2) {
				// TODO Auto-generated catch block
				e2.printStackTrace();

				return false;
			}

			int serial = PacketHeader.GetNextObjectSerial();

			// �����ļ�ͷ��Ϣ
			{
				StringBuffer str = new StringBuffer();
				str.append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
				str.append("<file>");
				str.append("<uri>" + fileItem.uri + "</uri>");
				str.append("<filename>" + fileName + "</filename>");
				str.append("<filelength>" + fileLen + "</filelength>");
				str.append("<mime>" + fileItem.mime + "</mime>");
				str.append("</file>");

				String fileInfo = str.toString();
				byte[] byteArray = null;
				try {
					byteArray = fileInfo.getBytes("UTF-8");
				} catch (UnsupportedEncodingException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();

					return false;
				}

				PacketHeader header = new PacketHeader();
				header.ObjectSerial = serial;
				header.Index = 0;
				header.PacketType = PacketHeader.PACKET_TYPE_FILE_INFO;
				header.SubPacketType = 0;
				header.AnswerObjectSerial = 0;
				header.ContentLength = byteArray.length;
				header.WriteHeader(os);

				os.write(byteArray);
				os.flush();
			}

			// �����ļ�����
			{
				PacketHeader fileHeader = new PacketHeader();
				fileHeader.ObjectSerial = serial;
				fileHeader.Index = 1;
				fileHeader.PacketType = PacketHeader.PACKET_TYPE_FILE_CONTENT;
				fileHeader.SubPacketType = 0;
				fileHeader.AnswerObjectSerial = 0;
				fileHeader.ContentLength = fileLen;

				// head
				fileHeader.WriteHeader(os);
				// os.flush();

				// content
				final int PACKET_LEN = 1024;
				byte[] buffer = new byte[PACKET_LEN];

				int currPos = 0;

				while (currPos < fileLen) {
					int readLen = is.read(buffer);
					if (readLen <= 0) {
						Log.e(TAG, "ReadBreaked, readLen=" + readLen);
						break;
					}

					os.write(buffer, 0, readLen);
					currPos += readLen;

					// ֪ͨ���������
					notifyFileSendProgress((int) (currPos * 100.0 / fileLen));
				}

				Log.d(TAG, "All sended:" + currPos);


				os.flush();

				// �ر��ļ�
				try {
					is.close();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

			}
			
			return true;

		}

	}

}
