using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class DebugViewModel : PageViewModel
	{
		private readonly IIPAddressManager _ipAddressManager;
		private readonly ILocationService _locationService;

		public DebugViewModel(IIPAddressManager ipAddressManager, ILocationService locationService)
		{
			_ipAddressManager = ipAddressManager;
			_locationService = locationService;

			DebugEntries = new ObservableCollection<string>();
		}

		public async void Init()
		{
			DebugEntries.AddRange(GetIpAddressesOutput());
			DebugEntries.AddRange(await GetGeolocOutput());
		}

		private ObservableCollection<string> _debugEntries;
		public ObservableCollection<string> DebugEntries
		{
			get { return _debugEntries; }
			set 
			{ 
				_debugEntries = value; 
				RaisePropertyChanged(); 
			}
		}

		private List<string> GetIpAddressesOutput()
		{
			var entries = new List<string>();

			entries.Add("IP Addresses");
		
			entries.Add(string.Format("\tCellular: {0}", _ipAddressManager.GetIPAddress()));

			entries.Add(string.Empty);

			return entries;
		}

		private async Task<List<string>> GetGeolocOutput()
		{
			var entries = new List<string>();

			entries.Add("Geoloc");

			var address = await _locationService.GetUserPosition();
			entries.Add(string.Format("\t{0}", address.Latitude));
			entries.Add(string.Format("\t{0}", address.Longitude));

			entries.Add(string.Empty);

			return entries;
		}
	}
}
