using System.Runtime.Serialization;

using FluentValidation.Results;

namespace Conduit.Application.Exceptions;

[Serializable]
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message = "One or more validation failures have occurred.")
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}