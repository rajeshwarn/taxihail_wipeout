using MonoTouch.UIKit;
using CrossUI.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client.MonoTouchDialog
{
    public class RadioElementWithId<TId> : RadioElement where TId: struct
    {
        readonly string _image;

        public RadioElementWithId(TId? id, string caption, string image = null) : base( caption )
        {
            Id = id;
            _image = image;
        }
       
        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl (tv);
            cell.BackgroundColor = UIColor.White;
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



        public TId? Id {
// ReSharper disable once UnusedAutoPropertyAccessor.Global
            get;
            private set;
        }
    }
}


