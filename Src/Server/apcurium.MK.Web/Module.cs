using System.Data.Entity;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using Microsoft.Practices.Unity;
using apcurium.MK.Common.Entity;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace apcurium.MK.Web
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterInfrastructure(container);

            new MK.Common.Module().Init(container);
            new MK.Booking.Module().Init(container);
            new MK.Booking.IBS.Module().Init(container);
            new MK.Booking.Api.Module().Init(container);
        }

        private void RegisterInfrastructure(IUnityContainer container)
        {
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMessageSender>(new MessageSender(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Commands"));
            container.RegisterInstance<ICommandBus>(new CommandBus(container.Resolve<IMessageSender>(), container.Resolve<ITextSerializer>()));
        }
    }
}