using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetryConfiguration;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ExampleController(ILogger<ExampleController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private sealed record Example1Request(string Name);

        [HttpPost(Name = "Example1")]
        public async Task<IActionResult> Example1()
        {
            _logger.LogInformation("Begin Example1");

            var httpClient = _httpClientFactory.CreateClient("WebApplication2");
            using var response = await httpClient.PostAsJsonAsync("Example/Example1/", new Example1Request(Name: Guid.NewGuid().ToString()));
            response.EnsureSuccessStatusCode();

            using (var activity = OpenTelemetryHelper.StartActivity("xyz"))
            {
                using var response2 = await httpClient.PostAsJsonAsync("Example/Example2/", new Example1Request(Name: Guid.NewGuid().ToString()));
                response2.EnsureSuccessStatusCode();

                activity?.AddTag("a", "c");
                activity?.AddEvent(new ActivityEvent("d"));
            }

            _logger.LogInformation("End Example1");
            return Ok();
        }

        [HttpGet("example2/{name}")]
        public async Task<IActionResult> Example2([Required, FromRoute] string name, [Required, FromQuery] string name2, [Required, FromHeader] string name3)
        {
            _logger.LogInformation("Begin Example1");

            var httpClient = _httpClientFactory.CreateClient("WebApplication2");
            using var response = await httpClient.PostAsJsonAsync("Example/Example1/", new Example1Request(Name: name));
            response.EnsureSuccessStatusCode();

            using (var activity = OpenTelemetryHelper.StartActivity("xyz"))
            {
                using var response2 = await httpClient.PostAsJsonAsync("Example/Example2/", new Example1Request(Name: name2 + name3));
                response2.EnsureSuccessStatusCode();

                activity?.AddTag("a", "c");
                activity?.AddEvent(new ActivityEvent("d"));
            }

            _logger.LogInformation("End Example1");
            return Ok();
        }
    }
}