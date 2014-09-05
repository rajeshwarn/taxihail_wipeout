
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Enumeration
{
    public static class ChargeTypes
    {
        public static ListItem PaymentInCar = new ListItem { Id = 1, Display = "PaymentInCar" };
        public static ListItem Account = new ListItem { Id = 2, Display = "ChargeAccount" };
        public static ListItem CardOnFile = new ListItem { Id = 3, Display = "CardOnFile" };

        public static IEnumerable<ListItem> GetList()
        {
            return new List<ListItem> { PaymentInCar, Account, CardOnFile };
        }
    }
}
