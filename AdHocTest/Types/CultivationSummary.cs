using System.ComponentModel.DataAnnotations;

namespace AdHocTest.Types;

public class CultivationSummary
{
    [Key] public int id_cultivation { get; set; }       //Chave primária
    public string watering { get; set; }
    public string sunlight { get; set; }
    public string scientific_name { get; set; }
    
    public PlantSummary PlantSummary { get; set; }
}