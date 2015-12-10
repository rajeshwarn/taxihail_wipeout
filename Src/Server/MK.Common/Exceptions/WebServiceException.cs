using System;
using System.Collections.Generic;
using System.Net;
using apcurium.MK.Common.Extensions;

namespace MK.Common.Exceptions
{
    public class WebServiceException: Exception
    {
        private string _errorCode;
        private string _errorMessage;
        private string _serverStackTrace;

        public WebServiceException(string message):base(message)
        {
            
        }

        public int StatusCode { get; set; }

        public string ErrorCode
        {
            get
            {
                if (_errorCode == null)
                {
                    ParseResponseDto();
                }
                return _errorCode;
            }

        }

        public string ResponseDto { get; set; }

        public string ResponseBody { get; set; }

        public string StatusDescription { get; set; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        public string ServerStackTrace
        {
            get { return _serverStackTrace; }
        }

        private void ParseResponseDto()
        {
            string responseStatus;
            if (!TryGetResponseStatusFromResponseDto(out responseStatus))
            {
                if (!TryGetResponseStatusFromResponseBody(out responseStatus))
                {
                    _errorCode = StatusDescription;
                    return;
                }
            }

            var rsMap = responseStatus.FromJson<Dictionary<string, string>>();
            if (rsMap == null)
            {
                return;
            }
            
            rsMap.TryGetValue("ErrorCode", out _errorCode);
            rsMap.TryGetValue("Message", out _errorMessage);
            rsMap.TryGetValue("StackTrace", out _serverStackTrace);
        }

        private bool TryGetResponseStatusFromResponseDto(out string responseStatus)
        {
            responseStatus = string.Empty;
            try
            {
                if (ResponseDto == null)
                {
                    return false;
                }
                var jsv = ResponseDto.ToJson();
                var map = jsv.FromJson<Dictionary<string, string>>();

                return map.TryGetValue("ResponseStatus", out responseStatus);
            }
            catch
            {
                return false;
            }
        }

        private bool TryGetResponseStatusFromResponseBody(out string responseStatus)
        {
            responseStatus = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(ResponseBody))
                {
                    return false;
                }
                var map = ResponseBody.FromJson<Dictionary<string, string>>();

                return map.TryGetValue("ResponseStatus", out responseStatus);
            }
            catch
            {
                return false;
            }
        }
    }
}