using System;

namespace apcurium.MK.Common.Entity
{
    public class Gratuity
    {
		public Guid AccountId { get; set; }

        public Guid OrderId { get; set; }
        
        public int Percentage { get; set; }
        public static int[] GratuityOptions { get { return new int[] { 0, 5, 10, 15 }; } }
    }
}