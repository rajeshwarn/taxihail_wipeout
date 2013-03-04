using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class DriverWebServiceClient : BaseService<WebDriverService>, IDriverWebServiceClient
    {

        public DriverWebServiceClient(IConfigurationManager configManager, ILogger logger)
            : base(configManager, logger)
        {
        }


        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBDriver";
        }


        public IBSDriverInfos GetDriverInfos(string driverId)
        {
            var infos = new IBSDriverInfos();
            UseService(s =>
                {
                    
                    var webDriver = s.GetWEBDriver(UserNameApp, PasswordApp, driverId);
                    infos.FirstName = webDriver.FirstName;
                    infos.LastName = webDriver.LastName;
                    infos.MobilePhone = webDriver.MobilePhone;
                    infos.VehicleColor = webDriver.VehicleColor;
                    infos.VehicleMake = webDriver.VehicleMake;
                    infos.VehicleModel = webDriver.VehicleModel;
                    infos.VehicleRegistration = webDriver.VehicleRegistration;
                    infos.VehicleType = webDriver.VehicleType;
                   
                });
            return infos;
        }


    }
}
