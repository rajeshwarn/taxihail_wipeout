using System;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Response;

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

        private string[] _errorCodes;
        public string[] ErrorCodes 
        {
            get
            {
                if (_errorCodes == null)
                {
                    ParseResponseDto();
                }

                return _errorCodes;
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (_errorMessage == null)
                {
                    ParseResponseDto();
                }
                return _errorMessage;
            }
        }

        public string ServerStackTrace
        {
            get
            {
                if (_serverStackTrace == null)
                {
                    ParseResponseDto();
                }
                return _serverStackTrace;
            }
        }

        private void ParseResponseDto()
        {
            string responseStatus;
            if (!TryGetResponseStatusFromResponseDto(out responseStatus))
            {
                if (!TryGetResponseStatusFromResponseBody(out responseStatus))
                {
                    _errorCode = StatusDescription;
                    _errorCodes = new[] {StatusDescription};
                    return;
                }
            }

            var rsMap = responseStatus.FromJson<Dictionary<string, string>>();
            if (rsMap == null)
            {
                return;
            }
            
            rsMap.TryGetValue("errorCode", out _errorCode);
            rsMap.TryGetValue("message", out _errorMessage);
            rsMap.TryGetValue("stackTrace", out _serverStackTrace);
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
				var map = ResponseBody.FromJson<ErrorResponse>();
                if (map.ValidationResponseStatus != null)
                {
                    _errorCodes = map.ValidationResponseStatus.ValidationErrorCodes;
                }
				responseStatus = map.ResponseStatus.ToJson();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}