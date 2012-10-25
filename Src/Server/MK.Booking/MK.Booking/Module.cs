﻿using System.Configuration;
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

            AutoMapper.Mapper.CreateMap<EmailSender.SmtpConfiguration, SmtpClient>()
                .ForMember(x => x.Credentials, opt => opt.MapFrom(x => new NetworkCredential(x.Username, x.Password)));

            AutoMapper.Mapper.CreateMap<OrderCreated, AddressDetails>()
                .ForMember(p => p.Apartment, opt => opt.MapFrom(m => m.PickupAddress.Apartment))
                .ForMember(p => p.FullAddress, opt => opt.MapFrom(m => m.PickupAddress.FullAddress))
                .ForMember(p => p.RingCode, opt => opt.MapFrom(m => m.PickupAddress.RingCode))
                .ForMember(p=> p.BuildingName, opt => opt.MapFrom(m=>m.PickupAddress.BuildingName))
                .ForMember(p => p.Latitude, opt => opt.MapFrom(m => m.PickupAddress.Latitude))
                .ForMember(p => p.Longitude, opt => opt.MapFrom(m => m.PickupAddress.Longitude));

            AutoMapper.Mapper.CreateMap<FavoriteAddressAdded, AddressDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            AutoMapper.Mapper.CreateMap<FavoriteAddressUpdated, AddressDetails>()
                .ForMember(p => p.Id, options => options.MapFrom(m => m.AddressId));

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddressAdded, DefaultAddressDetails>();

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddressUpdated, DefaultAddressDetails>();

            AutoMapper.Mapper.CreateMap<PopularAddressAdded, PopularAddressDetails>();

            AutoMapper.Mapper.CreateMap<PopularAddressUpdated, PopularAddressDetails>();
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
            container.RegisterType<ICommandHandler, CompanyCommandHandler>("CompanyCommandHandler");
        }
    }
}
