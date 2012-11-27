using System;
using System.Collections.ObjectModel;
using Android.OS;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CmtBookViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IPreCogService>
    {
        private IBookingService _bookingService;
        private ILocationService _geolocator;
        private IAccountService _accountService;
        private IPreCogService _preCogService;

        protected override void Initialize()
        {
            _accountService = this.GetService<IAccountService>();
            _geolocator = this.GetService<ILocationService>();
            _bookingService = this.GetService<IBookingService>();
            _preCogService = this.GetService<IPreCogService>();
            _preCogService.Start();
        }

        public ObservableCollection<CmtMessageViewModel> Messages { get; set; }

        public MvxRelayCommand GuideMe
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    Messages.Add(new CmtMessageViewModel { Message = Resources.GetString("GuidancePlease"), IsUser = true });
                    Messages.Add(new CmtMessageViewModel { Message = Resources.GetString("GuidanceLocationConfirmation"), IsUser = false });
                    Messages.Add(new CmtMessageViewModel { Message = Resources.GetString("GuidanceDestinationRequest"), IsUser = false });
                    
                });
            }
        }

    }

    public class CmtMessageViewModel
    {
        public string Message { get; set; }
        public bool IsUser { get; set; }
    }
}