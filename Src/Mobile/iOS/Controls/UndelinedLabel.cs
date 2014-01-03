using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("UnderlinedLabel")]
    public class UnderlinedLabel : UILabel
    {
        public event EventHandler TouchUpInside;

        public UnderlinedLabel(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public UnderlinedLabel(RectangleF rect) : base( rect )
        {
            Initialize();
        }
        
        private void Initialize()
        {

        }

        void HandleTouchUpInside (object sender, EventArgs e)
        {
            if ( TouchUpInside != null )
            {
                TouchUpInside( this, EventArgs.Empty );
            }
        }

        private UIButton _btn;

        public override void Draw (RectangleF rect)
        {
          

            if ( _btn == null )
            {
                UserInteractionEnabled = true;
                _btn = UIButton.FromType (UIButtonType.Custom );
                _btn.TouchUpInside += HandleTouchUpInside;
                AddSubview ( _btn );
            }

            _btn.Frame = new RectangleF( Bounds.X -10, Bounds.Y - 10, Bounds.Width + 20, Bounds.Height + 20 );

            var context = UIGraphics.GetCurrentContext();

            float r;
            float g;
            float b;
            float a;
            TextColor.GetRGBA ( out r, out g, out b, out a );

            context.SetStrokeColor ( r,g,b,a );

            context.SetLineWidth( 0.5f );
                                              
                
            var labelSize = new NSString( Text ).StringSize ( Font, Frame.Size, UILineBreakMode.Clip );

            context.MoveTo ( 0, Bounds.Size.Height - 1 );
            context.AddLineToPoint( labelSize.Width, Bounds.Size.Height -1 );

            context.StrokePath ();

            base.Draw (rect);
        }

       
    }
}

