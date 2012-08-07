using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Models
{
    public class AddressItemListModel
    {
        public Address Address { get; set; }
        public int BackgroundImageResource { get; set; }
        public int NavigationIconResource { get; set; }
    }
}