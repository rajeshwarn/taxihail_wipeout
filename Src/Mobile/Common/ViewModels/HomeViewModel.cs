using System;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : BaseViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;

		public HomeViewModel(IOrderWorkflowService orderWorkflowService, IMvxWebBrowserTask browserTask) : base()
		{
			_orderWorkflowService = orderWorkflowService;
			Panel = new PanelMenuViewModel(this, browserTask);
		}

		public void Init()
		{

		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);
			if (firstTime)
			{
				Map = AddChild<MapViewModel>();
				OrderOptions = AddChild<OrderOptionsViewModel>();
				OrderReview = AddChild<OrderReviewViewModel>();
				BottomBar = AddChild<BottomBarViewModel>();
			}
			this.Services().Vehicle.Start();
		}

		public override void OnViewStopped()
		{
			base.OnViewStopped();
			this.Services().Vehicle.Stop();
		}

		public PanelMenuViewModel Panel { get; set; }

		private MapViewModel _map;
		public MapViewModel Map
		{ 
			get { return _map; }
			private set
			{ 
				_map = value;
				RaisePropertyChanged();
			}
		}

		private OrderOptionsViewModel _orderOptions;
		public OrderOptionsViewModel OrderOptions
		{ 
			get { return _orderOptions; }
			private set
			{ 
				_orderOptions = value;
				RaisePropertyChanged();
			}
		}

		private OrderReviewViewModel _orderReview;
		public OrderReviewViewModel OrderReview
		{
			get { return _orderReview; }
			set
			{
				_orderReview = value;
				RaisePropertyChanged();
			}
		}

        private BottomBarViewModel _bottomBar;
		public BottomBarViewModel BottomBar
		{
			get { return _bottomBar; }
			set
			{
				_bottomBar = value;
				RaisePropertyChanged();
			}
		}

		public ICommand LocateMe
		{
			get
			{
				return this.GetCommand(() =>{
					_orderWorkflowService.SetAddressToUserLocation();
				});
			}
		}


    }
}

