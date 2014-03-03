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
using System.Drawing;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : BaseViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;

		public HomeViewModel(IOrderWorkflowService orderWorkflowService, IMvxWebBrowserTask browserTask) : base()
		{
			_orderWorkflowService = orderWorkflowService;
			Panel = new PanelMenuViewModel(this, browserTask, orderWorkflowService);
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);
            this.Services().Location.Start();
			if (firstTime)
			{
				Map = AddChild<MapViewModel>();
				OrderOptions = AddChild<OrderOptionsViewModel>();
				OrderReview = AddChild<OrderReviewViewModel>();
				OrderEdit = AddChild<OrderEditViewModel>();
				BottomBar = AddChild<BottomBarViewModel>();
				AddressPicker = AddChild<AddressPickerViewModel>();

				BottomBar.Save = OrderEdit.Save;
				BottomBar.CancelEdit = OrderEdit.Cancel;

				LocateMe.Execute();

				this.Services().ApplicationInfo.CheckVersionAsync();

				this.Services().Tutorial.DisplayTutorialToNewUser();

				this.Services().PushNotification.RegisterDeviceForPushNotifications(force: true);
			}
			this.Services().Vehicle.Start();
		}

		public override void OnViewStopped()
		{
			base.OnViewStopped();
            this.Services().Location.Stop();
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

		private OrderEditViewModel _orderEdit;
		public OrderEditViewModel OrderEdit
		{
			get { return _orderEdit; }
			set
			{
				_orderEdit = value;
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

		private AddressPickerViewModel _addressPickerViewModel;
		public AddressPickerViewModel AddressPicker
		{
			get { return _addressPickerViewModel; }
			set
			{
				_addressPickerViewModel = value;
				RaisePropertyChanged();
			}
		}

		public ICommand LocateMe
		{
			get
			{
				return this.GetCommand(async () =>
				{
					var address = await _orderWorkflowService.SetAddressToUserLocation();
					if(address.HasValidCoordinate())
					{
                        this.ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
					}
				});
			}
		}
    }
}