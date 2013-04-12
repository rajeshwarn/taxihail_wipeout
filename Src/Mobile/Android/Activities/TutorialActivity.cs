using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Binding.Android.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/Theme.Modal", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxBindingActivityView<TutorialViewModel>
    {
        private BitmapDrawable _yellowCircle;
        private BitmapDrawable _grayCircle;

        protected override void OnViewModelSet()
        {
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.View_Tutorial);
            var pipsLayout = this.FindViewById<LinearLayout>(Resource.Id.layout_pips);
            var horizontalPager = this.FindViewById<HorizontalPager>(Resource.Id.details);
            this.Title = null;

            _yellowCircle =  Resources.GetDrawable(Resource.Drawable.tutorial_yellow_circle) as BitmapDrawable;
            _grayCircle =  Resources.GetDrawable(Resource.Drawable.tutorial_grey_circle) as BitmapDrawable;

            horizontalPager.mOnScreenSwitchListener += (sender, args) =>
                                                           {
                                                               var newScreen = args.Screen;
                                                               CleanAllPips(pipsLayout);
                                                               ImageView currentPip = (ImageView)pipsLayout.GetChildAt(newScreen);
                                                                currentPip.SetImageDrawable(_yellowCircle);
                                                           };

            for (int i = 0; i < ViewModel.TutorialItemsList.Count(); i++)
            {

                var d = _yellowCircle;
                var w = d.Bitmap.Width;
                var h = d.Bitmap.Height;
                var layoutParams = new RelativeLayout.LayoutParams(DrawHelper.GetPixels((12 * w) / h), DrawHelper.GetPixels(12));

               
                layoutParams.LeftMargin = 10;
                layoutParams.RightMargin = 10;
                layoutParams.AlignWithParent = true;


                ImageView imageFill = new ImageView(this);
                imageFill.SetImageDrawable(_yellowCircle);
                imageFill.LayoutParameters = layoutParams;
               
                
                ImageView imageNotFill = new ImageView(this);
                imageNotFill.SetImageDrawable(_grayCircle);
                imageNotFill.LayoutParameters = layoutParams;

                if (i == 0)
                {
                    pipsLayout.AddView(imageFill, i);
                }
                else
                {
                    pipsLayout.AddView(imageNotFill, i);
                }
            }
        }

        public void CleanAllPips(LinearLayout pipsLayout)
        {
            for (int i = 0; i < ViewModel.TutorialItemsList.Count(); i++)
            {                              
                ((ImageView)pipsLayout.GetChildAt(i)).SetImageDrawable(_grayCircle);
            }
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
        }



    }
}