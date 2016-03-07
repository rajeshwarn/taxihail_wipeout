using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class TermsAndConditionsService : BaseApiService
    {
        private readonly ICompanyDao _dao;
        private readonly ICommandBus _commandBus;

        public TermsAndConditionsService(ICompanyDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }


        public bool IsNotModified()
        {
            var company = _dao.Get();


            return company.Version.HasValueTrimmed() && HttpRequest.Headers.IfNoneMatch.Any(p => p.Tag == company.Version);
        }

        public string GetCompanyVersion()
        {
            var company = _dao.Get();

            return company.Version;
        }

        public TermsAndConditions Get()
        {
            var company = _dao.Get();

            var result = new TermsAndConditions
                {
                    Content = company.TermsAndConditions.ToSafeString(),
                    Updated = company.Version.HasValueTrimmed()
                };
            return result;
        }

        public void Post(TermsAndConditionsRequest request)
        {
            var command = new UpdateTermsAndConditions
            {
                CompanyId = AppConstants.CompanyId,
                TermsAndConditions = request.TermsAndConditions
            };
            _commandBus.Send(command);
        }

        public void RetriggerTermsAndConditions()
        {
            var command = new RetriggerTermsAndConditions
            {
                CompanyId = AppConstants.CompanyId
            };
            _commandBus.Send(command);
        }
    }
}