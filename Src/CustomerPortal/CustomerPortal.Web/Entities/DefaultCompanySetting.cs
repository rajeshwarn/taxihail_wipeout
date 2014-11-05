using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoRepository;

namespace CustomerPortal.Web.Entities
{
    public class DefaultCompanySetting : IEntity
    {
        public string Key { get { return Id; } }
        public string Value { get; set; }
        public bool IsClient  { get; set; }        
        public string Id { get; set; }
    }
}