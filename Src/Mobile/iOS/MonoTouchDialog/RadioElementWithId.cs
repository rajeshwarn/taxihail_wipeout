using System;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RadioElementWithId<TId> : RadioElement where TId: struct
    {
        string _image;

        public RadioElementWithId(TId id, string caption, string image = null) : base( caption )
        {
            this.Id = id;
            _image = image;
        }
       
        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl (tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }

        protected override void UpdateDetailDisplay (UITableViewCell cell)
        {
            base.UpdateDetailDisplay (cell);

            if (cell == null)
                return;

            if (_image != null) {                
                cell.ImageView.Image = UIImage.FromFile (_image);                
            }
        }



        public TId Id {
            get;
            private set;
        }
    }
}


