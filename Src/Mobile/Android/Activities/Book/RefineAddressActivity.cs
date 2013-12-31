using Android.App;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "RefineAddressActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class RefineAddressActivity : BaseBindingActivity<RefineAddressViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RefineAddress; }
        }
        
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_RefineAddress);
        }
    }
}