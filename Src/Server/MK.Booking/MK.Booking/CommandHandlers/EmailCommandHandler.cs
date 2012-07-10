using System;
using System.Net.Mail;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class EmailCommandHandler : ICommandHandler<SendPasswordResettedEmail>
    {
        const string PasswordResettedTemplateName = "PasswordResetted";
        const string PasswordResettedEmailSubject = "Your password has been resetted";
        private readonly IConfigurationManager _configurationManager;
        private readonly ITemplateService _templateService;
        private readonly IEmailSender _emailSender;

        public EmailCommandHandler(IConfigurationManager configurationManager, ITemplateService templateService, IEmailSender emailSender)
        {
            _configurationManager = configurationManager;
            _templateService = templateService;
            _emailSender = emailSender;
        }

        public void Handle(SendPasswordResettedEmail command)
        {
            var template = _templateService.Find(PasswordResettedTemplateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + PasswordResettedTemplateName);
             
            var messageBody = _templateService.Render(template, new {command.Password});

            var mailMessage = new MailMessage(from: _configurationManager.GetSetting("Email.NoReply"),
                                              to: command.EmailAddress,
                                              subject: PasswordResettedEmailSubject,
                                              body: messageBody) { IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
            _emailSender.Send(mailMessage);
        }
    }
}
