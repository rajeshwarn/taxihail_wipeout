using MongoRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public class UserPreference : IEntity
    {
        public UserPreference()
        {
            LastAccessedCompany = new Dictionary<string, DateTime>();

        }

        public string Id { get; set; }
        
        public string UserIdentity{ get; set; }

        public string HomeViewSelected { get; set; }

        public Dictionary<string,DateTime> LastAccessedCompany { get; set; }
    }
}