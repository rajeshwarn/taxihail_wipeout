using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration;
using MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class UserTaxiHailNetworkSettingsViewModel : PageViewModel
    {
        private readonly IAccountService _accountService;
        private readonly INetworkRoamingService _networkRoamingService;

        public UserTaxiHailNetworkSettingsViewModel(IAccountService accountService, INetworkRoamingService networkRoamingService)
        {
            _accountService = accountService;
            _networkRoamingService = networkRoamingService;
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadUserTaxiHailNetworkSettings();
        }

        public override void OnViewStopped()
        {
            base.OnViewStopped();
            UpdateUserTaxiHailNetworkSettings();
        }

        public ObservableCollection<ToggleItem> UserTaxiHailNetworkSettings { get; set; }

        private bool _isTaxiHailNetworkEnabled;
        public bool IsTaxiHailNetworkEnabled
        {
            get { return _isTaxiHailNetworkEnabled; }
            set
            {
                _isTaxiHailNetworkEnabled = value;
                RaisePropertyChanged();
            }
        }

        private async void LoadUserTaxiHailNetworkSettings()
        {
            UserTaxiHailNetworkSettings = new ObservableCollection<ToggleItem>();

            var networkFleets = await _networkRoamingService.GetNetworkFleets();
            var settings = await _accountService.GetUserTaxiHailNetworkSettings();

            IsTaxiHailNetworkEnabled = settings.IsEnabled;

            foreach (var networkFleet in networkFleets)
            {
                UserTaxiHailNetworkSettings.Add(new ToggleItem
                {
                    Display = networkFleet.CompanyName,
                    Name = networkFleet.CompanyKey,
                    Value = !settings.DisabledFleets.Contains(networkFleet.CompanyKey)
                });
            }
        }

        private void UpdateUserTaxiHailNetworkSettings()
        {
            var disabledFleets = UserTaxiHailNetworkSettings.Where(s => !s.Value)
                .Select(s => s.Name)
                .ToArray();

            var updatedUserTaxiHailNetworkSettings = new UserTaxiHailNetworkSettings
            {
                IsEnabled = IsTaxiHailNetworkEnabled,
                DisabledFleets = disabledFleets
            };

            _accountService.UpdateUserTaxiHailNetworkSettings(updatedUserTaxiHailNetworkSettings);
        }
    }
}