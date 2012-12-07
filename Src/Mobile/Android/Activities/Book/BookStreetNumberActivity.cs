
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
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "BookStreetNumberActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookStreetNumberActivity : BaseBindingActivity<BookStreetNumberViewModel> 
	{
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.StreetNumberTitle; }
		}

		protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_BookStreetNumber);
			var buttonSearch = FindViewById<SearchButton>(Resource.Id.StreetNumberBtSearch);
			buttonSearch.Text = Resources.GetString(Resource.String.StreetNumberSearchBt);

		    var streetNumberText = FindViewById<EditText>(Resource.Id.streetNumberText);
		    streetNumberText.RequestFocus();
            streetNumberText.SelectAll();
		}
	}
}

