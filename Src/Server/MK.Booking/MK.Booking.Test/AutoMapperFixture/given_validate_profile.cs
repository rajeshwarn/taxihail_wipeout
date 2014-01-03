#region

using apcurium.MK.Booking.IBS;
using AutoMapper;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AutoMapperFixture
{
    [TestFixture]
    public class given_validate_profile
    {
        [Test]
        public void validate_ibs_profile()
        {
            var profile = new IbsAutoMapperProfile();
            Mapper.AddProfile(profile);
            Mapper.AssertConfigurationIsValid(profile.ProfileName);
        }
    }
}