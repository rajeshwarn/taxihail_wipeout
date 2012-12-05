using System.Configuration;
using System.Net;
using System.Net.Mail;
using Infrastructure.Messaging.Handling;
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
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            System.Data.Entity.Database.SetInitializer<BookingDbContext>(null);
            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor(container.Resolve<ConnectionStringSettings>(apcurium.MK.Common.Module.MKConnectionString).ConnectionString));


            container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderDao>(new OrderDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IDefaultAddressDao>(new DefaultAddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<ITariffDao>(new TariffDao(() => container.Resolve<BookingDbContext>()));

            container.RegisterInstance<IOrderRatingsDao>(new OrderRatingsDao(() => container.Resolve<BookingDbContext>()));

            container.RegisterInstance<IRatingTypeDao>(new RatingTypeDao(() => container.Resolve<BookingDbContext>()));

            container.RegisterInstance<IPopularAddressDao>(new PopularAddressDao(() => container.Resolve<BookingDbContext>()));

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
            AutoMapper.Mapper.CreateMap<CreateOrder.BookingSettings, BookingSettings>();
            AutoMapper.Mapper.CreateMap<Address, AddressDetails>();

            AutoMapper.Mapper.CreateMap<EmailSender.SmtpConfiguration, SmtpClient>()
                .ForMember(x => x.Credentials, opt => opt.MapFrom(x => new NetworkCredential(x.Username, x.Password)));

            AutoMapper.Mapper.CreateMap<FavoriteAddressAdded, AddressDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            AutoMapper.Mapper.CreateMap<FavoriteAddressUpdated, AddressDetails>();

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddressAdded, DefaultAddressDetails>();

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddressUpdated, DefaultAddressDetails>();

            AutoMapper.Mapper.CreateMap<PopularAddressAdded, PopularAddressDetails>();
            AutoMapper.Mapper.CreateMap<PopularAddressUpdated, PopularAddressDetails>();
            AutoMapper.Mapper.CreateMap<PopularAddressDetails, Address>();
            AutoMapper.Mapper.CreateMap<TariffDetail, Tariff>();

        }

        private static void RegisterEventHandlers(IUnityContainer container)
        {
            container.RegisterType<IEventHandler, AccountDetailsGenerator>("AccountDetailsGenerator");
            container.RegisterType<IEventHandler, AddressListGenerator>("AddressListGenerator");
            container.RegisterType<IEventHandler, OrderGenerator>("OrderGenerator");
            container.RegisterType<IEventHandler, TariffDetailsGenerator>("TariffDetailsGenerator");
            container.RegisterType<IEventHandler, RatingTypeDetailsGenerator>("RatingTypeDetailsGenerator");
            container.RegisterType<IEventHandler, AppSettingsGenerator>("AppSettingsGenerator");
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, AddressCommandHandler>("FavoriteAddressCommandHandler");
            container.RegisterType<ICommandHandler, EmailCommandHandler>("EmailCommandHandler");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
            container.RegisterType<ICommandHandler, CompanyCommandHandler>("CompanyCommandHandler");
        }
    }
}
