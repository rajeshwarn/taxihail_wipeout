using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public enum LoadingOverlayPosition
    {
        Center = 0,
        BottomRight = 1
    }

    public enum CAlertViewType
    {
        ProgressBar,
        ActivityIndicator
    }

    [Register("CAlertView")]
    public class CAlertView : UIAlertView
    {
        public CAlertViewType AlertViewType { get; set; }
        public UIProgressView ProgressView { get; set; }
        public UIActivityIndicatorView ActivityIndicator { get; set; }

        public CAlertView(IntPtr handle) : base(handle)
        {
        }

        [Export("CAlertView:")]
        public CAlertView(NSCoder coder) : base(coder)
        {
        }

        public CAlertView()
        {
            _offset = null;
        }

        private float? _offset;
        private bool _setUpComplete;

        public CAlertView(float offset)
        {
            _offset = offset;
        }

        public override void Draw(RectangleF rect)
        {
            if (_setUpComplete == false)
            {
                switch (AlertViewType)
                {
                    case CAlertViewType.ProgressBar:
                        if (ProgressView == null)
                        {
                            ProgressView = new UIProgressView();
                            ProgressView.Style = UIProgressViewStyle.Default;
                            ProgressView.Progress = 0f;
                        }

                        ProgressView.Frame = new RectangleF(
                        30.0f,
                        rect.Height - 50f,
                        225.0f,
                        11f
                        );
                        AddSubview(ProgressView);
                        break;
                    case CAlertViewType.ActivityIndicator:
                    
                        if (ActivityIndicator == null)
                        {
                            ActivityIndicator = new UIActivityIndicatorView();
                            if (_offset.HasValue)
                            {
                                ActivityIndicator.Frame = new RectangleF(
                                139.0f - 18.0f,
                                rect.Height - 63f - _offset.Value,
                                37.0f,
                                37.0f
                                );
                            }
                            else
                            {
                                ActivityIndicator.Frame = new RectangleF(
                                139.0f - 18.0f,
                                rect.Height - 63f,
                                37.0f,
                                37.0f
                                );
                            }
                        }
                        if (ActivityIndicator != null)
                        {
                            AddSubview(ActivityIndicator);
                            ActivityIndicator.StartAnimating();
                        }
                        break;
                }
                _setUpComplete = true;
            }
            base.Draw(rect);
        }

        public void Hide(bool animated)
        {
            DismissWithClickedButtonIndex(0, animated);
        }
    }

    public class LoadingOverlay : UIView
    {
        //private static LoadingOverlay _loading;
        private static CAlertView _loading;
        private static readonly object Lock = new object();

        public static void StartAnimatingLoading(LoadingOverlayPosition position, string text, int? width, int? height, Action canceled)
        {
            lock (Lock)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    if (_loading == null)
                    {
                        if (canceled != null)
                        {
                            _loading = new CAlertView(45);
                            _loading.AddButton("Cancel");
                            _loading.CancelButtonIndex = 0;
                            _loading.Canceled += delegate
                            {
                                canceled();
                            };
                            _loading.Clicked += delegate
                            {
                                canceled();
                            };
                        
                        }
                        else
                        {
                            _loading = new CAlertView();
                        }

                        _loading.Message = text.IsNullOrEmpty() ? Localize.GetValue("LoadingMessage") : text;
                    
                        if (canceled != null)
                        {
                           _loading.Message += Environment.NewLine + " " + Environment.NewLine + " " + Environment.NewLine;
                        }
                        _loading.AlertViewType = CAlertViewType.ActivityIndicator;
                        _loading.Show();
                    }
                });
            }
        }

        public static void StopAnimatingLoading()
        {
            lock (Lock)
            {
                if (_loading != null)
                {
                    try
                    {
                        UIApplication.SharedApplication.InvokeOnMainThread(() => _loading.Hide(false));
                        UIApplication.SharedApplication.InvokeOnMainThread(() => _loading.RemoveFromSuperview());
                    }
// ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    
                    }
                    _loading = null;
                }
            }
        }

        public LoadingOverlay(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public LoadingOverlay(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public LoadingOverlay()
        {
            Initialize();
        }

        private void Initialize()
        {
            Width = 130;
            Height = 30;
            Hidden = true;
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}


