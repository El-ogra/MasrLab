namespace MasrLab.Domain.Exceptions;

public class DuplicateVisitLabIdException : Exception
{
    public DuplicateVisitLabIdException()
    {
    }

    public DuplicateVisitLabIdException(string message)
        : base(message)
    {
    }

    public DuplicateVisitLabIdException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
