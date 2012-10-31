using System;
using System.Collections.Specialized;
using System.Net;
using System.IO;

namespace MK.DeploymentService.Mobile
{
	public class FileUploader
	{
		public void SendIpa(string filename, string file)
		{
			
			//setup some variables end			
			String result = string.Empty;
			String strPost = string.Format("uploader_0_tmpname={0}&uploader_0_name={1}&ploader_0_status=done&uploader_count=1&email=matthieu.duluc@apcurium.com",
			                               filename, file);
			StreamWriter myWriter = null;
			
			var objRequest = (HttpWebRequest)WebRequest.Create("http://www.diawi.com/result.php");
			objRequest.Method = "POST";
			objRequest.ContentLength = strPost.Length;
			objRequest.ContentType = "application/x-www-form-urlencoded";

			try
			{
				myWriter = new StreamWriter(objRequest.GetRequestStream());
				myWriter.Write(strPost);
			}
			finally {
				myWriter.Close();
			}
			
			HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
			using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()) )
			{
				result = sr.ReadToEnd();
				sr.Close();
			}
		}

		public static void UploadFilesToDiawi(string file,string filename)
		{
			NameValueCollection nvc = new NameValueCollection();

			long length = 0;
			string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
			
			var httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://www.diawi.com/upload.php");
			httpWebRequest2.ContentType = "multipart/form-data; boundary=" + boundary;
			httpWebRequest2.Method = "POST";
			httpWebRequest2.KeepAlive = true;
			httpWebRequest2.Credentials = System.Net.CredentialCache.DefaultCredentials;
				
			
			var memStream = new System.IO.MemoryStream();			
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
			
			
			string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
			
			foreach (string key in nvc.Keys)
			{
				string formitem = string.Format(formdataTemplate, key, nvc[key]);
				byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
				memStream.Write(formitembytes, 0, formitembytes.Length);
			}
			
			
			memStream.Write(boundarybytes, 0, boundarybytes.Length);
			
			string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"blob\"\r\n Content-Type: application/octet-stream\r\n\r\n";
			
			string header = string.Format(headerTemplate, filename, file);			
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);			
			memStream.Write(headerbytes, 0, headerbytes.Length);

			
			FileStream fileStream = new FileStream(file, FileMode.Open,
			                                       FileAccess.Read);
			byte[] buffer = new byte[1024];
			
			int bytesRead = 0;
			
			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				memStream.Write(buffer, 0, bytesRead);				
			}			
			memStream.Write(boundarybytes, 0, boundarybytes.Length);
					
			fileStream.Close();
			
			httpWebRequest2.ContentLength = memStream.Length;
			
			Stream requestStream = httpWebRequest2.GetRequestStream();
			
			memStream.Position = 0;
			byte[] tempBuffer = new byte[memStream.Length];
			memStream.Read(tempBuffer, 0, tempBuffer.Length);
			memStream.Close();
			requestStream.Write(tempBuffer, 0, tempBuffer.Length);
			requestStream.Close();
			
			
			WebResponse webResponse2 = httpWebRequest2.GetResponse();
			
			Stream stream2 = webResponse2.GetResponseStream();
			StreamReader reader2 = new StreamReader(stream2);		
					
			webResponse2.Close();
			httpWebRequest2 = null;
			webResponse2 = null;
		}
	}
}

