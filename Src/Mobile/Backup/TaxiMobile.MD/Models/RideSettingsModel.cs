using System.Collections.Generic;
using System.Linq;
using TaxiMobile.Lib.Data;

namespace TaxiMobile.Models
{
	public class RideSettingsModel
	{
		private ListItem[] _companyList;
		private ListItem[] _vehicleTypeList;
		private ListItem[] _chargeTypeList;
		
		public RideSettingsModel( BookingSetting data, ListItem[] companyList, ListItem[] vehicleTypeList, ListItem[] chargeTypeList )
		{
			_companyList = companyList;
			_vehicleTypeList = vehicleTypeList;
			_chargeTypeList = chargeTypeList;
			Data = data;
		}
		
		public BookingSetting Data {
			get;
			private set;
		}
		
		public string Name { 
			get { return Data.LastName; }
			set { Data.LastName = value; }
		}
		
		public string Phone {
			get { return Data.Phone; }
			set { Data.Phone = value; }
		}	
		
		public string NbOfPassenger { 
			get { return Data.Passengers.ToString(); }
			set {
				int nbPassengers;
				if( int.TryParse( value, out nbPassengers ) )
				{
					Data.Passengers = nbPassengers;
				}
			}
		}		
		
		public int VehicleType {
			get { return Data.VehicleType; }
			set { Data.VehicleType = value;
				if( VehicleTypeList != null )
				{
					var vehicleType = VehicleTypeList.SingleOrDefault( vt => vt.Id == VehicleType );
					Data.VehicleTypeName = vehicleType.Display;
				}
			}
		}	
	
		public string VehicleTypeName { 
			get { return Data.VehicleTypeName; }
			private set { Data.VehicleTypeName = value; }
		}	
		
		
		public int Company {
			get { return Data.Company; }
			set { Data.Company = value;
				if( CompanyList != null )
				{
					var company = CompanyList.SingleOrDefault( c => c.Id == Company );
					Data.CompanyName = company.Display;
				}
			}
		}	
	
		public string CompanyName { 
			get { return Data.CompanyName; }
			private set { Data.CompanyName = value; }
		}	
		
		public int ChargeType {
			get { return Data.ChargeType; }
			set { Data.ChargeType = value;
				if( ChargeTypeList != null )
				{
					var chargeType = ChargeTypeList.SingleOrDefault( ct => ct.Id == ChargeType );
					Data.ChargeTypeName = chargeType.Display;
				}
			}
		}				
		public string ChargeTypeName { 
			get { return Data.ChargeTypeName; }
			private set { Data.ChargeTypeName = value; }
		}	
			
		public ListItem[] CompanyList { get { return _companyList; } }
		public ListItem[] VehicleTypeList { get { return _vehicleTypeList;} }
		public ListItem[] ChargeTypeList { get { return _chargeTypeList;} }
		
		public List<string> ToStringList( ListItem[] listItem )
		{
			return listItem.Select( l => l.Display ).ToList();
		}
	}
}

