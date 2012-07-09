namespace TaxiMobile.Lib.Data
{
	public class BookingSetting
	{
		public BookingSetting ()
		{
			NumberOfTaxi =1;
		}
		
		public string LastName {
			get;
			set;
		}

        public string FirstName
        {
            get;
            set;
        }
		
		public string Phone {
			get;
			set;
		}
		
		public int Passengers {
			get;
			set;
		}
		
		public string DefaultOrigin {
			get;
			set;
		}
		
		public string PickupTime {
			get;
			set;
		}
		
		public int VehicleType {
			get;
			set;
		}
		
		public string VehicleTypeName {
			get;
			set;
		}
		
		public int ChargeType {
			get;
			set;
		}
		
		public string ChargeTypeName {
			get;
			set;
		}
		
		public int Company {
			get;
			set;
		}
		
		public string CompanyName {
			get;
			set;
		}
		
		public int NumberOfTaxi {
			get;
			set;
		}
		
		
	
		public BookingSetting Copy()
		{
			var copy = new BookingSetting();
			copy.LastName = LastName;
			copy.Phone = Phone;
			copy.Passengers = Passengers;
			copy.DefaultOrigin = DefaultOrigin;
			copy.PickupTime = PickupTime;
			copy.VehicleType = VehicleType;
			copy.VehicleTypeName = VehicleTypeName;
			copy.ChargeType = ChargeType;		
			copy.ChargeTypeName = ChargeTypeName;		
			copy.Company = Company;		
			copy.CompanyName = CompanyName;
			copy.NumberOfTaxi = NumberOfTaxi;
			return copy;
		}
	}
}

