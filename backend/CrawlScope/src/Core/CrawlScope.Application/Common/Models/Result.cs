
namespace CrawlScope.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }

        protected Result(bool isSuccess, T? value, string? errorMessage)
        {
            if (isSuccess && errorMessage != null)
                throw new InvalidOperationException("Successful result cannot have an error message.");
            if (!isSuccess && errorMessage == null)
                throw new InvalidOperationException("Failed result must have an error message.");

            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }

    public class Result : Result<string>
    {
        protected Result(bool isSuccess, string? errorMessage) 
            : base(isSuccess, string.Empty, errorMessage)
        {
        }

        public static Result Success() => new(true, null);
        public new static Result Failure(string errorMessage) => new(false, errorMessage);
    }
}
