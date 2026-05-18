namespace Giglio.EduCore.Financial.Application.Commands.Payments;

public interface IResult
{
    bool IsSuccess { get; }
    object? Data { get; }
    string? Error { get; }
    int StatusCode { get; }
}

public class Result : IResult
{
    public bool IsSuccess { get; private set; }
    public object? Data { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }

    private Result() { }

    public static Result Success(object? data = null) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = 200
    };

    public static Result Created(object? data = null) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = 201
    };

    public static Result Fail(string error, int statusCode = 400) => new()
    {
        IsSuccess = false,
        Error = error,
        StatusCode = statusCode
    };
}
