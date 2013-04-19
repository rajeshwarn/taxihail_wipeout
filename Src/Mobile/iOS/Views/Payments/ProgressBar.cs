using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public class ProgressBar : UIView
    {
        UIImageView _barStart;
        UIImageView _barEnd;
        UIImageView _barBody;

        float _barEndWidth;
        float _barHeight;
        float _topLeft;

        public ProgressBar (UIImage barEnd,UIImage barBody, RectangleF frame, bool isMinor = false): base(frame)
        {
            _barEndWidth = barEnd.Size.Width;

            _topLeft= (frame.Height-barEnd.Size.Height)/2;
            _barHeight = barEnd.Size.Height;

            _barStart = new UIImageView(barEnd);            
            _barEnd = new UIImageView(barEnd);
            _barBody = new UIImageView(barBody);

            Resize(Frame.Width-_barEndWidth);

            if(isMinor)
            {
                Add(_barStart);
            }
            else{
                Add(_barEnd);
            }
            Add (_barBody);
        }

        public void Resize(float endPosition){
            
            _barStart.Frame = _barStart.Frame.SetX(0).SetY(_topLeft);
            _barEnd.Frame = _barEnd.Frame.SetX(endPosition).SetY(_topLeft);
            _barBody.Frame = new RectangleF(_barStart.Frame.Right, _barStart.Frame.Top, _barEnd.Frame.Left - _barStart.Frame.Right, _barHeight);
        }
    }
}

