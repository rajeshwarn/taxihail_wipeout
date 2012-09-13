using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class AddressSelected : GenericTinyMessage<Address>
    {

        public AddressSelected(object sender, Address address, string ownerId)
            : base(sender, address)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }



    }
}