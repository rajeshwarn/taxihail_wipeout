#region

using AutoMapper;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public class IbsAutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<TVehiclePosition, IbsVehiclePosition>()
                .ForMember(x => x.FleetId, opt => opt.Ignore())
                .ForMember(x => x.VehicleType, opt => opt.Ignore())
                .ForMember(x => x.VehicleNumber, opt => opt.MapFrom(x => x.VehicleNumber.Trim()))
                .ForMember(x => x.PositionDate, opt => opt
                    .MapFrom(x => x.GPSLastUpdated.ToDateTime().GetValueOrDefault())
                )
                .ForMember(p => p.Eta, opt => opt.Ignore());
        }
    }
}