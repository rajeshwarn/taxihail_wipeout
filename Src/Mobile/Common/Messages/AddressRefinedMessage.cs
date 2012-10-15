using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class AddressRefinedMessage : GenericTinyMessage<RefineAddressViewModel>
    {
        public AddressRefinedMessage(object sender, RefineAddressViewModel address)
            : base(sender, address)
        {            
        }
    }
}