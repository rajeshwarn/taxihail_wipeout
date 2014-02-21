using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;
using ServiceStack.ServiceClient.Web;

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

		readonly ISubject<Address> _pickupAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<Address> _destinationAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<AddressSelectionMode> _addressSelectionModeSubject = new BehaviorSubject<AddressSelectionMode>(AddressSelectionMode.PickupSelection);
		readonly ISubject<DateTime?> _pickupDateSubject = new BehaviorSubject<DateTime?>(null);
        readonly ISubject<BookingSettings> _bookingSettingsSubject;
		readonly ISubject<string> _estimatedFareSubject;
		readonly ISubject<string> _noteToDriverSubject = new BehaviorSubject<string>(string.Empty);

		public OrderWorkflowService(ILocationService locationService,
			IAccountService accountService,
			IGeolocService geolocService,
			IAppSettings configurationManager,
			ILocalization localize,
			IBookingService bookingService,
			ICacheService cacheService)
		{
			_cacheService = cacheService;
			_appSettings = configurationManager;
			_geolocService = geolocService;
			_accountService = accountService;
			_locationService = locationService;

			_bookingSettingsSubject = new BehaviorSubject<BookingSettings>(accountService.CurrentAccount.Settings);
			_localize = localize;
			_bookingService = bookingService;

			_estimatedFareSubject = new BehaviorSubject<string>(_localize["NoFareText"]);

		}

		public async Task SetAddress(Address address)
		{
			await SetAddressToCurrentSelection(address);
		}

		public async Task<Address> SetAddressToUserLocation()
		{
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

		public void ClearDestinationAddress()
		{
			_destinationAddressSubject.OnNext(new Address());
		}

		public void SetPickupDate(DateTime? date)
		{
			_pickupDateSubject.OnNext(date);
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

		public async Task ValidatePickupDestinationAndTime()
		{
			var pickupAddress = await _pickupAddressSubject.Take(1).ToTask();
			var pickupIsValid = pickupAddress.BookAddress.HasValue()
			                     && pickupAddress.HasValidCoordinate();

			if (!pickupIsValid)
			{
				throw new OrderValidationException("Pickup address required", OrderValidationError.PickupAddressRequired);
			}

			var destinationIsRequired = _appSettings.Data.DestinationIsRequired;
			var destinationAddress = await _destinationAddressSubject.Take(1).ToTask();
			var destinationIsValid = destinationAddress.BookAddress.HasValue()
			                         && destinationAddress.HasValidCoordinate();

			if (destinationIsRequired && !destinationIsValid)
			{
				throw new OrderValidationException("Destination address required", OrderValidationError.DestinationAddressRequired);
			}

			var pickupDate = await _pickupDateSubject.Take(1).ToTask();
			bool pickupDateIsValid = !pickupDate.HasValue || (pickupDate.HasValue && pickupDate.Value.ToUniversalTime() >= DateTime.UtcNow);

			if (!pickupDateIsValid)
			{
				throw new OrderValidationException("Invalid pickup date", OrderValidationError.InvalidPickupDate);
			}
		}

		public async Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder()
		{
			bool callIsEnabled = !_appSettings.Data.HideCallDispatchButton;

			CreateOrder order = await GetOrder();

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
				string message = callIsEnabled
					? _localize["ServiceError_ErrorCreatingOrderMessage"]
					: _localize["ServiceError_ErrorCreatingOrderMessage_NoCall"];

				if (e.ErrorCode == ErrorCode.CreateOrder_RuleDisable.ToString())
				{
					// Order creation is temporarily disabled by rules
					message = e.Message;
				}
				else
				{
					// Miscellaneous error
					var messageKey = "ServiceError" + e.ErrorCode;
					if (_localize.Exists(messageKey))
					{
						// Message when call is enabled
						message = _localize[messageKey];
					}
						
					messageKey += "_NoCall";
					if (!callIsEnabled
						&& _localize.Exists(messageKey))
					{
						// messasge when call is disabled
						message = _localize[messageKey];
					}
				}

				if (callIsEnabled)
				{
					message = string.Format(message, _appSettings.Data.ApplicationName, _appSettings.Data.DefaultPhoneNumberDisplay);
				}

				throw new OrderCreationException(message);
			}
		}

		public void SetBookingSettings(BookingSettings bookingSettings)
		{
			_bookingSettingsSubject.OnNext(bookingSettings);
		}

		public void SetPickupAddress(Address address)
		{
			_pickupAddressSubject.OnNext(address);
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

		public IObservable<BookingSettings> GetAndObserveBookingSettings()
		{
			return _bookingSettingsSubject;
		}

		public IObservable<string> GetAndObserveEstimatedFare()
		{
			return _estimatedFareSubject;
		}

		public IObservable<string> GetAndObserveNoteToDriver()
		{
			return _noteToDriverSubject;
		}

		public IObservable<DateTime?> GetAndObservePickupDate()
		{
			return _pickupDateSubject;
		}
		
		private async Task<Address> SearchAddressForCoordinate(Position p)
		{
			//IsExecuting = true;
			using (Logger.StartStopwatch("SearchAddress : " + p.Latitude.ToString(CultureInfo.InvariantCulture) + ", " + p.Longitude.ToString(CultureInfo.InvariantCulture)))
			{
				var accountAddress = _accountService.FindInAccountAddresses(p.Latitude, p.Longitude);
				if (accountAddress != null)
				{
					Logger.LogMessage("Address found in account");
					return accountAddress;
				}
				else
				{
					var address = await Task.Run(() => _geolocService.SearchAddress(p.Latitude, p.Longitude));
					Logger.LogMessage("Found {0} addresses", address.Count());
					if (address.Any())
					{
						return address[0];
					}
					else
					{
						Logger.LogMessage("clear addresses");

						// TODO: Refactor. We should probably throw an exception here.
						// Error should be handled by the caller.
						return new Address(){ Latitude = p.Latitude, Longitude = p.Longitude };
					}
				}
			}

			//IsExecuting = false;
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

            var estimatedFareString = await _bookingService.GetFareEstimateDisplay(pickupAddress, destinationAddress, pickupDate, "EstimatePriceFormat", "NoFareText", true, "EstimatedFareNotAvailable");

            _estimatedFareSubject.OnNext(estimatedFareString);
		}

		private void PrepareForNewOrder()
		{
			_noteToDriverSubject.OnNext(string.Empty);
			_pickupAddressSubject.OnNext(new Address());
			_destinationAddressSubject.OnNext(new Address());
			_addressSelectionModeSubject.OnNext(AddressSelectionMode.PickupSelection);
			_pickupDateSubject.OnNext(null);
			_bookingSettingsSubject.OnNext(_accountService.CurrentAccount.Settings);
			_estimatedFareSubject.OnNext(_localize["NoFareText"]);
		}

		public void ResetOrderSettings()
		{
			_noteToDriverSubject.OnNext(string.Empty);
			_bookingSettingsSubject.OnNext(_accountService.CurrentAccount.Settings);
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

		public async Task<OrderValidationResult> ValidateOrder()
		{
			var orderToValidate = await GetOrder();
			var validationResult = await _bookingService.ValidateOrder(orderToValidate);
			return validationResult;
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

			return order;
		}

		public void Rebook(Order previous)
		{
			_pickupAddressSubject.OnNext(previous.PickupAddress);
			_destinationAddressSubject.OnNext(previous.DropOffAddress);
			_bookingSettingsSubject.OnNext(previous.Settings);
			_noteToDriverSubject.OnNext(previous.Note);
		}
    }
}

