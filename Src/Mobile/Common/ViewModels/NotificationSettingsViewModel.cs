using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using MK.Common.Entity;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class NotificationSettingsViewModel : PageViewModel
    {
        private readonly IAccountService _accountService;
        
        public NotificationSettingsViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadNotificationSettings();
        }
			
		public override void OnViewStopped()
		{
			base.OnViewStopped();
			UpdateNotificationSettings ();
		}

        public ObservableCollection<ToggleItem> NotificationSettings { get; set; }

        private bool _isNotificationEnabled;
        public bool IsNotificationEnabled
        {
            get { return _isNotificationEnabled; }
            set
            {
                _isNotificationEnabled = value;
                RaisePropertyChanged();
            }
        }

        private async void LoadNotificationSettings()
        {
            NotificationSettings = new ObservableCollection<ToggleItem>();

            var notificationSettings = await _accountService.GetNotificationSettings();
            var type = notificationSettings.GetType();

            foreach (var setting in type.GetProperties())
            {
                if (setting.PropertyType == typeof(bool) || setting.PropertyType == typeof(bool?))
                {
                    var value = (bool?)setting.GetValue(notificationSettings);

                    // Special case for this property since it resides outside the MvxListView
                    if (setting.Name == "Enabled")
                    {
                        IsNotificationEnabled = value.Value;
                        continue;
                    }

                    // if the value is null, this means the company set the setting as "Not available"
                    // don't show it to the client
                    if (value.HasValue)
                    {
                        NotificationSettings.Add(new ToggleItem
                        {
                            Name = setting.Name,
                            Display = this.Services().Localize["Notification_" + setting.Name],
                            Value = value.Value
                        });
                    }
                }
            }
        }

        private void UpdateNotificationSettings()
        {
            var updatedNotificationSettings = new NotificationSettings
            {
                Enabled = IsNotificationEnabled
            };

            var type = updatedNotificationSettings.GetType();
            var updatedNotificationProperties = type.GetProperties();

            foreach (var notificationSetting in NotificationSettings)
            {
                var updatedNotificationSetting = updatedNotificationProperties.FirstOrDefault(n => n.Name == notificationSetting.Name);
                if (updatedNotificationSetting != null)
                {
                    updatedNotificationSetting.SetValue(updatedNotificationSettings, notificationSetting.Value);
                }
            }

            _accountService.UpdateNotificationSettings(updatedNotificationSettings);
        }
    }
}