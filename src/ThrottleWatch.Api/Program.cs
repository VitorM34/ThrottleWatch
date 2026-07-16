using ThrottleWatch.Api.Endpoints;
using ThrottleWatch.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddThrottleWatchOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseThrottleWatchOpenApi();
app.UseHttpsRedirection();
app.UseCors();

app.MapMetricsEndpoints();
app.MapAlertsEndpoints();
app.MapInsightsEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
