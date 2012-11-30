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

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "Tutorial", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxBindingActivityView<TutorialViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_tutorial);
            var dialog = new AlertDialog.Builder(this);
           // var intent = new Intent(this, typeof (ViewFlipperActivity));
          //  StartActivity(typeof(ViewFlipperActivity));

        }
    }
}