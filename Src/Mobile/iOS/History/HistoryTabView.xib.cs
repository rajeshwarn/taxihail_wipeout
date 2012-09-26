
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class HistoryTabView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public HistoryTabView (IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public HistoryTabView (NSCoder coder) : base(coder)
		{
		}

		public HistoryTabView () : base("HistoryTabView", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			lblInfo.Text = Resources.HistoryInfo;	
			lblInfo.TextColor = AppStyle.TitleTextColor;
			lblNoHistory.Text = Resources.NoHistoryLabel;
			lblNoHistory.Hidden = true;
			tableHistory.Hidden = true;
			tableHistory.RowHeight = 35;
            tableHistory.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableHistory.BackgroundColor = UIColor.Clear; // UIColor.Red ;
		}


		public string GetTitle ()
		{
			return Resources.HistoryViewTitle;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			LoadGridData();
		}

		private void LoadGridData ()
		{
			if (tableHistory != null) {

                var structure = GetHistoricStructure();				
				if (structure.Sections.ElementAt(0).Items.Count () == 0) {
					lblNoHistory.Hidden = false;
					tableHistory.Hidden = true;
				} else {
					lblNoHistory.Hidden = true;
					tableHistory.Hidden = false;

					tableHistory.DataSource = new HistoryTableViewDataSource (structure);
					tableHistory.Delegate = new HistoryTableViewDelegate (this, structure );
					tableHistory.ReloadData ();
				}
			}
		}

		private InfoStructure GetHistoricStructure()
		{
			var historic = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrders();

			var s = new InfoStructure( 50, false );
			var sect = s.AddSection( Resources.HistoryViewTitle );
			historic.ForEach( item => sect.AddItem( new TwoLinesAddressItem( item.Id, string.Format( Resources.OrderNumber, item.IBSOrderId.Value.ToString() ), item.PickupAddress.FullAddress ) { Data = item } ) );

			return s;

		}
		#endregion
	}
}

