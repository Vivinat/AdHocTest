using AdHocTest.Context;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AdHocTest.Types;

namespace AdHocTest.Reports;

public class AdHocReport
{
    private readonly DBContext _context;

    public AdHocReport(DBContext context)
    {
        _context = context;
    }
    public async Task<List<dynamic>> GenerateReportAsync(string query)
    {
        var parts = query.Split(':');
        var mainTable = parts[0];
        var relatedTable = parts[1];
        var parameters = parts[2].Split(';');

        IQueryable<dynamic> queryable = null;

        switch (mainTable.ToLower())
        {
            case "plant":
                switch (relatedTable.ToLower())
                {
                    case "plantdetails":
                        queryable = _context.plant
                            .Join(_context.plant_details,
                                plant => plant.scientific_name,
                                detail => detail.scientific_name,
                                (plant, detail) => new { Plant = plant, Related = detail });
                        break;
                    case "plantdangerous":
                        queryable = _context.plant
                            .Join(_context.dangerous_plants,
                                plant => plant.scientific_name,
                                danger => danger.scientific_name,
                                (plant, danger) => new { Plant = plant, Related = danger });
                        break;
                    case "cultivation":
                        queryable = _context.plant
                            .Join(_context.cultivation,
                                plant => plant.scientific_name,
                                cultivation => cultivation.scientific_name,
                                (plant, cultivation) => new { Plant = plant, Related = cultivation });
                        break;
                    // Adicione outras tabelas relacionadas conforme necessário
                }
                break;
            case "plantdangerous":
                switch (relatedTable.ToLower())
                {
                    case "plant":
                        queryable = _context.dangerous_plants
                            .Join(_context.plant,
                                danger => danger.scientific_name,
                                plant => plant.scientific_name,
                                (danger, plant) => new { Plant = plant, Related = danger });
                        break;
                    // Adicione outras tabelas relacionadas conforme necessário
                }
                break;
            
            case "cultivation":
                switch (relatedTable.ToLower())
                {
                    case "plant":
                        queryable = _context.cultivation
                            .Join(_context.plant,
                                cult => cult.scientific_name,
                                plant => plant.scientific_name,
                                (cult, plant) => new { Plant = plant, Related = cult });
                        break;
                    // Adicione outras tabelas relacionadas conforme necessário
                }
                break;
            
            case "plantdetails":
                switch (relatedTable.ToLower())
                {
                    case "plant":
                        queryable = _context.plant_details
                            .Join(_context.plant,
                                details => details.scientific_name,
                                plant => plant.scientific_name,
                                (details, plant) => new { Plant = plant, Related = details });
                        break;
                    // Adicione outras tabelas relacionadas conforme necessário
                }
                break;
            
            // Adicione outros casos para outras tabelas principais
        }

        var whereClauses = new List<string>();
        var values = new List<object>();

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var key = p.Split('=')[0];
            var value = p.Split('=')[1];
            
            var plantType = queryable.ElementType.GetProperty("Plant").PropertyType;
            var relatedType = queryable.ElementType.GetProperty("Related").PropertyType;
            
            if (plantType.GetProperty(key) != null)
            {
                whereClauses.Add($"Plant.{key} == @{i}");
            }
            else if (relatedType.GetProperty(key) != null)
            {
                whereClauses.Add($"Related.{key} == @{i}");
            }
            else
            {
                throw new Exception($"Property {key} not found in either {mainTable} or {relatedTable}");
            }
            
            if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Growth_Rate), value, true));
            }
            else if (key.Equals("care_level", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Care_Level), value, true));
            }
            else
            {
                values.Add(value);
            }
        }

        var whereClause = string.Join(" AND ", whereClauses);

        // Aplique a cláusula where dinâmica
        queryable = queryable.Where(whereClause, values.ToArray());

        return await queryable.ToDynamicListAsync();
    }
}


