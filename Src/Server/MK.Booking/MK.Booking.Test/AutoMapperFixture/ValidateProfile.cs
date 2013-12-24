using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Test.AutoMapperFixture
{
    [TestFixture]
    public class ValidateProfile
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
