namespace ApiVrEdu.Models.Elements;

public class ElementChildren
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int Position { get; set; }

    public Element Parent { get; set; }
    public Element Children { get; set; }
}