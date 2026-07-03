using System.Net;

namespace LeaveManagementSystem.Domain.Common;

public class ApiResponse
{
    public bool IsSuccess { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = [];

    public static ApiResponse Success(
        string message = "Request completed successfully.",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse
        {
            IsSuccess = true,
            StatusCode = (int)statusCode,
            Message = message
        };
    }

    public static ApiResponse Failure(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        List<string>? errors = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = (int)statusCode,
            Message = message,
            Errors = errors ?? []
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Success(
        T? data,
        string message = "Request completed successfully.",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = (int)statusCode,
            Message = message,
            Data = data
        };
    }

    public new static ApiResponse<T> Failure(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = (int)statusCode,
            Message = message,
            Errors = errors ?? []
        };
    }
}