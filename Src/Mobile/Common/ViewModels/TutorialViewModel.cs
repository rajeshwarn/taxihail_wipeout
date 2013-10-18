using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Client;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using Cirrious.MvvmCross.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TutorialViewModel : BaseViewModel,                 
            IMvxServiceConsumer<ITutorialService>

    {
        private TutorialItemModel[] _tutorialItemsList;
        public TutorialItemModel[] TutorialItemsList
        {
            get { return _tutorialItemsList; }
            set { _tutorialItemsList =value; FirePropertyChanged(()=>TutorialItemsList); }
        }

        public TutorialViewModel()
        {
            var service  = this.GetService<ITutorialService>();
            TutorialItemsList = service.GetTutorialItems ( ).Select ( item => new TutorialItemModel { TopText = item.TopText, TopTitle = item.TopTitle , BottomText = item.BottomText , BottomTitle = item.BottomTitle, ImageUri = item.ImageUri }).ToArray (); 
        }

    }
}