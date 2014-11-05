#region

using System.IO;

#endregion

namespace Cupertino.Model
{
    public class DownloadProfileResponse
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}