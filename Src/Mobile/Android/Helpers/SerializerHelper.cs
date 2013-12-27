using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
	public static class SerializerHelper
	{

		public static string Serialize<T> (this T pObject)
		{
			
			try
			{
				string XmlizedString = null;
				using (var memoryStream = new MemoryStream ())
				{
					XmlSerializer xs = new XmlSerializer (typeof(T));
					//XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8);
					XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, System.Text.UTF8Encoding.UTF8);
					xs.Serialize (xmlTextWriter, pObject);
					XmlizedString = UTF8ByteArrayToString (((MemoryStream)xmlTextWriter.BaseStream).ToArray ());
					return XmlizedString;
				}
			}
			catch
			{				
				return null;
			}
		}



		private static string UTF8ByteArrayToString (byte[] characters)
		{
			
			UTF8Encoding encoding = new UTF8Encoding ();
			string constructedString = encoding.GetString (characters);
			return (constructedString);
		}

		private static byte[] StringToUTF8ByteArray (string pXmlString)
		{
			UTF8Encoding encoding = new UTF8Encoding ();
			byte[] byteArray = encoding.GetBytes (pXmlString);
			return byteArray;
		}

		public static T DeserializeObject<T> (string pXmlizedString)
		{
			if (pXmlizedString.HasValue ())
			{
				try
				{
					XmlSerializer xs = new XmlSerializer (typeof(T));
					using (var memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)))
					{
						//XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8);
						return (T)xs.Deserialize (memoryStream);
					}
				}
				catch( Exception ex )
				{
						TinyIoCContainer.Current.Resolve<ILogger>().LogMessage( "Deserialization error" );
                        TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
				}
			}
			return default(T);
		}
		
	}
}

