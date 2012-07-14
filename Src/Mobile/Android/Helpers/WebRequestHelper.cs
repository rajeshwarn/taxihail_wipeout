using System;
using System.IO;
using System.Net;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
	public class WebRequestHelper
	{
		public WebRequestHelper ()
		{
		}


		private static int _countConnection;

		public static void UploadFile ( string file, string pageUrl, Action afterUpload )
		{
			ShowNetworkActivity (  );
			WebClient client = new WebClient (  );
			client.UploadFileCompleted += delegate
			{
				try
				{
					if ( afterUpload != null )
					{
						afterUpload (  );
					}
				}
				finally
				{
					HideNetworkActivity (  );
				}
			};
			client.UploadFileAsync ( new Uri ( pageUrl ), file );
			
		}

		public static void ShowNetworkActivity ()
		{			
			//UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			_countConnection++;
		}

		public static void HideNetworkActivity ()
		{
			_countConnection--;
			
			if ( _countConnection <= 0 )
			{
				//UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				_countConnection = 0;
			}
			
		}


		public static Stream Read ( string pageUrl, int timeoutInSecond )
		{
			ShowNetworkActivity (  );
			
			HttpWebRequest request = null;
			HttpWebResponse response = null;
			MemoryStream result = null;
			byte[] buf = new byte[8192];			
			
			try
			{
				// prepare the web page we will be asking for
				request = ( HttpWebRequest ) WebRequest.Create ( pageUrl );
				request.Timeout = timeoutInSecond * 1000;
											
				// execute the request
				response = ( HttpWebResponse ) request.GetResponse (  );
				
				// we will read data via the response stream
				using ( var resStream = response.GetResponseStream (  ) )
				{
					
					int count = 0;					
					result = new MemoryStream (  );
					
					do
					{
						// fill the buffer with data
						count = resStream.Read ( buf, 0, buf.Length );
						
						// make sure we read some data
						if ( count != 0 )
						{
							result.Write ( buf, 0, count );
						}
					}
					while ( count > 0 );
					// any more data to read?
					result.Position = 0;
				}
				
			}
			catch ( Exception ex )
			{
				TinyIoCContainer.Current.Resolve<ILogger>().LogError ( ex );
			}
			finally
			{
				if ( response != null )
				{
					response.Close (  );
				}
				HideNetworkActivity (  );
			}
			return result;
		}

		public static string ReadString ( string pageUrl, int timeoutInSecond )
		{
			
			string result = null;
			ShowNetworkActivity (  );
			HttpWebResponse response = null;
			HttpWebRequest request = null;
			
			try
			{
				// prepare the web page we will be asking for
				request = ( HttpWebRequest ) WebRequest.Create ( pageUrl );
				request.Timeout = timeoutInSecond * 1000;
								
				// execute the request
				response = ( HttpWebResponse ) request.GetResponse (  );
				
				// we will read data via the response stream
				using ( var resStream = response.GetResponseStream (  ) )
				{
					
					Encoding encode = System.Text.Encoding.GetEncoding ( "utf-8" );
					// Pipe the stream to a higher level stream reader with the required encoding format. 
					StreamReader readStream = new StreamReader ( resStream, encode );
					
					char[] read = new char[256];
					
					// Read 256 charcters at a time.    
					int count = readStream.Read ( read, 0, 256 );
					result = "";
					while ( count > 0 )
					{
						// Dump the 256 characters on a string and display the string onto the console.
						string str = new string ( read, 0, count );
						
						result += str;
						
						count = readStream.Read ( read, 0, 256 );
					}
					
				}
				
			}
			catch ( Exception ex )
			{
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
			}
			finally
			{
				if ( response != null )
				{
					response.Close (  );
				}
				HideNetworkActivity (  );
			}
			return result;
		}

		private static string UTF8ByteArrayToString ( byte[] characters )
		{
			
			var encoding = new UTF8Encoding (  );
			string constructedString = encoding.GetString ( characters );
			return ( constructedString );
		}


		public static string GetServerFile ( string url, int timeoutInSecond )
		{
			ShowNetworkActivity (  );
			
			string stream;
			try
			{
				stream = WebRequestHelper.ReadString ( url, timeoutInSecond );				
				if ( stream == null )
				{
					//StandardMessages.ShowNotConnectedWarning (  );
				}
			}
			finally
			{
				HideNetworkActivity (  );
			}
			return stream;
		}
		
		
		
	}
}

