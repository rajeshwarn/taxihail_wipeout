using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using MK.Common.Android.Entity;
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

        public override void OnViewUnloaded()
        {
            base.OnViewUnloaded();

            // TODO: don't send request if settings didn't change
            UpdateNotificationSettings();
        }

        private async void LoadNotificationSettings()
        {
            _notificationSettings = new ObservableCollection<ToggleItem>();

            var notificationSettings = await _accountService.GetNotificationSettings();
            var type = notificationSettings.GetType();

            foreach (var setting in type.GetProperties())
            {
                if (setting.PropertyType == typeof(bool)
                    || setting.PropertyType == typeof(bool?))
                {
                    var value = (bool)setting.GetValue(notificationSettings, null);

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
            var updatedNotificationSettings = new NotificationSettings();
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