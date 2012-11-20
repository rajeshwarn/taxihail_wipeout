using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyMessenger;

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