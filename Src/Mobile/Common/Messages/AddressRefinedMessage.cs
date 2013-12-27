using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class AddressRefinedMessage : SubNavigationResultMessage<RefineAddressViewModel>
    {
        public AddressRefinedMessage(object sender, string messageId, RefineAddressViewModel result)
            : base(sender, messageId, result)
        {            
        }
    }
}