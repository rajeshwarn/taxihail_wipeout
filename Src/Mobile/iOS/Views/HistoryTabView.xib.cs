using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class HistoryTabView : BaseViewController<HistoryViewModel>
	{

		private const string Cellid = "HistoryCell";
		
		static readonly string CellBindingText =
			   new B("FirstLine","Title")
				.Add("FirstLineTextColor","Status","OrderStatusToTextColorConverter")
				.Add("SecondLine","PickupAddress.DisplayAddress")
				.Add("ShowRightArrow","ShowRightArrow")
				.Add("ShowPlusSign","ShowPlusSign")
				.Add("IsFirst","IsFirst")
				.Add("IsLast","IsLast");

		#region Constructors

		public HistoryTabView() 
			: base(new MvxShowViewModelRequest<HistoryViewModel>( null, true, new MvxRequestedBy()   ) )
		{
		}
		
		public HistoryTabView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public HistoryTabView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}

		#endregion

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
            NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_HistoryList"), true);

			lblInfo.Text = Resources.HistoryInfo;	
			lblInfo.TextColor = AppStyle.TitleTextColor;
			lblNoHistory.Text = Resources.NoHistoryLabel;
            lblNoHistory.Hidden = true;
            tableHistory.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableHistory.BackgroundColor = UIColor.Clear;

            var source = new BindableCommandTableViewSource(
				tableHistory, 
				UITableViewCellStyle.Subtitle,
				new NSString(Cellid), 
				CellBindingText,
				UITableViewCellAccessory.None);
			
			source.CellCreator = (tview , iPath, state ) => { 
                return new TwoLinesCell( Cellid, CellBindingText ); 
            };
			this.AddBindings(new Dictionary<object, string>{
                {tableHistory, "{'Hidden': {'Path': 'HasOrders', 'Converter': 'BoolInverter'}}"},
                {lblNoHistory, "{'Hidden': {'Path': 'HasOrders'}}"},
                {source, "{'ItemsSource':{'Path':'Orders'}, 'SelectedCommand':{'Path':'NavigateToHistoryDetailPage'}}"} ,
			});
			
			tableHistory.Source = source;

            View.ApplyAppFont ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}
	}
}

