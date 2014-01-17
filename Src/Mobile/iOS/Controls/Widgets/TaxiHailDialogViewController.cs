using System;
using CrossUI.Touch.Dialog;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class TaxiHailDialogViewController : DialogViewController
	{
		public TaxiHailDialogViewController(UITableViewStyle tableViewStyle, RootElement _rootElement, bool pushing) : base (tableViewStyle, _rootElement, pushing)
		{
			((UITableView)this.View).RowHeight = 40;
			((UITableView)this.View).ScrollEnabled = false;
			((UITableView)this.View).SeparatorStyle = UITableViewCellSeparatorStyle.None;
			((UITableView)this.View).BackgroundColor = UIColor.Clear;
		}

		public override Source CreateSizingSource(bool unevenRows)
		{
			return new TaxiHailDialogSource(this);
		}
	}

	public class TaxiHailDialogSource : CrossUI.Touch.Dialog.DialogViewController.Source
	{
		public TaxiHailDialogSource(DialogViewController container) : base(container)
		{

		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell =  base.GetCell(tableView, indexPath);
			cell.Frame  = new RectangleF(cell.ContentView.Frame.X, cell.ContentView.Frame.Y, 200, cell.ContentView.Frame.Height);
			cell.ContentView.Frame = new RectangleF(cell.ContentView.Frame.X, cell.ContentView.Frame.Y, 200, cell.ContentView.Frame.Height);
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

			var view = new RoundedCornerView( );
			view.Borders = borders;
			view.Corners = cornerPlace;
			view.BackColor = UIColor.White;
			view.FirstRowOfTwoRowsTable = indexPath.Row == 0 && tableView.NumberOfRowsInSection(indexPath.Section) == 2;

			if (UIHelper.IsOS7orHigher)
			{
				view.Frame = new RectangleF(0, 0, 304, 40);
				var container = new UIView { BackgroundColor = UIColor.Clear };
				container.AddSubview(view);
				cell.BackgroundView = container;  
				cell.SeparatorInset = new UIEdgeInsets(9, 9, 9, 9);
			}
			else
			{
				cell.BackgroundView = view; 
			}

			cell.ContentView.BackgroundColor = UIColor.Clear;
			cell.BackgroundColor = UIColor.Clear;
			cell.IndentationLevel = 1;

			return cell;
		}
	}
}

