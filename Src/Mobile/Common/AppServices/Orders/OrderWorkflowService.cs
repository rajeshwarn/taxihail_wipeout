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

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderWorkflowService: BaseService, IOrderWorkflowService
    {
		readonly AbstractLocationService _locationService;
		readonly IAccountService _accountService;
		readonly IGeolocService _geolocService;
		readonly ISubject<Address> _pickupAddressSubject = new BehaviorSubject<Address>(new Address());
		readonly IConfigurationManager _configurationManager;

		public OrderWorkflowService(AbstractLocationService locationService,
			IAccountService accountService,
			IGeolocService geolocService,
			IConfigurationManager configurationManager)
		{
			_configurationManager = configurationManager;
			_geolocService = geolocService;
			_accountService = accountService;
			_locationService = locationService;
        	
		}
		public async Task SetPickupAddressToUserLocation()
		{
            //CancelCurrentLocationCommand.Execute ();
			//TODO: Handle when location services are not available
			if (_locationService.BestPosition != null)
			{
				var address = await SearchAddressForCoordinate(_locationService.BestPosition);
				_pickupAddressSubject.OnNext(address);
				return;
			}

			//IsExecuting = true;
			var positionSet = false;

			_locationService.GetNextPosition(TimeSpan.FromSeconds(6), 50).Subscribe(
				async pos =>
				{
					positionSet = true;
					var address = await SearchAddressForCoordinate(pos);
					_pickupAddressSubject.OnNext(address);
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
								_pickupAddressSubject.OnNext(address);
							}
						}
					}
				});

		}

		public async Task ValidatePickupDestinationAndTime()
		{
			var pickupAddress = await _pickupAddressSubject.Take(1).ToTask();
			bool pickupIsValid = pickupAddress.BookAddress.HasValue()
			                     && pickupAddress.HasValidCoordinate();

			if (!pickupIsValid)
			{
				throw new OrderValidationException("Pickup address required", OrderValidationError.PickupAddressRequired);
			}

			bool destinationIsRequired = _configurationManager.GetSetting<bool>("Client.DestinationIsRequired", false);
			bool destinationIsValid = false;
			//TODO: Destination not implemented v0.01pre-alpha
			// info.DropOffAddress.BookAddress.HasValue () 
			//&& info.DropOffAddress.HasValidCoordinate ()

			if (destinationIsRequired && !destinationIsValid)
			{
				throw new OrderValidationException("Destination address required", OrderValidationError.DestinationAddressRequired);
			}

			bool pickupDateIsValid = true; // TODO: Not impletmented:  Order.PickupDate.HasValue && Order.PickupDate.Value < DateTime.Now

			if (!pickupDateIsValid)
			{
				throw new OrderValidationException("Invalid pickup date", OrderValidationError.InvalidPickupDate);
			}

		}

		public IObservable<Address> GetAndObservePickupAddress()
		{
			return _pickupAddressSubject;
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



        
    }
}

