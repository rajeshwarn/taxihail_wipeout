using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;

namespace MK.Booking.Test.OrderStatusUpdate
{
    public class FakeOrderDao : IOrderDao
    {
        public OrderDetail FindById(Guid id)
        {
            return new OrderDetail()
            {
                IBSOrderId = 1
            };
        }
        public IList<OrderStatusDetail> GetOrdersInProgress(bool forManualRideLinq)
        {
            var response = new List<OrderStatusDetail>();
            var i = 1;
            while (i <=145)
            {
                    response.Add(new OrderStatusDetail
                    {
                        IBSOrderId = i,
                        OrderId = new Guid(),
                        CompanyKey = "Apcurium",
                        Market = "MTL",
                        Status = OrderStatus.Created,
                        IsManualRideLinq = forManualRideLinq
                    });
                i++;
            }

            return response;
        }
        public OrderStatusDetail FindOrderStatusById(Guid orderId)
        {
           return new OrderStatusDetail()
           {
               CompanyKey = "Apcurium"
           };
        }
       
#region
        public IList<OrderDetail> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId)
        {
            throw new NotImplementedException();
        }

        public IList<OrderDetail> FindByAccountId(Guid id)
        {
            throw new NotImplementedException();
        }

        public OrderPairingDetail FindOrderPairingById(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public void UpdateVehiclePosition(Guid orderId, double? newLatitude, double? newLongitude)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Position> GetVehiclePositions(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public TemporaryOrderCreationInfoDetail GetTemporaryInfo(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public TemporaryOrderPaymentInfoDetail GetTemporaryPaymentInfo(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public void DeleteTemporaryPaymentInfo(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public OrderManualRideLinqDetail GetManualRideLinqById(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public OrderManualRideLinqDetail GetCurrentManualRideLinq(string pairingCode, Guid accountId)
        {
            throw new NotImplementedException();
        }

#endregion


    }
}
