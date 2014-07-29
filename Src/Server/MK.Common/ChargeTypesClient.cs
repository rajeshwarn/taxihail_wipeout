using System;
using System.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Common
{
    public static class ChargeTypesClient
    {
        public static ListItem[] GetPaymentsList(ListItem company)
        {
            if (company == null) throw new ArgumentNullException("company");
            if (company.Id == null) throw new ArgumentException("company Id should not be null");

            return (from type in (ChargeTypes[])Enum.GetValues(typeof(ChargeTypes))
                    select new ListItem
                    {
                        Id = (int)type,
                        Display = type.ToString(),
                        Parent = company
                    }).ToArray();
        }
    }
}
