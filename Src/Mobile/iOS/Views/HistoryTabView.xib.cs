
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
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Threading.Tasks;
using System.Threading;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Binding;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class HistoryTabView : BaseViewController<HistoryViewModel>
	{

		private const string CELLID = "HistoryCell";
		
		static string CellBindingText =
			   new B("FirstLine","Title")
				.Add("FirstLineTextColor","Status","OrderStatusToTextColorConverter")
				.Add("SecondLine","PickupAddress.BookAddress")
				.Add("ShowRightArrow","ShowRightArrow")
				.Add("ShowPlusSign","ShowPlusSign")
				.Add("IsFirst","IsFirst")
				.Add("IsLast","IsLast");

		#region Constructors

		public HistoryTabView() 
			: base(new MvxShowViewModelRequest<HistoryViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_HistoryList"), true);

			lblInfo.Text = Resources.HistoryInfo;	
			lblInfo.TextColor = AppStyle.TitleTextColor;
			lblNoHistory.Text = Resources.NoHistoryLabel;
            lblNoHistory.Hidden = true;
            tableHistory.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableHistory.BackgroundColor = UIColor.Clear;

            var source = new BindableCommandTableViewSource(
				tableHistory, 
				UITableViewCellStyle.Subtitle,
				new NSString(CELLID), 
				CellBindingText,
				UITableViewCellAccessory.None);
			
			source.CellCreator = (tview , iPath, state ) => { 
                return new TwoLinesCell( CELLID, CellBindingText ); 
            };
			this.AddBindings(new Dictionary<object, string>(){
                {tableHistory, "{'Hidden': {'Path': 'HasOrders', 'Converter': 'BoolInverter'}}"},
                {lblNoHistory, "{'Hidden': {'Path': 'HasOrders'}}"},
                {source, "{'ItemsSource':{'Path':'Orders'}, 'SelectedCommand':{'Path':'NavigateToHistoryDetailPage'}}"} ,
			});
			
			tableHistory.Source = source;

            this.View.ApplyAppFont ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}
	}
}

