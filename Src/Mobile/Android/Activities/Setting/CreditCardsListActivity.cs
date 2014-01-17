using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
    [Activity(Label = "CreditCardsListActivity", Theme = "@android:style/Theme.NoTitleBar",
        WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreditCardsListActivity : BaseBindingActivity<CreditCardsListViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.CreditCardsListTitle; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var listView = FindViewById<ListView>(Resource.Id.CreditCardsListView);
            listView.Divider = null;
            listView.DividerHeight = 0;
            listView.SetPadding(10, 0, 10, 0);
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_CreditCardsList);
            ViewModel.OnViewLoaded();
        }
    }
}