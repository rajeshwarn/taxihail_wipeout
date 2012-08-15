using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RadioElementWithId : RadioElement
    {


        public RadioElementWithId(string caption) : base( caption )
        {
        }

        public override MonoTouch.UIKit.UITableViewCell GetCell(MonoTouch.UIKit.UITableView tv)
        {
            var cell =  base.GetCell(tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;

        }
        public int Id {
            get;
            set;
        }
    }
}

