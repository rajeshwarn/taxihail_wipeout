using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Enumeration
{
    public class PromotionTriggerTypes
    {
        public static readonly ListItem NoTrigger = new ListItem { Id = 0, Display = "No Trigger" };
        public static readonly ListItem AccountCreated = new ListItem { Id = 1, Display = "Account Created" };
        public static readonly ListItem RideCount = new ListItem { Id = 2, Display = "Ride Count" };
        public static readonly ListItem AmountSpent = new ListItem { Id = 3, Display = "Amount Spent" };

        public static IEnumerable<ListItem> GetList()
        {
            return new[] { NoTrigger, AccountCreated, RideCount, AmountSpent };
        }
    }
}