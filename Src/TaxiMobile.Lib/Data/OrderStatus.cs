namespace TaxiMobile.Lib.Data
{
	public class OrderStatus
	{
		public OrderStatus ()
		{
		}
		
		public int Id {
			get;
			set;
		}
		
		public double Longitude {
			get;
			set;
		}
		
		public double Latitude {
			get;
			set;
		}
		
		public WsStatus Status {
			get;
			set;
		}
		
		public enum WsStatus{
			wosNone,
			wosSCHED,
			wosCANCELLED,
			wosDONE,
			wosWAITING,
			wosASSIGNED,
			wosARRIVED,
			wosLOADED,
			wosNOSHOW,
			wosCANCELLED_DONE,
		}
	}
}

