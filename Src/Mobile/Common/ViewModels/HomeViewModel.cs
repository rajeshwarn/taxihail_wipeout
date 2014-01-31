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

				Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			}

			AddressSelectionMode = AddressSelectionMode.PickupSelection;
		}

		public PanelMenuViewModel Panel { get; set; }
		public MapViewModel Map { get; private set; }

		private AddressSelectionMode _addressSelectionMode;
		public AddressSelectionMode AddressSelectionMode
		{
			get
			{
				return _addressSelectionMode;
			}
			set
			{
				if (_addressSelectionMode != value)
				{
					_addressSelectionMode = value;
					RaisePropertyChanged();
				}
			}
		}

		private Address _pickupAddress;
		public Address PickupAddress
		{
			get
			{
				return _pickupAddress;
			}

			private set
			{
				if(value != _pickupAddress)
				{
					_pickupAddress = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand LocateMe
		{
			get
			{
				return this.GetCommand(() =>{
					_orderWorkflowService.SetPickupAddressToUserLocation();
				});
			}
		}
    }
}

