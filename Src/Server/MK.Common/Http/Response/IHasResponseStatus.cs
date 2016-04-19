namespace apcurium.MK.Common.Http.Response
{
    public interface IHasResponseStatus
    {
        ResponseStatus ResponseStatus { get; set; }
    }

    public class ErrorResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }

        public ValidationResponseStatus ValidationResponseStatus { get; set; }
    }

    public class ValidationResponseStatus
    {
        public string[] ValidationErrorCodes { get; set; }
    }

    public class ResponseStatus
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string StackTrace { get; set; }
    }
}
