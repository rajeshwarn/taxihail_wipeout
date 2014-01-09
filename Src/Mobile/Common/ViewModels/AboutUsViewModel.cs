using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AboutUsViewModel : BaseViewModel
    {
        private string _uri;
        public string Uri
        {
            get { return _uri; }
			set
			{ 
				_uri = value;
				RaisePropertyChanged();
			}
        }

        public AboutUsViewModel()
        {
            Uri = this.Services().Config.GetSetting("Client.AboutUsUrl");
        }
    }
}