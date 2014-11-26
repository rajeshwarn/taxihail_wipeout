using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class UserTaxiHailNetworkSettingsViewModel : PageViewModel
    {
        private readonly IAccountService _accountService;

        public UserTaxiHailNetworkSettingsViewModel(IAccountService accountService)
        {
            _accountService = accountService;
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

            var userTaxiHailNetworkSettings = await _accountService.GetUserTaxiHailNetworkSettings();
            var type = userTaxiHailNetworkSettings.GetType();

            foreach (var setting in type.GetProperties())
            {
                if (setting.PropertyType == typeof(bool) || setting.PropertyType == typeof(bool?))
                {
                    var value = (bool?)setting.GetValue(userTaxiHailNetworkSettings);

                    // Special case for this property since it resides outside the MvxListView
                    if (setting.Name == "IsEnabled")
                    {
                        IsTaxiHailNetworkEnabled = value.Value;
                        continue;
                    }

                    // if the value is null, this means the company set the setting as "Not available"
                    // don't show it to the client
                    if (value.HasValue)
                    {
                        UserTaxiHailNetworkSettings.Add(new ToggleItem
                        {
                            Name = setting.Name,
                            Display = this.Services().Localize["Notification_" + setting.Name],
                            Value = value.Value
                        });
                    }
                }
            }
        }

        private void UpdateUserTaxiHailNetworkSettings()
        {
            
        }
    }
}