﻿#region

using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{ 
    [Route("/account/register", "POST")]
    public class RegisterAccount : BaseDto
    {
        public Guid AccountId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }
        [Required]
        public CountryISOCode Country { get; set; }

        public string Phone { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public string Language { get; set; }

		public bool IsConfirmed { get; set; }

        public string PayBack { get; set; }

        /// <summary>
        /// if null we will use the company settings to know if we need to send an email or a sms
        /// </summary>
        public ActivationMethod? ActivationMethod { get; set; }
    }
}