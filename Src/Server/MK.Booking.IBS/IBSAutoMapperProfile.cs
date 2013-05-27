using AutoMapper;

namespace apcurium.MK.Booking.IBS
{
    public class IBSAutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            this.CreateMap<TVehiclePosition, IBSVehiclePosition>()
                .ForMember(x => x.Longitude, opt => opt.MapFrom(x => x.Latitude))
                .ForMember(x => x.Latitude, opt => opt.MapFrom(x => x.Longitude))
                .ForMember(x => x.PositionDate, opt => opt
                    .MapFrom(x => x.GPSLastUpdated.ToDateTime().GetValueOrDefault()));
        }
    }
}
