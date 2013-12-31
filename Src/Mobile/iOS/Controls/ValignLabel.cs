using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{ 
    [Register ("ValignLabel")]
    public class ValignLabel: UILabel
    {
        public enum VerticalAlignments
        { 
            Middle = 0,    //the default (what standard UILabels do) 
            Top,    
            Bottom        
        }  


        private VerticalAlignments _verticalAlignment;
            
        public ValignLabel ()
        {        
            VerticalAlignment = VerticalAlignments.Top;
        }

        public ValignLabel(IntPtr handle) : base(  handle )
        {
            VerticalAlignment = VerticalAlignments.Top;
        }
        public ValignLabel (RectangleF rF) : base(rF)
        { 
            VerticalAlignment = VerticalAlignments.Top;
        }



        public VerticalAlignments VerticalAlignment { 
            get { return _verticalAlignment; } 
            set { 
                if (_verticalAlignment != value) { 
                    _verticalAlignment = value; 
                    SetNeedsDisplay ();    //redraw if value changed 
                } 
            }
        }

        public override void DrawText (RectangleF rect)
        { 
            var rErg = TextRectForBounds (rect, Lines); 
            base.DrawText (rErg); 
        }
    
        public RectangleF CalculatedSize {
            get  { return base.TextRectForBounds (Frame, Lines); }          
        }

        public override RectangleF TextRectForBounds (RectangleF rBounds, int nNumberOfLines)
        {
            var rCalculated = base.TextRectForBounds (rBounds, nNumberOfLines); 
            if (_verticalAlignment != VerticalAlignments.Top) {    //no special handling for top
                if (_verticalAlignment == VerticalAlignments.Bottom) { 
                    rBounds.Y += rBounds.Height - rCalculated.Height;    //move down by difference
                } else {    //middle == nothing set == somenthing strange ==> act like standard UILabel
                    rBounds.Y += (rBounds.Height - rCalculated.Height) / 2; 
                } 
            } 
            //CalculatedSize = rCalculated;
            rBounds.Height = rCalculated.Height;    //always the calculated height 
            return (rBounds); 
        }    
    }

}