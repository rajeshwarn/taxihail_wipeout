using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			this.Uri = Config.GetSetting("Client.AboutUsUrl");
        }
    }
}