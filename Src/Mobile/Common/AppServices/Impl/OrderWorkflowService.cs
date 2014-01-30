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
		public void SetPickupAddressToUserLocation()
		{
			//CancelCurrentLocationCommand.Execute ();
			//TODO: Handle when location services are not available
			if (_locationService.BestPosition != null)
			{
				SearchAddressForCoordinate(_locationService.BestPosition);
				return;
			}

			//IsExecuting = true;
			var positionSet = false;

			_locationService.GetNextPosition(TimeSpan.FromSeconds(6), 50).Subscribe(
				pos =>
				{
					positionSet = true;
					SearchAddressForCoordinate(pos);
				},
				() =>
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
								SearchAddressForCoordinate(_locationService.BestPosition);    
							}
						}


					}
				});

		}

		public IObservable<Address> GetAndObservePickupAddress()
		{
			return _pickupAddressSubject;
		}

		private async void SearchAddressForCoordinate(Position p)
		{
			//IsExecuting = true;
			Logger.LogMessage("Start Call SearchAddress : " + p.Latitude.ToString(CultureInfo.InvariantCulture) + ", " + p.Longitude.ToString(CultureInfo.InvariantCulture));

			var accountAddress = _accountService.FindInAccountAddresses(p.Latitude, p.Longitude);
			if (accountAddress != null)
			{
				//Logger.LogMessage("Address found in account");
				_pickupAddressSubject.OnNext(accountAddress);
			}
			else
			{
				var address = await Task.Run(() => _geolocService.SearchAddress(p.Latitude, p.Longitude));
				Logger.LogMessage("Call SearchAddress finsihed, found {0} addresses", address.Count());
				if (address.Any())
				{
					//Logger.LogMessage(" found {0} addresses", address.Count());
					_pickupAddressSubject.OnNext(address[0]);
					//SetAddress(address[0], false);
				}
				else
				{
					Logger.LogMessage(" clear addresses");
					// TODO: Clarify why we clear address here
					_pickupAddressSubject.OnNext(new Address());
					//ClearAddress();
				}
				Logger.LogMessage("Exiting SearchAddress thread");
			}

			//IsExecuting = false;
		}



        
    }
}

