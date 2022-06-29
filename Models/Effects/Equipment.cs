namespace ApiVrEdu.Models.Effects;

public class Equipment : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;

    public User User { get; set; }
    public TypeEffect? TypeEffect { get; set; }
}