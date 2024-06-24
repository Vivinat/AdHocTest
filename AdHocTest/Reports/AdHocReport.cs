using AdHocTest.Context;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AdHocTest.Types;
using Microsoft.Extensions.Logging;
using System;

namespace AdHocTest.Reports
{
    public class AdHocReport
    {
        private readonly DBContext _context;
        private readonly ILogger<AdHocReport> _logger;

        public AdHocReport(DBContext context, ILogger<AdHocReport> logger) //Injeção de dependencia para DBContext e Logger
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<dynamic>> GenerateReportAsync(string query)
        {
            _logger.LogInformation("Received query: {Query}", query);

            try
            {
                var parts = query.Split(':'); //Divisao da query de entrada 
                var mainTable = parts[0]; //Primeiro termo é a main table
                var relatedTable = parts[1]; //Segundo, a related table
                var parameters = parts[2].Split(';'); //restante, parametros

                _logger.LogInformation("Main Table: {MainTable}", mainTable);
                _logger.LogInformation("Related Table: {RelatedTable}", relatedTable);
                _logger.LogInformation("Parameters: {Parameters}", parameters);

                IQueryable<dynamic> queryable = null;

                switch (mainTable.ToLower()) //Qual é a main table?
                {
                    case "plant":
                        switch (relatedTable.ToLower()) //Qual a related?
                        {
                            case "plantdetails": //Faz o join apenas com aquela tabela
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
                        }
                        break;
                }

                var whereClauses = new List<string>(); //Aqui os parametros são processados
                var predicate = "true"; // Initializing the predicate with a true condition
                var values = new List<object>();

                for (int i = 0; i < parameters.Length; i++) //Parametros são quebrados para iniciar a filtragem
                {
                    var p = parameters[i];
                    if (string.IsNullOrEmpty(p)) continue;

                    var keyValue = p.Split('=');
                    if (keyValue.Length != 2)
                    {
                        _logger.LogError("Invalid parameter format: {Parameter}", p);
                        throw new Exception($"Invalid parameter format: {p}");
                    }

                    var key = keyValue[0];
                    var value = keyValue[1];

                    _logger.LogInformation("Processing parameter: {Key}={Value}", key, value);

                    var plantType = queryable.ElementType.GetProperty("Plant").PropertyType; //MAIN TABLE  
                    var relatedType = queryable.ElementType.GetProperty("Related").PropertyType; //RELATED TABLE

                    if (plantType.GetProperty(key) != null) //O que estou tentando filtrar está aonde?
                    {
                        predicate += $" AND Plant.{key} == @{i}";
                    }
                    else if (relatedType.GetProperty(key) != null)
                    {
                        predicate += $" AND Related.{key} == @{i}";
                    }
                    else
                    {
                        _logger.LogError("Property {Key} not found in either {MainTable} or {RelatedTable}", key, mainTable, relatedTable);
                        throw new Exception($"Property {key} not found in either {mainTable} or {relatedTable}"); //NAO ENCONTREI
                    }

                    if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase)) //O QUE EU ACHEI É UM ENUM?
                    {
                        values.Add(Enum.Parse(typeof(Growth_Rate), value, true)); //SE SIM, TEMOS QUE CONVERTELO
                    }
                    else if (key.Equals("care_level", StringComparison.OrdinalIgnoreCase))
                    {
                        values.Add(Enum.Parse(typeof(Care_Level), value, true));
                    }
                    else
                    {
                        values.Add(value); //ADICIONA O FILTRO PARA A STRING DE BUSCA
                    }
                }

                _logger.LogInformation("Predicate: {Predicate}", predicate);

                queryable = queryable.Where(predicate, values.ToArray()); //APLICAÇAO DE CLAUSULA WHERE DINAMICA

                var requestedFields = parameters.Select(p => p.Split('=')[0]).ToList(); //ADICIONA CAMPOS PARA IMPRESSAO
                requestedFields.Insert(0, "scientific_name");
                requestedFields.Insert(1, "common_name");

                var resultList = new List<dynamic>(); //LISTA DINAMICA COM OS CAMPOS REQUISITADOS

                var queryResults = await queryable.ToListAsync();

                _logger.LogInformation("Query results count: {Count}", queryResults.Count);

                foreach (var result in queryResults)
                {
                    var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
                    //Um expando é um objeto que pode ter seus membros adicionados ou removidos em tempo de execução
                    foreach (var field in requestedFields)
                    {
                        var plantProp = result.Plant.GetType().GetProperty(field); //Os campos que solicitei estão onde?
                        var relatedProp = result.Related.GetType().GetProperty(field);

                        if (plantProp != null)
                        {
                            expando.Add(field, plantProp.GetValue(result.Plant));
                        }
                        else if (relatedProp != null)
                        {
                            expando.Add(field, relatedProp.GetValue(result.Related));
                        }
                    }
                    resultList.Add(expando);
                }

                _logger.LogInformation("Report generated successfully");

                return resultList; //Retorna lista
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for query: {Query}", query);
                throw;
            }
        }
    }
}
