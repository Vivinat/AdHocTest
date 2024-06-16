using System.ComponentModel.DataAnnotations;

namespace AdHocTest.Types;

public enum carelevel
{
    nothing,
    low,
    medium,
    moderate,
    high
}
public class DangerousPlantsSummary
{
    [Key]public int id_dangerousp { get; set; }     //Chave primária
    public carelevel care_level { get; set; }
    public bool poisonous_to_pets { get; set; }
    public string scientific_name { get; set; }
    public PlantSummary PlantSummary { get; set; }
}