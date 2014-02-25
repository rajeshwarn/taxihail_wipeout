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
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Activities;
using System.Threading.Tasks;

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

        public static void StartAnimatingLoading(RelativeLayout masterLayout, Activity activity)
        {
            _activity = activity;

            var layoutParent = new LinearLayout(_activity);
            _layoutCenter = new LinearLayout(_activity);
            _layoutImage = new LinearLayout(_activity);

            var layoutParentParameters = new ViewGroup.LayoutParams(-1, -1);
            layoutParentParameters.Width = masterLayout.LayoutParameters.Width;
            layoutParentParameters.Height = masterLayout.LayoutParameters.Height;
            layoutParent.LayoutParameters = layoutParentParameters;

            masterLayout.SetBackgroundDrawable(_activity.Resources.GetDrawable(Resource.Drawable.loading_overlay));
            layoutParent.SetBackgroundColor(Android.Graphics.Color.Argb(80, 0, 0, 0));


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
            masterLayout.AddView(layoutParent, layoutParentParameters);

            _layoutCenter.ClearAnimation();
            _layoutImage.SetBackgroundDrawable(null);

            _car = BitmapFactory.DecodeResource(_activity.Resources, Resource.Drawable.taxi_progress);

            if (_progressImage != null)
            {
                _progressImage.Recycle();
                _progressImage = null;
            }


            _colorToUse = (Android.Graphics.Color)_activity.Resources.GetColor(Resource.Color.company_color);
            _car = DrawHelper.Colorize(_car, _colorToUse);

            var displaySize = _activity.Resources.DisplayMetrics;
            var windowHeight = (int)(_car.Width * 1.5f);
            _windowSize = new Size(displaySize.WidthPixels, windowHeight);

            var _radius = _car.Width * 1.3f;

            Progress = 0;

            _zoneCircle = new RectF((_windowSize.Width * 0.5f) - _radius / 2f, (_windowSize.Height * 0.5f) - _radius / 2f,  (_windowSize.Width * 0.5f) + _radius / 2f, (_windowSize.Height * 0.5f) + _radius / 2f);

            _isLoading = true;

            StartAnimationLoop();
        }



        public static void StopAnimatingLoading()
        {                
            _isLoading = false;       
        }

        private static async void StartAnimationLoop()
        {
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
                _layoutImage.SetBackgroundDrawable(GetCircleForProgress());
                await Task.Delay(500);
                RelativeLayout root = (RelativeLayout) _layoutCenter.Parent.Parent;
                if (root != null && _layoutCenter.Parent != null)
                {
                    root.RemoveView((LinearLayout)_layoutCenter.Parent);
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
