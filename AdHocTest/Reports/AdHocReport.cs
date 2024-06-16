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

        var whereClauses = new List<string>();
        var values = new List<object>();

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var key = p.Split('=')[0];
            var value = p.Split('=')[1];
            whereClauses.Add($"Related.{key} == @{i}");
            if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Growth_Rate), value, true));
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

    private object ConvertParameter(string key, string value)
    {
        // Adapte este método para incluir todas as suas propriedades enum
        if (key.Equals("growthrate", StringComparison.OrdinalIgnoreCase))
        {
            return Enum.Parse(typeof(Growth_Rate), value.ToLower(), true);
        }

        if (key.Equals("carelevel", StringComparison.OrdinalIgnoreCase))
        {
            return Enum.Parse(typeof(carelevel), value.ToLower(), true);
        }

        // Adicione outros enums ou tipos de conversão conforme necessário

        return value; // Retorne o valor como string para outros tipos
    }
}


