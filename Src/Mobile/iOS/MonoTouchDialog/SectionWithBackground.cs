using MonoTouch.UIKit;
using CrossUI.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client.MonoTouchDialog
{
    public class SectionWithBackground : Section
    {
        public SectionWithBackground(string caption ) :base( caption )
        {

        }

        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl(tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }
    }
}

