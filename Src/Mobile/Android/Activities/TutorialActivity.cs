using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Binding.Android.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/Theme.Modal", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxBindingActivityView<TutorialViewModel>
    {
        protected override void OnViewModelSet()
        {
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.View_Tutorial);
            var pipsLayout = this.FindViewById<LinearLayout>(Resource.Id.layout_pips);
            var horizontalPager = this.FindViewById<HorizontalPager>(Resource.Id.details);
            this.Title = null;
            horizontalPager.mOnScreenSwitchListener += (sender, args) =>
                                                           {
                                                               var newScreen = args.Screen;
                                                               CleanAllPips(pipsLayout);
                                                               ImageView currentPip = (ImageView)pipsLayout.GetChildAt(newScreen);

                                                               //set fill image
                                                               currentPip.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.tutorial_yellow_circle));
                                                               //unfill image
                                                               //previousPip.SetImageDrawable();
                                                           };
            for (int i = 0; i < ViewModel.TutorialItemsList.Count; i++)
            {
                ImageView imageFill = new ImageView(this);
                
                imageFill.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.tutorial_yellow_circle));
                imageFill.SetMaxWidth(20);
                ImageView imageNotFill = new ImageView(this);
                
                imageNotFill.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.tutorial_grey_circle));
                imageNotFill.SetMaxWidth(20);
                if (i == 0)
                {
                    //image fill
                    pipsLayout.AddView(imageFill, i);
                }
                else
                {
                    //image not fill
                    pipsLayout.AddView(imageNotFill, i);
                }
            }

            // var intent = new Intent(this, typeof (ViewFlipperActivity));
            //  StartActivity(typeof(ViewFlipperActivity));
        }

        public void CleanAllPips(LinearLayout pipsLayout)
        {
            for (int i = 0; i < ViewModel.TutorialItemsList.Count; i++)
            {
                ((ImageView)pipsLayout.GetChildAt(i)).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.tutorial_grey_circle));
            }
        }
    }
}