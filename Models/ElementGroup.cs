namespace ApiVrEdu.Models;

public class ElementGroup : BaseModel
{
    public string Name { get; set; } = string.Empty;

    public List<Element> Elements { get; set; } = new();
}