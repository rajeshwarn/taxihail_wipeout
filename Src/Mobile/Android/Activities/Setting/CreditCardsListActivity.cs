
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
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Label = "CreditCardsListActivity", Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class CreditCardsListActivity : BaseBindingActivity<CreditCardsListViewModel>
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            var _listView = FindViewById<ListView>(Resource.Id.CreditCardsListView);
            _listView.Divider = null;
            _listView.DividerHeight = 0;
            _listView.SetPadding(10, 0, 10, 0);

			// Create your application here
		}

		protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_CreditCardsList);
			ViewModel.Load();
            
		}

		protected override int ViewTitleResourceId
		{
			get { return Resource.String.CreditCardsListTitle; }
		}
	}
}

