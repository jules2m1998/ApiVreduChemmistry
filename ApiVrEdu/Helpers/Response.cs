namespace ApiVrEdu.Helpers;

public class Response
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Errors { get; set; }
}