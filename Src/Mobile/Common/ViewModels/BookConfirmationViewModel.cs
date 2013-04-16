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
		public BookConfirmationViewModel (string order)
        {
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);	
			Order.Settings = AccountService.CurrentAccount.Settings;
        }

        public override void Load ()
        {
			base.Load ();

            Console.WriteLine("Opening confirmation view....");

			MessageService.ShowProgress(true);


			Task.Factory.StartNew<RideSettingsModel>(() => new RideSettingsModel(Order.Settings, AccountService.GetCompaniesList(), AccountService.GetVehiclesList(), AccountService.GetPaymentsList()))
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
                            	ShowChooseProviderDialogIfNecessary();                            
                            	ShowWarningIfNecessary();
							}

                        });
                    });


            Task.Factory.StartNew<string>(() => BookingService.GetFareEstimateDisplay(Order, null, "NotAvailable", false, "NotAvailable"))
                .HandleErrors()
                .ContinueWith(t => InvokeOnMainThread(() =>
                        {
                            FareEstimate = t.Result;
                            ShowFareEstimateAlertDialogIfNecessary();
                        }));




            Console.WriteLine("Done opening confirmation view....");

        }
        private string _fareEstimate;
        public string FareEstimate
        {
            get
            {
                return _fareEstimate;
            }
            set
            {
                if (value != _fareEstimate)
                {
                    _fareEstimate = value;
                    FirePropertyChanged(()=>FareEstimate);
                }
            }
        }
      

        public string VehicleName 
		{
            get { return RideSettings != null  ? RideSettings.VehicleTypeName : null; }
        }

        public string ChargeType
		{
            get { return RideSettings != null  ? RideSettings.ChargeTypeName : null;  }
        }

		public CreateOrder Order { get; private set; }

        public string OrderPassengerNumber
        {
            get { return Order.Settings.Passengers.ToString(); }
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
            get { return Config.Client.ShowPassengerName.GetValueOrDefault(); }
        }

        public bool ShowPassengerPhone
        {
			get { return Config.Client.ShowPassengerPhone.GetValueOrDefault(); }
        }

        public bool ShowPassengerNumber
		{
			get { return Config.Client.ShowPassengerNumber.GetValueOrDefault(); }
        }

		public string AptRingCode
		{
			get	{ return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode); }
		}
		private string FormatAptRingCode(string apt, string rCode)
		{
			string result = apt.HasValue() ? apt : Resources.GetString("ConfirmNoApt");
			result += @" / ";
			result += rCode.HasValue() ? rCode : Resources.GetString("ConfirmNoRingCode");
			return result;
		}

		public string BuildingName
		{
			get	
			{
				var buildingName = Order.PickupAddress.BuildingName;

				if (buildingName.HasValue()) {	return buildingName; }
				else { return Resources.GetString(Str.HistoryDetailBuildingNameNotSpecified); }
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
					FirePropertyChanged(()=>RideSettings);
				}
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
								FirePropertyChanged(()=>AptRingCode);
								FirePropertyChanged(()=>BuildingName);
							});
						}
					});

				});
			}
        }
        public IMvxCommand NavigateToEditInformations
        {
            get
            {
                return GetCommand(() => RequestSubNavigate<BookEditInformationViewModel, Order>(new { order = Order.ToJson() }.ToSimplePropertyDictionary(), result =>
                    {
                        if (result != null)
                        {
                            Order.PickupAddress.Apartment = result.PickupAddress.Apartment;
                            Order.PickupAddress.RingCode = result.PickupAddress.RingCode;
                            Order.PickupAddress.BuildingName = result.PickupAddress.BuildingName;
                            Order.Settings.Name = result.Settings.Name;
                            Order.Settings.VehicleTypeId = result.Settings.VehicleTypeId;
                            Order.Settings.ChargeTypeId = result.Settings.ChargeTypeId;
                            Order.Settings.Phone = result.Settings.Phone;
                            Order.Settings.Passengers = result.Settings.Passengers;
                            InvokeOnMainThread(() =>
                           {
                               FirePropertyChanged(()=>AptRingCode);
                               FirePropertyChanged(()=>BuildingName);
                               FirePropertyChanged(() => OrderPassengerNumber);
                               FirePropertyChanged(() => OrderPhone);
                               FirePropertyChanged(() => OrderName);
                               FirePropertyChanged(() => OrderApt);
                               FirePropertyChanged(() => OrderRingCode);
                               FirePropertyChanged(() => VehicleName);
                               FirePropertyChanged(() => ChargeType);
                           });
                        }
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
						var orderInfo = BookingService.CreateOrder (Order);
    					
    					if (orderInfo.IBSOrderId.HasValue && orderInfo.IBSOrderId > 0) 
						{
    						var orderCreated = new Order 
							{ 
								CreatedDate = DateTime.Now, 
								DropOffAddress = Order.DropOffAddress, 
								IBSOrderId = orderInfo.IBSOrderId, 
								Id = Order.Id, 
								PickupAddress = Order.PickupAddress, 
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
    				} 
					catch (Exception) 
					{
    					InvokeOnMainThread (() =>
    					{
    						var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
    						string err = Str.GetServiceErrorCreatingOrderMessage((Order.Settings.ProviderId.HasValue ? Order.Settings.ProviderId.Value : 1));
    						MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, err);
    					});
    				} 
					finally
					{
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


        private void ShowWarningIfNecessary()
        {
			var validationInfo = BookingService.ValidateOrder( Order );
            if ( validationInfo.HasWarning )
            {

                MessageService.ShowMessage(Str.WarningTitle, validationInfo.Message,  Str.ContinueButtonText , () => validationInfo.ToString(), Str.CancelButtonText , ()=> RequestClose ( this) );
            }
        }


		private void ShowFareEstimateAlertDialogIfNecessary()
		{
			if (Config.Client.ShowEstimate.GetValueOrDefault())
            {
                if (this.GetService<ICacheService>().Get<string>("WarningEstimateDontShow").IsNullOrEmpty()
				    && Order.DropOffAddress.HasValidCoordinate())
                {
                    MessageService.ShowMessage(Str.WarningEstimateTitle, Str.WarningEstimate, 
                        "Ok", delegate { },
						Str.WarningEstimateDontShow,
						() => this.GetService<ICacheService>().Set("WarningEstimateDontShow", "yes"));
                }
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
                        FirePropertyChanged(()=>RideSettings);
					}

                    this.GetService<IAccountService>().UpdateSettings(Order.Settings, AccountService.CurrentAccount.DefaultCreditCard, AccountService.CurrentAccount.DefaultTipPercent );
				});
			}
            else if(Order.Settings.ProviderId == null)
            {
                Order.Settings.ProviderId = RideSettings.ProviderId;
            }
		}



     

    }
}

