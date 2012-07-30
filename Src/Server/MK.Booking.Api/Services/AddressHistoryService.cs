using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class AddressHistoryService : RestServiceBase<AddressHistoryRequest>
    {
        private readonly IAddressDao _dao;
        private readonly ICommandBus _commandBus;

        public AddressHistoryService(IAddressDao dao)
        {
            _dao = dao;
        }

        public override object OnGet(AddressHistoryRequest request)
        {
            var session = this.GetSession();
            return _dao.FindHistoricByAccountId(new Guid(session.UserAuthId));
        }

    }
}
