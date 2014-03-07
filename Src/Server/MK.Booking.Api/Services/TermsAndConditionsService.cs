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

            var shouldForceDisplayOfTermsOnClient = true;
            if(company.Version != null)
            {
                Response.AddHeader(HttpHeaders.ETag, company.Version);
            }
            else
            {
                // version is null, so the terms and conditions have never been triggered
                // don't force them to the user at startup
                shouldForceDisplayOfTermsOnClient = false;
            }

            var result = new TermsAndConditions
                {
                    Content = company.TermsAndConditions.ToSafeString(),
                    Updated = shouldForceDisplayOfTermsOnClient
                };
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

        public object Post(RetriggerTermsAndConditionsRequest request)
        {
            var command = new RetriggerTermsAndConditions
            {
                CompanyId = AppConstants.CompanyId
            };
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}