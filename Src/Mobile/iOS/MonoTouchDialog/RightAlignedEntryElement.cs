using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RightAlignedEntryElement: EntryElement
        {
            public RightAlignedEntryElement (string caption, string placeholder, string value, bool isPassword)
                : base (caption, placeholder, value,isPassword)
            {
            }

            public RightAlignedEntryElement (string caption, string placeholder, string value)
                : base (caption, placeholder, value)
            {
            }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell = base.GetCell(tv);
            cell.BackgroundColor = UIColor.Clear;
            //cell.BackgroundView = new UIView();
            return cell;
        }
            protected override UITextField CreateTextField (RectangleF frame)
            {
                var res = base.CreateTextField (new RectangleF(frame.X, frame.Y, frame.Width - 10, frame.Height ));
                res.TextAlignment = UITextAlignment.Right;
                return res;
            }

            NSString cellKey = new NSString ("RightAligned");
            protected override NSString CellKey {
                get {
                    return cellKey;
                }
            }
        }

}