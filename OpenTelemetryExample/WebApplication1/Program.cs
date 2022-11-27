
using OpenTelemetryConfiguration;

var builder = WebApplication.CreateBuilder(args);


var clientBaseAddress = builder.Configuration.GetValue<string>("WebApplication2Url") ?? throw new Exception("WebApplication2Url is not set");
var clientBaseAddressUri = new Uri(clientBaseAddress);
var clientTimeout = TimeSpan.FromSeconds(5);
builder.Services.AddHttpClient("WebApplication2", client =>
{
    client.BaseAddress = clientBaseAddressUri;
    client.Timeout = clientTimeout;
});

builder.Logging.AddOpenTelemetry();
builder.Services.ConfigureOpenTelemetry(builder.Configuration);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
