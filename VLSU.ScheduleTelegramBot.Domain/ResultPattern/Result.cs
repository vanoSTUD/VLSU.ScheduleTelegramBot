namespace VLSU.ScheduleTelegramBot.Domain.ResultPattern;

public class Result<TValue>
{
    private Result(TValue value)
    {
        Value = value;
    }

    private Result(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public TValue? Value { get; private set; }
    public string? ErrorMessage { get; private set; } = null;
    public bool IsSuccess => ErrorMessage == null;
    public bool IsFailure => ErrorMessage != null;

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static Result<TValue> Failure(string error)
    {
        return new Result<TValue>(error);
    }

    public static implicit operator Result<TValue>(string error) => new Result<TValue>(error);

    public static implicit operator Result<TValue>(TValue value) => new Result<TValue>(value);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<string, TResult> failure)
    {
        if (IsSuccess)
            return success(Value!);
        else
            return failure(ErrorMessage!);
    }
}
