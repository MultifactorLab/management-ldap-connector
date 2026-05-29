using Api.Extensions;
using Api.Features;
using Api.Health;
using Api.Integrations;
using Api.Logging;
using Api.Proxy;
using Infra.SharedAdapters;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.AddLogging();
builder.Services.AddHealthChecks();

builder.AddIntegrations();
builder.Services.AddSharedAdapters(builder.Configuration);
builder.AddFeatures();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();

builder.ConfigureAuthentication();
builder.Services.AddAuthorization();

builder.ConfigureNginxProxy();

builder.Host.UseDefaultServiceProvider(x =>
{
    x.ValidateOnBuild = true;
    x.ValidateScopes = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapFeatures();

app.UseHealthCheckV2();
app.Run();

public partial class Program;