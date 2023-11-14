using FluentValidation.Results;

namespace Conduit.Application.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();

    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}