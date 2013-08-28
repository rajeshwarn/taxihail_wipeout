using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class EmailCommandHandler : ICommandHandler<SendPasswordResetEmail>, 
        ICommandHandler<SendAccountConfirmationEmail>,
        ICommandHandler<SendBookingConfirmationEmail>,
        ICommandHandler<SendReceipt> 
    {
        const string ApplicationNameSetting = "TaxiHail.ApplicationName";
        const string AccentColorSetting = "TaxiHail.AccentColor";
        const string VATEnabledSetting = "VATIsEnabled";
        const string VATPercentageSetting = "VATPercentage";
        const string VATRegistrationNumberSetting = "VATRegistrationNumber";

        const string PasswordResetTemplateName = "PasswordReset"; 
        const string PasswordResetEmailSubject = "{{ ApplicationName }} - Your password has been reset";
        
        const string AccountConfirmationTemplateName = "AccountConfirmation"; 
        const string AccountConfirmationEmailSubject = "Welcome to {{ ApplicationName }}"; 
        
        const string ReceiptEmailSubject = "{{ ApplicationName }} - Receipt";
        const string ReceiptTemplateName = "Receipt";
        const string ReceiptWithVATTemplateName = "ReceiptWithVAT";

        const string BookingConfirmationTemplateName = "BookingConfirmation";
        const string BookingConfirmationEmailSubject = "{{ ApplicationName }} - Booking confirmation";
        

        private readonly IConfigurationManager _configurationManager;
        private readonly ITemplateService _templateService;
        private readonly IEmailSender _emailSender;

        public EmailCommandHandler(IConfigurationManager configurationManager, ITemplateService templateService, IEmailSender emailSender)
        {
            _configurationManager = configurationManager;
            _templateService = templateService;
            _emailSender = emailSender;
        }

        public void Handle(SendPasswordResetEmail command)
        {
            var template = _templateService.Find(PasswordResetTemplateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + PasswordResetTemplateName);

            var templateData = new {
                                       command.Password,
                                       ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),

                                   };

            SendEmail(command.EmailAddress, template, PasswordResetEmailSubject, templateData);
        }

        public void Handle(SendAccountConfirmationEmail command)
        {
            var template = _templateService.Find(AccountConfirmationTemplateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + AccountConfirmationTemplateName);

            var templateData = new {
                                       command.ConfirmationUrl,
                                       ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                                       AccentColor = _configurationManager.GetSetting(AccentColorSetting)
                                   };
            
            SendEmail(command.EmailAddress, template, AccountConfirmationEmailSubject, templateData);
        }

        public void Handle(SendBookingConfirmationEmail command)
        {
            var template = _templateService.Find(BookingConfirmationTemplateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + BookingConfirmationTemplateName);

            var hasDropOffAddress = command.DropOffAddress != null && !string.IsNullOrWhiteSpace(command.DropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                IBSOrderId = command.IBSOrderId,
                PickupDate = command.PickupDate.ToString("dddd, MMMM d"),
                PickupTime = command.PickupDate.ToString("t" /* Short time pattern */),
                PickupAddress = command.PickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? command.DropOffAddress.DisplayAddress : "-",
                /* Mandatory settings */
                Name = command.Settings.Name,
                Phone = command.Settings.Phone,
                Passengers = command.Settings.Passengers,
                VehicleType = command.Settings.VehicleType,
                ChargeType = command.Settings.ChargeType,
                /* Optional settings */
                Note = string.IsNullOrWhiteSpace(command.Note) ? "-" : command.Note,
                Apartment = string.IsNullOrWhiteSpace(command.PickupAddress.Apartment) ? "-" : command.PickupAddress.Apartment,
                RingCode = string.IsNullOrWhiteSpace(command.PickupAddress.RingCode) ? "-" : command.PickupAddress.RingCode,
            };

            SendEmail(command.EmailAddress, template, BookingConfirmationEmailSubject, templateData);
        }

        public void Handle(SendReceipt command)
        {
            var vatEnabled = _configurationManager.GetSetting(VATEnabledSetting, false);
            var templateName = vatEnabled 
                                    ? ReceiptWithVATTemplateName 
                                    : ReceiptTemplateName;

            var template = _templateService.Find(templateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + templateName);

            var priceFormat = CultureInfo.GetCultureInfo(_configurationManager.GetSetting("PriceFormat"));

            var vatAmount = 0d;
            var fareAmountWithoutVAT = command.Fare;
            if (vatEnabled)
            {
                fareAmountWithoutVAT = command.Fare / (1 + _configurationManager.GetSetting<double>(VATPercentageSetting, 0)/100);
                vatAmount = command.Fare - fareAmountWithoutVAT;
            }

            var templateData = new {
                                       ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                                       AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                                       IBSOrderId = command.IBSOrderId,
                                       VehicleNumber = command.VehicleNumber,
                                       Date = command.TransactionDate.ToString("dddd, MMMM d, yyyy"),
                                       Fare = fareAmountWithoutVAT.ToString("C", priceFormat),
                                       Toll = command.Toll.ToString("C", priceFormat),
                                       Tip = command.Tip.ToString("C", priceFormat),
                                       TotalFare = command.TotalFare.ToString("C", priceFormat),
                                       Note = _configurationManager.GetSetting("Receipt.Note"),
                                       VATAmount = vatAmount.ToString("C", priceFormat),
                                       VATRegistrationNumber = _configurationManager.GetSetting(VATRegistrationNumberSetting)
                                   };

            SendEmail(command.EmailAddress, template, ReceiptEmailSubject, templateData);
        }

        private void SendEmail(string to, string bodyTemplate, string subjectTemplate, object templateData, params KeyValuePair<string,string>[] embeddedIMages)
        {
            var messageSubject = _templateService.Render(subjectTemplate, templateData);
            var messageBody = _templateService.Render(bodyTemplate, templateData);

            var mailMessage = new MailMessage(@from: _configurationManager.GetSetting("Email.NoReply"),
                                              to: to,
                                              subject: messageSubject,
                                              body:null)
                                  {IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8};

            var view = AlternateView.CreateAlternateViewFromString(messageBody, Encoding.UTF8, "text/html");
            mailMessage.AlternateViews.Add(view);

            if(embeddedIMages != null)
            {
                foreach (var image in embeddedIMages)
                {
                    var linkedImage = new LinkedResource(image.Value) { ContentId = image.Key };
                    view.LinkedResources.Add(linkedImage);
                }
            }
           
            _emailSender.Send(mailMessage);
        }
    }
}
