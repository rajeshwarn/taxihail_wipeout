using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Subjects;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class OrderWorkflowService: BaseService, IOrderWorkflowService
    {
		readonly AbstractLocationService _locationService;
		readonly IAccountService _accountService;
		readonly IGeolocService _geolocService;
		readonly ISubject<Address> _pickupAddressSubject = new ReplaySubject<Address>(1);

		public OrderWorkflowService(AbstractLocationService locationService, IAccountService accountService, IGeolocService geolocService)
		{
			_geolocService = geolocService;
			_accountService = accountService;
			_locationService = locationService;
        	
		}
		public async Task SetPickupAddressToUserLocation()
		{
            await Task.Delay(20000);
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

