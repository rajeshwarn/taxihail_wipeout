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
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IGeolocService
    {
        Address ValidateAddress(string address);

        Address[] SearchAddress(double latitude, double longitude);

        DirectionInfo GetDirectionInfo(double originLong, double originLat, double destLong, double destLat);

        IEnumerable<Address> FindSimilar(string address);
    }
}