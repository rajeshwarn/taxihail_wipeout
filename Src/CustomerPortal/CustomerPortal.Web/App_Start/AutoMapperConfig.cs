#region

using System;
using AutoMapper;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;

#endregion

namespace CustomerPortal.Web
{
    public class AutoMapperConfig
    {
        private const double OneMileInKilometers = 1.6;

        public static void RegisterMaps()
        {
            Mapper.CreateMap<Questionnaire, QuestionnaireViewModel>()
                .ForMember(x => x.FlagDropRate,
                    opt => opt.ResolveUsing(x => ConvertToKilometers(x.UnitOfLength, x.FlagDropRate)))
                .ForMember(x => x.MileageRate,
                    opt => opt.ResolveUsing(x => ConvertToKilometers(x.UnitOfLength, x.MileageRate)))
                .ForMember(x => x.IsReadOnly, opt => opt.Ignore());

            Mapper.CreateMap<QuestionnaireViewModel, Questionnaire>()
                .ForMember(x => x.FlagDropRate,
                    opt => opt.ResolveUsing(x => ConvertToUnit(x.UnitOfLength, x.FlagDropRate)))
                .ForMember(x => x.MileageRate,
                    opt => opt.ResolveUsing(x => ConvertToUnit(x.UnitOfLength, x.MileageRate)));

            Mapper.CreateMap<StoreSettingsViewModel, StoreSettings>();


            Mapper.AssertConfigurationIsValid();
        }

        private static decimal? ConvertToKilometers(UnitOfLength unit, decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }
            switch (unit)
            {
                case UnitOfLength.Kilometers:
                    return value;
                case UnitOfLength.Miles:
                    return value/(decimal) OneMileInKilometers;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static decimal? ConvertToUnit(UnitOfLength unit, decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }
            switch (unit)
            {
                case UnitOfLength.Kilometers:
                    return value;
                case UnitOfLength.Miles:
                    return value*(decimal) OneMileInKilometers;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}