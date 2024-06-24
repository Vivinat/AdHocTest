using Microsoft.AspNetCore.Mvc;
using AdHocTest.Context;
using AdHocTest.Reports;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Web;

namespace AdHocTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantsController : ControllerBase
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<PlantsController> _logger;
        private readonly ILogger<AdHocReport> _adhocReportLogger;

        public PlantsController(DBContext dbContext, ILogger<PlantsController> logger, ILogger<AdHocReport> adhocReportLogger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _adhocReportLogger = adhocReportLogger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlants(string query)
        {
            _logger.LogInformation("Received encoded query: {Query}", query);

            // Decodificar a query string
            string decodedQuery = HttpUtility.UrlDecode(query);
            _logger.LogInformation("Decoded query: {DecodedQuery}", decodedQuery);

            try
            {
                var report = new AdHocReport(_dbContext, _adhocReportLogger);
                var result = await report.GenerateReportAsync(decodedQuery);

                _logger.LogInformation("Generated report: {Result}", result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for query: {Query}", decodedQuery);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
