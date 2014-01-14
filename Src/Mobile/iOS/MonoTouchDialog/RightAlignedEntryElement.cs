using System.Drawing;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.MonoTouchDialog
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

		protected override UITableViewCell GetCellImpl(UITableView tv)
        {
            var cell = base.GetCell(tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }
            protected override UITextField CreateTextField (RectangleF frame)
            {
                var res = base.CreateTextField (new RectangleF(frame.X, frame.Y, frame.Width - 10, frame.Height ));
                res.TextAlignment = UITextAlignment.Right;
                return res;
            }

        readonly NSString _cellKey = new NSString ("RightAligned");
            protected override NSString CellKey {
                get {
                    return _cellKey;
                }
            }
        }

}