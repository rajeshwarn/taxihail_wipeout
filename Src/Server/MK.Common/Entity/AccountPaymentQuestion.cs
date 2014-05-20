using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Entity
{
	public class AccountChargeQuestion
	{
        [Key]
        public Guid Id { get; set; }

		public string Question { get; set; }

		public string Answer { get; set; }

		public bool IsEnabled { get { return Question.HasValue(); } }
	}
}