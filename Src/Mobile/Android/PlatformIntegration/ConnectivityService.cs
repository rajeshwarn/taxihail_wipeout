using System;
using Connectivity.Plugin;
using System.Reactive.Subjects;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public class ConnectivityService : IConnectivityService
    {
        private readonly IMessageService _messageService;
        private readonly ILocalization _localize;
        private bool _isDisplayed = false;

        public ConnectivityService(IMessageService messageService, ILocalization localize)
        {
            _messageService = messageService;
            _localize = localize;

            IsConnected = CrossConnectivity.Current.IsConnected;
            CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
                {
                    IsConnected = args.IsConnected;
                };

        }

        public void HandleToastInNewView()
        {
            if (IsConnected)
            {
                _messageService.DismissToast();
                _isDisplayed = false;
            }
            else
            {
                _isDisplayed = _messageService.ShowToast(_localize["NoConnectionMessage"]);
            }
        }

        public void ToastDismissed()
        {
            _isDisplayed = false;
        }

        public void ShowToast()
        {
            if (!_isDisplayed)
            {
                _isDisplayed = _messageService.ShowToast(_localize["NoConnectionMessage"]);
            }
        }

        private bool _isConnected = true;
        public bool IsConnected
        {
            get
            { 
                return _isConnected;
            }
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;

                    if (IsConnected)
                    {
                        _messageService.DismissToast();
                        _isDisplayed = false;
                    }
                    else
                    {
                        if (!_isDisplayed)
                        {
                            _isDisplayed = _messageService.ShowToast(_localize["NoConnectionMessage"]);
                        }
                    }
                }
            }
        }
    }
}

