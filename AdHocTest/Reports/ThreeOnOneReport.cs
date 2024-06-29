using System.Linq.Dynamic.Core;
using AdHocTest.Context;
using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;

namespace AdHocTest.Reports;

public class ThreeOnOneReport
{
    private readonly DBContext _context;
    public ThreeOnOneReport(DBContext context)
    {
        _context = context;
    }
    
    public async Task<List<dynamic>> GenerateReportAsync(string query)
    {
        var parts = query.Split(':');
        var mainTable = parts[0];
        var relatedTable1 = parts[1];
        var relatedTable2 = parts[2];
        var relatedTable3 = parts[3];
        var parameters = parts[4].Split(';').ToList();

        var tableMap = new Dictionary<string, IQueryable<object>>
        {
            { "plant", _context.plant },
            { "plantdetails", _context.plant_details },
            { "plantdangerous", _context.dangerous_plants },
            { "cultivation", _context.cultivation }
        };

        if (!tableMap.ContainsKey(mainTable) || !tableMap.ContainsKey(relatedTable1) || 
            !tableMap.ContainsKey(relatedTable2) || !tableMap.ContainsKey(relatedTable3))
        {
            throw new Exception("One or more tables not found in the context.");
        }

        var mainType = tableMap[mainTable].ElementType;
        var relatedType1 = tableMap[relatedTable1].ElementType;
        var relatedType2 = tableMap[relatedTable2].ElementType;
        var relatedType3 = tableMap[relatedTable3].ElementType;

        var queryable = _context.plant
            .Join(_context.cultivation, p => p.scientific_name, c => c.scientific_name, (p, c) => new { Main = p, Related1 = c })
            .Join(_context.plant_details, pc => pc.Main.scientific_name, pd => pd.scientific_name, (pc, pd) => new { pc.Main, pc.Related1, Related2 = pd })
            .Join(_context.dangerous_plants, pcd => pcd.Main.scientific_name, dp => dp.scientific_name, (pcd, dp) => new { pcd.Main, pcd.Related1, pcd.Related2, Related3 = dp });

        var predicate = string.Empty;
        var values = new List<object>();

        bool hasScientificName = parameters.Contains("@scientific_name");
        bool hasCommonName = parameters.Contains("@common_name");

        if (hasScientificName)
        {
            parameters.Remove("@scientific_name");
        }

        if (hasCommonName)
        {
            parameters.Remove("@common_name");
        }
        
        parameters = parameters.Where(p => !string.IsNullOrEmpty(p)).ToArray().ToList();

        foreach (var parameter in parameters)
        {
            var key = parameter.Split('=')[0];
            var value = parameter.Split('=')[1];

            var mainProp = mainType.GetProperty(key);
            var related1Prop = relatedType1.GetProperty(key);
            var related2Prop = relatedType2.GetProperty(key);
            var related3Prop = relatedType3.GetProperty(key);

            if (mainProp != null)
            {
                if (!string.IsNullOrEmpty(predicate))
                {
                    predicate += " AND ";
                }
                predicate += $"Main.{key} == @{values.Count}";
            }
            else if (related1Prop != null)
            {
                if (!string.IsNullOrEmpty(predicate))
                {
                    predicate += " AND ";
                }
                predicate += $"Related1.{key} == @{values.Count}";
            }
            else if (related2Prop != null)
            {
                if (!string.IsNullOrEmpty(predicate))
                {
                    predicate += " AND ";
                }
                predicate += $"Related2.{key} == @{values.Count}";
            }
            else if (related3Prop != null)
            {
                if (!string.IsNullOrEmpty(predicate))
                {
                    predicate += " AND ";
                }
                predicate += $"Related3.{key} == @{values.Count}";
            }
            else
            {
                throw new Exception($"Property {key} not found in {mainTable}, {relatedTable1}, {relatedTable2}, or {relatedTable3}");
            }

            if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Growth_Rate), value, true));
            }
            else if (key.Equals("care_level", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Care_Level), value, true));
            }
            else if (bool.TryParse(value, out var boolValue))
            {
                values.Add(boolValue);
            }
            else
            {
                values.Add(value);
            }
        }

        queryable = queryable.Where(predicate, values.ToArray());

        var requestedFields = parameters.Select(p => p.Split('=')[0]).ToList();
        if (hasScientificName)
        {
            requestedFields.Insert(0, "scientific_name");
        }
        if (hasCommonName)
        {
            requestedFields.Insert(0, "common_name");
        }

        var queryResults = await queryable.ToListAsync();

        var resultList = new List<dynamic>();

        foreach (var result in queryResults)
        {
            var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
            foreach (var field in requestedFields)
            {
                var mainProp = result.Main.GetType().GetProperty(field);
                var related1Prop = result.Related1.GetType().GetProperty(field);
                var related2Prop = result.Related2.GetType().GetProperty(field);
                var related3Prop = result.Related3.GetType().GetProperty(field);

                if (mainProp != null)
                {
                    expando.Add(field, mainProp.GetValue(result.Main));
                }
                else if (related1Prop != null)
                {
                    expando.Add(field, related1Prop.GetValue(result.Related1));
                }
                else if (related2Prop != null)
                {
                    expando.Add(field, related2Prop.GetValue(result.Related2));
                }
                else if (related3Prop != null)
                {
                    expando.Add(field, related3Prop.GetValue(result.Related3));
                }
            }
            resultList.Add(expando);
        }

        return resultList;
    }
}