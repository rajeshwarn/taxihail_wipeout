using System.Net;
using System.Net.Mail;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            System.Data.Entity.Database.SetInitializer<BookingDbContext>(null);
            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("Booking"));

            container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderDao>(new OrderDao(() => container.Resolve<BookingDbContext>()));

            container.RegisterInstance<IPasswordService>(new PasswordService());
            container.RegisterInstance<ITemplateService>(new TemplateService());
            container.RegisterInstance<IEmailSender>(new EmailSender(container.Resolve<IConfigurationManager>()));

            RegisterMaps();
            RegisterCommandHandlers(container);
            RegisterEventHandlers(container);
        }

        public void RegisterMaps()
        {
            AutoMapper.Mapper.CreateMap<UpdateBookingSettings, BookingSettings>();
            AutoMapper.Mapper.CreateMap<CreateOrder.BookingSettings, BookingSettings>();

            AutoMapper.Mapper.CreateMap<EmailSender.SmtpConfiguration, SmtpClient>()
                .ForMember(x => x.Credentials, opt => opt.MapFrom(x => new NetworkCredential(x.Username, x.Password)));

            AutoMapper.Mapper.CreateMap<OrderCreated, Address>()
                .ForMember(p => p.Apartment, opt => opt.MapFrom(m => m.PickupApartment))
                .ForMember(p => p.FullAddress, opt => opt.MapFrom(m => m.PickupAddress))
                .ForMember(p => p.RingCode, opt => opt.MapFrom(m => m.PickupRingCode))
                .ForMember(p => p.Latitude, opt => opt.MapFrom(m => m.PickupLatitude))
                .ForMember(p => p.Longitude, opt => opt.MapFrom(m => m.PickupLongitude));

            AutoMapper.Mapper.CreateMap<FavoriteAddressAdded, Address>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            AutoMapper.Mapper.CreateMap<FavoriteAddressUpdated, ReadModel.Address>()
                .ForMember(p => p.Id, options => options.MapFrom(m => m.AddressId));
        }

        private static void RegisterEventHandlers(IUnityContainer container)
        {
            container.RegisterType<IEventHandler, AccountDetailsGenerator>("AccountDetailsGenerator");
            container.RegisterType<IEventHandler, AddressListGenerator>("AddressListGenerator");
            container.RegisterType<IEventHandler, OrderGenerator>("OrderGenerator");
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, AddressCommandHandler>("FavoriteAddressCommandHandler");
            container.RegisterType<ICommandHandler, EmailCommandHandler>("EmailCommandHandler");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
        }
    }
}
