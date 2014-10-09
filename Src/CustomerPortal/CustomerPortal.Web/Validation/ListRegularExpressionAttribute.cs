#region

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#endregion

namespace CustomerPortal.Web.Validation
{
    public class ListRegularExpressionAttribute : RegularExpressionAttribute
    {
        public ListRegularExpressionAttribute(string pattern)
            : base(pattern)
        {
        }

        public override bool IsValid(object value)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.Cast<object>().All(x => base.IsValid(x));
            }
            return base.IsValid(value);
        }
    }
}