using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Extensions;
namespace apcurium.MK.Booking.Commands
{
    public class UpdateAccount : ICommand
    {
        public UpdateAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

    }
}
