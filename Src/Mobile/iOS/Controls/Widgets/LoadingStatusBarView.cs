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
        private LoadingAnimation _loader;

        public LoadingStatusBarView (CGRect frame)
        {
            _loader = new LoadingAnimation(frame);

            Frame = frame;
            Add(_loader);
            ClipsToBounds = true;

            _loader.RunAnimation = true;
            _loader.Animate();
        }

        private class LoadingAnimation : UIImageView
        {
            CGRect _startingRect { get; set; }

            public bool RunAnimation { get; set; }

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
                    completion: () => 
                    {
                        UIView.Animate(4, 
                            animation: () => Frame = Frame.IncrementX(Frame.Width), 
                            completion : ()=>
                            {
                                if(!RunAnimation)
                                {
                                    return;
                                }

                                Animate();
                            });
                    });
            }
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();

            _loader.RunAnimation = false;

            _loader.RemoveFromSuperview();

            _loader = null;
        }
    }
}

