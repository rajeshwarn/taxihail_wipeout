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

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CreditCardRemoved: GenericTinyMessage<Guid>
    {

        public CreditCardRemoved(object sender, Guid creditCardId, string ownerId)
            : base(sender, creditCardId)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}