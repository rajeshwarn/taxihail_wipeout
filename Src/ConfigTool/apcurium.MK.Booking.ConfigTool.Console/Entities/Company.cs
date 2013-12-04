using System;
using System.Collections.Generic;

using System.Linq;

namespace CustomerPortal.Web.Entities
{
    public class Company
    {
        public Company()
        {
            
			CompanySettings = new List<CompanySetting> ();            
            
        }


        public string Id { get; set; }
        public string CompanyKey { get; set; }        
        public string CompanyName { get; set; }

		public List<CompanySetting> CompanySettings { get; set; }


    }
    
}