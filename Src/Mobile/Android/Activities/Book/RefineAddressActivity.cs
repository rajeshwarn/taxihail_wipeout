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
using Cirrious.MvvmCross.Binding.Android.Views;
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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_RefineAddress);
        }
    }
}