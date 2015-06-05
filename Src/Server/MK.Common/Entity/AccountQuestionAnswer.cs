using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Entity
{
    public class AccountQuestionAnswer
	{
        [Key, Column(Order = 1)]
        public Guid AccountId { get; set; }

        [Key, Column(Order = 2)]
        public int AccountChargeQuestionId { get; set; }

        public string LastAnswer { get; set; }
	}
}