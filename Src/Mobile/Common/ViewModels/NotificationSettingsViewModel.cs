using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class NotificationSettingsViewModel : PageViewModel
    {
        private ObservableCollection<ListItem> _notificationsSettings;

        public ObservableCollection<ListItem> NotificationsSettings
        {
            get { return _notificationsSettings; }
            set { _notificationsSettings = value; RaisePropertyChanged(); }
        }

        public NotificationSettingsViewModel()
        {
            
        }

        public async void Init()
        {
            // TODO ?
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadNotificationSettings();
        }

        private void LoadNotificationSettings()
        {
            _notificationsSettings = new ObservableCollection<ListItem>();
            _notificationsSettings.Add(new ListItem
                {
                    Display = "Allow Notifications"
                });
            _notificationsSettings.Add(new ListItem
                {
                    Display = "Booking confirmation (email)"
                });
        }
    }
}