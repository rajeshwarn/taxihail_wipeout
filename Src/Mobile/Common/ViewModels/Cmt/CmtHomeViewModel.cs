using System;
using System.Collections.ObjectModel;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CmtHomeViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IPreCogService>
    {
        private IBookingService _bookingService;
        private ILocationService _geolocator;
        private IAccountService _accountService;
        private IPreCogService _preCogService;

        private Address PickupAddress { get; set; }
        private Address DestinationAddress { get; set; }

        protected override void Initialize()
        {
			Messages = new ObservableCollection<CmtMessageViewModel>();
            Panel = new PanelViewModel(this);
            _accountService = this.GetService<IAccountService>();
            _geolocator = this.GetService<ILocationService>();
            _bookingService = this.GetService<IBookingService>();
            _preCogService = this.GetService<IPreCogService>();

			_preCogService.OnStatusSent = DebugStatusRequestResponse;

            _preCogService.Start();

            PickupAddress = new Address();
            DestinationAddress = new Address();

            Pickup = new BookAddressViewModel(() => PickupAddress, address => { PickupAddress = address; }, _geolocator)
            {
                EmptyAddressPlaceholder = Resources.GetString("BookPickupLocationEmptyPlaceholder")
            };
			Pickup.RequestCurrentLocationCommand.Execute(null);
            Dropoff = new BookAddressViewModel(() => DestinationAddress, address => { DestinationAddress = address; }, _geolocator)
            {
                 EmptyAddressPlaceholder = Resources.GetString("BookDropoffLocationEmptyPlaceholder")
            };
			Dropoff.OnAddressSelected(new AddressSelected(this, new Address{ FullAddress = "87 E 42nd St" }, "notimportant")); //grand central
        }

        public PanelViewModel Panel { get; set; }

        public BookAddressViewModel Pickup { get; set; }

        public BookAddressViewModel Dropoff { get; set; }

        public ObservableCollection<CmtMessageViewModel> Messages { get; set; }

		public AsyncCommand GuideMe
        {
            get
            {
				return new AsyncCommand(() =>
                {
					InvokeOnMainThread(() => {
	                    Messages.Add(new CmtMessageViewModel { Message = Resources.GetString("GuidancePlease"), IsUser = true });
	                    Messages.Add(new CmtMessageViewModel { Message = string.Format(Resources.GetString("GuidanceLocationConfirmation"), Pickup.Model.BookAddress), IsUser = false });
	                    Messages.Add(new CmtMessageViewModel { Message = string.Format(Resources.GetString("GuidanceDestinationRequest"), _accountService.CurrentAccount.Name), IsUser = false });
						Messages.Add(new CmtMessageViewModel { Message = string.Format("Destination : {0}", Dropoff.Model.FullAddress), IsUser = false });
					});
					var request = new PreCogRequest{
						Type = PreCogType.Guide,
						DestDesc = "Grand Central",
						DestLat = Dropoff.Model.Latitude,
						DestLon = Dropoff.Model.Longitude
					};
					var response = 	_preCogService.SendRequest(request);
					InvokeOnMainThread(() => {
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Guide request: {0}", request.Dump())});
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Guide response: {0}", response.Dump())});
					});

                });
            }
        }

		public MvxRelayCommand ClearMessages
		{
			get
			{
				return new MvxRelayCommand(() =>{
					Messages.Clear ();					
				});
			}
		}

		public AsyncCommand Brodcast
		{
			get
			{
				return new AsyncCommand(() =>{
					var request = new PreCogRequest{
						Type = PreCogType.Broadcast
					};
					var response = 	_preCogService.SendRequest(request);
					InvokeOnMainThread(() => {
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Broadcast request: {0}", request.Dump())});
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Broadcast response: {0}", response.Dump())});
					});
				});
			}
		}

		public AsyncCommand Ehail
		{
			get
			{
				return new AsyncCommand(() =>{
					var request = new PreCogRequest{
						Type = PreCogType.Ehail
					};
					var response = 	_preCogService.SendRequest(request);
					InvokeOnMainThread(() => {
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Ehail request: {0}", request.Dump())});
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Ehail response: {0}", response.Dump())});
					});
				});
			}
		}

		public AsyncCommand CancelEhail
		{
			get
			{
				return new AsyncCommand(() =>{
					var request = new PreCogRequest{
						Type = PreCogType.CancelEhail
					};
					var response = 	_preCogService.SendRequest(request);
					InvokeOnMainThread(() => {
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("CancelEhail request: {0}", request.Dump())});
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("CancelEhail response: {0}", response.Dump())});
					});
				});
			}
		}

		public AsyncCommand Connect
		{
			get
			{
				return new AsyncCommand(() =>{
					var request = new PreCogRequest{
						Type = PreCogType.Connect
					};
					var response = 	_preCogService.SendRequest(request);
					InvokeOnMainThread(() => {
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Connect request: {0}", request.Dump())});
						Messages.Add(new CmtMessageViewModel{ Message = string.Format("Connect response: {0}", response.Dump())});
					});
				});
			}
		}


		void DebugStatusRequestResponse (PreCogRequest request, PreCogResponse response)
		{
			InvokeOnMainThread(() => {
				Messages.Add(new CmtMessageViewModel{ Message = string.Format("Status request: {0}", request.Dump())});
				Messages.Add(new CmtMessageViewModel{ Message = string.Format("Status response: {0}", response.Dump())});
			});
		}
    }

    public class CmtMessageViewModel
    {
        public string Message { get; set; }
        public bool IsUser { get; set; }
    }
}