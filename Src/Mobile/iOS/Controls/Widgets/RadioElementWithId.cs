using MonoTouch.UIKit;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class RadioElementWithId<TId> : RadioElement where TId: struct
    {
        private bool _showBottomBar;
        private readonly string _image;

        public RadioElementWithId(TId? id, string caption, string image = null, bool showBottomBar = true) : base( caption )
        {
            Id = id;
            _image = image;
            _showBottomBar = showBottomBar;
        }

        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl (tv);

            cell.Frame  = cell.Frame.SetHeight(tv.RowHeight);
            cell.ContentView.Frame = cell.ContentView.Frame.SetHeight(tv.RowHeight);

            cell.BackgroundColor = UIColor.Clear;
            cell.ContentView.BackgroundColor = UIColor.Clear;
            cell.TextLabel.BackgroundColor = UIColor.Clear;
            cell.TextLabel.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32/2);
            cell.TextLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			cell.TextLabel.TextAlignment ();

			cell.BackgroundView = new CustomCellBackgroundView(cell.ContentView.Frame, 10, UIColor.White, UIColor.FromRGB(190, 190, 190)) 
            {
                HideBottomBar = !_showBottomBar
            };

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

        public TId? Id { get; private set; }
    }
}


