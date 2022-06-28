namespace ApiVrEdu.Exceptions;

public class ExceptionResponse : Exception
{
    public int? StatusCode { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
}