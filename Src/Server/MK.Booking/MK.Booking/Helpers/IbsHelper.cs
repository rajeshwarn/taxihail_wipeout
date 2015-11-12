using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;

namespace apcurium.MK.Booking.Helpers
{
    public static class IbsHelper
    {
        public static IbsOrderParams PrepareForIbsOrder(IBSSettingContainer ibsSettingsContainer, VehicleTypeDetail defaultVehicleType,
            int? chargeTypeId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            IList<ListItem> referenceDataCompanyList, string market, int? requestProviderId)
        {
            int? ibsChargeTypeId;

            if (chargeTypeId == ChargeTypes.CardOnFile.Id
                || chargeTypeId == ChargeTypes.PayPal.Id)
            {
                ibsChargeTypeId = ibsSettingsContainer.PaymentTypeCardOnFileId;
            }
            else if (chargeTypeId == ChargeTypes.Account.Id)
            {
                ibsChargeTypeId = ibsSettingsContainer.PaymentTypeChargeAccountId;
            }
            else
            {
                ibsChargeTypeId = ibsSettingsContainer.PaymentTypePaymentInCarId;
            }

            var ibsPickupAddress = Mapper.Map<IbsAddress>(pickupAddress);
            var ibsDropOffAddress = dropOffAddress != null && dropOffAddress.IsValid()
                ? Mapper.Map<IbsAddress>(dropOffAddress)
                : null;

            var customerNumber = GetCustomerNumber(accountNumberString, customerNumberString);

            var defaultVehicleTypeId = defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1;

            var defaultCompany = referenceDataCompanyList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                                 ?? referenceDataCompanyList.FirstOrDefault();

            var providerId = market.HasValue() && referenceDataCompanyList.Any() && defaultCompany != null
                ? defaultCompany.Id
                : requestProviderId;

            return new IbsOrderParams
            {
                CustomerNumber = customerNumber,
                DefaultVehicleTypeId = defaultVehicleTypeId,
                IbsChargeTypeId = ibsChargeTypeId,
                IbsPickupAddress = ibsPickupAddress,
                IbsDropOffAddress = ibsDropOffAddress,
                ProviderId = providerId
            };
        }

        public static string BuildNote(string noteTemplate, string chargeType, string note, string buildingName, int largeBags, bool hideChargeTypeInUserNote)
        {
            // Building Name is not handled by IBS
            // Put Building Name in note, if specified

            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                // Quickfix: If the address comes from our Google Places service
                // the building name will be formatted like this: "Building Name (Place Type)"
                // We need to remove the text in parenthesis

                const string pattern = @"
\(         # Look for an opening parenthesis
[^\)]+     # Take all characters that are not a closing parenthesis
\)$        # Look for a closing parenthesis at the end of the string";

                buildingName = new Regex(pattern, RegexOptions.IgnorePatternWhitespace)
                    .Replace(buildingName, string.Empty).Trim();
                buildingName = "Building name: " + buildingName;
            }

            var largeBagsString = string.Empty;
            if (largeBags > 0)
            {
                largeBagsString = "Large bags: " + largeBags;
            }

            if (!string.IsNullOrWhiteSpace(noteTemplate))
            {
                if (!hideChargeTypeInUserNote)
                {
                    noteTemplate = string.Format("{0}{1}{2}",
                        chargeType,
                        Environment.NewLine,
                        noteTemplate);
                }

                var transformedTemplate = noteTemplate
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("{{userNote}}", note ?? string.Empty)
                    .Replace("{{buildingName}}", buildingName ?? string.Empty)
                    .Replace("{{largeBags}}", largeBagsString)
                    .Trim();

                return transformedTemplate;
            }

            // In versions prior to 1.4, there was no note template
            // So if the IBS.NoteTemplate setting does not exist, use the old way 
            var formattedNote = string.Format("{0}{0}{1}{2}{3}",
                    Environment.NewLine,
                    chargeType,
                    Environment.NewLine,
                    note);

            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                formattedNote += (Environment.NewLine + buildingName).Trim();
            }
            // "Large bags" appeared in 1.4, no need to concat it here
            return formattedNote;
        }

        private static int? GetCustomerNumber(string accountNumber, string customerNumber)
        {
            if (!accountNumber.HasValue() || !customerNumber.HasValue())
            {
                return null;
            }

            int result;
            if (int.TryParse(customerNumber, out result))
            {
                return result;
            }

            return null;
        }
    }
}