using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class Dot : UIView
    { 
        private float _size;
        private UIColor _color;
        public Dot( float size, UIColor color )
        {
            _size = size;
            _color = color;
            BackgroundColor = UIColor.Clear;
        }

        public override void Draw(System.Drawing.RectangleF rect)
        {
            base.Draw(rect);

            var x = (this.Bounds.Width - _size) / 2;
            var y = (this.Bounds.Height - _size) / 2;

            //// Oval Drawing
            var ovalPath = UIBezierPath.FromOval(new RectangleF(x,y,_size,_size) );
            _color.SetFill();
            ovalPath.Fill();
            _color.SetStroke();
            ovalPath.LineWidth = 1;
            ovalPath.Stroke();
        }
    }
}

