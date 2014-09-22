using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using AutoMapper;

namespace ExportTool
{
    static class Mapping
    {
        public static void RegisterMaps()
        {
            Mapper.CreateMap<UpdateBookingSettings, BookingSettings>();
            Mapper.CreateMap<CreateOrder.PaymentInformation, PaymentInformation>();

            Mapper.CreateMap<Address, AddressDetails>();
            Mapper.CreateMap<Address, DefaultAddressDetails>();
            Mapper.CreateMap<Address, PopularAddressDetails>();
            Mapper.CreateMap<PopularAddressDetails, Address>();

            Mapper.CreateMap<TariffDetail, Tariff>();
            Mapper.CreateMap<RuleDetail, Rule>();

            Mapper.CreateMap<OrderStatusDetail, OrderStatusDetail>();

            Mapper.CreateMap<OrderDetail, OrderDetailWithAccount>()
                .ForMember(d => d.MdtFare, opt => opt.MapFrom(m => m.Fare))
                .ForMember(d => d.MdtTip, opt => opt.MapFrom(m => m.Tip))
                .ForMember(d => d.ChargeType, opt => opt.MapFrom(m => m.Settings.ChargeType))
                .ForMember(d => d.MdtToll, opt => opt.MapFrom(m => m.Toll));

            Mapper.CreateMap<AccountDetail, OrderDetailWithAccount>()
                .ForMember(d => d.Name, opt => opt.MapFrom(m => m.Settings.Name))
                .ForMember(d => d.Phone, opt => opt.MapFrom(m => m.Settings.Phone));

            Mapper.CreateMap<CreditCardDetails, OrderDetailWithAccount>()
                .ForMember(d => d.AccountDefaultCardToken, opt => opt.MapFrom(m => m.Token));


            Mapper.CreateMap<OrderPaymentDetail, OrderDetailWithAccount>()
                .ForMember(d => d.PaymentMeterAmount, opt => opt.MapFrom(m => m.Meter))
                .ForMember(d => d.PaymentTotalAmount, opt => opt.MapFrom(m => m.Amount))
                .ForMember(d => d.PaymentTipAmount, opt => opt.MapFrom(m => m.Tip))
                .ForMember(d => d.PaymentType, opt => opt.MapFrom(m => m.Type))
                .ForMember(d => d.PaymentProvider, opt => opt.MapFrom(m => m.Provider));

            Mapper.CreateMap<OrderStatusDetail, OrderDetailWithAccount>()
                .ForMember(d => d.VehicleType, opt => opt.MapFrom(m => m.DriverInfos.VehicleType))
                .ForMember(d => d.VehicleColor, opt => opt.MapFrom(m => m.DriverInfos.VehicleColor))
                .ForMember(d => d.VehicleMake, opt => opt.MapFrom(m => m.DriverInfos.VehicleMake))
                .ForMember(d => d.VehicleModel, opt => opt.MapFrom(m => m.DriverInfos.VehicleModel))
                .ForMember(d => d.DriverFirstName, opt => opt.MapFrom(m => m.DriverInfos.FirstName))
                .ForMember(d => d.DriverLastName, opt => opt.MapFrom(m => m.DriverInfos.LastName))
                .ForMember(d => d.VehicleRegistration, opt => opt.MapFrom(m => m.DriverInfos.VehicleRegistration));
        }
    }
}
