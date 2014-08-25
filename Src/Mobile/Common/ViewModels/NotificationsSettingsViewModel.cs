using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class NotificationsSettingsViewModel : PageViewModel
    {
        //private ObservableCollection<ListItem> _notificationsSettings; 

        public NotificationsSettingsViewModel()
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
            //_notificationsSettings = new ObservableCollection<ListItem>();
            //_notificationsSettings.Add(new ListItem
            //    {
            //        Display = "Allow Notifications"
            //    });
        }
    }
}