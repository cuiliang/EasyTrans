package leal.easytrans.misc;

import java.util.LinkedList;
import java.util.Queue;

public class SendFileTask {

	/** 文件任务队列 */
	private Queue<FileItem> _fileQueue = new LinkedList<FileItem>();
	
	private LinkedList<FileItem> _sendedFiles = new LinkedList<FileItem>();
	
	public void AddFileTask(FileItem item)
	{
		synchronized(this)
		{
			_fileQueue.offer(item);
			this.notifyAll();
		}
	}
	
	public int GetPendingFileCount()
	{
		return _fileQueue.size();
	}
	
	public int GetSendedFileCount()
	{
		return _sendedFiles.size();
	}
	
	public FileItem GetNextFile()
	{		
		return _fileQueue.peek();
	}
	
	public void OnFileSended(FileItem item)
	{
		synchronized(this)
		{
			_fileQueue.remove(item);
			
		}
		
		_sendedFiles.add(item);
	}
	
	
	public void ClearObjects()
	{
		synchronized(this)
		{
			_fileQueue.clear();		
		}
		
		_sendedFiles.clear();
	}
	
	
}
