using System.Configuration;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Events.Migration.Processor;
using apcurium.MK.Events.Migration.Projections;
using Microsoft.Practices.Unity;

namespace apcurium.MK.Events.Migration
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            var eventProcessor = new EventProcessor();
            container.RegisterInstance(container);
            RegisterEventMigrators(container, eventProcessor);

            container.RegisterInstance(new EventMigrator(eventProcessor));
        }

        private void RegisterEventMigrators(IUnityContainer container, EventProcessor eventProcessor)
        {
            var connectionString = container.Resolve<ConnectionStringSettings>(Common.Module.MkConnectionString).ConnectionString;
            eventProcessor.Register(new AccountDetailsMigrator(container.Resolve<IServerSettings>(), () => new BookingDbContext(connectionString)));
        }
    }
}