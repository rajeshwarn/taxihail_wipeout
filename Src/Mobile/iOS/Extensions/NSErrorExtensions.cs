using System;
using Foundation;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class NSErrorExtensions
    {
        public static string GetNSErrorString (this NSError nsError)
        {
            if ( nsError == null )
            {
                return "No Error Info Available";
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Error Code:  {0}\r\n", nsError.Code.ToString());
                sb.AppendFormat("Description: {0}\r\n", nsError.LocalizedDescription);
                var userInfo = nsError.UserInfo;
                for ( int i = 0; i < userInfo.Keys.Length; i++ )
                {
                    sb.AppendFormat("[{0}]: {1}\r\n", userInfo.Keys[i].ToString(), userInfo.Values[i].ToString() );
                }
                return sb.ToString();
            }
            catch
            {
                return "Error parsing NSError object. Ironic, is it not ?";
            }
        }
    }
}

