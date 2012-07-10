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
        private readonly IHistoricAddressDao _dao;
        private readonly ICommandBus _commandBus;

        public AddressHistoryService(IHistoricAddressDao dao)
        {
            _dao = dao;
        }

        public override object OnGet(AddressHistoryRequest request)
        {
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }

            var session = this.GetSession();
            return _dao.FindByAccountId(new Guid(session.UserAuthId));
        }

    }
}
