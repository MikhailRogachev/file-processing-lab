namespace floci_management.Contracts;

public class InvalidMenuKeyException : Exception
{
    public InvalidMenuKeyException(string message) : base(message)
    {
    }
}
