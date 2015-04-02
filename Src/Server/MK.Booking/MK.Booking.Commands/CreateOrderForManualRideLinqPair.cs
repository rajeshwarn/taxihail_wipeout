using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CreateOrderForManualRideLinqPair: ICommand
    {
        public CreateOrderForManualRideLinqPair()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string RideLinQId { get; set; }
        public DateTime StartTime { get; set; }
        public string ClientLanguageCode { get; set; }
        public string UserAgent { get; set; }
        public string ClientVersion { get; set; }
        public string CompanyKey { get; set; }
        public string CompanyName { get; set; }
        public string Market { get; set; }
    }
}
