#region

using System.ComponentModel.DataAnnotations;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Entities
{
    public class Revision : IEntity
    {
        [Display(Name = "Tag Name")]
        public string Tag { get; set; }

        [Display(Name = "Commit")]
        public string Commit { get; set; }

        [Display(Name = "Hidden")]
        public bool Hidden { get; set; }

        public string Id { get; set; }
        
        [Display(Name = "Customer Visible")]
        public bool CustomerVisible { get; set; }

        [Display(Name = "Inactive")]
        public bool Inactive{ get; set; }
    }
}