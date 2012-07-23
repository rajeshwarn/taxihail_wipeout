using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking
{
    public class Module
    {

        public void Init(IUnityContainer container)
        {
            container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderDao>(new OrderDao(() => container.Resolve<BookingDbContext>()));

            container.RegisterInstance<IPasswordService>(new PasswordService());
            container.RegisterInstance<ITemplateService>(new TemplateService());
            container.RegisterInstance<IEmailSender>(new EmailSender(container.Resolve<IConfigurationManager>()));

            //Command Handlers
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, AddressCommandHandler>("FavoriteAddressCommandHandler");
            container.RegisterType<ICommandHandler, EmailCommandHandler>("EmailCommandHandler");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
            
        }

        public void RegisterEventHandlers(IUnityContainer container, IEventHandlerRegistry registry)
        {
            registry.Register(container.Resolve<AccountDetailsGenerator>());
            registry.Register(container.Resolve<AddressListGenerator>());
            registry.Register(container.Resolve<AddressHistoryGenerator>());
            registry.Register(container.Resolve<OrderGenerator>());

        }

    }
}
