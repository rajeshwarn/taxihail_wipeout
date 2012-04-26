using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using TaxiMobile.Lib.IBS;
using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public class SettingMapper
	{
		public SettingMapper ()
		{
		}

		
			
		
		public void SetWSSetting (AccountInfo toUpdate, AccountData account)
		{
			toUpdate.PreferenceChargeTypeId = account.DefaultSettings.ChargeType;
			toUpdate.PreferenceVehicleTypeId = account.DefaultSettings.VehicleType;
			toUpdate.PreferenceNumberOfPassenger = account.DefaultSettings.Passengers;
			toUpdate.PreferenceCompanyId = account.DefaultSettings.Company;
			toUpdate.PhoneNumber  = account.DefaultSettings.Phone;
			var fullNameArray = account.DefaultSettings.Name.Split( ' ' );
			toUpdate.FirstName = fullNameArray[0];
			toUpdate.LastName = fullNameArray.Count() > 1 ? fullNameArray[1] : "";
			
			Console.WriteLine( "SetWSSetting : " + toUpdate.PreferenceChargeTypeId.ToString() );
			                  
		}
		public void SetSetting (AccountData toUpdate, AccountInfo account)
		{
		
	        
			if (toUpdate.DefaultSettings == null) {
				toUpdate.DefaultSettings = new BookingSetting ();
			}
			
			toUpdate.DefaultSettings.ChargeType = account.PreferenceChargeTypeId;
			
			var company = ServiceLocator.Current.GetInstance<IAccountService> ().GetCompaniesList ().FirstOrDefault (c => c.Id == account.PreferenceCompanyId);
			
			if (company == null) {
                company = ServiceLocator.Current.GetInstance<IAccountService>().GetCompaniesList().First();
			}
			
			toUpdate.DefaultSettings.Company = company.Id;
			toUpdate.DefaultSettings.CompanyName = company.Display;
			
			var vehicule = ServiceLocator.Current.GetInstance<IAccountService> ().GetVehiclesList ().FirstOrDefault (c => c.Id == account.PreferenceVehicleTypeId);
			
			if (vehicule == null ){
                vehicule = ServiceLocator.Current.GetInstance<IAccountService>().GetVehiclesList().First();
			}
			
			toUpdate.DefaultSettings.VehicleType = vehicule.Id;
			toUpdate.DefaultSettings.VehicleTypeName = vehicule.Display;
			
			
			var pList = ServiceLocator.Current.GetInstance<IAccountService> ().GetPaymentsList ();
			
			var payement = pList.FirstOrDefault (c => c.Id == account.PreferenceChargeTypeId);
			
		
			if (payement == null ){
                payement = ServiceLocator.Current.GetInstance<IAccountService>().GetPaymentsList().First();
			}
			
			toUpdate.DefaultSettings.ChargeType = payement.Id;
			toUpdate.DefaultSettings.ChargeTypeName = payement.Display;
			
			toUpdate.DefaultSettings.Passengers = account.PreferenceNumberOfPassenger == 0 ? 1 : account.PreferenceNumberOfPassenger;
			
			
			if ( toUpdate.DefaultSettings.Name.IsNullOrEmpty() )
			{
				toUpdate.DefaultSettings.Name = toUpdate.Name;			
			}			
			toUpdate.DefaultSettings.Phone = account.PhoneNumber;
			
			Console.WriteLine( "SetSetting : " + toUpdate.DefaultSettings.ChargeType );
		}
	}
}

