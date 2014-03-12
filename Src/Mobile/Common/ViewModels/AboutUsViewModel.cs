namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AboutUsViewModel : PageViewModel
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

		public override void Start()
		{
			Uri = Settings.AboutUsUrl;
		}
    }
}