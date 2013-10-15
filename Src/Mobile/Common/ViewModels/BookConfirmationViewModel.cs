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
using Cirrious.MvvmCross.Interfaces.ViewModels;
using System.Threading.Tasks;

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
        }



        public override void Load ()
        {
			base.Load ();

            Console.WriteLine("Opening confirmation view....");

			MessageService.ShowProgress(true);




            Task.Factory.StartNew<RideSettingsViewModel>(() => new RideSettingsViewModel( Order.Settings ) )
                .HandleErrors( )
                .ContinueWith(t =>
                    {
                        InvokeOnMainThread(() =>
                        {
							if ( t.Result != null )
							{
                            	this.RideSettings = t.Result;
                            
                            	FirePropertyChanged(() => VehicleName);
                            	FirePropertyChanged(() => ChargeType);
                            	MessageService.ShowProgress(false);                            
                            	ShowWarningIfNecessary();
							}

                        });
                    });

            ShowFareEstimateAlertDialogIfNecessary();

            Console.WriteLine("Done opening confirmation view....");

        }

        public string FareEstimate
        {
            get
            {
                return FormatPrice(Order.Estimate.Price);
            }

        }

        public RideSettingsViewModel RideSettings {get;set;}
      

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


        public string OrderPassengerNumber
        {
            get { return Order.Settings.Passengers.ToString(); }
        }

        public string OrderLargeBagsNumber
        {
            get { return Order.Settings.LargeBags.ToString(); }
        }

        public string OrderName
        {
            get { return Order.Settings.Name; }
        }

        public string OrderPhone
        {
            get { return Order.Settings.Phone; }
        }
        public string OrderApt
        {
            get { return !string.IsNullOrEmpty(Order.PickupAddress.Apartment) ? Order.PickupAddress.Apartment : "N/A"; }
        }
        public string OrderRingCode
        {
            get { return !string.IsNullOrEmpty(Order.PickupAddress.RingCode) ?  Order.PickupAddress.RingCode :  "N/A"; }
        }

        public bool ShowPassengerName
        {
            get
            {
                var ret = true;
                try
                {
                    ret = Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerName"));
                }
                catch (Exception)
                {
                    return false;
                }
                return ret;
            }
         }

        public bool ShowPassengerPhone
        {
            get
            {
                var ret = true;
                try
                {
                    ret = Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerPhone"));
                }
                catch (Exception)
                {
                    return false;
                }
                return ret;
            }
        }

        public bool ShowPassengerNumber
        {
            get
            {
                var ret = true;
                try
                {
                    ret = Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerNumber"));
                }
                catch (Exception)
                {
                    return false;
                }
                return ret;
            }
        }

		
		public IMvxCommand NavigateToRefineAddress
		{
			get{
                return GetCommand(() => RequestSubNavigate<RefineAddressViewModel, RefineAddressViewModel>(new Dictionary<string, string>() {
                    {"apt", Order.PickupAddress.Apartment},
                    {"ringCode", Order.PickupAddress.RingCode},
                    {"buildingName", Order.PickupAddress.BuildingName},
                }, result =>{
                    if (result == null) return;

                    Order.PickupAddress.Apartment = result.AptNumber;
                    Order.PickupAddress.RingCode = result.RingCode;
                    Order.PickupAddress.BuildingName = result.BuildingName;
                    InvokeOnMainThread(() => 
                    {
                        FirePropertyChanged(()=>AptRingCode);
                        FirePropertyChanged(()=>BuildingName);
                    });
                }));
			}
        }
        public IMvxCommand NavigateToEditInformations
        {
            get
            {
                return GetCommand(() => RequestSubNavigate<BookEditInformationViewModel, Order>( 
                    new {
                            order = Order.ToJson()
                        }.ToSimplePropertyDictionary(), result =>
                                                            {
                                                                if (result == null) return;

                                                                Order.PickupAddress.Apartment = result.PickupAddress.Apartment;
                                                                Order.PickupAddress.RingCode = result.PickupAddress.RingCode;
                                                                Order.PickupAddress.BuildingName = result.PickupAddress.BuildingName;
                                                                Order.Settings.Name = result.Settings.Name;
                                                                Order.Settings.VehicleTypeId = result.Settings.VehicleTypeId;
                                                                Order.Settings.ChargeTypeId = result.Settings.ChargeTypeId;
                                                                Order.Settings.Phone = result.Settings.Phone;
                                                                Order.Settings.Passengers = result.Settings.Passengers;
                                                                Order.Settings.LargeBags = result.Settings.LargeBags;
                                                                InvokeOnMainThread(() =>
                                                                {
                                                                    FirePropertyChanged(() => AptRingCode);
                                                                    FirePropertyChanged(() => BuildingName);
                                                                    FirePropertyChanged(() => OrderPassengerNumber);
                                                                    FirePropertyChanged(() => OrderLargeBagsNumber);
                                                                    FirePropertyChanged(() => OrderPhone);
                                                                    FirePropertyChanged(() => OrderName);
                                                                    FirePropertyChanged(() => OrderApt);
                                                                    FirePropertyChanged(() => OrderRingCode);
                                                                    FirePropertyChanged(() => VehicleName);
                                                                    FirePropertyChanged(() => ChargeType);
                                                                });
                                                            }));
            }
        }



        public IMvxCommand ConfirmOrderCommand
        {
            get
            {

                return GetCommand(() => 
                    {


                        Order.Id = Guid.NewGuid ();
	    				try
						{
	    					MessageService.ShowProgress (true);
	    					var orderInfo = _bookingService.CreateOrder (Order);

						    if (!orderInfo.IBSOrderId.HasValue || !(orderInfo.IBSOrderId > 0)) return;

						    var orderCreated = new Order 
						        {
						            CreatedDate = DateTime.Now, 
						            DropOffAddress = Order.DropOffAddress, 
						            IBSOrderId = orderInfo.IBSOrderId, 
						            Id = Order.Id, PickupAddress = Order.PickupAddress,
						            Note = Order.Note, 
						            PickupDate = Order.PickupDate.HasValue ? Order.PickupDate.Value : DateTime.Now,
						            Settings = Order.Settings
						        };
	    						
						    RequestNavigate<BookingStatusViewModel>(new
						        {
						            order = orderCreated.ToJson(),
						            orderStatus = orderInfo.ToJson()
						        });	
						    Close();
						    MessengerHub.Publish(new OrderConfirmed(this, Order, false ));
						} 
						catch (Exception ex) 
						{
            				InvokeOnMainThread (() =>
            				{
            					var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
                                var err = string.Format (Resources.GetString ("ServiceError_ErrorCreatingOrderMessage"), settings.ApplicationName,Config.GetSetting( "DefaultPhoneNumberDisplay" ));
            					MessageService.ShowMessage (Resources.GetString ("ErrorCreatingOrderTitle"), err);
            				});
            			}
						finally {
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


        private async void ShowWarningIfNecessary()
        {
            var validationInfo = await _bookingService.ValidateOrder( Order );
            if ( validationInfo.HasWarning )
            {

                MessageService.ShowMessage(Resources.GetString("WarningTitle"), validationInfo.Message,  Resources.GetString("ContinueButton") , () => validationInfo.ToString(), Resources.GetString("CancelBoutton") , ()=> RequestClose ( this) );
            }
        }

        private bool _showEstimate
        {
            get
            {
                var ret = true;
                try
                {
                    ret = Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowEstimate"));
                }
                catch (Exception)
                {
                    return true;
                }
                return ret;
            }
        }

		private void ShowFareEstimateAlertDialogIfNecessary()
		{
            if (_showEstimate)
            {
                var estimateEnabled = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting<bool>("Client.ShowEstimateWarning", true);

                if (estimateEnabled &&
                    this.GetService<ICacheService>().Get<string>("WarningEstimateDontShow").IsNullOrEmpty() &&
                    Order.DropOffAddress.HasValidCoordinate())
                {
                    MessageService.ShowMessage(Resources.GetString("WarningEstimateTitle"), Resources.GetString("WarningEstimate"),
                        "Ok", delegate { },
                        Resources.GetString("WarningEstimateDontShow"), () => this.GetService<ICacheService>().Set("WarningEstimateDontShow", "yes"));
                }
            }
		}      

        public string AptRingCode
        {
            get
            {
                return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
            }
        }
        public string BuildingName
        {
            get
            {
                return FormatBuildingName(Order.PickupAddress.BuildingName);
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
            if (buildingName.HasValue())
            {
                return buildingName;
            }
            else
            {
                return Resources.GetString(Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
            }
        }

        private string FormatDateTime(DateTime? pickupDate)
        {
            var formatTime = new CultureInfo(CultureInfoString).DateTimeFormat.ShortTimePattern;
			string format = "{0:dddd, MMMM d}, {0:"+formatTime+"}";
            string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.GetString("TimeNow");
            return result;
        }

        private string FormatPrice(double? price)
        {
            if (price.HasValue)
            {
                var culture = ConfigurationManager.GetSetting("PriceFormat");
                return string.Format(new CultureInfo(culture), "{0:C}", price);
            }
            else
            {
                return "";
            }

        }

        public string CultureInfoString
        {
            get
            {
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("PriceFormat");
                if (culture.IsNullOrEmpty())
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

