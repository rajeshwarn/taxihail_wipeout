using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
    public class SaveOriginalEta : ICommand
    {
        public SaveOriginalEta()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public long OriginalEta { get; set; }
    }
}
