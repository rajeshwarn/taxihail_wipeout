using System;
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
        const string PasswordResetTemplateName = "PasswordReset";
        const string AccountConfirmationTemplateName = "AccountConfirmation";
        const string PasswordResetEmailSubject = "Your password has been reset";
        const string AccountConfirmationEmailSubject = "Welcome to Taxi Hail";
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
             
            var messageBody = _templateService.Render(template, new {command.Password});

            var mailMessage = new MailMessage(from: _configurationManager.GetSetting("Email.NoReply"),
                                              to: command.EmailAddress,
                                              subject: PasswordResetEmailSubject,
                                              body: messageBody) { IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
            _emailSender.Send(mailMessage);
        }

        public void Handle(SendAccountConfirmationEmail command)
        {
            var template = _templateService.Find(AccountConfirmationTemplateName);
            if (template == null) throw new InvalidOperationException("Template not found: " + AccountConfirmationTemplateName);

            var messageBody = _templateService.Render(template, command);

            var mailMessage = new MailMessage(from: _configurationManager.GetSetting("Email.NoReply"),
                                              to: command.EmailAddress,
                                              subject: AccountConfirmationEmailSubject,
                                              body: messageBody) { IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
            _emailSender.Send(mailMessage);
        }
    }
}
