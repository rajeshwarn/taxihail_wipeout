using System.Net;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class TermsAndConditionsService : Service
    {
        private readonly ICompanyDao _dao;
        private readonly ICommandBus _commandBus;

        public TermsAndConditionsService(ICompanyDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        public object Get(TermsAndConditionsRequest request)
        {
            var company = _dao.Get();
            if (company.Version != null 
                && Request.Headers[HttpHeaders.IfNoneMatch] == company.Version)
            {
                return new HttpResult(HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString()); 
            }

            Response.AddHeader(HttpHeaders.ETag, company.Version);
            var result = new TermsAndConditions { Content = company.TermsAndConditions.ToSafeString() };
            return result;
            
        }

        public object Post(TermsAndConditionsRequest request)
        {
            var command = new UpdateTermsAndConditions
            {
                CompanyId = AppConstants.CompanyId,
                TermsAndConditions = request.TermsAndConditions
            };
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}