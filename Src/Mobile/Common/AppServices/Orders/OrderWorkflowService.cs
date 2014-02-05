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

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderWorkflowService: BaseService, IOrderWorkflowService
    {
		readonly AbstractLocationService _locationService;
		readonly IAccountService _accountService;
		readonly IGeolocService _geolocService;
		readonly IAppSettings _configurationManager;

		readonly ISubject<Address> _pickupAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<Address> _destinationAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly ISubject<AddressSelectionMode> _addressSelectionModeSubject = new BehaviorSubject<AddressSelectionMode>(AddressSelectionMode.PickupSelection);
		readonly ISubject<DateTime?> _pickupDateSubject = new BehaviorSubject<DateTime?>(null);
        readonly ISubject<BookingSettings> _bookingSettingsSubject;

		public OrderWorkflowService(AbstractLocationService locationService,
			IAccountService accountService,
			IGeolocService geolocService,
			IAppSettings configurationManager)
		{
			_configurationManager = configurationManager;
			_geolocService = geolocService;
			_accountService = accountService;
			_locationService = locationService;

			// TODO: Listen to account booking settings changes
			_bookingSettingsSubject = new BehaviorSubject<BookingSettings>(accountService.CurrentAccount.Settings);
		}

		public async Task SetAddressToUserLocation()
		{
            //CancelCurrentLocationCommand.Execute ();
			//TODO: Handle when location services are not available
			if (_locationService.BestPosition != null)
			{
				var address = await SearchAddressForCoordinate(_locationService.BestPosition);
				await SetAddressToCurrentSelection(address);

				return;
			}

			//IsExecuting = true;
			var positionSet = false;

			_locationService.GetNextPosition(TimeSpan.FromSeconds(6), 50).Subscribe(
				async pos =>
				{
					positionSet = true;
					var address = await SearchAddressForCoordinate(pos);
					await SetAddressToCurrentSelection(address);
				},
				async () =>
				{  
					positionSet = false;

					if (!positionSet)
					{
						{
							if (_locationService.BestPosition == null)
							{
								//this.Services().Message.ShowToast("Cant find location, please try again", ToastDuration.Short);
							}
							else
							{
								var address = await SearchAddressForCoordinate(_locationService.BestPosition);    
								await SetAddressToCurrentSelection(address);
							}
						}
					}
				}
			);
		}

		public async Task ClearDestinationAddress()
		{
			_destinationAddressSubject.OnNext(new Address());
		}

		public async Task SetPickupDate(DateTime? date)
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

			var destinationIsRequired = _configurationManager.Data.DestinationIsRequired;
			var destinationAddress = await _destinationAddressSubject.Take(1).ToTask();
			var destinationIsValid = destinationAddress.BookAddress.HasValue()
			                         && destinationAddress.HasValidCoordinate();

			if (destinationIsRequired && !destinationIsValid)
			{
				throw new OrderValidationException("Destination address required", OrderValidationError.DestinationAddressRequired);
			}

			var pickupDate = await _pickupDateSubject.Take(1).ToTask();
			bool pickupDateIsValid = !pickupDate.HasValue || (pickupDate.HasValue && pickupDate.Value >= DateTime.Now);

			if (!pickupDateIsValid)
			{
				throw new OrderValidationException("Invalid pickup date", OrderValidationError.InvalidPickupDate);
			}
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

		public IObservable<BookingSettings> GetAndObserveBookingSettings()
		{
			return _bookingSettingsSubject;
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
						// TODO: Clarify why we clear address here
						return new Address();
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
		}
    }
}

