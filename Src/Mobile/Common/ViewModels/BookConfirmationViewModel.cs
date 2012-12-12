using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookConfirmationViewModel : BaseViewModel,
		IMvxServiceConsumer<IAccountService>,
		IMvxServiceConsumer<IBookingService>,
		IMvxServiceConsumer<ICacheService>
    {
		IBookingService _bookingService;
        IAccountService _accountService;

        public BookConfirmationViewModel (string order)
        {
            _accountService = this.GetService<IAccountService>();
			_bookingService = this.GetService<IBookingService>();
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);	
			Order.Settings = _accountService.CurrentAccount.Settings;
        }

        public override void OnViewLoaded ()
        {
            base.OnViewLoaded ();
            try {

                MessageService.ShowProgress (true);                
                RideSettings = new RideSettingsModel (Order.Settings, _accountService.GetCompaniesList (), _accountService.GetVehiclesList (), _accountService.GetPaymentsList ());
                FareEstimate = _bookingService.GetFareEstimateDisplay (Order, null, "NotAvailable", false, "NotAvailable");
                ShowFareEstimateAlertDialogIfNecessary();
                ShowChooseProviderDialogIfNecessary();
				FirePropertyChanged ( () => Vehicles );
				FirePropertyChanged ( () => Payments );
				FirePropertyChanged ( () => VehicleName );
				FirePropertyChanged ( () => ChargeType );
            } finally {
                MessageService.ShowProgress (false);
            }
        }

        public void SetVehicleTypeId( int id )
        {
            Order.Settings.VehicleTypeId = id;
            FirePropertyChanged ( () => VehicleName );
        }

        public void SetChargeTypeId( int id )
        {
            Order.Settings.ChargeTypeId = id;
            FirePropertyChanged ( () => ChargeType );
        }


        public int VehicleTypeId {
            get { return Order.Settings.VehicleTypeId ; }
            set {  SetVehicleTypeId( value ); }
        }
        public int ChargeTypeId {
            get { return Order.Settings.ChargeTypeId ; }
            set {  SetChargeTypeId( value ); }
        }


            
            
        public ListItem[] Vehicles {
            get {
				return RideSettings != null  ? RideSettings.VehicleTypeList : null;
            }
        }

        public ListItem[] Payments {
            get {
				return RideSettings != null  ? RideSettings.ChargeTypeList : null;
            }
        }

        public string VehicleName {
            get {
				return RideSettings != null  ? RideSettings.VehicleTypeName : null;
            }
        }

        public string ChargeType{
            get {
				return RideSettings != null  ? RideSettings.ChargeTypeName : null;
            }
        }

		public CreateOrder Order { get; private set; }
		public string AptRingCode {
			get {
				return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
			}
		}
		public string BuildingName {
			get {
				return FormatBuildingName(Order.PickupAddress.BuildingName);
			}
		}
		public string FormattedPickupDate {
			get {
				return FormatDateTime(Order.PickupDate);
			}
		}
		private string _fareEstimate;
		public string FareEstimate {
			get {
				return _fareEstimate;
			}
			set {
				if(value != _fareEstimate) {
					_fareEstimate = value;
					FirePropertyChanged("FareEstimate");
				}
			}
		}
		private RideSettingsModel _rideSettings;
		public RideSettingsModel RideSettings {
			get {
				return _rideSettings;
			}
			private set {
				if(value != _rideSettings)
				{
					_rideSettings = value;
					FirePropertyChanged("RideSettings");
				}
			}
		}

		public IMvxCommand NavigateToEditBookingSettings {
			get {
                return GetCommand(() =>
                {
					RequestSubNavigate<RideSettingsViewModel, BookingSettings>(new Dictionary<string, string>{
						{ "bookingSettings", Order.Settings.ToJson () }
					}, result=>{
						if(result != null)
						{
							Order.Settings = result;
                            RideSettings.Data = result;
                            FirePropertyChanged("RideSettings");
						}
					});
				});
			}
		}
		
		public IMvxCommand NavigateToRefineAddress
		{
			get{
                return GetCommand(() =>
                {

					RequestSubNavigate<RefineAddressViewModel, RefineAddressViewModel>(new Dictionary<string, string>() {
						{"apt", Order.PickupAddress.Apartment},
						{"ringCode", Order.PickupAddress.RingCode},
						{"buildingName", Order.PickupAddress.BuildingName},
					}, result =>{
						if(result != null)
						{
							Order.PickupAddress.Apartment = result.AptNumber;
							Order.PickupAddress.RingCode = result.RingCode;
							Order.PickupAddress.BuildingName = result.BuildingName;
							InvokeOnMainThread(() => {
								FirePropertyChanged("AptRingCode");
								FirePropertyChanged("BuildingName");
							});
						}
					});

				});
			}
        }

        public IMvxCommand NavigateToPaymentDetails {
            get {
                return GetCommand(() => {

                    RequestSubNavigate<PaymentDetailsViewModel, PaymentDetailsViewModelResult>(new Dictionary<string, string> {
                        { "selectedCreditCardId", new PaymentDetailsViewModelResult().ToJson () }
                    }, result => {
                        this.Order.Payment.PayWithCreditCard = result.CreditCardId != Guid.Empty;
                        this.Order.Payment.CreditCardId = result.CreditCardId;
                        this.Order.Payment.TipAmount = result.TipAmount;
                        this.Order.Payment.TipPercent = result.TipPercent;
                    });
                });
            }
        }
		
		public IMvxCommand ConfirmOrderCommand
        {
            get
            {

                return GetCommand(() => 
                    {
						Order.Id = Guid.NewGuid ();
						try {
						MessageService.ShowProgress (true);
						var orderInfo = _bookingService.CreateOrder (Order);
						
						if (orderInfo.IBSOrderId.HasValue
						    && orderInfo.IBSOrderId > 0) {
							var orderCreated = new Order { CreatedDate = DateTime.Now, DropOffAddress = Order.DropOffAddress, IBSOrderId = orderInfo.IBSOrderId, Id = Order.Id, PickupAddress = Order.PickupAddress, Note = Order.Note, PickupDate = Order.PickupDate.HasValue ? Order.PickupDate.Value : DateTime.Now, Settings = Order.Settings };
							
							RequestNavigate<BookingStatusViewModel>(new
							                                        {
								order = orderCreated.ToJson(),
								orderStatus = orderInfo.ToJson()
							});	
							Close();
							MessengerHub.Publish(new OrderConfirmed(this, Order, false ));
						}		
						
					} catch (Exception ex) {
						InvokeOnMainThread (() =>
						                    {
							var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
							string err = string.Format (Resources.GetString ("ServiceError_ErrorCreatingOrderMessage"), settings.ApplicationName, settings.PhoneNumberDisplay (Order.Settings.ProviderId.HasValue ? Order.Settings.ProviderId.Value : 1));
							MessageService.ShowMessage (Resources.GetString ("ErrorCreatingOrderTitle"), err);
						});
					} finally {
						MessageService.ShowProgress(false);
					}                       
                    }); 
            }
        }

        public IMvxCommand CancelOrderCommand
        {
            get
            {
                return GetCommand(() => 
                                           {
                    Close();
                    MessengerHub.Publish(new OrderConfirmed(this, Order, true ));
                });            
            }
        }

		private void ShowFareEstimateAlertDialogIfNecessary()
		{
			if(this.GetService<ICacheService>().Get<string>("WarningEstimateDontShow").IsNullOrEmpty() 
			   && Order.DropOffAddress.HasValidCoordinate())
			{
				MessageService.ShowMessage(Resources.GetString("WarningEstimateTitle"), Resources.GetString("WarningEstimate"),
	           		"Ok", delegate {},
					Resources.GetString("WarningEstimateDontShow"), () => this.GetService<ICacheService>().Set("WarningEstimateDontShow", "yes"));
			}
		}

        private void ShowChooseProviderDialogIfNecessary()
        {
            var service = TinyIoCContainer.Current.Resolve<IAccountService>();
            var companyList = service.GetCompaniesList();
			if (Settings.CanChooseProvider && Order.Settings.ProviderId ==null)
			{
				MessageService.ShowDialog(Resources.GetString("ChooseProviderDialogTitle"), companyList, x=>x.Display, result => {
					if(result != null) {
						Order.Settings.ProviderId =  result.Id;
                        RideSettings.Data = Order.Settings;
                        FirePropertyChanged("RideSettings");
					}

					this.GetService<IAccountService>().UpdateSettings(Order.Settings);
				});
			}
            else if(Order.Settings.ProviderId == null)
            {
                Order.Settings.ProviderId = RideSettings.ProviderId;
            }
		}
		
		
		private string FormatAptRingCode(string apt, string rCode)
		{
			string result = apt.HasValue() ? apt : Resources.GetString("ConfirmNoApt");
			result += @" / ";
			result += rCode.HasValue() ? rCode : Resources.GetString("ConfirmNoRingCode");
			return result;
		}

		private string FormatBuildingName(string buildingName)
		{
			if ( buildingName.HasValue() )
			{
				return buildingName;
			}
			else
			{
				return Resources.GetString(Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
			}
		}

		private string FormatDateTime(DateTime? pickupDate )
		{
            var formatTime = new CultureInfo( CultureInfoString ).DateTimeFormat.ShortTimePattern;
			string format = "{0:ddd, MMM d}, {0:"+formatTime+"}";
			string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.GetString("TimeNow");
			return result;
		}

        public string CultureInfoString
        {
            get{
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting ( "PriceFormat" );
                if ( culture.IsNullOrEmpty() )
                {
                    return "en-US";
                }
                else
                {
                    return culture;                
                }
            }
        }

    }
}

