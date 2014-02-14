using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public static class LoadingBar
    {
        static LoadingBarView _loadingBar;
        static IDisposable _subscription;

        private static float Top { get; set; }
        public static bool IsShown { get; set; }

        private static void PositionLoadingBar ()
        {
            _loadingBar.Frame = _loadingBar.Frame.SetY(Top).SetWidth(UIScreen.MainScreen.Bounds.Width);
        }

        public static void Show (IObservable<bool> hideOnFirstTrue = null, float yPosition = 75)
        {
            Top = yPosition;

            UIApplication.SharedApplication.InvokeOnMainThread(()=>
            {
                if(_loadingBar != null)
                {
                    PositionLoadingBar ();
                }
                else 
                {
                    _loadingBar = new LoadingBarView(0, Top, (int)UIApplication.SharedApplication.KeyWindow.Frame.Width, 2);
                }

                _loadingBar.RemoveFromSuperview();
                UIApplication.SharedApplication.KeyWindow.AddSubview(_loadingBar);
            });

            if(hideOnFirstTrue == null) return;

            _subscription = hideOnFirstTrue.Subscribe(hide =>
            {
                if(hide)
                {
                    Hide();
                }
            });

            IsShown = true;
        }

        public static void Hide ()
        {
            if(_loadingBar == null) return;

            //this should put the whole line blue, but it doesn't appear because the view gets disposed too fast
            _loadingBar.BackgroundColor = UIColor.Blue;

            UIApplication.SharedApplication.InvokeOnMainThread(()=>
            {
                _loadingBar.RemoveFromSuperviewAnimated(()=>_loadingBar = null);
            });

            if (_subscription == null) return;

            _subscription.Dispose();
            IsShown = false;
        }

        public class LoadingBarView : UIView
        {
            LoadingAnimation loader;
            public LoadingBarView (float x = 0, float y = 75, int width = 320, int height = 6, UIColor baseColor = null)
            {
                loader = new LoadingAnimation(width, height, baseColor);

                Frame = new RectangleF (x, y, width, height);

                Add(loader);
                ClipsToBounds = true;

                loader.Animate();
            }

            public void RestartAnimation ()
            {
                loader.ResetFrame();
                loader.Animate();
            }

            public void RemoveFromSuperviewAnimated (Action callback)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(()=>
                {
                    RemoveFromSuperview();
                    callback();
                });
            }

            public class GradientMaker
            {
                public static CAGradientLayer Make (UIColor baseColor, RectangleF frame)
                {
                    float red;
                    float green;
                    float blue;
                    float alpha;

                    baseColor.GetRGBA(out red, out green, out blue, out alpha);

                    var startColor = UIColor.FromRGBA (red, green, blue, 255).CGColor;
                    var midColor = UIColor.FromRGBA (red, green, blue, 255).CGColor;
                    var endColor = UIColor.FromRGBA (red, green, blue, 255).CGColor;
                    var gradientLayer = new CAGradientLayer () {
                        StartPoint = new PointF (0, 0),
                        EndPoint = new Point (1, 0),
                        Colors = new CGColor[] {
                            startColor,
                            midColor,
                            endColor
                        },
                        Locations = new NSNumber[] {
                            0,
                            0.24f,
                            1
                        },
                        Frame = frame
                    };

                    return gradientLayer;
                }
            }

            private class LoadingAnimation : UIView
            {
                RectangleF StartingRect {
                    get;
                    set;
                }

                public void ResetFrame ()
                {
                    Frame = StartingRect;
                }

                public LoadingAnimation (int width, int height, UIColor baseColor = null)
                {
                    StartingRect = new RectangleF (0 - width, 0, width, height);

                    if (baseColor == null) {
                        baseColor = UIColor.Blue;
                    }

                    var gradientLayer = GradientMaker.Make(baseColor, StartingRect);                

                    Layer.AddSublayer(gradientLayer);
                    Layer.MasksToBounds = true;
                    Layer.Bounds = StartingRect;
                    ResetFrame ();
                }

                public void Animate ()
                {
                    ResetFrame();
                    UIView.Animate(2, () => { Frame = Frame.IncrementX(Frame.Width); });
                }
            }
        }
    }
}

