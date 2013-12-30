using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseSubViewModel<bool>
	{

        public TermsAndConditionsViewModel (string messageId ) : base(messageId)
        {
        }

        public AsyncCommand RejectTermsAndConditions
		{
			get
			{
                return GetCommand(() => ReturnResult(false));
			}
		}

        public AsyncCommand AcceptTermsAndConditions
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
                    _text = this.Services().Terms.GetText();
                }
                return @_text; 
            } 
        }
	}
}

