#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IPromotionDao _promotionDao;

        public OrderService(IOrderDao dao, IOrderPaymentDao orderPaymentDao, IPromotionDao promotionDao, IAccountDao accountDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _orderPaymentDao = orderPaymentDao;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public object Get(OrderRequest request)
        {
            
        }

        

        
    }
}