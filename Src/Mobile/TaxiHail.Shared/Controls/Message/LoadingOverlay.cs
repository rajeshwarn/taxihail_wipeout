﻿using System.Drawing;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
#if CALLBOX
using apcurium.MK.Callbox.Mobile.Client;
#endif
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading;


namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class LoadingOverlay
    {
        private static bool _isLoading;
        private static Activity _activity;
        private static float Progress;
        private static Size _windowSize;
        private static RectF _zoneCircle;
        private static Bitmap _car;
        private static Bitmap _progressImage;
        private static LinearLayout _layoutCenter;
        private static LinearLayout _layoutImage;
        private static Android.Graphics.Color _colorToUse = Android.Graphics.Color.ParseColor("#0378ff");

        public static void StartAnimatingLoading()
        {
            _activity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            var rootView = _activity.Window.DecorView.RootView as ViewGroup;

            if (rootView == null)
            {
                return;
            }                

            LoadingOverlay.WaitMore();

            if (WaitStack > 1)
            {
                return;
            }

            if (_activity.Intent.Categories == null || !_activity.Intent.Categories.Contains("Progress"))
            {
                _activity.Intent.AddCategory("Progress");
            }

			Initialize((FrameLayout)rootView);
        }

		private static void Initialize(FrameLayout rootView)
		{
			var layoutParent = new LinearLayout(_activity.ApplicationContext);
			_layoutCenter = new LinearLayout(_activity.ApplicationContext);
			_layoutImage = new LinearLayout(_activity.ApplicationContext);

            var layoutParentParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            layoutParent.LayoutParameters = layoutParentParameters;
			layoutParent.SetBackgroundDrawable(_activity.Resources.GetDrawable(Resource.Drawable.loading_overlay));

			var layoutCenterParameters = new LinearLayout.LayoutParams(-2, 0);
            layoutCenterParameters.Weight = 1.0f;
            layoutCenterParameters.SetMargins(0, 0, 0, 0);
            layoutCenterParameters.Gravity = GravityFlags.CenterVertical;

            var layoutImageParameters = new LinearLayout.LayoutParams(-1, -1);
            layoutCenterParameters.Weight = 1.0f;
            layoutCenterParameters.SetMargins(0, 0, 0, 0);

            _layoutCenter.LayoutParameters = layoutCenterParameters;
            _layoutCenter.SetBackgroundColor(Android.Graphics.Color.White);

            _layoutImage.LayoutParameters = layoutImageParameters;

            _layoutCenter.AddView(_layoutImage);
            layoutParent.AddView(_layoutCenter);
            
			rootView.AddView (layoutParent, layoutParentParameters);
			layoutParent.BringToFront ();

            _layoutCenter.ClearAnimation();
            _layoutImage.SetBackgroundDrawable(null);

			if (_car == null) 
            {
                _colorToUse = _activity.Resources.GetColor (Resource.Color.company_color);
                _car = DrawHelper.ApplyThemeColorToImage (Resource.Drawable.taxi_progress, true, new SizeF(52, 20), Android.Graphics.Color.Argb (255, 0, 122, 255), new System.Drawing.Point (25, 10));
			}

            var displaySize = _activity.Resources.DisplayMetrics;
            var windowHeight = (int)(_car.Width * 1.5f);
            _windowSize = new Size(displaySize.WidthPixels, windowHeight);

            var _radius = _car.Width * 1.3f;

            _zoneCircle = new RectF((_windowSize.Width * 0.5f) - _radius / 2f, (_windowSize.Height * 0.5f) - _radius / 2f,  (_windowSize.Width * 0.5f) + _radius / 2f, (_windowSize.Height * 0.5f) + _radius / 2f);

            StartAnimationLoop (layoutParent);
        }

        public static void StopAnimatingLoading()
        {                
            Interlocked.Decrement(ref WaitStack);

            if (WaitStack < 1)
            {
                _isLoading = false;    
                if (_activity.Intent.Categories != null && _activity.Intent.Categories.Contains("Progress"))
                {
                    _activity.Intent.Categories.Remove("Progress");
                }
            }
        }

        public static void WaitMore()
        {
            Interlocked.Increment(ref WaitStack);
            _isLoading = true;

            if (Progress > 20)
            {
                Progress /= 2f;
            }
        }

        public static int WaitStack = 0;

        private static async void StartAnimationLoop(LinearLayout overlay)
        {
			Progress = 0;
			_isLoading = true;

			await Task.Run(async () =>
            {
                while (_isLoading)
                {
                    await IncreaseProgressDependingOnCurrentProgress();
                }
            });

            Progress = 100;

            _activity.RunOnUiThread(async () =>
            {
                try
                {
                    _layoutImage.SetBackgroundDrawable(GetCircleForProgress());
                }
                catch
                {

                }

                await Task.Delay(500);

                if (overlay != null)
                {
                    var root = overlay.Parent as ViewGroup;
                    if (root != null)
                    {
                        root.RemoveView(overlay);
                    }
                }
            });
        }

        private static async Task IncrementProgress(float increment)
        {
            var nextProgress = Progress + increment;

            if (nextProgress < 20)
            {
                await Task.Delay(20);
            }
            else
            {
                await Task.Delay(60);
            }

            Progress = nextProgress;

            _activity.RunOnUiThread(() =>
            {
                _layoutImage.SetBackgroundDrawable(GetCircleForProgress());                                    
            });                
        }

        private static async Task IncreaseProgressDependingOnCurrentProgress()
        {
            const float speed = 1f;
            var currentProgress = Progress;
            var increment = 0f;

            if (currentProgress <= 50)
            {
                increment = speed;
            }
            else if (currentProgress > 95)
            {
                increment = 0;
            }         
            else
            {
                increment = (100 - currentProgress) / 80;
            } 

            await IncrementProgress(increment);
                           
        }

        private static BitmapDrawable GetCircleForProgress()
        {
            var conf = Bitmap.Config.Argb4444;

            if (_progressImage == null)
            {
                _progressImage = Bitmap.CreateBitmap(_windowSize.Width, _windowSize.Height, conf);
            }

            var canvas = new Canvas(_progressImage);
            var paint = new Paint();
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeWidth = 3;
            paint.AntiAlias = true;
            paint.Color = _colorToUse;
            canvas.DrawPaint(new Paint() { Color = Android.Graphics.Color.White });

            if (Progress > 20)
            {
                _activity.RunOnUiThread(() =>
                {
                    var ll = _layoutCenter.LayoutParameters;
                    if (ll.Height != _windowSize.Height)
                    {
                        ll.Height = _windowSize.Height;
                        _layoutCenter.LayoutParameters = ll;
                        _layoutCenter.RequestFocus();
                    }
                });
                    
                canvas.DrawBitmap(_car, _zoneCircle.CenterX() - _car.Width / 2f, _zoneCircle.CenterY() - _car.Height / 2f, null);

                var startAngle = -90f;
                var sweepingAngle = ((Progress - 20f) / 80f) * 360f;
                canvas.DrawArc(_zoneCircle, startAngle, sweepingAngle, false, paint);
            }
            else
            {
                _activity.RunOnUiThread(() =>
                {
                    var ll = _layoutCenter.LayoutParameters;
                    ll.Height = (int)(((float)Progress / 20f) * _windowSize.Height);
                    _layoutCenter.LayoutParameters = ll;
                    _layoutCenter.RequestLayout();
                });
            }

            _layoutCenter.RequestLayout();
            return new BitmapDrawable(_progressImage);
        }
    }
}
