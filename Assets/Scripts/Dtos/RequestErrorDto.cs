using UnityEditor.PackageManager.Requests;

public class RequestErrorDto
{
    public RequestErrorDto(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }
}