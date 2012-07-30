using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : RestServiceBase<OrderRequest>
    {
        public OrderService(IOrderDao dao)
        {
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public override object OnGet(OrderRequest request)
        {
            return new OrderMapper().ToResource( Dao.FindById(request.OrderId));
        }
    }
}
