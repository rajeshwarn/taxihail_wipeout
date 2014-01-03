using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class SerializerHelper
	{

		public static string Serialize<T> (this T pObject)
		{
			
			try
			{
				string XmlizedString;
				using (var memoryStream = new MemoryStream ())
				{
					var xs = new XmlSerializer (typeof(T));
					var xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8);
					xs.Serialize (xmlTextWriter, pObject);
					XmlizedString = UTF8ByteArrayToString (((MemoryStream)xmlTextWriter.BaseStream).ToArray ());
					return XmlizedString;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e);
				return null;
			}
		}



// ReSharper disable once InconsistentNaming
		private static string UTF8ByteArrayToString (byte[] characters)
		{
			
			var encoding = new UTF8Encoding ();
			var constructedString = encoding.GetString (characters);
			return (constructedString);
		}

// ReSharper disable once InconsistentNaming
		private static byte[] StringToUTF8ByteArray (string pXmlString)
		{
			var encoding = new UTF8Encoding ();
			var byteArray = encoding.GetBytes (pXmlString);
			return byteArray;
		}

		public static T DeserializeObject<T> (string pXmlizedString)
		{
			if (pXmlizedString.HasValue ())
			{
				try
				{
					var xs = new XmlSerializer (typeof(T));
					using (var memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)))
					{
						//XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8);
						return (T)xs.Deserialize (memoryStream);
					}
				}
				catch( Exception ex )
				{
					Logger.LogMessage( "Deserialization error" );
					Logger.LogError( ex );
				}
			}
			return default(T);
		}
		
	}
}

