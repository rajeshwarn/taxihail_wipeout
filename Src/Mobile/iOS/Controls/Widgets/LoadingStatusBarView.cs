using System;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using CoreAnimation;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class LoadingStatusBarView : UIView
    {

        public LoadingStatusBarView (CGRect frame)
        {
            var loader = new LoadingAnimation(frame);

            Frame = frame;
            Add(loader);
            ClipsToBounds = true;

            loader.Animate();
        }

        private class LoadingAnimation : UIImageView
        {
            CGRect _startingRect { get; set; }

            public void ResetFrame ()
            {
                Frame = _startingRect;
            }

            public LoadingAnimation (CGRect frame)
            {
                var gradientsWidth = 80;
                var frameWidth = frame.Width + gradientsWidth;

                _startingRect = new CGRect (0 - frameWidth, 0, frameWidth, frame.Height);

                Image = UIImage.FromBundle("status_gradient");
                ResetFrame ();
            }

            public void Animate ()
            {
                ResetFrame();
                UIView.Animate(4, 
                    animation: () => { Frame = Frame.IncrementX(Frame.Width); }, 
                    completion: () => UIView.Animate(4, () => 
                        { 
                            Frame = Frame.IncrementX(Frame.Width);
                        }, 
                        completion : ()=> Animate()));
            }
        }
    }
}

