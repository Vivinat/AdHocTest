using AdHocTest.Context;
using AdHocTest.Reports;

namespace AdHocTest;

class Program
{
    public static async Task Main(string[] args)
    {
        using (var dbContext = new DBContext())     //Tenho uma batch de 30 plantas ao final do loop
        {
            var oneOnOneReport = new OneOnOneReport(dbContext);
            var oneReport = new OneReport(dbContext);
            var twoReport = new TwoOnOneReport(dbContext);
            var threeReport = new ThreeOnOneReport(dbContext);
            
            // Query de teste: buscando na tabela plant e plantdetails com 3 atributos
            var query = "3plant:cultivation:plantdetails:plantdangerous:@scientific_name;@common_name;watering=Average;growth_rate=high;poisonous_to_pets=true";
            object result = null;
            
            if (query.StartsWith("1"))
            {
                query = query.Substring(1);
                result = await oneOnOneReport.GenerateReportAsync(query);    
            }
            else if (query.StartsWith("0"))
            {
                query = query.Substring(1);
                result = await oneReport.GenerateReportAsync(query);    
            }
            else if (query.StartsWith("2"))
            {
                query = query.Substring(1);
                result = await twoReport.GenerateReportAsync(query);
            }
            else if (query.StartsWith("3"))
            {
                query = query.Substring(1);
                result = await threeReport.GenerateReportAsync(query);

            }

            if (result != null) // Check if result is not null before iterating
            {
                int index = 0;
                foreach (var item in (IEnumerable<object>)result)
                {
                    index++;
                    foreach (var key in (IDictionary<string, object>)item)
                    {
                        Console.WriteLine($"{key.Key}: {key.Value}");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("Retornados " + index + " registros");
            }
        }
    }
}