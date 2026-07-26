namespace MasrLab.Domain.Exceptions;

public class DuplicateLabIdException : Exception
{
    public DuplicateLabIdException()
    {
    }

    public DuplicateLabIdException(string message)
        : base(message)
    {
    }

    public DuplicateLabIdException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
