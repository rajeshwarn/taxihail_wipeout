﻿using System.Text;

namespace CMTPayment.Utilities
{
    public static class OAuthUtils
    {
        // 
        // This url encoder is different than regular Url encoding found in .NET 
        // as it is used to compute the signature based on a url.   Every document
        // on the web omits this little detail leading to wasting everyone's time.
        //
        // This has got to be one of the lamest specs and requirements ever produced
        //
        public static string PercentEncode(string s)
        {
            var sb = new StringBuilder();

            foreach (var c in Encoding.UTF8.GetBytes(s))
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' ||
                    c == '.' || c == '~')
                    sb.Append((char)c);
                else
                {
                    sb.AppendFormat("%{0:X2}", c);
                }
            }
            return sb.ToString();
        }
    }
}
