#region

using System;

#endregion

namespace apcurium.MK.Common.Entity
{
    public class RatingType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
        public string Language { get; set; }
    }
}