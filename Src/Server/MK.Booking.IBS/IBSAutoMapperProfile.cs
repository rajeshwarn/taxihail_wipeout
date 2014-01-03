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
                .ForMember(x => x.VehicleNumber, opt => opt.MapFrom(x => x.VehicleNumber.Trim()))
                .ForMember(x => x.PositionDate, opt => opt
                    .MapFrom(x => x.GPSLastUpdated.ToDateTime().GetValueOrDefault()));
        }
    }
}