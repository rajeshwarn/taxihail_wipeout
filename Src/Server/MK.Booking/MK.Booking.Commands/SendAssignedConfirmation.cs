using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class SendAssignedConfirmation : ICommand
    {
        public SendAssignedConfirmation()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VehicleNumber { get; set; }
        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        public SendBookingConfirmationEmail.BookingSettings Settings { get; set; }
        public double Fare { get; set; }
    }
}