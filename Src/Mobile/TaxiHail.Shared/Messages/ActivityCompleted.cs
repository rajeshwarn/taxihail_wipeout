using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Messages
{
    public class ActivityCompleted : GenericTinyMessage<string>
    {
        public ActivityCompleted(object sender, string result, string ownerId)
            : base(sender, result)
        {
            OwnerId = ownerId;
        }


        public string OwnerId { get; private set; }
    }
}