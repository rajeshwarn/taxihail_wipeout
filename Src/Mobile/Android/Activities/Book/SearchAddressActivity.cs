using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.History;
using apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Reactive;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "SearchActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SearchAddressActivity : BaseBindingActivity<AddressSearchViewModel>
	{
      
      
        protected override int ViewTitleResourceId
        {
            get
            {
                return Resource.String.View_SearchAddress;
            }
        }

	   
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

		}

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_SearchAddress);

			
            var searchAddressText = FindViewById<EditText>(Resource.Id.searchAddressText);


            var button = FindViewById<ImageButton>(Resource.Id.clearable_button_clear);
            button.Touch  += (sender, args) =>
                                {
                                    searchAddressText.Text = "";
                                    ViewModel.ClearResults();
                                    

                                };

            searchAddressText.TextChanged += delegate
            {
                if (searchAddressText.Text.Length > 0)
                {
                    button.Visibility = ViewStates.Visible;
                }
                else
                {
                    button.Visibility = ViewStates.Gone;
                }
            };

            if(searchAddressText.Length()> 0)
            {
                button.Visibility = ViewStates.Visible;
            }
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
