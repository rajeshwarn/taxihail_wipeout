using System;

namespace apcurium.MK.Common
{
    public class CreditCard
    {

        public string Number;
        public int AvcCvvCvv2;
        public DateTime ExpirationDate = DateTime.Today.AddMonths(3);

        public CreditCard(string number, int cvv = 135)
        {
            Number = number;
            AvcCvvCvv2 = cvv;
        }
    }
}