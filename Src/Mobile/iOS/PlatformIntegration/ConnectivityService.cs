using System;
using Connectivity.Plugin;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using System.Collections.Generic;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public class ConnectivityService : IConnectivityService
    {
        readonly ISubject<bool> _isConnectedSubject = new BehaviorSubject<bool>(false);

        //To remove when toast service is done
        UIAlertView cav = new UIAlertView();

        public ConnectivityService()
        {
            //To remove when toast service is done
            cav.Message = "testmessage";
            cav.Title = "testtitle";

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

        private bool _isConnected;
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
                        //To remove when toast service is done
                        cav.DismissWithClickedButtonIndex(0,true);
                    }
                    else
                    {
                        //To remove when toast service is done
                        cav.Show();
                    }
                }
            }
        }
    }
}

