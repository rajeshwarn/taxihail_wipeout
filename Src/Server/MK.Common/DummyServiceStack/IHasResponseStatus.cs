namespace ServiceStack.ServiceInterface.ServiceModel
{
    public interface IHasResponseStatus
    {
        ResponseStatus ResponseStatus { get; set; }
    }

    public class ErrorResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }

        private string[] _errorCodes;

        public string[] ErrorCodes
        {
            get
            {
                return _errorCodes ?? new[] { ResponseStatus.ErrorCode };
            }
            set { _errorCodes = value; }
        }
    }

    public class ResponseStatus
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string StackTrace { get; set; }
    }
}
