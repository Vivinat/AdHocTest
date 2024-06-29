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

        var mainScientificNameProperty = mainType.GetProperty("scientific_name");
        var related1ScientificNameProperty = relatedType1.GetProperty("scientific_name");
        var related2ScientificNameProperty = relatedType2.GetProperty("scientific_name");

        var queryable = tableMap[mainTable]
            .Join(tableMap[relatedTable1], main => EF.Property<string>(main, "scientific_name"), related1 => EF.Property<string>(related1, "scientific_name"), (main, related1) => new { Main = main, Related1 = related1 })
            .Join(tableMap[relatedTable2], joined => EF.Property<string>(joined.Main, "scientific_name"), related2 => EF.Property<string>(related2, "scientific_name"), (joined, related2) => new { joined.Main, joined.Related1, Related2 = related2 });
        
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
        
        foreach (var parameter in parameters)
        {
            var key = parameter.Split('=')[0];
            var value = parameter.Split('=')[1];

            var mainProp = mainType.GetProperty(key);
            var related1Prop = relatedType1.GetProperty(key);
            var related2Prop = relatedType2.GetProperty(key);

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
            else
            {
                throw new Exception($"Property {key} not found in either {mainTable}, {relatedTable1}, or {relatedTable2}");
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
            }
            resultList.Add(expando);
        }

        return resultList;
    }
}
