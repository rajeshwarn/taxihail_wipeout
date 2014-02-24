#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class EmailCommandHandler : ICommandHandler<SendPasswordResetEmail>,
        ICommandHandler<SendAccountConfirmationEmail>,
        ICommandHandler<SendBookingConfirmationEmail>,
        ICommandHandler<SendAssignedConfirmation>,
        ICommandHandler<SendReceipt>
    {
        private const string ApplicationNameSetting = "TaxiHail.ApplicationName";
        private const string AccentColorSetting = "TaxiHail.AccentColor";
        private const string VATEnabledSetting = "VATIsEnabled";
        private const string VATPercentageSetting = "VATPercentage";
        private const string VATRegistrationNumberSetting = "VATRegistrationNumber";

        private const string PasswordResetTemplateName = "PasswordReset";
        private const string PasswordResetEmailSubject = "{{ ApplicationName }} - Your password has been reset";

        private const string AccountConfirmationTemplateName = "AccountConfirmation";
        private const string AccountConfirmationEmailSubject = "Welcome to {{ ApplicationName }}";

        private const string ReceiptEmailSubject = "{{ ApplicationName }} - Receipt";
        private const string ReceiptTemplateName = "Receipt";


        private const string BookingConfirmationTemplateName = "BookingConfirmation";
        private const string BookingConfirmationEmailSubject = "{{ ApplicationName }} - Booking confirmation";

        private const string DriverAssignedTemplateName = "DriverAssigned";
        private const string DriverAssignedWithVATTemplateName = "DriverAssignedWithVAT";
        private const string DriverAssignedEmailSubject = "{{ ApplicationName }} - Driver assigned confirmation";

        private readonly IConfigurationManager _configurationManager;
        private readonly IEmailSender _emailSender;
        private readonly ITemplateService _templateService;

        public EmailCommandHandler(IConfigurationManager configurationManager, ITemplateService templateService,
            IEmailSender emailSender)
        {
            _configurationManager = configurationManager;
            _templateService = templateService;
            _emailSender = emailSender;
        }

        public void Handle(SendAccountConfirmationEmail command)
        {
            var template = _templateService.Find(AccountConfirmationTemplateName);
            if (template == null)
                throw new InvalidOperationException("Template not found: " + AccountConfirmationTemplateName);

            var templateData = new
            {
                command.ConfirmationUrl,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting)
            };

            SendEmail(command.EmailAddress, template, AccountConfirmationEmailSubject, templateData);
        }

        public void Handle(SendAssignedConfirmation command)
        {
            var vatEnabled = _configurationManager.GetSetting(VATEnabledSetting, false);
            var templateName = vatEnabled
                ? DriverAssignedWithVATTemplateName
                : DriverAssignedTemplateName;

            var template = _templateService.Find(templateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + templateName);

            var priceFormat = CultureInfo.GetCultureInfo(_configurationManager.GetSetting("PriceFormat"));

            var vatAmount = 0d;
            var fareAmountWithoutVAT = command.Fare;
            if (vatEnabled)
            {
                fareAmountWithoutVAT = GetAmountWithoutVAT(command.Fare);
                vatAmount = command.Fare - fareAmountWithoutVAT;
            }

            var hasDropOffAddress = command.DropOffAddress != null &&
                                    !string.IsNullOrWhiteSpace(command.DropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                command.IBSOrderId,
                PickupDate = command.PickupDate.ToString("dddd, MMMM d"),
                PickupTime = command.PickupDate.ToString("t" /* Short time pattern */),
                PickupAddress = command.PickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? command.DropOffAddress.DisplayAddress : "-",
                command.Settings.Name,
                command.Settings.Phone,
                command.Settings.Passengers,
                command.Settings.VehicleType,
                command.Settings.ChargeType,
                Apartment =
                    string.IsNullOrWhiteSpace(command.PickupAddress.Apartment) ? "-" : command.PickupAddress.Apartment,
                RingCode =
                    string.IsNullOrWhiteSpace(command.PickupAddress.RingCode) ? "-" : command.PickupAddress.RingCode,
                command.VehicleNumber,
                TransactionDate = command.TransactionDate.ToString("dddd, MMMM d, yyyy"),
                TransactionTime = command.TransactionDate.ToString("t" /* Short time pattern */),
                Fare = fareAmountWithoutVAT.ToString("C", priceFormat),
                VATAmount = vatAmount.ToString("C", priceFormat),
                TotalFare = command.Fare.ToString("C", priceFormat)
            };

            SendEmail(command.EmailAddress, template, DriverAssignedEmailSubject, templateData);
        }

        public void Handle(SendBookingConfirmationEmail command)
        {
            var template = _templateService.Find(BookingConfirmationTemplateName);
            if (template == null)
                throw new InvalidOperationException("Template not found: " + BookingConfirmationTemplateName);

            var hasDropOffAddress = command.DropOffAddress != null &&
                                    !string.IsNullOrWhiteSpace(command.DropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                command.IBSOrderId,
                PickupDate = command.PickupDate.ToString("dddd, MMMM d"),
                PickupTime = command.PickupDate.ToString("t" /* Short time pattern */),
                PickupAddress = command.PickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? command.DropOffAddress.DisplayAddress : "-",
                /* Mandatory settings */
                command.Settings.Name,
                command.Settings.Phone,
                command.Settings.Passengers,
                command.Settings.VehicleType,
                command.Settings.ChargeType,
                /* Optional settings */
                command.Settings.LargeBags,
                Note = string.IsNullOrWhiteSpace(command.Note) ? "-" : command.Note,
                Apartment =
                    string.IsNullOrWhiteSpace(command.PickupAddress.Apartment) ? "-" : command.PickupAddress.Apartment,
                RingCode =
                    string.IsNullOrWhiteSpace(command.PickupAddress.RingCode) ? "-" : command.PickupAddress.RingCode,
                /* Mandatory visibility settings */
                VisibilityLargeBags = _configurationManager.GetSetting("Client.ShowLargeBagsIndicator", false) || command.Settings.LargeBags > 0                    
            };

            SendEmail(command.EmailAddress, template, BookingConfirmationEmailSubject, templateData);
        }

        public void Handle(SendPasswordResetEmail command)
        {
            var template = _templateService.Find(PasswordResetTemplateName);
            if (template == null)
                throw new InvalidOperationException("Template not found: " + PasswordResetTemplateName);

            var templateData = new
            {
                command.Password,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
            };

            SendEmail(command.EmailAddress, template, PasswordResetEmailSubject, templateData);
        }

        public void Handle(SendReceipt command)
        {
            var vatEnabled = _configurationManager.GetSetting(VATEnabledSetting, false);
            var template = _templateService.Find(ReceiptTemplateName);

            if (template == null) throw new InvalidOperationException("Template not found: " + ReceiptTemplateName);

            var priceFormat = CultureInfo.GetCultureInfo(_configurationManager.GetSetting("PriceFormat"));

            var isCardOnFile = command.CardOnFileInfo != null;
            var cardOnFileAmount = string.Empty;
            var cardNumber = string.Empty;
            var cardOnFileTransactionId = string.Empty;
            var cardOnFileAuthorizationCode = string.Empty;
            if (isCardOnFile)
            {
                cardOnFileAmount = command.CardOnFileInfo.Amount.ToString("C", priceFormat);
                cardNumber = command.CardOnFileInfo.Company;
                cardOnFileAuthorizationCode = command.CardOnFileInfo.AuthorizationCode;

                if (!string.IsNullOrWhiteSpace(command.CardOnFileInfo.LastFour))
                {
                    cardNumber += " XXXX " + command.CardOnFileInfo.LastFour;
                }

                if (!string.IsNullOrWhiteSpace(command.CardOnFileInfo.FriendlyName))
                {
                    cardNumber += " (" + command.CardOnFileInfo.FriendlyName + ")";
                }

                cardOnFileTransactionId = command.CardOnFileInfo.TransactionId;
            }

            var hasDropOffAddress = command.DropOffAddress != null &&
                                    !string.IsNullOrWhiteSpace(command.DropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                command.IBSOrderId,
                command.VehicleNumber,
                Date = command.TransactionDate.ToString("dddd, MMMM d, yyyy"),
                Fare = command.Fare.ToString("C", priceFormat),
                Toll = command.Toll.ToString("C", priceFormat),
                Tip = command.Tip.ToString("C", priceFormat),
                TotalFare = command.TotalFare.ToString("C", priceFormat),
                Note = _configurationManager.GetSetting("Receipt.Note"),
                VATAmount = command.Tax.ToString("C", priceFormat),
                VatEnabled = vatEnabled,
                VATRegistrationNumber = _configurationManager.GetSetting(VATRegistrationNumberSetting),
                IsCardOnFile = isCardOnFile,
                CardOnFileAmount = cardOnFileAmount,
                CardNumber = cardNumber,
                CardOnFileTransactionId = cardOnFileTransactionId,
                CardOnFileAuthorizationCode = cardOnFileAuthorizationCode,
                PickupAddress = command.PickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? command.DropOffAddress.DisplayAddress : "-",
            };


            SendEmail(command.EmailAddress, template, ReceiptEmailSubject, templateData);
        }

        private double GetAmountWithoutVAT(double totalAmount)
        {
            return totalAmount/(1 + _configurationManager.GetSetting<double>(VATPercentageSetting, 0)/100);
        }

        private void SendEmail(string to, string bodyTemplate, string subjectTemplate, object templateData,
            params KeyValuePair<string, string>[] embeddedIMages)
        {
            var messageSubject = _templateService.Render(subjectTemplate, templateData);
            var messageBody = _templateService.Render(bodyTemplate, templateData);

            var mailMessage = new MailMessage(_configurationManager.GetSetting("Email.NoReply"), to, messageSubject,
                null)
            {IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8};

            var view = AlternateView.CreateAlternateViewFromString(messageBody, Encoding.UTF8, "text/html");
            mailMessage.AlternateViews.Add(view);

            if (embeddedIMages != null)
            {
                foreach (var image in embeddedIMages)
                {
                    var linkedImage = new LinkedResource(image.Value) {ContentId = image.Key};
                    view.LinkedResources.Add(linkedImage);
                }
            }

            _emailSender.Send(mailMessage);
        }
    }
}