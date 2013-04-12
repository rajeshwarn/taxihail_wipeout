using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common
{
    public class  TestCreditCards
        {
            public class Visa
            {
                public static string Number = "4012 0000 3333 0026".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 135;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class Mastercard
            {
                public static string Number = "5424 1802 7979 1732".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 135;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class AmericanExpress
            {
                public static string Number = "3410 9293 659 1002".Replace(" ", "");
                public static string ZipCode = "55555";
                public static int AvcCvvCvv2 = 1002;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

            public class Discover
            {
                public static string Number = "6011 0002 5950 5851".Replace(" ", "");
                public static string ZipCode = "00000";
                public static int AvcCvvCvv2 = 111;
                public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
            }

        }
}
