using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MK.Common.Android.Entity;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TutorialViewModel : BaseViewModel
    {
        private List<TutorialItemModel> _tutorialItemsList;
        public List<TutorialItemModel> TutorialItemsList
        {
            get { return _tutorialItemsList; }
            set { _tutorialItemsList =value; FirePropertyChanged(()=>TutorialItemsList); }
        }

        public TutorialViewModel()
        {
            TutorialItemsList = Resources.GetTutorialItemsList();
        }
    }
}