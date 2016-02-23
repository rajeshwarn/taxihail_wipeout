using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Entity
{
    public class AccountChargeQuestionAnswer
	{
        [Key, Column(Order = 1)]
        public Guid AccountId { get; set; }

        [Key, Column(Order = 2)]
        public Guid AccountChargeId { get; set; }

        [Key, Column(Order = 3)]
        public int AccountChargeQuestionId { get; set; }

        public string LastAnswer { get; set; }
	}
}