using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdHocTest.Types;

public class plant
{
    public int id_plant { get; set; }
    public string common_name { get; set; }
    [Key][DatabaseGenerated(DatabaseGeneratedOption.None)]public string scientific_name { get; set; }
    
    public ICollection<plant_details> PlantDetails { get; set; }
    public ICollection<dangerous_plants> PlantDangerous { get; set; }
    public ICollection<cultivation> Cultivations { get; set; }
}