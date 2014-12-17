﻿namespace apcurium.MK.Booking.Email
{
    public interface ITemplateService
    {
        string Find(string templateName, string languageCode = "en");
        string Render(string template, object data);
        string InlineCss(string body);
        string ImagePath(string imageName);
    }
}