namespace ApiVrEdu.Models.Effects;

public class TypeEffect : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Unity { get; set; } = string.Empty;
    public string UnitySymbol { get; set; } = string.Empty;

    public virtual User User { get; set; }
    public virtual List<Equipment> Equipments { get; set; } = new();
    public virtual List<Effect> Effects { get; set; } = new();
}