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
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Binding.Android.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "Tutorial", Theme = "@style/android:Theme.Dialog", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxBindingActivityView<TutorialViewModel>
    {
        protected override void OnViewModelSet()
        {
            
            SetContentView(Resource.Layout.View_Tutorial);
            var rootLayout = this.FindViewById<LinearLayout>(Resource.Id.layout_main);
            var horizontalPager = this.FindViewById<HorizontalPager>(Resource.Id.details);

            horizontalPager.mOnScreenSwitchListener += (sender, args) =>
                                                           {
                                                               args.Screen.ToString();
                                                               rootLayout.GetChildAt(args.Screen);
                                                           };
            for (int i = 0; i < ViewModel.TutorialItemsList.Count; i++)
            {
                rootLayout.AddView(new ImageView(this, at),i);
            }

            // var intent = new Intent(this, typeof (ViewFlipperActivity));
            //  StartActivity(typeof(ViewFlipperActivity));

        }
    }
}