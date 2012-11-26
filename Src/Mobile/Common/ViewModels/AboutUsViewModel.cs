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
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AboutUsViewModel : BaseViewModel
    {
        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { this._uri = value; FirePropertyChanged(()=>Uri); }
        }

        public AboutUsViewModel()
        {
            this.Uri = Settings.SiteUrl; 
        }
    }
}