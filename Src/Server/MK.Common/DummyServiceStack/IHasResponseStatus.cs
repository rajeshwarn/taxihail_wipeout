namespace ServiceStack.ServiceInterface.ServiceModel
{
    public interface IHasResponseStatus
    {
        ResponseStatus ResponseStatus { get; set; }
    }

    public class ErrorResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ResponseStatus
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}
