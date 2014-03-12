using apcurium.MK.Booking.Api.Contract.Requests;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderConfirmed : GenericTinyMessage<CreateOrder>
    {
        public OrderConfirmed(object sender, CreateOrder address, bool isCancelled)
            : base(sender, address)
        {            
            IsCancelled = isCancelled;
        }

        public bool IsCancelled {
            get;
            set;
        }
    }
}