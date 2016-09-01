using System;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using log4net;

namespace MK.DeploymentService.Mobile
{
	public class FileUploader
	{   
        private static Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden
		private string RandomString(int size)
		{
			var builder = new StringBuilder();
		    for (var i = 0; i < size; i++)
			{
			    var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
			    builder.Append(ch);
			}

		    return builder.ToString();
		}

		public void Upload (string filePath)
		{
			var tempName = RandomString(8) + ".ipa";
			UploadFilesToDiawi(tempName, filePath);
			var fileInfo = new FileInfo(filePath);
			SendIpa(tempName, fileInfo.Name);
		}

		private void SendIpa(string tempName, string fileName)
		{
			
			//setup some variables end			
		    var strPost = string.Format("uploader_0_tmpname={0}&uploader_0_name={1}&ploader_0_status=done&uploader_count=1&email=taxihail.build@apcurium.com",
			                               tempName, fileName);
			
            var objRequest = (HttpWebRequest)WebRequest.Create("http://www.diawi.com/result.php");
			objRequest.Method = "POST";
			objRequest.ContentLength = strPost.Length;
			objRequest.ContentType = "application/x-www-form-urlencoded";

			using(var myWriter = new StreamWriter(objRequest.GetRequestStream()))
			{
                    myWriter.Write(strPost);
			}
			
            var objResponse = (HttpWebResponse)objRequest.GetResponse();
			using (var sr = new StreamReader(stream: objResponse.GetResponseStream()) )
			{
				var result = sr.ReadToEnd();
                var json = JObject.Parse(result);
                var resultValue = json.Value<string>("status");
                if (resultValue != "ok")
                {
                    throw new Exception("Error during upload to diawi : " + resultValue);
                }
				sr.Close();
			}
		}

		private void UploadFilesToDiawi(string tempName, string filePath)
		{
			var nvc = new NameValueCollection();
			nvc.Add("name", tempName);

            var boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

		    var httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://www.diawi.com/upload.php");
			httpWebRequest2.ContentType = "multipart/form-data; boundary=" + boundary;
			httpWebRequest2.Method = "POST";
			httpWebRequest2.KeepAlive = true;
			
			var memStream = new MemoryStream();			
			var boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
			
			foreach (string key in nvc.Keys)
			{
                var formitem = string.Format(formdataTemplate, key, nvc[key]);
				var formitembytes = Encoding.UTF8.GetBytes(formitem);
				memStream.Write(formitembytes, 0, formitembytes.Length);
			}
			
			
			memStream.Write(boundarybytes, 0, boundarybytes.Length);

            const string header = "Content-Disposition: form-data; name=\"file\"; filename=\"blob\"\r\n Content-Type: application/octet-stream\r\n\r\n";
			
			var headerbytes = Encoding.UTF8.GetBytes(header);			
			memStream.Write(headerbytes, 0, headerbytes.Length);


            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var buffer = new byte[1024];

            int bytesRead;
			
			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				memStream.Write(buffer, 0, bytesRead);				
			}			
			memStream.Write(boundarybytes, 0, boundarybytes.Length);
					
			fileStream.Close();
			
			httpWebRequest2.ContentLength = memStream.Length;

            var requestStream = httpWebRequest2.GetRequestStream();
			
			memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
			memStream.Read(tempBuffer, 0, tempBuffer.Length);
			memStream.Close();
			requestStream.Write(tempBuffer, 0, tempBuffer.Length);
			requestStream.Close();


            using(var webResponse2 = httpWebRequest2.GetResponse())
            {
                using (var stream2 = webResponse2.GetResponseStream())
                {
                    if (stream2 != null)
                        using (var reader2 = new StreamReader(stream2))
                        {
                            var result = reader2.ReadToEnd();
                            var json = JObject.Parse(result);
                            var resultValue = json.Value<string>("result");
                            if(resultValue != null)
                            {
                                throw  new Exception("Error during upload to diawi : " + resultValue);
                            }
                        }
                }
            }
			
		}
	}
}

