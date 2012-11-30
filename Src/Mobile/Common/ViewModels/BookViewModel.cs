using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Navigation;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using ServiceStack.Text;
using System.Threading;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace apcurium.MK.Booking.Mobile.ViewModels
{

    public class BookViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IBookingService>

    {
        private bool _initialized;
        private IAccountService _accountService;
        private ILocationService _geolocator;
        private IBookingService _bookingService;
        private bool _pickupIsActive = true;
        private bool _dropoffIsActive = false;
        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        public BookViewModel()
        {
			MessengerHub.Subscribe<LogOutRequested>(msg => RequestNavigate<LoginViewModel>(true));
			InitializeOrder();

            CenterMap(true);
            CheckVersion();

            PickupIsActive = true;
            DropoffIsActive = false;
            Pickup.RequestCurrentLocationCommand.Execute();

            _bookingService.GetLastOrderStatus().ContinueWith(t => 
            {
                var isCompleted = _bookingService.IsStatusCompleted(t.Result.IBSStatusId);
                if (isCompleted)
                {
                    _bookingService.ClearLastOrder();
                }
                else
                {
                    var order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(t.Result.OrderId);
                    ShowStatusActivity(order, t.Result);
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);           
        }

        public BookViewModel(string order)
        {
			Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);   
            Rebook(JsonSerializer.DeserializeFromString<Order>(order));
        }

        protected override void Initialize()
        {
            if(_initialized) throw new InvalidOperationException();
            _initialized = true;

            _accountService = this.GetService<IAccountService>();
            _geolocator = this.GetService<ILocationService>();
            _bookingService = this.GetService<IBookingService>();
                                 

            Pickup = new BookAddressViewModel(() => Order.PickupAddress, address => Order.PickupAddress = address, _geolocator)
            {
                Title = Resources.GetString("BookPickupLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookPickupLocationEmptyPlaceholder")
            };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator)
            {
                Title = Resources.GetString("BookDropoffLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookDropoffLocationEmptyPlaceholder")
            };

			Panel = new PanelViewModel();

            Pickup.AddressChanged += AddressChanged;
            Dropoff.AddressChanged += AddressChanged;

            _fareEstimate = Resources.GetString("NoFareText");

            ThreadPool.QueueUserWorkItem(UpdateServerInfo);
        }

        private void CheckVersion()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //The 2 second delay is required because the view might not be created.
                Thread.Sleep(2000);
                TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersion();
            });

        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() => FirePropertyChanged(() => Pickup));
            InvokeOnMainThread(() => FirePropertyChanged(() => Dropoff));
            CenterMap(sender is bool ? !(bool)sender : false );

            Task.Factory.SafeStartNew( ()=> CalculateEstimate(  ) );
            FirePropertyChanged ( () => CanClearAddress );
        }


        private void CalculateEstimate()
        {
            _fareEstimate = TinyIoCContainer.Current.Resolve<IBookingService>().GetFareEstimateDisplay(Order, "EstimatePrice" , "NoFareText");
            
            InvokeOnMainThread(() => FirePropertyChanged(() => FareEstimate));

        }


        public void InitializeOrder()
        {
            Order = new CreateOrder();
            if (_accountService.CurrentAccount != null)
            {
                Order.Settings = _accountService.CurrentAccount.Settings;
            }
            else
            {
                Order.Settings = new BookingSettings { Passengers = 2 };
            }
        }


        private void NewOrder()
        {
            RequestMainThreadAction(() =>
                {
                    InitializeOrder();

                    ForceRefresh();

                    if (!PickupIsActive)
                    {
                        ActivatePickup.Execute();
                        Thread.Sleep(300);
                    }

                    Pickup.RequestCurrentLocationCommand.Execute();
                });

        }

        public void Reset()
        {
            InitializeOrder();
            ForceRefresh();
        }

        public void Rebook(Order order)
        {

            var serialized = JsonSerializer.SerializeToString<Order>(order);
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(serialized);
            Order.Id = Guid.Empty;
            Order.PickupDate = null;
			Pickup.SetAddress( Order.PickupAddress, false );
			Dropoff.SetAddress( Order.DropOffAddress, false );
			PickupIsActive=true;
			DropoffIsActive=false;
			CenterMap(false);
			ForceRefresh();
        }

        private void ForceRefresh()
        {
            FirePropertyChanged(() => Order);
            FirePropertyChanged(() => Pickup);
            FirePropertyChanged(() => Dropoff);
            FirePropertyChanged(() => SelectedAddress);
            FirePropertyChanged(() => PickupIsActive);
            FirePropertyChanged(() => DropoffIsActive);
			FirePropertyChanged(() => FareEstimate);
            FirePropertyChanged(() => IsInTheFuture);
        }


        public string FareEstimate
        {
            get { return _fareEstimate; }
            set
            {
                _fareEstimate = value;
                FirePropertyChanged(() => FareEstimate);
            }
        }

        public CreateOrder Order
        {
            get;
            private set;
        }

        public BookAddressViewModel SelectedAddress
        {
            get
            {
                if (PickupIsActive)
                {
                    return Pickup;
                }
                else if (DropoffIsActive)
                {
                    return Dropoff;
                }
                else
                {
                    return null;
                }
            }
        }


        public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                _mapCenter = value;
                FirePropertyChanged(() => MapCenter);
            }
        }

		public PanelViewModel Panel { get; set; }
        public BookAddressViewModel Pickup { get; set; }
        public BookAddressViewModel Dropoff { get; set; }

        public bool PickupIsActive
        {
            get { return _pickupIsActive; }
            set
            {
                _pickupIsActive = value;
                FirePropertyChanged(() => PickupIsActive);

            }
        }

        public bool DropoffIsActive
        {
            get { return _dropoffIsActive; }
            set
            {
                _dropoffIsActive = value;
                FirePropertyChanged(() => DropoffIsActive);
                FirePropertyChanged(() => CanClearAddress);
            }
        }

        public bool CanClearAddress
        {
            get { return DropoffIsActive && ( Dropoff != null) && (Dropoff.Model != null) && Dropoff.Model.HasValidCoordinate(); }
        }

        public bool NoAddressActiveSelection
        {
            get { return !DropoffIsActive && !PickupIsActive; }
        }
        public MvxRelayCommand ActivatePickup
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    PickupIsActive = !PickupIsActive;
                    if (DropoffIsActive && PickupIsActive)
                    {
                        DropoffIsActive = false;
                    }
                    if (PickupIsActive)
                    {
                        var res = TinyIoCContainer.Current.Resolve<IAppResource>();
                        TinyIoCContainer.Current.Resolve<IMessageService>().ShowToast(res.GetString("PickupWasActivatedToastMessage"), ToastDuration.Long );
                    }
                    FirePropertyChanged(() => SelectedAddress);
                    FirePropertyChanged(() => NoAddressActiveSelection);
                    CenterMap(false);
                });
            }

        }

        
        public MvxRelayCommand ActivateDropoff
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        DropoffIsActive = !DropoffIsActive;
                        if (DropoffIsActive && PickupIsActive)
                        {
                            PickupIsActive = false;
                        }
                        if (DropoffIsActive)
                        {
                            var res = TinyIoCContainer.Current.Resolve<IAppResource>();
                            TinyIoCContainer.Current.Resolve<IMessageService>().ShowToast(res.GetString("DropoffWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
                        FirePropertyChanged(() => NoAddressActiveSelection);

                        CenterMap(false);

                    });
            }
        }

               
        private void CenterMap(bool changeZoom)
        {
            if (DropoffIsActive && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (PickupIsActive && Pickup.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if ((!PickupIsActive && Pickup.Model.HasValidCoordinate()) && (!DropoffIsActive && Dropoff.Model.HasValidCoordinate()))
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                                            new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = ZoomLevel.DontChange }};
            }

        }

        private void UpdateServerInfo(object state)
        {
			string appVersion = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;
			var versionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("Version");
            var serverInfo = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfo();
			var version = string.Format(versionFormat, appVersion);
			if (serverInfo != null)
			{
				var serverVersionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServerInfo");
				version += " " + string.Format(serverVersionFormat, serverInfo.SiteName, serverInfo.Version);
			}
			Panel.Version =  version;
        }

        

        public bool IsInTheFuture { get { return Order.PickupDate.HasValue; } }

        public string PickupDateDisplay
        {
            get
            {
                if (Order.PickupDate.HasValue)
                {
                    var format = Resources.GetString("PickupDateDisplay");
                    return String.Format(format, Order.PickupDate.Value);
                }
                else
                {
                    return "";
                }

            }
        }
        public void PickupDateSelected()
        {
            FirePropertyChanged(() => IsInTheFuture);
            FirePropertyChanged(() => PickupDateDisplay);
            Task.Factory.SafeStartNew ( CalculateEstimate );
        }
		public IMvxCommand PickupDateSelectedCommand
		{
			get
			{
				return new MvxRelayCommand<DateTime?>(date => {
					if( date.HasValue && date < DateTime.Now )
					{
						var res = TinyIoCContainer.Current.Resolve<IAppResource>();
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage( res.GetString("InvalidChoiceTitle"), res.GetString("BookViewInvalidDate") );
						Order.PickupDate = null;
					}
					else
					{
						Order.PickupDate = date;
						InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, Order.PickupDate )));
					}
					PickupDateSelected();
				});
			}
		}

		public IMvxCommand BookTaxi
		{
			get
			{
				return ConfirmOrder;
			}
		}

        public IMvxCommand ConfirmOrder
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
				

					if (Order.Settings.Passengers == 0) {
						var account = _accountService.CurrentAccount;
						Order.Settings = account.Settings;
					}

					bool isValid = _bookingService.IsValid (Order);
					if (!isValid)
                    {
                        InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("InvalidBookinInfo")));
						return;
                    }

					if (Order.PickupDate.HasValue && Order.PickupDate.Value < DateTime.Now) {
						InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("BookViewInvalidDate")));
						return;
					}

                    TinyMessageSubscriptionToken token = null;
                    token = MessengerHub.Subscribe<OrderConfirmed>(msg =>
                    {
                        if (token != null)
                        {
                            MessengerHub.Unsubscribe<OrderConfirmed>(token);
                        }
						Task.Factory.StartNew(() => CompleteOrder(msg.Content));
                    });

                    InvokeOnMainThread(() =>
                    {
                        var serialized = Order.ToJson();
						RequestNavigate<BookConfirmationViewModel>(new { order = serialized }, false, MvxRequestedBy.UserAction);
                    });
                });
            }
        }

        public IMvxCommand NavigateToRateOrder
        {
            get
            {
                return new MvxRelayCommand(() => RequestNavigate<BookRatingViewModel>(
                    new KeyValuePair<string, bool>("canRate", true)));
               
            }
        }

        public IMvxCommand NavigateToOrderStatus 
		{
			get {
				return new MvxRelayCommand<Dictionary<string,object>> (order =>
				{					
					var orderGet = (Order)order ["order"];                  
					var orderInfoGet = (OrderStatusDetail)order ["orderInfo"];
					var orderWithStatus = new OrderWithStatusModel () { Order = orderGet, OrderStatusDetail = orderInfoGet };
					var serialized = JsonSerializer.SerializeToString (orderWithStatus, typeof(OrderWithStatusModel));
                    RequestNavigate<BookingStatusViewModel> (new {order = serialized});
				});
			}
		}

        private void CompleteOrder (CreateOrder order)
		{

			order.Id = Guid.NewGuid ();
			try {
				MessageService.ShowProgress (true);
				var orderInfo = _bookingService.CreateOrder (order);

				if (orderInfo.IBSOrderId.HasValue
					&& orderInfo.IBSOrderId > 0) {
					var orderCreated = new Order { CreatedDate = DateTime.Now, DropOffAddress = order.DropOffAddress, IBSOrderId = orderInfo.IBSOrderId, Id = order.Id, PickupAddress = order.PickupAddress, Note = order.Note, PickupDate = order.PickupDate.HasValue ? order.PickupDate.Value : DateTime.Now, Settings = order.Settings };

					ShowStatusActivity (orderCreated, orderInfo);

				}

				NewOrder ();

			} catch (Exception ex) {
				InvokeOnMainThread (() =>
				{
					var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
					string err = string.Format (Resources.GetString ("ServiceError_ErrorCreatingOrderMessage"), settings.ApplicationName, settings.PhoneNumberDisplay (order.Settings.ProviderId.HasValue ? order.Settings.ProviderId.Value : 1));
					MessageService.ShowMessage (Resources.GetString ("ErrorCreatingOrderTitle"), err);
				});
			} finally {
				MessageService.ShowProgress(false);
			}
        }

        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            RequestNavigate<BookingStatusViewModel>(new
            {
                order = data.ToJson(),
                orderStatus = orderInfo.ToJson()
            });
        }

   }
}
