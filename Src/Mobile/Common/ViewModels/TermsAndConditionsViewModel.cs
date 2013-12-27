using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseSubViewModel<bool>,                 
        IMvxServiceConsumer<ITermsAndConditionsService>

	{

        public TermsAndConditionsViewModel ( string messageId ) : base(messageId)
		{
		}

		public IMvxCommand RejectTermsAndConditions
		{
			get
			{
                return GetCommand(() => ReturnResult(false));
			}
		}

		public IMvxCommand AcceptTermsAndConditions
		{
			get
			{
				return GetCommand (() => ReturnResult(true));

			}			
		}

        private string _text;
        public string TermsAndConditions { 
            get 
            { 
                if (_text.IsNullOrEmpty())
                {
                    var service = this.GetService<ITermsAndConditionsService>();
                    _text = service.GetText();
                }
                return @_text; 
            } 
        }
	}
}

