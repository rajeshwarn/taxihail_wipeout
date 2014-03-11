#region

using System;

#endregion

namespace apcurium.MK.Common
{
    public class CreditCard
    {
        public int AvcCvvCvv2;
        public DateTime ExpirationDate = DateTime.Today.AddYears(3);
        public string Number;

        public CreditCard(string number, int cvv = 135)
        {
            Number = number;
            AvcCvvCvv2 = cvv;
        }
    }
}