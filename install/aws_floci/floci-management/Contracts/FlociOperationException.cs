namespace floci_management.Contracts;

public class FlociOperationException : Exception
{
    public FlociOperationException(string message) : base(message)
    { }
}
