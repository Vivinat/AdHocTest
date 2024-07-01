using System.Linq.Dynamic.Core;
using AdHocTest.Context;
using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;

namespace AdHocTest.Reports
{
    public class TwoOnOneReport
    {
        private readonly DBContext _context;

        public TwoOnOneReport(DBContext context)
        {
            _context = context;
        }

        public async Task<List<IDictionary<string, object>>> GenerateReportAsync(string query)
        {
            var parts = query.Split(':');   //Quebra a string
            if (parts.Length < 4)
            {
                throw new ArgumentException("Formato incorreto");
            }

            var mainTable = parts[0];       //Quais são minhas tabelas?
            var relatedTable1 = parts[1];
            var relatedTable2 = parts[2];
            var parameters = parts[3].Split(';').ToList();

            IQueryable<ResultRow> queryable = _context.plant    //Join com todas
                .Join(_context.cultivation, p => p.scientific_name, c => c.scientific_name, (p, c) => new ResultRow { Main = p, Related1 = c })
                .Join(_context.plant_details, pc => pc.Main.scientific_name, pd => pd.scientific_name, (pc, pd) => new ResultRow { Main = pc.Main, Related1 = pc.Related1, Related2 = pd });

            var resultList = await queryable.ToListAsync();

            var predicate = "true"; // Inicie o predicate com uma condição true para facilitar a adição de outras condições
            var values = new List<object>();

            bool hasScientificName = parameters.Contains("@scientific_name");
            bool hasCommonName = parameters.Contains("@common_name");

            if (hasScientificName)      //Se scientific ou common forem requisitados, marca os bool como true. Nao queremos eles no where
            {
                parameters.Remove("@scientific_name");
            }

            if (hasCommonName)
            {
                parameters.Remove("@common_name");
            }

            parameters = parameters.Where(p => !string.IsNullOrEmpty(p)).ToList();

            foreach (var parameter in parameters)       //Tratamento de parametros
            {
                var keyValue = parameter.Split('=');
                if (keyValue.Length != 2)
                {
                    throw new ArgumentException($"Invalid parameter format: {parameter}");
                }

                var key = keyValue[0];
                var value = keyValue[1];

                var mainProp = typeof(PlantSummary).GetProperty(key);       //Usa reflection para obter a qual classe pertence aquele parametro
                var related1Prop = typeof(CultivationSummary).GetProperty(key);
                var related2Prop = typeof(PlantDetailsSummary).GetProperty(key);

                if (mainProp != null)
                {
                    predicate += $" AND Main.{key} == @{values.Count}"; //Insere no predicado
                }
                else if (related1Prop != null)
                {
                    predicate += $" AND Related1.{key} == @{values.Count}";
                }
                else if (related2Prop != null)
                {
                    predicate += $" AND Related2.{key} == @{values.Count}";
                }
                else
                {
                    throw new Exception($"Property {key} not found in {mainTable}, {relatedTable1}, or {relatedTable2}");
                }

                if (key.Equals("growth_rate", StringComparison.OrdinalIgnoreCase))      //Tratamento de enums
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

            var filteredResults = resultList.AsQueryable().Where(predicate, values.ToArray()).ToList();

            var requestedFields = parameters.Select(p => p.Split('=')[0]).ToList();
            if (hasScientificName)
            {
                requestedFields.Insert(0, "scientific_name");
            }
            if (hasCommonName)
            {
                requestedFields.Insert(0, "common_name");
            }

            var finalResultList = new List<IDictionary<string, object>>();

            foreach (var result in filteredResults) //Filtra os resultados no expando
            {
                var expando = new Dictionary<string, object>();
                foreach (var field in requestedFields)
                {
                    var mainProp = typeof(PlantSummary).GetProperty(field);
                    var related1Prop = typeof(CultivationSummary).GetProperty(field);
                    var related2Prop = typeof(PlantDetailsSummary).GetProperty(field);

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
                finalResultList.Add(expando);
            }

            return finalResultList;
        }

        private class ResultRow     //Result row compõe as tabelas
        {
            public PlantSummary Main { get; set; }
            public CultivationSummary Related1 { get; set; }
            public PlantDetailsSummary Related2 { get; set; }
        }
    }
}
