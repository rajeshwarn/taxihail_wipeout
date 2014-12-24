﻿#region

using System.ComponentModel.DataAnnotations;

#endregion
// ReSharper disable once CheckNamespace
namespace CustomerPortal.Web.Entities
{
    public class IBSSettings
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ServiceUrl { get; set; }

        [Required]
        public string RestApiUrl { get; set; }

        [Required]
        public string RestApiUser { get; set; }

        [Required]
        public string RestApiSecret { get; set; }        
    }
}