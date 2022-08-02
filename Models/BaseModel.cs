using System.Text.Json.Serialization;

namespace ApiVrEdu.Models;

public class BaseModel
{
    public int Id { get; set; }

    [JsonIgnore] public DateTime CreatedDate { get; set; }

    [JsonIgnore] public DateTime UpdatedDate { get; set; }

    public bool IsActivated { get; set; }
}