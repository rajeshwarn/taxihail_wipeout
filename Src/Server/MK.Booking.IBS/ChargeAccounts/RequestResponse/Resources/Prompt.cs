using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources
{
    [DataContract]
    public class Prompt
    {
        [DataMember(Name = "caption")]
        public String Caption { get; set; }

        [DataMember(Name = "length")]
        public Int16 Length { get; set; }

        [DataMember(Name = "prompt_number")]
        public Int16 PromptNumber { get; set; }

        [DataMember(Name = "type")]
        public String Type { get; set; }

        [DataMember(Name = "to_be_validated")]
        public Boolean ToBeValidated { get; set; }

        // MKTAXI-2652 - at this time it will not be stored in IBS but having it as part of the contract will allow this to be changed in the future
        [DataMember(Name = "taxihail_save_response")]
        public Boolean SaveResponse { get; set; }
    }
}
