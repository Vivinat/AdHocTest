﻿using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AdHocTest.Context;
using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;

namespace AdHocTest.Reports;

public class TwoOnOneReport
{
    private readonly DBContext _context;

    public TwoOnOneReport(DBContext context)
    {
        _context = context;
    }

    public async Task<List<dynamic>> GenerateReportAsync(string query)
    {
        var parts = query.Split(':');
        var mainTable = parts[0];
        var relatedTable1 = parts[1];
        var relatedTable2 = parts[2];
        var parameters = parts[3].Split(';').ToList();

        var tableMap = new Dictionary<string, IQueryable<object>>
        {
            { "plant", _context.plant },
            { "plantdetails", _context.plant_details },
            { "plantdangerous", _context.dangerous_plants },
            { "cultivation", _context.cultivation }
        };

        if (!tableMap.ContainsKey(mainTable) || !tableMap.ContainsKey(relatedTable1) || !tableMap.ContainsKey(relatedTable2))
        {
            throw new Exception("One or more tables not found in the context.");
        }

        var mainType = tableMap[mainTable].ElementType;
        var relatedType1 = tableMap[relatedTable1].ElementType;
        var relatedType2 = tableMap[relatedTable2].ElementType;

        Type related3Type;
        if (relatedTable2 == "plantdangerous")
        {
            related3Type = typeof(DangerousPlantsSummary);
        }
        else if (relatedTable2 == "plantdetails")
        {
            related3Type = typeof(PlantDetailsSummary);
        }
        else
        {
            related3Type = typeof(CultivationSummary);
        }

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

        parameters = parameters.Where(p => !string.IsNullOrEmpty(p)).ToList();
        
        foreach (var parameter in parameters)
        {
            var key = parameter.Split('=')[0];
            var value = parameter.Split('=')[1];

            var prop = mainType.GetProperty(key) ?? relatedType1.GetProperty(key) ?? relatedType2.GetProperty(key);
            if (prop != null)
            {
                if (!string.IsNullOrEmpty(predicate))
                {
                    predicate += " AND ";
                }
                predicate += $"{prop.DeclaringType.Name}.{key} == @{values.Count}";
            }
            else
            {
                throw new Exception($"Property {key} not found in {mainTable}, {relatedTable1}, or {relatedTable2}");
            }

            values.Add(value);
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
