#region

using System;
using System.Net.Http.Headers;
using System.Text;
using apcurium.MK.Common.Extensions;
using Newtonsoft.Json;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public static class Extensions
    {
        public static DateTime? ToDateTime(this TWEBTimeStamp etaTime)
        {
            return etaTime == null || etaTime.Year < DateTime.Now.Year
                ? (DateTime?) null
                : new DateTime(
                    etaTime.Year,
                    etaTime.Month,
                    etaTime.Day,
                    etaTime.Hour,
                    etaTime.Minute,
                    etaTime.Second);
        }
        public static TWEBTimeStamp ToTWEBTimeStamp(this DateTime date, int addMinutes = 0)
        {
            return new TWEBTimeStamp()
            {
                Day = date.Day,
                Month = date.Month,
                Year = date.Year,
                Hour = date.Hour,
                Minute = date.Minute + addMinutes,
                Second = date.Second
            };
        }

        public static string FormatWith(this string format, params object[] parameters)
        {
            return string.Format(format, parameters);
        }
        public static string Format(this string format, object arg1, object arg2)
        {
            return string.Format(format, arg1, arg2);
        }
        public static string Format(this string format, object arg1, object arg2, object arg3)
        {
            return string.Format(format, arg1, arg2, arg3);
        }


        public static T FromJson<T>(this string json)
        {
            return json.FromJsonSafe<T>();
        }
        public static HttpRequestHeaders Set(this HttpRequestHeaders headers, string key, string value)
        {
            headers.Remove(key);
            headers.Add(key, value);
            return headers;
        }
        public static HttpRequestHeaders SetLoose(this HttpRequestHeaders headers, string key, string value)
        {
            headers.Remove(key);
            headers.TryAddWithoutValidation(key, value);
            return headers;
        }


        private static readonly Encoding Encoding = Encoding.ASCII;
        public static string FromBase64(this string encoded)
        {
            var tobedecodedbytes = Convert.FromBase64String(encoded);
            return Encoding.GetString(tobedecodedbytes);
        }
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this string plainText)
        {
            var plainTextBytes = Encoding.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static byte[] ToBytes(this string str)
        {
            return Encoding.GetBytes(str);


            //var bytes = new byte[str.Length * sizeof(char)];
            //Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            //return bytes;
        }
        public static string FromBytes(this byte[] ba)
        {

            //string hex = BitConverter.ToString(ba);
            //return hex.Replace("-", "");

            //char[] chars = new char[ba.Length / sizeof(char)];
            //System.Buffer.BlockCopy(ba, 0, chars, 0, ba.Length);
            //return new string(chars);


            return Encoding.GetString(ba);

            //var hex = new StringBuilder(ba.Length * 2);
            //foreach (byte b in ba)
            //    hex.Append((char)b);
            //return hex.ToString();
        }
        public static string FromHexBytes(this byte[] ba)
        {

            //string hex = BitConverter.ToString(ba);
            //return hex.Replace("-", "");

            //char[] chars = new char[ba.Length / sizeof(char)];
            //System.Buffer.BlockCopy(ba, 0, chars, 0, ba.Length);
            //return new string(chars);


            //return System.Text.Encoding.Default.GetString(ba);

            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}