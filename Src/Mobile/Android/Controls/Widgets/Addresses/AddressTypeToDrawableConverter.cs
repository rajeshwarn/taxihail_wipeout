using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    internal class AddressTypeToDrawableConverter
    {
        public static int GetDrawable(AddressType addressType)
        {
            switch (addressType)
            {
                case AddressType.Favorites:
                    return Resource.Drawable.favorites;
                case AddressType.Places:
					return Resource.Drawable.places;
                case AddressType.History:
                    return Resource.Drawable.history;
                default:
					return Resource.Drawable.places;
            }
        }
    }
}