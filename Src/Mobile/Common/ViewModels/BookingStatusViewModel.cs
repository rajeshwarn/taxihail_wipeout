using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using System.Reactive.Linq;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookingStatusViewModel : BaseViewModel,
        IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<ILocationService>
    {
        private readonly IBookingService _bookingService;
        private const string _doneStatus = "wosDONE";
        private const string _loadedStatus = "wosLOADED";
        private ILocationService _geolocator;
        private const int _refreshPeriod = 20 ; //20 sec
        private bool _isThankYouDialogDisplayed = false;

        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable();

		public BookingStatusViewModel(string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order>(order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail>(orderStatus);
			ShowRatingButton = true;
			MessengerHub.Subscribe<OrderRated>( OnOrderRated , o=>o.Content.Equals (Order.Id) );
			_bookingService = this.GetService<IBookingService>();
            StatusInfoText = string.Format(Resources.GetString("StatusStatusLabel"), Resources.GetString("LoadingMessage"));
             _geolocator = this.GetService<ILocationService>();

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

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_refreshPeriod)).Select(c => new Unit())
                .Subscribe(unit => InvokeOnMainThread(RefreshStatus))
                .DisposeWith(Subscriptions);

            CenterMap(true);
		}

        private IEnumerable<CoordinateViewModel> _mapCenter { get; set; }

        public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                _mapCenter = value;
                FirePropertyChanged(() => MapCenter);
            }
        }

        

        public BookAddressViewModel Pickup { get; set; }
        public BookAddressViewModel Dropoff { get; set; }

        public Address PickupModel
        {
            get { return Pickup.Model; }
            set { Pickup.Model = value; FirePropertyChanged(()=>PickupModel); }
        }

        private string _confirmationNoTxt { get; set; }

        public string ConfirmationNoTxt
        {
            get
            {
                return _confirmationNoTxt;
            }
            set { _confirmationNoTxt = value; FirePropertyChanged(()=>ConfirmationNoTxt); }
        }

        public bool IsCallButtonVisible
        {
            get { return !bool.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.HideCallDispatchButton")); }
            private set{}
        }

        private string _statusInfoText { get; set; }

        public string StatusInfoText
        {
            get { return _statusInfoText; }
            set { _statusInfoText = value; FirePropertyChanged(()=>StatusInfoText); }
        }

        private void OnOrderRated(OrderRated msg )
        {
            IsRated = true;
        }

        public bool IsRated{get;set;}

        private Order _order;

        public Order Order
        {
            get { return _order; }
            set
            {
                _order = value;
                FirePropertyChanged(() => Order);
            }
        }

        private OrderStatusDetail _orderStatusDetail;

        public OrderStatusDetail OrderStatusDetail
        {
            get { return _orderStatusDetail; }
            set
            {
                _orderStatusDetail = value;
                FirePropertyChanged(() => OrderStatusDetail);
            }
        }

		public bool CloseScreenWhenCompleted {
			get;
			set;
		}

        private bool _showRatingButton;

        public bool ShowRatingButton
        {
            get
            {
                if (!TinyIoCContainer.Current.Resolve<IAppSettings>().RatingEnabled)
                {
                    return false;
                }
                else
                {
                    return _showRatingButton;
                }
            }
            set 
            { 
                _showRatingButton = value;
                FirePropertyChanged(()=>ShowRatingButton);
            }
        }

        public BookingStatusViewModel()
        {
            ShowRatingButton = true;
            SetStatusText(Resources.GetString("LoadingMessage"));
            if (OrderStatusDetail.IBSOrderId.HasValue)
            {
                ConfirmationNoTxt = string.Format(Resources.GetString("StatusDescription"), OrderStatusDetail.IBSOrderId.Value);
            }
        }

       

        private void HideRatingButton(OrderRated orderRated)
        {
            ShowRatingButton = false;
            ShowThankYouDialog();
        }

        private void SetStatusText(string message)
        {
            this.StatusInfoText = string.Format(Resources.GetString("StatusStatusLabel"), message);
        }

        private void RefreshStatus()
        {

                try
                {
                    var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(Order.Id);
                    var isDone = TinyIoCContainer.Current.Resolve<IBookingService>().IsStatusDone(status.IBSStatusId);
                   
                    if (status != null)
                    {
                        StatusInfoText = status.IBSStatusDescription;
                        CenterMap(true);
                        //this.OrderStatusDetail = status;
                        if (OrderStatusDetail.IBSOrderId.HasValue)
                        {
                            ConfirmationNoTxt = string.Format(Resources.GetString("StatusDescription"), OrderStatusDetail.IBSOrderId.Value);
                        }
                        if (isDone)
                        {
                            if (!_isThankYouDialogDisplayed)
                            {
                                _isThankYouDialogDisplayed = true;
                                InvokeOnMainThread(ShowThankYouDialog);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
        }

        private void ShowThankYouDialog()
        {
            string stringNeutral = null;
            Action actionNeutral = null;
            TinyMessageSubscriptionToken orderRatedToken = null;
            if (ShowRatingButton)
            {
                stringNeutral = Resources.GetString("RateBtn");
                actionNeutral = () =>
                                    {
                                        if ((Common.Extensions.GuidExtensions.HasValue(Order.Id)))
                                        {
                                            Order.Id = Order.Id;
                                            orderRatedToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>()
                                                            .Subscribe<OrderRated>(HideRatingButton);
                                            NavigateToRatingPage.Execute();
                                        }
                                    };
            }
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            MessageService.ShowMessage(Resources.GetString("View_BookingStatus_ThankYouTitle"),
                String.Format(Resources.GetString("View_BookingStatus_ThankYouMessage"), settings.ApplicationName),
                Resources.GetString("ReturnBookingScreen"),() =>
                                                               {
                                                                   if (orderRatedToken != null)
                                                                   {
                                                                       TinyIoCContainer.Current.Resolve<ITinyMessengerHub>()
                                                            .Unsubscribe<OrderRated>(orderRatedToken);
                                                                   }
                                                                   this.Close();
                                                               },
                Resources.GetString("HistoryDetailSendReceiptButton"), () =>
                {
                    if (Common.Extensions.GuidExtensions.HasValue(Order.Id))
                    {
                        TinyIoCContainer.Current.Resolve<IBookingService>().SendReceipt(Order.Id);
                    }
                },
                stringNeutral,actionNeutral
                );
        }

        private void CenterMap(bool changeZoom)
        {
            
            if (OrderStatusDetail.VehicleLatitude.HasValue  && OrderStatusDetail.VehicleLongitude.HasValue)
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                                            new CoordinateViewModel { Coordinate = new Coordinate { Latitude = OrderStatusDetail.VehicleLatitude.GetValueOrDefault(), Longitude = OrderStatusDetail.VehicleLongitude.GetValueOrDefault() }, Zoom = ZoomLevel.DontChange }};
            }
            else 
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
        }

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    MessengerHub.Subscribe<OrderRated>(HideRatingButton);
                    RequestNavigate<BookRatingViewModel>(new { orderId = Order.Id.ToString(), canRate = true.ToString(CultureInfo.InvariantCulture), isFromStatus = true.ToString(CultureInfo.InvariantCulture) });
                });
            }
        }

        public MvxRelayCommand NewRide
        {
            get
            {
                return new MvxRelayCommand(() =>
                                               {

                    MessageService.ShowMessage( Resources.GetString("StatusNewRideButton") ,  Resources.GetString("StatusConfirmNewBooking"),  Resources.GetString("YesButton"), () =>
                    {
                        _bookingService.ClearLastOrder();
                        RequestNavigate<BookViewModel>(clearTop:true);
                    },
                    Resources.GetString("NoButton"), () => { });   
                                        
                });
            }
        }

          


        public MvxRelayCommand CancelOrder
        {
            get
            {
                return new MvxRelayCommand(() =>
                                               {
                                                   if ((OrderStatusDetail.IBSStatusId == _doneStatus) || (OrderStatusDetail.IBSStatusId == _loadedStatus))
                                                   {
                                                        MessageService.ShowMessage(Resources.GetString("CannotCancelOrderTitle"),Resources.GetString("CannotCancelOrderMessage"));
                                                        return;
                                                   }

                                                   MessageService.ShowMessage("",Resources.GetString("StatusConfirmCancelRide"),Resources.GetString("YesButton"),()
                                                      
                                                                              =>
                                                                                  {
                                                                                      var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(Order.Id);      
                                                                                      if(isSuccess)
                                                                                      {
                                                                                          MessengerHub.Publish(new OrderCanceled(this, Order, null));
                                                                                          this.Close();
                                                                                      }
                                                                                      else
                                                                                      {
                                                                                          MessageService.ShowMessage(Resources.GetString("StatusConfirmCancelRideErrorTitle"), Resources.GetString("StatusConfirmCancelRideError"));
                                                                                      }
                                                                                  },
                                                                                  Resources.GetString("NoButton"),() =>
                                                                                                                      {
                                                                                                                          
                                                                                                                      }
                                                               );
                                               });
            }
        }

        public MvxRelayCommand CallCompany
        {
            get
            {
                return new MvxRelayCommand(()=>{
                    Action call = () => { PhoneService.Call(Settings.PhoneNumber(Order.Settings.ProviderId.Value)); };
                    MessageService.ShowMessage(string.Empty, 
                                               Settings.PhoneNumberDisplay(Order.Settings.ProviderId.Value), 
                                               Resources.GetString("CallButton"), 
                                               call, Resources.GetString("CancelBoutton"), 
                                               () => {});                    
                });
            }
        }

        public override void OnViewUnloaded ()
        {
            base.OnViewUnloaded ();
            Subscriptions.DisposeAll();
        }
    }
}