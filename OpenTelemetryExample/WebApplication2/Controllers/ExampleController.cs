using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetryConfiguration;
using WebApplication2.Database;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly ExampleDbContext _exampleDbContext;

        public ExampleController(ILogger<ExampleController> logger, ExampleDbContext exampleDbContext)
        {
            _logger = logger;
            _exampleDbContext = exampleDbContext;
        }

        public sealed record Example1Request(string Name);

        [HttpPost("Example1/")]
        public async Task<IActionResult> Example1([FromBody, Required] Example1Request request)
        {
            _logger.LogInformation("Begin Example1");

            await _exampleDbContext.ExampleTable.AddAsync(new ExampleTable { Name = request.Name });
            await _exampleDbContext.SaveChangesAsync();

            _logger.LogInformation("End Example1");
            return Ok();
        }

        [HttpPost("Example2/")]
        public async Task<IActionResult> Example2([FromBody, Required] Example1Request request)
        {
            _logger.LogInformation("Begin Example2");

            using (var activity = OpenTelemetryHelper.StartActivity("e2"))
            {
                await _exampleDbContext.ExampleTable.AddAsync(new ExampleTable {Name = request.Name});
                await _exampleDbContext.SaveChangesAsync();

                _logger.LogInformation("End Example2 in e2");
            }

            _logger.LogInformation("End Example2");
            return Ok();
        }
    }
}