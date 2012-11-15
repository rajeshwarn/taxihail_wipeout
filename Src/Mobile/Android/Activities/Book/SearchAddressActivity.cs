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
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
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
    public class SearchAddressActivity : MvxBindingActivityView<AddressSearchViewModel>
	{
	    private IObservable<string> subscription;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

		}

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_SearchAddress);

			var _listView = FindViewById<MvxBindableListView>(Resource.Id.SearchAddressListView);
			_listView.Divider = null;
			_listView.DividerHeight = 0;
			
			var _historicListView = FindViewById<MvxBindableListView>(Resource.Id.HistoricListView);
			_historicListView.Divider = null;
			_historicListView.DividerHeight = 0;

            var searchAddressText = FindViewById<AutoCompleteBindableTextView>(Resource.Id.SearchAddressText);

            var button = FindViewById<Button>(Resource.Id.eraseTextButton);
            button.Click += (sender, args) => searchAddressText.Text = "";
            var drawable = new BitmapDrawable();

             subscription = Observable.FromEventPattern<TextChangedEventArgs>(searchAddressText
                                                                                 , "TextChanged").Select(
                                                                                     c => ((EditText) c.Sender).Text);
                
            subscription.Subscribe(c =>
                                             {
                                                 RunOnUiThread(() =>
                                                                   {
                                                                       if (c.Length > 0)
                                                                       {
                                                                           button.Visibility = ViewStates.Visible;
                                                                       }
                                                                       else
                                                                       {
                                                                           button.Visibility = ViewStates.Gone;
                                                                       }
                                                                   });
                                             });

            if(searchAddressText.Length()> 0)
            {
                button.Visibility = ViewStates.Visible;
                //searchAddressText.SelectAll();
                //searchAddressText.ExtendSelection(0);
            }


			
			FindViewById<TextView>( Resource.Id.HistoricListViewTitle ).Text = Resources.GetString( Resource.String.LocationHistoryTitle );
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
