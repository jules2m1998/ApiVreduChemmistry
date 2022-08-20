namespace ApiVrEdu.Models.Elements;

public class ElementType : BaseModel
{
    public string Name { get; set; }

    public List<Element> Elements { get; set; }
    public User User { get; set; }
}