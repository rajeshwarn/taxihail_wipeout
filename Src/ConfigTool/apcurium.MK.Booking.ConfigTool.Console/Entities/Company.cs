using System;
using System.Collections.Generic;

using System.Linq;

namespace CustomerPortal.Web.Entities
{
    public class Company
    {
        public Company()
        {
            
            Settings = new Dictionary<string, string>();
            GraphicsPaths = new Dictionary<string, string>();
            
        }


        public string Id { get; set; }
        public string CompanyKey { get; set; }        
        public string CompanyName { get; set; }

        public Dictionary<string, string> Settings { get; set; }

        public Dictionary<string,string> GraphicsPaths { get; set; }

    }
    
}