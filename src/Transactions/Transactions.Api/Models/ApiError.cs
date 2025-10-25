public class ApiError
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    // Field-specific validation errors (key: property name, value: array of messages)
    public Dictionary<string, string[]>? Errors { get; set; }

    public ApiError(int statusCode, string message, Dictionary<string, string[]>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }
}
