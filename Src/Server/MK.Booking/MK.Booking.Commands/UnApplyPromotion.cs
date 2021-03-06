﻿using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UnapplyPromotion : ICommand
    {
        public UnapplyPromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid PromoId { get; set; }

        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }
    }
}
