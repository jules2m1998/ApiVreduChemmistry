namespace ApiVrEdu.Dto.Elements;

public class UpdateDto
{
    public string Name { get; set; } = string.Empty;

    public int Id { get; set; }

    public bool IsActivated { get; set; }
}