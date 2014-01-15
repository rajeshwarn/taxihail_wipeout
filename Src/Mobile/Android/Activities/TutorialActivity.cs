using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/Theme.Modal", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxActivity
    {
        private BitmapDrawable _grayCircle;
        private BitmapDrawable _yellowCircle;

		public new TutorialViewModel ViewModel
		{
			get
			{
				return (TutorialViewModel)DataContext;
			}
		}

        protected override void OnViewModelSet()
        {
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.View_Tutorial);
            var pipsLayout = FindViewById<LinearLayout>(Resource.Id.layout_pips);
            var horizontalPager = FindViewById<HorizontalPager>(Resource.Id.details);
            Title = null;

            _yellowCircle = Resources.GetDrawable(Resource.Drawable.tutorial_yellow_circle) as BitmapDrawable;
            _grayCircle = Resources.GetDrawable(Resource.Drawable.tutorial_grey_circle) as BitmapDrawable;

            horizontalPager.MOnScreenSwitchListener += (sender, args) =>
            {
                var newScreen = args.Screen;
                CleanAllPips(pipsLayout);
                var currentPip = (ImageView) pipsLayout.GetChildAt(newScreen);
                currentPip.SetImageDrawable(_yellowCircle);
            };
            for (int i = 0; i < ViewModel.TutorialItemsList.Count(); i++)
            {
                var d = _yellowCircle;
                if (d != null)
                {
                    var w = d.Bitmap.Width;
                    var h = d.Bitmap.Height;
// ReSharper disable once PossibleLossOfFraction
                    var layoutParams = new RelativeLayout.LayoutParams(DrawHelper.GetPixels(12*w/h),
                        DrawHelper.GetPixels(12)) {LeftMargin = 10, RightMargin = 10, AlignWithParent = true};


                    var imageFill = new ImageView(this);
                    imageFill.SetImageDrawable(_yellowCircle);
                    imageFill.LayoutParameters = layoutParams;


                    var imageNotFill = new ImageView(this);
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
        }

        private void CleanAllPips(LinearLayout pipsLayout)
        {
            for (int i = 0; i < ViewModel.TutorialItemsList.Count(); i++)
            {
                ((ImageView) pipsLayout.GetChildAt(i)).SetImageDrawable(_grayCircle);
            }
        }
    }
}