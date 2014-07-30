
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Enumeration
{
    public static class ChargeTypes
    {
        public static ListItem PaymentInCar = new ListItem { Id = 1, Display = "Payment In Car" };
        public static ListItem Account = new ListItem { Id = 2, Display = "Charge Account" };
        public static ListItem CardOnFile = new ListItem { Id = 3, Display = "Card on File" };

        public static IEnumerable<ListItem> GetList()
        {
            return new List<ListItem> { PaymentInCar, Account, CardOnFile };
        }
    }
}
