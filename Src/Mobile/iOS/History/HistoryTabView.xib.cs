
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

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class HistoryTabView : MvxBindingTouchViewController<HistoryViewModel>
	{

		private const string CELLID = "HistoryCell";
		
		const string CellBindingText = @"
                {
                   'TitleText':{'Path':'Title'},
                   'DetailText':{'Path':'PickupAddress.FullAddress'}
				}";

		private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource();
		
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
			tableHistory.Hidden = true;
			tableHistory.RowHeight = 35;
            tableHistory.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableHistory.BackgroundColor = UIColor.Clear; // UIColor.Red ;

			/*ViewModel.LoadOrders().ContinueWith( t=> 
                    {
				InvokeOnMainThread( () => 
				                   {
				var source = new MvxActionBasedBindableTableViewSource(
					tableHistory, 
					UITableViewCellStyle.Subtitle,
					new NSString(CELLID), 
					CellBindingText,
					UITableViewCellAccessory.None);
				
				this.AddBindings(new Dictionary<object, string>(){
					{source, "{'ItemsSource':{'Path':'Orders'}}"} ,
				});
				
				tableHistory.Source = source;
				});
				}

			);*/
			/*var source = new MvxActionBasedBindableTableViewSource(
				tableHistory, 
				UITableViewCellStyle.Subtitle,
				new NSString(CELLID), 
				CellBindingText,
				UITableViewCellAccessory.None);
			
			source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( CELLID, CellBindingText ); };
			this.AddBindings(new Dictionary<object, string>(){
				{source, "{'ItemsSource':{'Path':'Orders'}}"} ,
			});
			
			tableHistory.Source = source;*/

		}


		public string GetTitle ()
		{
			return Resources.HistoryViewTitle;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			LoadGridData();
			NavigationController.NavigationBar.Hidden = false;
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		private void LoadGridData ()
		{
			if (tableHistory == null) {
				return;
			}
			TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(true, () => CancelCurrentTask() );
			_searchCancellationToken = new CancellationTokenSource();
			var task = new Task<InfoStructure>(() => GetHistoricStructure(), _searchCancellationToken.Token);
			task.ContinueWith(RefreshData);
			ViewModel.LoadOrders().ContinueWith(t => task.Start());
		}

		public void RefreshData( Task<InfoStructure>  task )
		{
			if(task.IsCompleted && !task.IsCanceled)
			{
				InvokeOnMainThread( () => {
					if (task.Result.Sections.ElementAt(0).Items.Count () == 0) {
						lblNoHistory.Hidden = false;
						tableHistory.Hidden = true;
					} else {
						lblNoHistory.Hidden = true;
						tableHistory.Hidden = false;
						
						tableHistory.DataSource = new HistoryTableViewDataSource (task.Result);
						tableHistory.Delegate = new HistoryTableViewDelegate (this, task.Result );
						tableHistory.ReloadData ();
					}
				});
			}
			TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);
		}
		
		private void CancelCurrentTask()
		{
			if (_searchCancellationToken != null
			    && _searchCancellationToken.Token.CanBeCanceled)
			{
				_searchCancellationToken.Cancel();
				_searchCancellationToken.Dispose();
				_searchCancellationToken = null;
			}
		}


		private InfoStructure GetHistoricStructure()
		{
			var s = new InfoStructure( 50, false );
			var sect = s.AddSection( Resources.HistoryViewTitle );
			ViewModel.Orders.ForEach(item => sect.AddItem(new TwoLinesAddressItem(item.Id, string.Format(Resources.OrderHistoryListTitle, item.IBSOrderId.Value.ToString(), item.PickupDate), item.PickupAddress.FullAddress) { Data = item }));

			return s;
		}

	}
}

