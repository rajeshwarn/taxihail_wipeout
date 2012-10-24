using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Common.Entity;

namespace MK.Common.Android.Provider
{
    public interface IPopularAddressProvider
    {
         IEnumerable<Address> GetPopularAddresses();
    }
}