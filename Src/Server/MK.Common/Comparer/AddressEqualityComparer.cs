using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MK.Common.Comparer
{
    public class AddressEqualityComparer : IEqualityComparer<Address>
    {
        public bool Equals(Address x, Address y)
        {
            return string.Equals(x.FullAddress, y.FullAddress, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(Address obj)
        {
            return obj.FullAddress.ToUpper(CultureInfo.CurrentCulture).GetHashCode();
        }
    }
}