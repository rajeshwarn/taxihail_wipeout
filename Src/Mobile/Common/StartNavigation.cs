using System;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation
                : MvxApplicationObject
                    , IMvxStartNavigation
    {
        public void Start()
        {
            //RequestNavigate<HomeViewModel>();
        }
                
        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }

}

