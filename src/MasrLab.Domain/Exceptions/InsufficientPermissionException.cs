namespace MasrLab.Domain.Exceptions;

public class InsufficientPermissionException : Exception
{
    public InsufficientPermissionException()
        : base("لا توجد لديك صلاحية لتنفيذ هذا الإجراء.")
    {
    }

    public InsufficientPermissionException(string message)
        : base(message)
    {
    }

    public InsufficientPermissionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
