#region

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#endregion
// ReSharper disable once CheckNamespace
namespace CustomerPortal.Web.Entities
{
    public class CompanyIdGenerator
    {
        public string Generate(string companyName)
        {
            if (companyName == null)
            {
                throw new ArgumentNullException("companyName");
            }

            companyName = RemoveDiacritics(companyName);
            companyName = Regex.Replace(companyName, @"[^A-Za-z0-9]+", string.Empty);
            if (string.IsNullOrEmpty(companyName))
            {
                throw new InvalidOperationException("Cannot generate id");
            }
            return companyName;
        }

        private static string RemoveDiacritics(string stIn)
        {
            var stFormD = stIn.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (var ich = 0; ich < stFormD.Length; ich++)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
    }
}