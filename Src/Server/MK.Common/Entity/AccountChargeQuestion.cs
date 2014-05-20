using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Entity
{
	public class AccountChargeQuestion
	{
        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Key, Column(Order = 2)]
        public Guid AccountId { get; set; }

		public string Question { get; set; }

		public string Answer { get; set; }

		public bool IsEnabled { get { return Question.HasValue(); } }
	}
}