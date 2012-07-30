//using System;
//using System.Xml.Linq;
//
//namespace apcurium.MK.Booking.Mobile.Client
//{
//	public static class XMLLinqHelper
//	{
//		
//		
//		public static string GetValue ( this XElement data , string attribute )
//		{
//			var att = data.Element(attribute);
//			if ( att != null )
//			{
//				if ( att.Value == null )
//				{
//					return "";
//				}
//				else 
//				{
//					return att.Value;
//				}
//			}
//			
//			return "";
//		}
//		
//		public static  int? GetIntValue ( this XElement data , string attribute )
//		{
//			int? result = null;
//			var att = data.Element(attribute);
//			if ( att != null )
//			{
//				if ( att.Value != null )
//				{					
//					int v;
//					if ( int.TryParse( att.Value , out v ) )
//					{
//						result = v;
//					}
//					
//				}
//			}			
//			return result;
//		}
//	}
//}
//
