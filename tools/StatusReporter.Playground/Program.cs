using StatusReporter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStatusReporter(options => options.TimeZone = TimeZoneInfo.Local);

var app = builder.Build();

app.MapStatus();

app.Run();
