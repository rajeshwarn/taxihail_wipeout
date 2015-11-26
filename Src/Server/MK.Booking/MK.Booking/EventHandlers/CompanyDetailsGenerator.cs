using System.Globalization;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CompanyDetailsGenerator :
        IEventHandler<CompanyCreated>,
        IEventHandler<TermsAndConditionsUpdated>,
        IEventHandler<TermsAndConditionsRetriggered>,
        IEventHandler<PrivacyPolicyUpdated>
    {
        private readonly IProjectionSet<CompanyDetail> _companyProjectionSet;

        public CompanyDetailsGenerator(IProjectionSet<CompanyDetail> companyProjectionSet)
        {
            _companyProjectionSet = companyProjectionSet;
        }

        public void Handle(CompanyCreated @event)
        {
            _companyProjectionSet.Add(new CompanyDetail { Id = @event.SourceId });
        }

        public void Handle(TermsAndConditionsUpdated @event)
        {
            _companyProjectionSet.Update(@event.SourceId, company =>
            {
                company.TermsAndConditions = @event.TermsAndConditions;
            });
        }

        public void Handle(TermsAndConditionsRetriggered @event)
        {
            _companyProjectionSet.Update(@event.SourceId, company =>
            {
                var tAndC = company.TermsAndConditions ?? "";
                company.Version = tAndC.GetHashCode().ToString(CultureInfo.InvariantCulture);
            });
        }

        public void Handle(PrivacyPolicyUpdated @event)
        {
            _companyProjectionSet.Update(@event.SourceId, company =>
            {
                company.PrivacyPolicy = @event.Policy;
            });
        }
    }
}