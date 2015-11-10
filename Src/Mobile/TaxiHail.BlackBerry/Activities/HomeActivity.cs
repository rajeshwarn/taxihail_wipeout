using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.PresentationHints;
using Android.OS;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public partial class HomeActivity : BaseBindingFragmentActivity<HomeViewModel>, IChangePresentation
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainBundle = bundle;
        }
    }
}

