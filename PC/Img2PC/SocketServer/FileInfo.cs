using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Img2PC.SocketServer
{
    /// <summary>
    /// 文件信息
    /// </summary>
    class FileInfo
    {
        public string Uri;
        public string FileName;
        public long FileSize;
        public string PathName;
        public int ObjectSerial;
        public string Mime;

        public void ParseFromString(string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(value);
            XmlNode uriNode = doc.DocumentElement["uri"];
            XmlNode filenameNode = doc.DocumentElement["filename"];
            XmlNode filelengthNode = doc.DocumentElement["filelength"];
            XmlNode mimeNode = doc.DocumentElement["mime"];

            Uri = (uriNode == null) ? "" : uriNode.InnerText;
            FileName = (filenameNode == null) ? "" : filenameNode.InnerText;
            Mime = (mimeNode == null) ? "" : mimeNode.InnerText;
            if (filelengthNode != null && !String.IsNullOrEmpty(filelengthNode.InnerText))
            {
                FileSize = Convert.ToInt64(filelengthNode.InnerText);
            }

            //string caption = captionNode.InnerText;
            //string data = dataNode.InnerText;
            //byte[] bytes = Convert.FromBase64String(data);
            //try
            //{
            //    string filename = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".jpg";

            //    string pathName = Path.Combine(RecvFolder, filename);

            //    File.WriteAllBytes(pathName, bytes);

            //    MyInvoke mi = new MyInvoke(OnFileRecived); BeginInvoke(mi, new object[] { filename });
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
        }
    }
}
