using System;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RadioElementWithId<TId> : RadioElement where TId: struct
    {
        public RadioElementWithId(TId id, string caption) : base( caption )
        {
            this.Id = id;
        }
       
        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl (tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }

        public TId Id {
            get;
            private set;
        }
    }
}

