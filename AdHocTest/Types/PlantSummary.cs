using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdHocTest.Types;

public class PlantSummary
{
    public int id_plant { get; set; }
    public string common_name { get; set; }
    [Key][DatabaseGenerated(DatabaseGeneratedOption.None)]public string scientific_name { get; set; }
    
    public ICollection<PlantDetailsSummary> PlantDetails { get; set; }
    public ICollection<DangerousPlantsSummary> PlantDangerous { get; set; }
    public ICollection<CultivationSummary> Cultivations { get; set; }
}