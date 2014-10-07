#region

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#endregion

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
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
    }
}