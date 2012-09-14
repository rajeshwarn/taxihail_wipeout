using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.History;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "SearchActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class SearchAddressActivity : MvxBindingActivityView<AddressSearchViewModel>
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.SearchAddress);

            var _listView = FindViewById<MvxBindableListView>(Resource.Id.SearchAddressListView);
            _listView.Divider = null;
            _listView.DividerHeight = 0;

        }

    }
}