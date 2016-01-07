using System;
using Connectivity.Plugin;
using System.Reactive.Subjects;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public class ConnectivityService : IConnectivityService
    {
        readonly ISubject<bool> _isConnectedSubject = new BehaviorSubject<bool>(false);
        private readonly IMessageService _messageService;
        private readonly ILocalization _localize;

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

        public IObservable<bool> GetAndObserveIsConnected()
        {
            return _isConnectedSubject;
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
                    _isConnectedSubject.OnNext(IsConnected);

                    if (IsConnected)
                    {
                        _messageService.DismissToast();
                    }
                    else
                    {
                        _messageService.ShowToast(_localize["NoConnectionMessage"]);
                    }
                }
            }
        }
    }
}

