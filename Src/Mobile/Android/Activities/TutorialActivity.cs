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
using Android.OS;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "Tutorial", Theme = "@style/TutorialDialog", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TutorialActivity : MvxActivity
    {
        private BitmapDrawable _grayCircle;
        private BitmapDrawable _blackCircle;

		public new TutorialViewModel ViewModel
		{
			get
			{
				return (TutorialViewModel)DataContext;
			}
		}

        private int _tutorialInsetPixels = 7.ToPixels();

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            Window.SetFlags(WindowManagerFlags.Dither, WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.View_Tutorial);

            var pipsLayout = FindViewById<LinearLayout>(Resource.Id.layout_pips);
            var horizontalPager = FindViewById<HorizontalPager>(Resource.Id.details);
            var containerLayout = FindViewById<RelativeLayout>(Resource.Id.tutorial_container);
            _tutorialInsetPixels = Resources.GetDimension(Resource.Dimension.tutorial_insets).ToPixels();
            // ScaleType on TutorialImage could be changed to something with a minimal zoom for tablets so we would get 100% compatibility
            Title = null;

            _blackCircle = Resources.GetDrawable(Resource.Drawable.tutorial_black_circle) as BitmapDrawable;
            _grayCircle = Resources.GetDrawable(Resource.Drawable.tutorial_grey_circle) as BitmapDrawable;

            var maxWidth = Window.WindowManager.DefaultDisplay.Width;

            var lp = ((View)containerLayout.Parent).LayoutParameters;
            lp.Width = maxWidth;
            lp.Height = WindowManagerLayoutParams.MatchParent;
            ((View)containerLayout.Parent).LayoutParameters = lp;

            lp = ((View)containerLayout).LayoutParameters;

            lp.Width = maxWidth - (_tutorialInsetPixels * (PlatformHelper.IsAndroid23 ? 2 : 1));
            lp.Height = WindowManagerLayoutParams.MatchParent;
            ((View)containerLayout).LayoutParameters = lp;

            lp = ((View)pipsLayout).LayoutParameters;
            lp.Width = maxWidth / 2;
            lp.Height = 50.ToPixels();
            ((View)pipsLayout).LayoutParameters = lp;

            horizontalPager.MOnScreenSwitchListener += (sender, args) =>
            {
                var newScreen = args.Screen;
                CleanAllPips(pipsLayout);
                var currentPip = (ImageView) pipsLayout.GetChildAt(newScreen);
                currentPip.SetImageDrawable(_blackCircle);
            };

            for (int i = 0; i < ViewModel.TutorialItemsList.Count(); i++)
            {
                var d = _blackCircle;
                if (d != null)
                {
                    var layoutParams = new RelativeLayout.LayoutParams(20.ToPixels(),
                        12.ToPixels()) {LeftMargin = 10.ToPixels(), RightMargin = 10.ToPixels(), AlignWithParent = true};

                    var imageFill = new ImageView(this);
                    imageFill.SetImageDrawable(_blackCircle);
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