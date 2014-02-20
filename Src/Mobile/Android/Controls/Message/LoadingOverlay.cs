using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.App;
using System.Threading;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Graphics;
using System.Drawing;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class LoadingOverlay
    {
        private static Dialog _progressBar;
        private static bool _isLoading;
        private static Activity _activity;
        private static float Progress;
        private static Size _windowSize;
        private static RectF _zone;
        private static RectF _zoneCircle;
        private static Bitmap _car;
        private static Bitmap _progressImage;
        private static LinearLayout _layoutCenter;
        private static LinearLayout _layoutImage;
        private static Android.Graphics.Color _colorBgTheme;

        public static void StartAnimatingLoading(Activity activity)
        {
            _activity = activity;
            _progressBar = new Dialog(_activity);
            _progressBar.RequestWindowFeature((int)WindowFeatures.NoTitle);
            _progressBar.SetCancelable(false);
            _progressBar.SetContentView(Resource.Layout.Control_LoadingOverlay);
            _progressBar.Window.SetBackgroundDrawable(_activity.Resources.GetDrawable(Resource.Drawable.loading_overlay));
            _progressBar.Window.DecorView.Invalidate();

            WindowManagerLayoutParams mainLayoutParameters = new WindowManagerLayoutParams();
            mainLayoutParameters.CopyFrom(_progressBar.Window.Attributes);
            mainLayoutParameters.Width = WindowManagerLayoutParams.MatchParent;
            mainLayoutParameters.Height = WindowManagerLayoutParams.MatchParent;

            _progressBar.Show();
            _progressBar.Window.Attributes = mainLayoutParameters;

            var layoutParent = new LinearLayout(_activity);
            _layoutCenter = new LinearLayout(_activity);
            _layoutImage = new LinearLayout(_activity);

            var layoutParentParameters = new ViewGroup.LayoutParams(-1, -1);
            layoutParentParameters.Width = mainLayoutParameters.Width;
            layoutParentParameters.Height = mainLayoutParameters.Height;
            layoutParent.LayoutParameters = layoutParentParameters;

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
            _progressBar.AddContentView(layoutParent, layoutParentParameters);

            _layoutCenter.ClearAnimation();
            _layoutImage.SetBackgroundDrawable(null);

            var displaySize = _activity.Resources.DisplayMetrics;
            _windowSize = new Size(displaySize.WidthPixels, 120);

            if (_car != null)
            {
                _car.Recycle();
            }                

            if (_progressImage != null)
            {
                _progressImage.Recycle();
            }
                
            _car = BitmapFactory.DecodeResource(_activity.Resources, Resource.Drawable.taxi_progress);

            var _radius = _car.Width * 1.3f;

            Progress = 0;

            _zoneCircle = new RectF((_windowSize.Width * 0.5f) - _radius / 2f, (_windowSize.Height * 0.5f) - _radius / 2f,  (_windowSize.Width * 0.5f) + _radius / 2f, (_windowSize.Height * 0.5f) + _radius / 2f);

            _isLoading = true;

            var useColor = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.UseThemeColorForMapIcons;

            if (useColor != null && useColor)
            {
                _car = DrawHelper.Colorize(_car, (Android.Graphics.Color)_activity.Resources.GetColor(Resource.Color.login_background_color));
            }
            else
            {
                _colorBgTheme = Android.Graphics.Color.ParseColor("#0378ff");
            }

            ThreadPool.QueueUserWorkItem(d =>
            {
                Animate();
            });
        }

        public static void StopAnimatingLoading()
        {
            if (_progressBar != null && _progressBar.IsShowing)
            {
                _isLoading = false;
            }
        }

        private static void Animate()
        {
            if (_progressBar != null)
            {
                if (_isLoading)
                {
                    IncreaseProgress();
                    Animate();
                }
                else
                {
                    Progress = 100;

                    Thread.Sleep(500);

                    if (_activity != _progressBar.OwnerActivity)
                    {

                    }

                    _activity.RunOnUiThread(() =>
                    {
                        _progressBar.Dismiss();
                    });
                }
            }
        }

        private static void IncrementProgress(float increment)
        {
            var nextProgress = Progress + increment;

            if (nextProgress < 20)
            {
                Thread.Sleep(20);
                Progress = nextProgress;
            }
            else if (nextProgress < 80)
            {
                if(nextProgress >= 99)
                {
                    return;
                }

                Thread.Sleep(50);
                Progress = nextProgress;
            } 

            _activity.RunOnUiThread(() =>
            {
                _layoutImage.SetBackgroundDrawable(getCircleForProgress());                                    
            });                
        }

        private static void IncreaseProgress()
        {
            var currentProgress = Progress;

            float speed = 1f;

            if (currentProgress <= 20)
            {
                IncrementProgress(speed);
            }
            else if (currentProgress > 20 && currentProgress <= 80)
            {
                IncrementProgress(speed);
            }
            else if (currentProgress > 80)
            {
                IncrementProgress((100 - currentProgress) / 2);
            }                
        }

        private static BitmapDrawable getCircleForProgress()
        {
            Bitmap.Config conf = Bitmap.Config.Argb4444;

            DateTime dt = DateTime.Now;
            _progressImage = Bitmap.CreateBitmap(_windowSize.Width, _windowSize.Height, conf);

            Canvas canvas = new Canvas(_progressImage);
            Paint paint = new Paint();
            Paint paintRec = new Paint();
            paintRec.SetStyle(Paint.Style.Fill);
            paintRec.Color = Android.Graphics.Color.White;
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeWidth = 3;
            paint.AntiAlias = true;
            paint.Color = _colorBgTheme;
            canvas.DrawPaint(new Paint() { Color = Android.Graphics.Color.Argb(100, 0, 0, 0) });
            var _radius = _car.Width * 1.3f;

            if (Progress > 20)
            {
                _activity.RunOnUiThread(() =>
                {
                    var ll = _layoutCenter.LayoutParameters;
                    if (ll.Height != 120)
                    {
                        ll.Height = 120;
                        _layoutCenter.LayoutParameters = ll;
                        _layoutCenter.RequestFocus();
                    }
                });

                _zone = new RectF(0, 0, _windowSize.Width, _windowSize.Height);            
                canvas.DrawRect(_zone, paintRec);
                canvas.DrawBitmap(_car, _zoneCircle.CenterX() - _car.Width / 2f, _zoneCircle.CenterY() - _car.Height / 2f, null);
                canvas.DrawArc(_zoneCircle, 0f, ((Progress - 20f) / 80f) * 360f, false, paint);
            }
            else
            {
                _activity.RunOnUiThread(() =>
                {
                    var ll = _layoutCenter.LayoutParameters;
                    ll.Height = (int)(((float)Progress / 20f) * 120f);
                    _layoutCenter.LayoutParameters = ll;
                    _layoutCenter.RequestLayout();
                });
            }
            DateTime de = DateTime.Now;
            TimeSpan ts = de - dt;
            _layoutCenter.RequestLayout();
            var ms = ts.TotalMilliseconds;
            return new BitmapDrawable(_progressImage);
        }
    }
}

