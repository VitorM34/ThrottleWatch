using ThrottleWatch.Dashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddThrottleWatchDashboard(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseThrottleWatchDashboard("/");

app.Run();
