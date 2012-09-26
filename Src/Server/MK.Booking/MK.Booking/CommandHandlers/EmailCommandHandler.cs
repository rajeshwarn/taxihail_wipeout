using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class EmailCommandHandler : ICommandHandler<SendPasswordResetEmail>, ICommandHandler<SendAccountConfirmationEmail>
    {
        const string ApplicationNameSetting = "TaxiHail.ApplicationName";
        const string PasswordResetTemplateName = "PasswordReset";
        const string AccountConfirmationTemplateName = "AccountConfirmation";
        const string PasswordResetEmailSubject = "{{ ApplicationName }} - Your password has been reset";
        const string AccountConfirmationEmailSubject = "Welcome to {{ ApplicationName }}";

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
                                   };
            
            SendEmail(command.EmailAddress, template, AccountConfirmationEmailSubject, templateData);
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
