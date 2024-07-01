using System.Linq.Dynamic.Core;
using AdHocTest.Context;
using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;

namespace AdHocTest.Reports;

public class OneReport
{
    private readonly DBContext _context;

    public OneReport(DBContext context)
    {
        _context = context;
    }

    public async Task<List<dynamic>> GenerateReportAsync(string query)  //Trata a query string
    {
        var parts = query.Split(':');
        var mainTable = parts[0];
        var parameters = parts[1].Split(';');

        IQueryable<dynamic> queryable = null;

        switch (mainTable.ToLower())            //Qual a minha tabela principal?
        {
            case "cultivation":
                queryable = _context.cultivation;
                break;
            case "plantdetails":
                queryable = _context.plant_details;
                break;
            case "plantdangerous":
                queryable = _context.dangerous_plants;
                break;
            case "plant":
                queryable = _context.plant;
                break;
        }

        var predicate = "true";             //Prepara o predicado. Ele é o corpo da consulta
        var values = new List<object>();
        bool hasScientificName = false;
        bool hasCommonName = false;

        if (parameters.Contains("@scientific_name"))        //Scientific_name e common_name devem ser tratados de outra maneira por que nao envolvem comparações
        {
            hasScientificName = true;
            parameters = parameters.Where(p => !p.StartsWith("@scientific_name")).ToArray();
        }
        if (parameters.Contains("@common_name"))
        {
            hasCommonName = true;
            parameters = parameters.Where(p => !p.StartsWith("@common_name")).ToArray();
        }

        parameters = parameters.Where(p => !string.IsNullOrEmpty(p)).ToArray();

        foreach (var parameter in parameters)
        {
            var key = parameter.Split('=')[0];          //O nome do parametro
            var value = parameter.Split('=')[1];        //O valor que quero para este parametro

            if (!string.IsNullOrEmpty(predicate))
            {
                predicate += " && ";
            }

            if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase))      //Tratamentos de Enums
            {
                values.Add(Enum.Parse(typeof(Growth_Rate), value, true));
                predicate += $"{key} == @{values.Count - 1}";
            }
            else if (key.Equals("care_level", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Enum.Parse(typeof(Care_Level), value, true));
                predicate += $"{key} == @{values.Count - 1}";
            }
            else
            {
                values.Add(value);
                predicate += $"{key} == @{values.Count - 1}";
            }
        }

        queryable = queryable.Where(predicate, values.ToArray());          //Faz a consulta usando o metodo WHere 

        var requestedFields = parameters.Select(p => p.Split('=')[0]).ToList();
        if (hasScientificName)
        {
            if (requestedFields.Count == 0)             //Insere scientific_name caso tenha sido requisitado
            {
                requestedFields.Add("scientific_name");
            }
            else
            {
                requestedFields.Insert(0, "scientific_name");
            }
        }
        if (hasCommonName)          //Insere common_name caso tenha sido requisitado
        {
            if (requestedFields.Count == 0)
            {
                requestedFields.Add("common_name");
            }
            else
            {
                requestedFields.Insert(0, "common_name");
            }
        }

        var queryResults = await queryable.ToListAsync();       //Objeto expando recebe os resultados da query

        var resultList = new List<dynamic>();

        foreach (var result in queryResults)
        {
            var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
            foreach (var field in requestedFields)      //Basicamente um dicionario de valores
            {
                var prop = result.GetType().GetProperty(field);
                expando.Add(field, prop.GetValue(result));
            }
            resultList.Add(expando);
        }

        return resultList;
    }

}