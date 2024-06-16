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
            var query = "plant:plantdetails:edible_fruit=true;growth_rate=high;indoor=true";
            var result = await report.GenerateReportAsync(query);

            // Imprimindo os resultados
            foreach (var item in result)
            {
                Console.WriteLine($"Plant: {item.Plant.scientific_name}, Edible Fruit: {item.Related.edible_fruit}, Growth Rate: {item.Related.growth_rate}, Indoor: {item.Related.indoor}");
            }
        }
    }
}