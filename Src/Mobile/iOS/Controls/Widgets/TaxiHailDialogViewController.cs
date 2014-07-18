using System.Drawing;
using CrossUI.Touch.Dialog;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class TaxiHailDialogViewController : DialogViewController
    {
        private bool _willBeContainedInOtherView;

        public TaxiHailDialogViewController(RootElement _rootElement, bool pushing, bool willBeContainedInOtherView = true) : base (_rootElement, pushing)
        {
            _willBeContainedInOtherView = willBeContainedInOtherView;

            if (!_willBeContainedInOtherView)
            {
                Style = UITableViewStyle.Grouped;
                TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.FromRGB(242, 242, 242) };
                TableView.RowHeight = 44;
                Title = string.Empty;
            }
            else
            {
                Style = UITableViewStyle.Plain;
                TableView.RowHeight = 40;
            }

            TableView.ScrollEnabled = !_willBeContainedInOtherView;
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            TableView.BackgroundColor = UIColor.Clear;
        }

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new TaxiHailDialogSource(this, _willBeContainedInOtherView);
        }
    }

    public class TaxiHailDialogSource : CrossUI.Touch.Dialog.DialogViewController.Source
    {
        private bool _willBeContainedInOtherView;

        public TaxiHailDialogSource(DialogViewController container, bool willBeContainedInOtherView) : base(container)
        {
            _willBeContainedInOtherView = willBeContainedInOtherView;
        }

        public override float GetHeightForHeader(UITableView tableView, int section)
        {
            if (_willBeContainedInOtherView)
            {
                return 0;
            }
            return 22;
        }

        public override float GetHeightForFooter(UITableView tableView, int section)
        {
            if (_willBeContainedInOtherView)
            {
                return 0;
            }
            return 22;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell =  base.GetCell(tableView, indexPath);
            cell.Frame  = cell.ContentView.Frame.SetHeight(tableView.RowHeight);
            cell.ContentView.Frame = cell.ContentView.Frame.SetHeight(tableView.RowHeight);
            cell.ContentView.BackgroundColor = UIColor.Clear;
            cell.BackgroundColor = UIColor.Clear;
            cell.IndentationLevel = 1;

            if (_willBeContainedInOtherView)
            {
                var cellWidth = 304;
                cell.Frame  = cell.ContentView.Frame.SetWidth(cellWidth);
                cell.ContentView.Frame = cell.ContentView.Frame.SetWidth(cellWidth);


                UIRectCorner cornerPlace = 0;
                var borders = Border.Right | Border.Left | Border.Bottom;

                //first and last cells have different rounded corners
                if (indexPath.Section == 0
                    && indexPath.Row == 0)
                {
                    cornerPlace = UIRectCorner.TopLeft | UIRectCorner.TopRight;
                }

                if (indexPath.Row == tableView.NumberOfRowsInSection(indexPath.Section) - 1)
                {
                    cornerPlace = UIRectCorner.BottomLeft | UIRectCorner.BottomRight;
                }

                //the one before the last: remove the bottom border already present in the cell below
                if (indexPath.Row == tableView.NumberOfRowsInSection(indexPath.Section) - 2)
                {
                    borders = borders ^ Border.Bottom;
                }

                var view = new RoundedCornerView();
                view.Borders = borders;
                view.Corners = cornerPlace;
                view.BackColor = UIColor.White;
                view.FirstRowOfTwoRowsTable = indexPath.Row == 0 && tableView.NumberOfRowsInSection(indexPath.Section) == 2;

                if (UIHelper.IsOS7orHigher)
                {
                    view.Frame = new RectangleF(0, 0, cellWidth, tableView.RowHeight);
                    var container = new UIView { BackgroundColor = UIColor.Clear };
                    container.AddSubview(view);
                    cell.BackgroundView = container;  
                    cell.SeparatorInset = new UIEdgeInsets(9, 9, 9, 9);
                }
                else
                {
                    cell.BackgroundView = view;
                }
            }
            else
            {
                var cellWidth = 320;
                cell.Frame  = cell.ContentView.Frame.SetWidth(cellWidth);
                cell.ContentView.Frame = cell.ContentView.Frame.SetWidth(cellWidth);
                cell.SelectedBackgroundView = new UIView(cell.Frame) { BackgroundColor = UIColor.FromRGB(190, 190, 190) };
            }

            return cell;
        }
    }
}

