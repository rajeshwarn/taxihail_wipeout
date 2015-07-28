using System;
using CoreGraphics;
using CrossUI.Touch.Dialog;
using CrossUI.Touch.Dialog.Elements;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class TaxiHailDialogViewController : DialogViewController
    {
        private readonly bool _willBeContainedInOtherView;

        public TaxiHailDialogViewController(RootElement rootElement, bool pushing, bool willBeContainedInOtherView = true, float? rowHeight = null) : base (rootElement, pushing)
        {
            _willBeContainedInOtherView = willBeContainedInOtherView;

            if (!_willBeContainedInOtherView)
            {
                Style = UITableViewStyle.Grouped;
                TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.FromRGB(242, 242, 242) };
                TableView.RowHeight = rowHeight ?? 44;
                Title = string.Empty;
            }
            else
            {
                Style = UITableViewStyle.Plain;
                TableView.RowHeight = rowHeight ?? 40;
            }

            TableView.ScrollEnabled = !_willBeContainedInOtherView;
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            TableView.BackgroundColor = UIColor.Clear;
        }

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new TaxiHailDialogSource(this, _willBeContainedInOtherView);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // change color of status bar
            if (UIHelper.IsOS7orHigher)
            {
                NavigationController.NavigationBar.BarStyle = Theme.IsLightContent
                    ? UIBarStyle.Black
                    : UIBarStyle.Default;
            }
        }
    }

    public class TaxiHailDialogSource : DialogViewController.Source
    {
        private bool _willBeContainedInOtherView;

        public TaxiHailDialogSource(DialogViewController container, bool willBeContainedInOtherView) : base(container)
        {
            _willBeContainedInOtherView = willBeContainedInOtherView;
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            if (_willBeContainedInOtherView)
            {
                return 0;
            }
            return 22;
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
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
                var cellWidth = UIScreen.MainScreen.Bounds.Width - 2*8;
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
                    view.Frame = new CGRect(0, 0, cellWidth, tableView.RowHeight);
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
                var cellWidth = UIScreen.MainScreen.Bounds.Width;
                cell.Frame  = cell.ContentView.Frame.SetWidth(cellWidth);
                cell.ContentView.Frame = cell.ContentView.Frame.SetWidth(cellWidth);
                cell.SelectedBackgroundView = new UIView(cell.Frame) { BackgroundColor = UIColor.FromRGB(190, 190, 190) };
            }

            return cell;
        }
    }
}

