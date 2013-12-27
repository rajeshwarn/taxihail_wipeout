using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Models;
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