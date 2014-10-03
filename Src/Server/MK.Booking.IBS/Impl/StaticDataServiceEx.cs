using System.Net;

public class StaticDataserviceEx : StaticDataservice
{
    protected override WebResponse GetWebResponse(System.Net.WebRequest request)
    {
        var response = base.GetWebResponse(request);
        response.Headers["Content-Type"] = "text/xml; charset=utf-8"; //<==
        return response;
    }
}

