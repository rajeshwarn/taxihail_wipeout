using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public sealed class DraggableButton : UIButton
    {
        public DraggableButton (RectangleF frame) : base(frame)
        {
			ClipsToBounds = false;
        }

        private PointF _startLocation;

        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {
            _startLocation = ((UITouch)evt.TouchesForView(this).AnyObject).LocationInView(this);
            base.TouchesBegan (touches, evt);
        }

        public event DraggedHandler Dragged;
         

        public override void TouchesMoved (NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)evt.TouchesForView(this).AnyObject;
            var draggedTo = touch.LocationInView(this);

            var x= draggedTo.X - _startLocation.X;
            var y = draggedTo.Y - _startLocation.Y;

            if(Dragged != null)
            {
                Dragged.Invoke(this, new DraggedEventArgs(x, y));
            }

            base.TouchesMoved (touches, evt);
        }

    }

}

