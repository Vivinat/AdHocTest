using System.ComponentModel.DataAnnotations;

namespace AdHocTest.Types;

public enum Growth_Rate
{
    nothing,
    low,
    medium,
    moderate,
    high
}

public class PlantDetailsSummary
{
    [Key]public int id_detail{ get; set; }      //Chave primária
    public bool edible_fruit { get; set; }
    public Growth_Rate growth_rate { get; set; }
    public bool invasive { get; set; }
    public bool indoor { get; set; }
    public string scientific_name{ get; set; }
    public bool medicinal { get; set; }
    
    public PlantSummary PlantSummary { get; set; }
}