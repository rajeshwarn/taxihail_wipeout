using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public class DraggableButton : UIButton
    {
        public DraggableButton (RectangleF frame) : base(frame)
        {
        }

        private PointF StartLocation;

        public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            StartLocation = ((UITouch)evt.TouchesForView(this).AnyObject).LocationInView(this);
            base.TouchesBegan (touches, evt);
        }

        public event DraggedHandler Dragged;
         

        public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)evt.TouchesForView(this).AnyObject;
            var draggedTo = touch.LocationInView(this);

            var x= draggedTo.X - StartLocation.X;
            var y = draggedTo.Y - StartLocation.Y;

            if(Dragged != null)
            {
                Dragged.Invoke(this, new DraggedEventArgs(x, y));
            }

            base.TouchesMoved (touches, evt);
        }

        public override void TouchesEnded (MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {


            base.TouchesEnded (touches, evt);
        }



    }

}

