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
    public class BookUsingAddress : GenericTinyMessage<Address>
    {
        public BookUsingAddress(object sender, Address address)
            : base(sender, address)
        {            
        }
    }
}