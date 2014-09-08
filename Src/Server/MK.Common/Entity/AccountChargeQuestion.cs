using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Entity
{
	public class AccountChargeQuestion
	{
        [Key, Column(Order = 1)]
        public int Id { get; set; }

        [Key, Column(Order = 2)]
        public Guid AccountId { get; set; }

		public string Question { get; set; }

		public string Answer { get; set; }

		public bool IsEnabled { get { return Question.HasValue(); } }

	    public bool IsRequired { get; set; }

        public bool IsCaseSensitive { get; set; }

		public int? MaxLength { get; set; }
	}
}