using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class PrivacyPolicyService : Service
    {
        private readonly ICompanyDao _dao;
        private readonly ICommandBus _commandBus;

        public PrivacyPolicyService(ICompanyDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        public object Get(PrivacyPolicyRequest request)
        {
            return new
            {
                Content = _dao.Get().PrivacyPolicy
            };
        }

        public object Post(PrivacyPolicyRequest request)
        {
            var command = new UpdatePrivacyPolicy
            {
                CompanyId = AppConstants.CompanyId,
                Policy = request.Policy
            };
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}