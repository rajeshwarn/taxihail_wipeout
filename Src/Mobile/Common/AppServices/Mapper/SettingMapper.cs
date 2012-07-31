//using System;
//using System.Linq;
//using apcurium.Framework.Extensions;
//using TinyIoC;

//namespace apcurium.MK.Booking.Mobile
//{
//    public class SettingMapper
//    {
//        public SettingMapper ()
//        {
//        }

		
			
		
//        public void SetWSSetting (IBS.AccountInfo toUpdate, Account account)
//        {
//            toUpdate.PreferenceChargeTypeId = account.DefaultSettings.ChargeType;
//            toUpdate.PreferenceVehicleTypeId = account.DefaultSettings.VehicleType;
//            toUpdate.PreferenceNumberOfPassenger = account.DefaultSettings.Passengers;
//            toUpdate.PreferenceCompanyId = account.DefaultSettings.Company;
//            toUpdate.PhoneNumber  = account.DefaultSettings.Phone;
//            var fullNameArray = account.DefaultSettings.Name.Split( ' ' );
//            toUpdate.FirstName = fullNameArray[0];
//            toUpdate.LastName = fullNameArray.Count() > 1 ? fullNameArray[1] : "";
//            toUpdate.PreferenceRideExceptions = account.DefaultSettings.Exceptions.Select( e => new IBS.RideException() { Id = e.Id, Description = e.Display, Value = e.Value } ).ToArray();
			
//            Console.WriteLine( "SetWSSetting : " + toUpdate.PreferenceChargeTypeId.ToString() );
			                  
//        }
//        public void SetSetting (Account toUpdate, IBS.AccountInfo account)
//        {
		
	        
//            if (toUpdate.DefaultSettings == null) {
//                toUpdate.DefaultSettings = new BookingSetting ();
//            }
			
//            toUpdate.DefaultSettings.ChargeType = account.PreferenceChargeTypeId;
			
//            var company = TinyIoCContainer.Current.Resolve<IAccountService> ().GetCompaniesList ().FirstOrDefault (c => c.Id == account.PreferenceCompanyId);
			
//            if (company == null) {
//                company = TinyIoCContainer.Current.Resolve<IAccountService>().GetCompaniesList().First();
//            }
			
//            toUpdate.DefaultSettings.Company = company.Id;
//            toUpdate.DefaultSettings.CompanyName = company.Display;
			
//            var vehicule = TinyIoCContainer.Current.Resolve<IAccountService> ().GetVehiclesList ().FirstOrDefault (c => c.Id == account.PreferenceVehicleTypeId);
			
//            if (vehicule == null ){
//                vehicule = TinyIoCContainer.Current.Resolve<IAccountService>().GetVehiclesList().First();
//            }
			
//            toUpdate.DefaultSettings.VehicleType = vehicule.Id;
//            toUpdate.DefaultSettings.VehicleTypeName = vehicule.Display;
			
			
//            var pList = TinyIoCContainer.Current.Resolve<IAccountService> ().GetPaymentsList ();
			
//            var payement = pList.FirstOrDefault (c => c.Id == account.PreferenceChargeTypeId);
			
		
//            if (payement == null ){
//                payement = TinyIoCContainer.Current.Resolve<IAccountService>().GetPaymentsList().First();
//            }
			
//            toUpdate.DefaultSettings.ChargeType = payement.Id;
//            toUpdate.DefaultSettings.ChargeTypeName = payement.Display;
			
//            toUpdate.DefaultSettings.Passengers = account.PreferenceNumberOfPassenger == 0 ? 1 : account.PreferenceNumberOfPassenger;

//            toUpdate.DefaultSettings.Exceptions = account.PreferenceRideExceptions.Select( e => new ListItemValue() { Id = e.Id, Display = e.Description, Value = e.Value } ).ToList();
			
//            if ( toUpdate.DefaultSettings.Name.IsNullOrEmpty() )
//            {
//                toUpdate.DefaultSettings.Name = toUpdate.Name;			
//            }			
//            toUpdate.DefaultSettings.Phone = account.PhoneNumber;
			
//            Console.WriteLine( "SetSetting : " + toUpdate.DefaultSettings.ChargeType );
//        }
//    }
//}

