#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class SendAssignedConfirmation : ICommand
    {
        public SendAssignedConfirmation()
        {
            Id = Guid.NewGuid();
        }

        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VehicleNumber { get; set; }
        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        public string ClientLanguageCode { get; set; }
        public SendBookingConfirmationEmail.BookingSettings Settings { get; set; }
        public double Fare { get; set; }
        public Guid Id { get; set; }
    }
}