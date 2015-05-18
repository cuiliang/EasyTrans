package leal.easytrans.misc;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;

import android.app.Activity;
import android.database.Cursor;
import android.net.Uri;
import android.provider.MediaStore;
import android.util.Log;

public class FileHelper {

	/// Get Path from URI
		public static String getRealPathFromURI(Activity activity, Uri uri) {
//	        String[] proj = { MediaStore.Images.Media.DATA };
//	        Cursor cursor = activity.managedQuery(contentUri, proj, null, null, null);
//	        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
//	        cursor.moveToFirst();
//	        return cursor.getString(column_index);
			
			String fileName = "";
			
			String scheme = uri.getScheme();
			if (scheme.equals("file")) {
			    fileName = uri.getLastPathSegment();
			}
			else if (scheme.equals("content")) {
			    String[] proj = { MediaStore.Images.Media.TITLE };
			    Cursor cursor = activity.getContentResolver().query(uri, proj, null, null, null);
			    if (cursor != null && cursor.getCount() != 0) {
			        int columnIndex = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.TITLE);
			        cursor.moveToFirst();
			        fileName = cursor.getString(columnIndex);
			    }
			}
			
			return fileName;
	    }
		
		public static byte[] getBytesFromFile(InputStream is) {
			try {
				ByteArrayOutputStream buffer = new ByteArrayOutputStream();

				int nRead;
				byte[] data = new byte[16384];

				while ((nRead = is.read(data, 0, data.length)) != -1) {
					buffer.write(data, 0, nRead);
				}

				buffer.flush();

				return buffer.toByteArray();
			} catch (IOException e) {
				Log.e("com.eggie5.post_to_eggie5", e.toString());
				return null;
			}
		}
		
		/* 根据URI得到 路径*/
		public static String getFilePathFromUri(Activity activity,Uri uri)
		{	
			String fileName = "";			
			
			String scheme = uri.getScheme();
			
			if (scheme.equals("file")) {
			    fileName = uri.getPath();
			}
			else if (scheme.equals("content")) {
			    String[] proj = { MediaStore.Images.Media.DATA };
			    Cursor actualimagecursor = activity.managedQuery(uri,proj,null,null,null);

				int actual_image_column_index = actualimagecursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

				actualimagecursor.moveToFirst();

				fileName = actualimagecursor.getString(actual_image_column_index);
			}
			
			return fileName;
		}
		
//		/**
//		 * Gets the last image id from the media store
//		 * @return
//		 */
//		private int getLastImageId(){
//		    final String[] imageColumns = { MediaStore.Images.Media._ID, MediaStore.Images.Media.DATA };
//		    final String imageOrderBy = MediaStore.Images.Media._ID+" DESC";
//		    Cursor imageCursor = managedQuery(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, imageColumns, null, null, imageOrderBy);
//		    if(imageCursor.moveToFirst()){
//		        int id = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media._ID));
//		        String fullPath = imageCursor.getString(imageCursor.getColumnIndex(MediaStore.Images.Media.DATA));
//		        //Log.d(TAG, "getLastImageId::id " + id);
//		        //Log.d(TAG, "getLastImageId::path " + fullPath);
//		        imageCursor.close();
//		        return id;
//		    }else{
//		        return 0;
//		    }
//		}
		
		public static CharSequence getLastImagePathName(Activity activity){
		    final String[] imageColumns = { MediaStore.Images.Media._ID, MediaStore.Images.Media.DATA };
		    final String imageOrderBy = MediaStore.Images.Media._ID+" DESC";
		    Cursor imageCursor = activity.managedQuery(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, imageColumns, null, null, imageOrderBy);
		    if(imageCursor.moveToFirst()){
		        int id = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media._ID));
		        String fullPath = imageCursor.getString(imageCursor.getColumnIndex(MediaStore.Images.Media.DATA));
		        //Log.d(TAG, "getLastImageId::id " + id);
		        //Log.d(TAG, "getLastImageId::path " + fullPath);
		        imageCursor.close();
		        return fullPath;
		    }else{
		        return "";
		    }
		}
}
