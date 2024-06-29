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
        private readonly OneOnOneReport _oneOnOneReport;
        private readonly OneReport _oneReport;
        private readonly TwoOnOneReport _twoOnOneReport;
        private readonly ThreeOnOneReport _threeOnOneReport;

        public PlantsController(
            DBContext dbContext,
            ILogger<PlantsController> logger,
            ILogger<AdHocReport> adhocReportLogger,
            OneOnOneReport oneOnOneReport,
            OneReport oneReport,
            TwoOnOneReport twoOnOneReport,
            ThreeOnOneReport threeOnOneReport)
        {
            _dbContext = dbContext;
            _logger = logger;
            _adhocReportLogger = adhocReportLogger;
            _oneOnOneReport = oneOnOneReport;
            _oneReport = oneReport;
            _twoOnOneReport = twoOnOneReport;
            _threeOnOneReport = threeOnOneReport;
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
                object result = null;

                if (decodedQuery.StartsWith("1"))
                {
                    decodedQuery = decodedQuery.Substring(1);
                    result = await _oneOnOneReport.GenerateReportAsync(decodedQuery);
                }
                else if (decodedQuery.StartsWith("0"))
                {
                    decodedQuery = decodedQuery.Substring(1);
                    result = await _oneReport.GenerateReportAsync(decodedQuery);
                }
                else if (decodedQuery.StartsWith("2"))
                {
                    decodedQuery = decodedQuery.Substring(1);
                    result = await _twoOnOneReport.GenerateReportAsync(decodedQuery);
                }
                else if (decodedQuery.StartsWith("3"))
                {
                    decodedQuery = decodedQuery.Substring(1);
                    result = await _threeOnOneReport.GenerateReportAsync(decodedQuery);
                }
                else
                {
                    var report = new AdHocReport(_dbContext, _adhocReportLogger);
                    result = await report.GenerateReportAsync(decodedQuery);
                }

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
