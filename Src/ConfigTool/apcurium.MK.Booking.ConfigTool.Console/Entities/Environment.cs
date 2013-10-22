using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoRepository;

namespace CustomerPortal.Web.Entities
{
    public class Environment : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}