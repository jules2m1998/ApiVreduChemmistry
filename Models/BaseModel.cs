using System.ComponentModel;

namespace ApiVrEdu.Models;

public class BaseModel
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    [DefaultValue(false)] public bool IsActivated { get; set; }
}