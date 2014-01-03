#region

using System.ComponentModel.DataAnnotations;

#endregion
// ReSharper disable once CheckNamespace
namespace CustomerPortal.Web.Entities
{
    public class Revision
    {
        public string Id { get; set; }

        [Display(Name = "Tag Name")]
        public string Tag { get; set; }

        [Display(Name = "Commit")]
        public string Commit { get; set; }

        [Display(Name = "Hidden")]
        public bool Hidden { get; set; }
    }
}