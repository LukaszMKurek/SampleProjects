using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OpenTelemetryConfiguration
{
    public static class OpenTelemetryConfigurationExtensions
    {
        public static void ConfigureOpenTelemetry(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOpenTelemetryTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    //.AddConsoleExporter()
                    // sending directly to jaeger is also possible
                    /*.AddJaegerExporter(o =>
                    {
                        o.Protocol = JaegerExportProtocol.HttpBinaryThrift; // udp not work
                    })*/
                    .AddOtlpExporter(o =>
                    {
                        var oltpUrl = configuration.GetValue<string>("OtlpExporterUrl");
                        if (oltpUrl != null)
                            o.Endpoint = new Uri(oltpUrl);
                    })
                    .AddSource(OpenTelemetryHelper.ServiceName)
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: OpenTelemetryHelper.ServiceName, serviceVersion: null))
                    .AddHttpClientInstrumentation(options =>
                    {
                        //options.EnrichWithException
                        //options.EnrichWithHttpRequestMessage
                        //options.EnrichWithHttpResponseMessage
                        //options.EnrichWithHttpWebRequest
                        //options.EnrichWithHttpWebResponse

                        //options.FilterHttpRequestMessage
                        //options.FilterHttpWebRequest
                        options.RecordException = true;
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        //options.Filter
                        //options.EnableGrpcAspNetCoreSupport
                        options.RecordException = true;
                        
                        //options.EnrichWithException
                        //options.EnrichWithHttpRequest
                        //options.EnrichWithHttpResponse
                        
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            var controllerName = response.HttpContext.GetRouteValue("controller");
                            var actionName = response.HttpContext.GetRouteValue("action");

                            if (controllerName is null && actionName is null)
                                return;
                            
                            activity.DisplayName += $"{activity.DisplayName} ({controllerName}/{actionName})";
                        };
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        //options.Filter
                        options.Enrich = (activity, eventName, dbCommand) =>
                        {
                            var sqlCommand = (SqlCommand) dbCommand;
                            foreach (SqlParameter parameter in sqlCommand.Parameters)
                            {
                                activity.AddTag($"{parameter.ParameterName}", $"{parameter.Value} ({parameter.DbType};{(parameter.IsNullable ? "null" : "notnull")};{parameter.Size};{parameter.Precision})");
                            }
                        };
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                        options.RecordException = true;
                    });
            });
        }
    }

    public static class OpenTelemetryHelper
    {
        public static readonly string ServiceName = "abc";
        private static readonly ActivitySource ActivitySource = new (ServiceName);

        /// <summary>
        /// return null when there is no event listeners 
        /// </summary>
        public static Activity? StartActivity(string name)
        {
            return ActivitySource.StartActivity(name);
        }
    }
}