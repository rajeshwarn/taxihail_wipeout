namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AboutUsViewModel : BaseViewModel
    {
        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; FirePropertyChanged(()=>Uri); }
        }

        public AboutUsViewModel()
        {
			Uri = Config.GetSetting("Client.AboutUsUrl");
        }
    }
}