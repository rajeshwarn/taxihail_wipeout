using System;
using System.Globalization;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CompanyDetailsGenerator :
        IEventHandler<CompanyCreated>,
        IEventHandler<TermsAndConditionsUpdated>,
        IEventHandler<TermsAndConditionsRetriggered>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CompanyDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(CompanyCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new CompanyDetail { Id = @event.SourceId });
            }
        }

        public void Handle(TermsAndConditionsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var company = context.Find<CompanyDetail>(@event.SourceId);
                company.TermsAndConditions = @event.TermsAndConditions;
                context.Save(company);
            }
        }

        public void Handle(TermsAndConditionsRetriggered @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var company = context.Find<CompanyDetail>(@event.SourceId);
                company.Version = company.TermsAndConditions.GetHashCode().ToString(CultureInfo.InvariantCulture);
                context.Save(company);
            }
        }
    }
}