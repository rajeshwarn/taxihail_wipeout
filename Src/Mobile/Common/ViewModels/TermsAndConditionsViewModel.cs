using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseSubViewModel<bool>

	{
        private readonly ITermsAndConditionsService _termsAndConditionsService;

        public TermsAndConditionsViewModel ( string messageId , ITermsAndConditionsService termsAndConditionsService) : base(messageId)
        {
            _termsAndConditionsService = termsAndConditionsService;
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
                    _text = _termsAndConditionsService.GetText();
                }
                return @_text; 
            } 
        }
	}
}

