
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public partial class HistoryTabView : UIViewController, ITaxiViewController, ISelectableViewController, IRefreshableViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public HistoryTabView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public HistoryTabView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public HistoryTabView () : base("HistoryTabView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			 
			lblInfo.Text = Resources.HistoryInfo;	
			lblNoHistory.Text = Resources.NoHistoryLabel;
			lblNoHistory.Hidden = true;
			tableHistory.Hidden = true;
			tableHistory.RowHeight = 35;
			Selected ();
			
		}


		public string GetTitle ()
		{
			return Resources.HistoryViewTitle;
		}

		public UIView GetTopView ()
		{
			return null;
		}

		public void RefreshData ()
		{
			LoadGridData ();
		}
		public void Selected ()
		{
			LoadGridData ();
		}

		private void LoadGridData ()
		{
			if (tableHistory != null) {
				var historic = AppContext.Current.LoggedUser.BookingHistory.Where (b => !b.Hide).OrderByDescending( b=>b.Id );
				if (historic.Count () == 0) {
					lblNoHistory.Hidden = false;
					tableHistory.Hidden = true;
				} else {
					lblNoHistory.Hidden = true;
					tableHistory.Hidden = false;
					tableHistory.DataSource = new HistoryTableViewDataSource (historic);
					tableHistory.Delegate = new HistoryTableViewDelegate (this, historic);
					tableHistory.ReloadData ();
				}
			}
		}
		#endregion
	}
}

