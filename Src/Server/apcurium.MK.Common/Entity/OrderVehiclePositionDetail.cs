using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Entity
{
    public class OrderVehiclePositionDetail
    {
        public OrderVehiclePositionDetail()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }
        public DateTime DateOfPosition { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}