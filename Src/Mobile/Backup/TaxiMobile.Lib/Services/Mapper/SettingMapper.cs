using System;
using System.Linq;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services.IBS;

namespace TaxiMobile.Lib.Services.Mapper
{
	public class SettingMapper
	{
		
		public void SetWSSetting (TBookAccount3 toUpdate, AccountData account)
		{
			//toUpdate.PreferenceChargeTypeId = account.DefaultSettings.ChargeType;
			//toUpdate.PreferenceVehicleTypeId = account.DefaultSettings.VehicleType;
			//toUpdate.PreferenceNumberOfPassenger = account.DefaultSettings.Passengers;
			//toUpdate.PreferenceCompanyId = account.DefaultSettings.Company;
		}

		public void SetSetting (AccountData toUpdate, TBookAccount3 account)
		{
			if (toUpdate.DefaultSettings == null) {
				toUpdate.DefaultSettings = new BookingSetting ();
			}
			
			//toUpdate.DefaultSettings.ChargeType = account.;
			
			//var company = ServiceLocator.Current.GetInstance<IAccountService> ().GetCompaniesList ().FirstOrDefault (c => c.Id == account.PreferenceCompanyId);
            ListItem company = null;
			if (company == null) {
                company = ServiceLocator.Current.GetInstance<IStaticDataService>().GetCompaniesList().First();
			}
			
			toUpdate.DefaultSettings.Company = company.Id;
			toUpdate.DefaultSettings.CompanyName = company.Display;
			
			//var vehicule = ServiceLocator.Current.GetInstance<IAccountService> ().GetVehiclesList ().FirstOrDefault (c => c.Id == account.PreferenceVehicleTypeId);
            ListItem vehicule = null;
			if (vehicule == null ){
                vehicule = ServiceLocator.Current.GetInstance<IStaticDataService>().GetVehiclesList().First();
			}
			
			toUpdate.DefaultSettings.VehicleType = vehicule.Id;
			toUpdate.DefaultSettings.VehicleTypeName = vehicule.Display;


            //var pList = ServiceLocator.Current.GetInstance<IStaticDataService>().GetPaymentsList();
			
			//var payement = pList.FirstOrDefault (c => c.Id == account.PreferenceChargeTypeId);
            ListItem payement = null;
		
			if (payement == null ){
                payement = ServiceLocator.Current.GetInstance<IStaticDataService>().GetPaymentsList().First();
			}
			
			toUpdate.DefaultSettings.ChargeType = payement.Id;
			toUpdate.DefaultSettings.ChargeTypeName = payement.Display;
			
			//toUpdate.DefaultSettings.Passengers = account.PreferenceNumberOfPassenger == 0 ? 1 : account.PreferenceNumberOfPassenger;
			
			
			if ( toUpdate.DefaultSettings.LastName.IsNullOrEmpty() )
			{
				toUpdate.DefaultSettings.LastName = toUpdate.LastName;			
			}			
			toUpdate.DefaultSettings.Phone = account.Phone;
			
			Console.WriteLine( "SetSetting : " + toUpdate.DefaultSettings.ChargeType );
		}
	}
}

