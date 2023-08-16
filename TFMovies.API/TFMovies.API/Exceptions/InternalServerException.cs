namespace TFMovies.API.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException()
    { }
    public InternalServerException(string message) : base(message)
    { }
    public InternalServerException(string message, Exception innerException) : base(message, innerException)
    { }
}
