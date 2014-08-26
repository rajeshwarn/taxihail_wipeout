using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.Text;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderWorkflowService: BaseService, IOrderWorkflowService
    {
		readonly ILocationService _locationService;
		readonly IAccountService _accountService;
		readonly IGeolocService _geolocService;
		readonly IAppSettings _appSettings;
		readonly ILocalization _localize;
		readonly IBookingService _bookingService;
		readonly ICacheService _cacheService;
		readonly IAccountPaymentService _accountPaymentService;

		readonly ISubject<Address> _pickupAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<Address> _destinationAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<AddressSelectionMode> _addressSelectionModeSubject = new BehaviorSubject<AddressSelectionMode>(AddressSelectionMode.PickupSelection);
		readonly ISubject<DateTime?> _pickupDateSubject = new BehaviorSubject<DateTime?>(null);
		readonly ISubject<int?> _vehicleTypeSubject;
        readonly ISubject<BookingSettings> _bookingSettingsSubject;
		readonly ISubject<string> _estimatedFareDisplaySubject;
		readonly ISubject<DirectionInfo> _estimatedFareDetailSubject = new BehaviorSubject<DirectionInfo>( new DirectionInfo() );
		readonly ISubject<string> _noteToDriverSubject = new BehaviorSubject<string>(string.Empty);
		readonly ISubject<bool> _loadingAddressSubject = new BehaviorSubject<bool>(false);
		readonly ISubject<AccountChargeQuestion[]> _accountPaymentQuestions = new BehaviorSubject<AccountChargeQuestion[]> (null);

		readonly ISubject<bool> _orderCanBeConfirmed = new BehaviorSubject<bool>(false);

        private bool _isOrderRebooked;

		public OrderWorkflowService(ILocationService locationService,
			IAccountService accountService,
			IGeolocService geolocService,
			IAppSettings configurationManager,
			ILocalization localize,
			IBookingService bookingService,
			ICacheService cacheService,
			IAccountPaymentService accountPaymentService)
		{
			_cacheService = cacheService;
			_appSettings = configurationManager;
			_geolocService = geolocService;
			_accountService = accountService;
			_locationService = locationService;

			_bookingSettingsSubject = new BehaviorSubject<BookingSettings>(accountService.CurrentAccount.Settings);
			_vehicleTypeSubject = new BehaviorSubject<int?> (accountService.CurrentAccount.Settings.VehicleTypeId);
			_localize = localize;
			_bookingService = bookingService;
			_accountPaymentService = accountPaymentService;

			_estimatedFareDisplaySubject = new BehaviorSubject<string>(_localize[_appSettings.Data.DestinationIsRequired ? "NoFareTextIfDestinationIsRequired" : "NoFareText"]);
		}

		public async Task SetAddress(Address address)
		{
			await SetAddressToCurrentSelection(address);
		}

		public async Task<Address> SetAddressToUserLocation()
		{
			_loadingAddressSubject.OnNext(true);
			// TODO: Location service refactoring is needed in order
			// to simplify this method

			// TODO: Handle when location services are not available

			if (_locationService.BestPosition != null)
			{
				var address = await SearchAddressForCoordinate(_locationService.BestPosition);
				await SetAddressToCurrentSelection(address);

				return address;
			}

			var position = await _locationService
				.GetNextPosition(TimeSpan.FromSeconds(6), 50)
				.Take(1)
				.DefaultIfEmpty() // Will return null in case of a timeout
				.ToTask();

			if (position != null)
			{
				var address = await SearchAddressForCoordinate(position);
				await SetAddressToCurrentSelection(address);
				return address;
			}

			if (_locationService.BestPosition == null)
			{
				//this.Services().Message.ShowToast("Cant find location, please try again", ToastDuration.Short);
				_loadingAddressSubject.OnNext(false);
				return new Address();
			}
			else
			{
				var address = await SearchAddressForCoordinate(_locationService.BestPosition);    
				await SetAddressToCurrentSelection(address);
				return address;
			}
		}

        public async Task SetAddressToCoordinate(Position coordinate, CancellationToken cancellationToken)
		{
            var address = await SearchAddressForCoordinate(coordinate);
			address.Latitude = coordinate.Latitude;
			address.Longitude = coordinate.Longitude;
			cancellationToken.ThrowIfCancellationRequested();
			await SetAddressToCurrentSelection(address);
		}

		public async Task ClearDestinationAddress()
		{
			_destinationAddressSubject.OnNext(new Address());

			await CalculateEstimatedFare();
		}

		public async Task SetPickupDate(DateTime? date)
		{
			_pickupDateSubject.OnNext(date);

			await CalculateEstimatedFare();
		}

		public async Task ToggleBetweenPickupAndDestinationSelectionMode()
		{
			var currentSelectionMode = await _addressSelectionModeSubject.Take(1).ToTask();
			if (currentSelectionMode == AddressSelectionMode.PickupSelection)
			{
				_addressSelectionModeSubject.OnNext(AddressSelectionMode.DropoffSelection);
			}
			else
			{
				_addressSelectionModeSubject.OnNext(AddressSelectionMode.PickupSelection);
			}
		}

		public async Task ValidatePickupTime()
		{
			var pickupDate = await _pickupDateSubject.Take(1).ToTask();
			var pickupDateIsValid = !pickupDate.HasValue || (pickupDate.Value.ToUniversalTime() >= DateTime.UtcNow);

			if (!pickupDateIsValid)
			{
				throw new OrderValidationException("Invalid pickup date", OrderValidationError.InvalidPickupDate);
			}
		}

		public async Task ValidatePickupAndDestination()
		{
			var pickupAddress = await _pickupAddressSubject.Take(1).ToTask();
			var pickupIsValid = pickupAddress.BookAddress.HasValue()
				&& pickupAddress.HasValidCoordinate();

			if (!pickupIsValid)
			{
				throw new OrderValidationException("Pickup address required", OrderValidationError.PickupAddressRequired);
			}

			var destinationIsRequired = _appSettings.Data.DestinationIsRequired;

			if (destinationIsRequired)
			{
				var currentSelectionMode = await _addressSelectionModeSubject.Take(1).ToTask();
				if (currentSelectionMode == AddressSelectionMode.PickupSelection)
				{
					await ToggleBetweenPickupAndDestinationSelectionMode();
					throw new OrderValidationException("Open the destination selection", OrderValidationError.OpenDestinationSelection);
				}

				var destinationAddress = await _destinationAddressSubject.Take(1).ToTask();
				var destinationIsValid = destinationAddress.BookAddress.HasValue()
					&& destinationAddress.HasValidCoordinate();

				if (!destinationIsValid)
				{
					throw new OrderValidationException("Destination address required", OrderValidationError.DestinationAddressRequired);
				}
			}
		}

		public async Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder()
		{
		    _isOrderRebooked = false;

			var order = await GetOrder();

			try
			{
				var orderStatus = await _bookingService.CreateOrder(order);

				var orderCreated = new Order
				{
					CreatedDate = DateTime.Now, 
					DropOffAddress = order.DropOffAddress, 
					IBSOrderId = orderStatus.IBSOrderId, 
					Id = order.Id, PickupAddress = order.PickupAddress,
					Note = order.Note, 
					PickupDate = order.PickupDate.HasValue ? order.PickupDate.Value : DateTime.Now,
					Settings = order.Settings
				};

				PrepareForNewOrder();

				// TODO: Refactor so we don't have to return two distinct objects
				return Tuple.Create(orderCreated, orderStatus);
			}
			catch(WebServiceException e)
			{
				var message = "";
				var messageNoCall = "";

				switch (e.ErrorCode)
				{
					case "CreateOrder_PendingOrder":
						// Quick workaround for a bug in service stack where the response is not properly deserialized
						var cancelOrderError = e.ResponseBody.FromJson<ErrorResponse> ();
						if (cancelOrderError.ResponseStatus != null) {
							string pendingOrderId = cancelOrderError.ResponseStatus.Message;
							throw new OrderCreationException (e.ErrorCode, pendingOrderId);
						} 
						else 
						{
							goto default;
						}
					case "CreateOrder_RuleDisable":
						// Exception message comes from Rules admin tool, already localized
						// Quick workaround for a bug in service stack where the response is not properly deserialized
						var error = e.ResponseBody.FromJson<ErrorResponse>();
						if (error.ResponseStatus != null)
						{
							throw new OrderCreationException(error.ResponseStatus.Message, error.ResponseStatus.Message);
						}
						else
						{
							goto default;
						}
					case "CreateOrder_InvalidProvider":
					case "CreateOrder_NoFareEstimateAvailable": /* Fare estimate is required and was not submitted */
					case "CreateOrder_CannotCreateInIbs_1002": /* Pickup address outside of service area */
					case "CreateOrder_CannotCreateInIbs_7000": /* Inactive account */
					case "CreateOrder_CannotCreateInIbs_10000": /* Inactive charge account */
						message = string.Format(_localize["ServiceError" + e.ErrorCode], _appSettings.Data.ApplicationName, _appSettings.Data.DefaultPhoneNumberDisplay);
						messageNoCall = _localize["ServiceError" + e.ErrorCode + "_NoCall"];
						throw new OrderCreationException(message, messageNoCall);
					case "CreateOrder_CannotCreateInIbs_3000": /* Disabled account */
						message = string.Format(_localize["AccountDisabled"], _appSettings.Data.ApplicationName, _appSettings.Data.DefaultPhoneNumberDisplay);
						messageNoCall = _localize["AccountDisabled_NoCall"];
						throw new OrderCreationException(message, messageNoCall);
					default:
						// Unhandled errors
						// if ibs3000, there's a problem with the account, use a different one
						message = string.Format(_localize["ServiceError_ErrorCreatingOrderMessage"], _appSettings.Data.ApplicationName, _appSettings.Data.DefaultPhoneNumberDisplay);
						messageNoCall = _localize["ServiceError_ErrorCreatingOrderMessage_NoCall"];
						throw new OrderCreationException(message, messageNoCall);

				}
			}
		}

		public async Task SetVehicleType(int? vehicleTypeId)
		{
			_vehicleTypeSubject.OnNext(vehicleTypeId);

			var bookingSettings = await _bookingSettingsSubject.Take (1).ToTask ();
			bookingSettings.VehicleTypeId = vehicleTypeId;

			await SetBookingSettings (bookingSettings);
		}

		public async Task SetBookingSettings(BookingSettings bookingSettings)
		{
			// Get the vehicle type selected on the home screen and put them in the 
			// bookingsettings to prevent the default vehicle type to override the selected value
			var vehicleTypeId = await _vehicleTypeSubject.Take (1).ToTask ();
			bookingSettings.VehicleTypeId = vehicleTypeId;

			_bookingSettingsSubject.OnNext(bookingSettings);
		}

		public async Task SetPickupAptAndRingCode(string apt, string ringCode)
		{
			var address = await _pickupAddressSubject.Take(1).ToTask();
			address.Apartment = apt;
			address.RingCode = ringCode;
			_pickupAddressSubject.OnNext(address);
		}

        public async Task<Tuple<Order, OrderStatusDetail>> GetLastActiveOrder()
		{
			if (_bookingService.HasLastOrder) 
			{
				var status = await _bookingService.GetLastOrderStatus (); 
				if (!_bookingService.IsStatusCompleted (status.IBSStatusId)) 
				{
					var order = await _accountService.GetHistoryOrderAsync (status.OrderId);

                    return Tuple.Create(order, status);
				}
				else
				{
					_bookingService.ClearLastOrder();
				}
			}

			return null;
		}

		public IObservable<Address> GetAndObservePickupAddress()
		{
			return _pickupAddressSubject;
		}

		public IObservable<Address> GetAndObserveDestinationAddress()
		{
			return _destinationAddressSubject;
		}

		public IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode()
		{
			return _addressSelectionModeSubject;
		}

		public async Task<Address> GetCurrentAddress()
		{
			var currentSelectionMode = await _addressSelectionModeSubject.Take(1).ToTask();
			if (currentSelectionMode == AddressSelectionMode.PickupSelection)
			{
				return await _pickupAddressSubject.Take(1).ToTask();
			}
			else
			{
				return await _destinationAddressSubject.Take(1).ToTask();
			}
		}

		public IObservable<int?> GetAndObserveVehicleType()
		{
			return _vehicleTypeSubject;
		}

		public IObservable<BookingSettings> GetAndObserveBookingSettings()
		{
			return _bookingSettingsSubject;
		}

		public IObservable<string> GetAndObserveEstimatedFare()
		{
			return _estimatedFareDisplaySubject;
		}

		public IObservable<string> GetAndObserveNoteToDriver()
		{
			return _noteToDriverSubject;
		}

		public IObservable<bool> GetAndObserveOrderCanBeConfirmed()
		{
			return _orderCanBeConfirmed;
		}

		public IObservable<DateTime?> GetAndObservePickupDate()
		{
			return _pickupDateSubject;
		}

		public IObservable<bool> GetAndObserveLoadingAddress()
		{
			return _loadingAddressSubject;
		}
		
		private async Task<Address> SearchAddressForCoordinate(Position p)
		{
			_loadingAddressSubject.OnNext(true);
			using (Logger.StartStopwatch("SearchAddress : " + p.Latitude.ToString(CultureInfo.InvariantCulture) + ", " + p.Longitude.ToString(CultureInfo.InvariantCulture)))
			{
				var accountAddress = await _accountService
					.FindInAccountAddresses(p.Latitude, p.Longitude)
					.ConfigureAwait(false);

				if (accountAddress != null)
				{
					Logger.LogMessage("Address found in account");
					_loadingAddressSubject.OnNext(false);
					return accountAddress;
				}
				else
				{
					var address = await Task.Run(() => _geolocService.SearchAddress(p.Latitude, p.Longitude));
					Logger.LogMessage("Found {0} addresses", address.Count());
					if (address.Any())
					{
						_loadingAddressSubject.OnNext(false);
						return address[0];
					}
					else
					{
						Logger.LogMessage("clear addresses");

						// TODO: Refactor. We should probably throw an exception here.
						// Error should be handled by the caller.
						_loadingAddressSubject.OnNext(false);
						return new Address(){ Latitude = p.Latitude, Longitude = p.Longitude };
					}
				}
			}
		}

		private async Task SetAddressToCurrentSelection(Address address)
		{
			var selectionMode = await _addressSelectionModeSubject.Take(1).ToTask();
			if (selectionMode == AddressSelectionMode.PickupSelection)
			{
				_pickupAddressSubject.OnNext(address);
			}
			else
			{
				_destinationAddressSubject.OnNext(address);
			}

            await CalculateEstimatedFare();
		}

		private async Task CalculateEstimatedFare()
		{
			var pickupAddress = await _pickupAddressSubject.Take(1).ToTask();
			var destinationAddress = await _destinationAddressSubject.Take(1).ToTask();
			var pickupDate = await _pickupDateSubject.Take(1).ToTask();
		    var bookingSettings = await _bookingSettingsSubject.Take(1).ToTask();

            var direction = await _bookingService.GetFareEstimate(pickupAddress, destinationAddress, bookingSettings.VehicleTypeId, pickupDate);
			var estimatedFareString = _bookingService.GetFareEstimateDisplay(direction);

			_estimatedFareDetailSubject.OnNext (direction);
			_estimatedFareDisplaySubject.OnNext(estimatedFareString);
		}

		public async Task PrepareForNewOrder()
		{
			_noteToDriverSubject.OnNext(string.Empty);
			_pickupAddressSubject.OnNext(new Address());
			_destinationAddressSubject.OnNext(new Address());
			_addressSelectionModeSubject.OnNext(AddressSelectionMode.PickupSelection);
			_pickupDateSubject.OnNext(null);
			await SetBookingSettings (_accountService.CurrentAccount.Settings);
			_estimatedFareDisplaySubject.OnNext(_localize[_appSettings.Data.DestinationIsRequired ? "NoFareTextIfDestinationIsRequired" : "NoFareText"]);
			_orderCanBeConfirmed.OnNext (false);
		}

		public void BeginCreateOrder()
		{
			_orderCanBeConfirmed.OnNext (false);
		}

		public void EndCreateOrder()
		{
			Observable.Timer( TimeSpan.FromSeconds( 1 )).Subscribe( _ => _orderCanBeConfirmed.OnNext (true));
		}

		public async Task ResetOrderSettings()
		{
			_noteToDriverSubject.OnNext(string.Empty);
			await SetBookingSettings(_accountService.CurrentAccount.Settings);
		}

		public void SetNoteToDriver(string text)
		{
			_noteToDriverSubject.OnNext(text);
		}

		public async Task<bool> ShouldWarnAboutEstimate()
		{
			var destination = await _destinationAddressSubject.Take(1).ToTask();
			return _appSettings.Data.ShowEstimateWarning
					&& !_cacheService.Get<string>("WarningEstimateDontShow").HasValue()
					&& destination.HasValidCoordinate();
		}

		public async Task<bool> ShouldGoToAccountNumberFlow()
		{
			var settings = await _bookingSettingsSubject.Take(1).ToTask();
            return settings.ChargeTypeId == ChargeTypes.Account.Id;
		}

		public async Task<bool> ValidateAccountNumberAndPrepareQuestions(string accountNumber = null)
		{
			if (accountNumber == null)
			{
				var settings = await _bookingSettingsSubject.Take(1).ToTask();
				accountNumber = settings.AccountNumber;
			}

			if (!accountNumber.HasValue ())
			{
				return false;
			}

			try
			{
				var questions = await _accountPaymentService.GetQuestions (accountNumber);
				_accountPaymentQuestions.OnNext (questions);

				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool ValidateAndSaveAccountAnswers(AccountChargeQuestion[] questionsAndAnswers)
		{
			if (questionsAndAnswers.Any (x => x.IsEnabled
										&& x.IsRequired
										&& string.IsNullOrEmpty (x.Answer)))
			{
				return false;
			}

			if (questionsAndAnswers.Any(x => x.IsEnabled
				&& x.MaxLength != null
				&& !string.IsNullOrEmpty(x.Answer)
				&& x.Answer.Length > x.MaxLength))
			{
				return false;
			}

			_accountPaymentQuestions.OnNext (questionsAndAnswers);

			return true;
		}

		public async Task SetAccountNumber(string accountNumber)
		{
			_accountService.UpdateAccountNumber (accountNumber);

			var bookingSettings = await _bookingSettingsSubject.Take(1).ToTask();
			bookingSettings.AccountNumber = accountNumber;

			await SetBookingSettings (bookingSettings);
		}

		public async Task<AccountChargeQuestion[]> GetAccountPaymentQuestions()
		{
			return await _accountPaymentQuestions.Take (1).ToTask ();
		}

		public async Task<OrderValidationResult> ValidateOrder()
		{
			var orderToValidate = await GetOrder();
			var validationResult = await _bookingService.ValidateOrder(orderToValidate);
			return validationResult;
		}

		public async Task<bool> ValidateCardOnFile()
		{
			if (this._appSettings.Data.CreditCardChargeTypeId.HasValue) 
			{
				var orderToValidate = await GetOrder ();	
				if ( (orderToValidate.Settings.ChargeTypeId == _appSettings.Data.CreditCardChargeTypeId.Value)  &&
					(!_accountService.CurrentAccount.DefaultCreditCard.HasValue ))
				{
					return false;
				}
			}

			return true;
		}




		public void ConfirmValidationOrder()
		{
			_orderCanBeConfirmed.OnNext (true);
		}

		private async Task<CreateOrder> GetOrder()
		{
			var order = new CreateOrder();
			order.Id = Guid.NewGuid();
			order.PickupDate = await _pickupDateSubject.Take(1).ToTask();
			order.PickupAddress = await _pickupAddressSubject.Take(1).ToTask();
			order.DropOffAddress = await _destinationAddressSubject.Take(1).ToTask();
			order.Settings = await _bookingSettingsSubject.Take(1).ToTask();
			order.Note = await _noteToDriverSubject.Take(1).ToTask();
			var e = await _estimatedFareDetailSubject.Take (1).ToTask ();
			if (e != null) {
				order.Estimate = new CreateOrder.RideEstimate{ Price = e.Price, Distance = e.Distance.HasValue ? e.Distance.Value :0  };
			}

		    order.UserLatitude = _locationService.BestPosition != null
		        ? _locationService.BestPosition.Latitude
		        : (double?) null;
            order.UserLongitude = _locationService.BestPosition != null
                ? _locationService.BestPosition.Longitude
                : (double?)null;

			order.QuestionsAndAnswers = await _accountPaymentQuestions.Take (1).ToTask ();

			return order;
		}

		public void Rebook(Order previous)
		{
            _isOrderRebooked = true;
			_pickupAddressSubject.OnNext(previous.PickupAddress);
			_destinationAddressSubject.OnNext(previous.DropOffAddress);
			_bookingSettingsSubject.OnNext(previous.Settings);
			_noteToDriverSubject.OnNext(previous.Note);
		}

        public bool IsOrderRebooked()
        {
            return _isOrderRebooked;
        }

	    public void CancelRebookOrder()
	    {
	        _isOrderRebooked = false;
	    }
    }
}

