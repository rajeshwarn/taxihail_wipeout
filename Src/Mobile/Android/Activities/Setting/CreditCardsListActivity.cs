using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "CreditCardsListActivity", Theme = "@style/MainTheme",
        WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreditCardsListActivity : BaseBindingActivity<CreditCardsListViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var listView = FindViewById<MvxListView>(Resource.Id.CreditCardsListView);
            listView.CacheColorHint = Color.White;
            listView.Divider = null;
            listView.DividerHeight = 0;
            listView.SetPadding(0, 0, 0, 0);
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Payments_CreditCardsList);
        }
    }
}