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
            var query = "plant:plantdangerous:care_level=moderate;poisonous_to_pets=true";
            var result = await report.GenerateReportAsync(query);

            // Imprimindo os resultados
            foreach (var item in result)
            {
                Console.WriteLine($"Plant: {item.Plant.scientific_name}, Care Level: {item.Related.care_level}, Poisonous: {item.Related.poisonous_to_pets}");
            }
        }
    }
}