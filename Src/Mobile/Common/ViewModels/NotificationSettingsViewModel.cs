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

        private ObservableCollection<ToggleItem> _notificationSettings;
        public ObservableCollection<ToggleItem> NotificationSettings
        {
            get { return _notificationSettings; }
            set { _notificationSettings = value; RaisePropertyChanged(); }
        }

        private bool _isNotificationEnabled;
        public bool IsNotificationEnabled
        {
            get { return _isNotificationEnabled; }
            set
            {
                if (_isNotificationEnabled != value)
                {
                    _isNotificationEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        public NotificationSettingsViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public void Init()
        {
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

        private async void LoadNotificationSettings()
        {
            _notificationSettings = new ObservableCollection<ToggleItem>();

            var notificationSettings = await _accountService.GetNotificationSettings();
            var type = notificationSettings.GetType();

            foreach (var setting in type.GetProperties())
            {
                if (setting.PropertyType == typeof(bool) || setting.PropertyType == typeof(bool?))
                {
                    var value = (bool)setting.GetValue(notificationSettings, null);

                    // Special case for this property since it resides outside the MvxListView
                    if (setting.Name == "Enabled")
                    {
                        IsNotificationEnabled = value;
                        continue;
                    }

                    _notificationSettings.Add(new ToggleItem
                    {
                        Name = setting.Name,
                        Display = this.Services().Localize["Notification_" + setting.Name],
                        Value = value
                    });
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

            foreach (var notificationSetting in _notificationSettings)
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