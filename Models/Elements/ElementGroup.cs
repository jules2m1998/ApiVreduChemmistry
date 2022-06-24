namespace ApiVrEdu.Models.Elements;

public class ElementGroup : BaseModel
{
    public string Name { get; set; } = string.Empty;

    public List<Element> Elements { get; set; } = new();
    public User User { get; set; } = new();
}