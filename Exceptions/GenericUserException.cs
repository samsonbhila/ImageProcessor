namespace ImageProcessor.Exceptions;

public class GenericUserException: Exception
{
    public int code { get; set; }
    public GenericUserException(string message, int statusCode = StatusCodes.Status500InternalServerError) : base(message)
    {
        code = statusCode;
    }
}