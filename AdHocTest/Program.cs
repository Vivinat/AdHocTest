using AdHocTest.Context;
using AdHocTest.Reports;

namespace AdHocTest;

class Program
{
    public static async Task Main(string[] args)
    {
        using (var dbContext = new DBContext())     //Tenho uma batch de 30 plantas ao final do loop
        {
            var report = new AdHocReport(dbContext);

            // Query de teste: buscando na tabela plant e plantdetails com 3 atributos
            var query = "cultivation:plant:watering=Frequent;sunlight=full sun";
            var result = await report.GenerateReportAsync(query);

            // Imprimindo os resultados
            foreach (var item in result)
            {
                foreach (var key in (IDictionary<string, object>)item)
                {
                    Console.WriteLine($"{key.Key}: {key.Value}");
                }
                Console.WriteLine();
            }
        }
    }
}