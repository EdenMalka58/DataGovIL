using System.Reflection;
using System.Text.Json.Serialization;
using DataGovIL.Api.Data;
using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DataGovIL.Api",
        Version = "v1",
        Description = "Sample .NET Core Web API on top of the data.gov.il CKAN datastore " +
                      "(vehicle registrations + WLTP makes/models)."
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// Generic CKAN client (from DataGovIL.Client), configured from the "DataGovIL" section.
builder.Services.AddDataGovIlClient(builder.Configuration);

// Resource ids for the specific datasets this Api exposes.
builder.Services.Configure<VehicleDataResourceOptions>(
    builder.Configuration.GetSection("VehicleDataResources"));
builder.Services.Configure<ExportOptions>(
    builder.Configuration.GetSection("ExportOptions"));
builder.Services.Configure<SystemOptions>(
    builder.Configuration.GetSection("SystemOptions"));
builder.Services.Configure<VehicleCostOptions>(
    builder.Configuration.GetSection("VehicleCost"));

// Postgres (Supabase). Fill ConnectionStrings:Supabase via appsettings, env, or user-secrets.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Supabase");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:Supabase is not configured. " +
            "Set it in appsettings.json, environment variable ConnectionStrings__Supabase, " +
            "or: dotnet user-secrets set \"ConnectionStrings:Supabase\" \"Host=...;...\"");
    }

    var commandTimeout = configuration.GetSection("ExportOptions").Get<ExportOptions>()?.DbCommandTimeoutSeconds ?? 120;
    options.UseNpgsql(connectionString, npgsql => npgsql.CommandTimeout(commandTimeout));
});

// App-specific services built on top of ICkanApiClient.
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<IManufacturerCsvExportService, ManufacturerCsvExportService>();
builder.Services.AddSingleton<IEnergyPriceProvider, ConfigEnergyPriceProvider>();
builder.Services.AddScoped<IVehicleEnergyCostService, VehicleEnergyCostService>();
builder.Services.AddSingleton<IManufacturerExportStatusStore, ManufacturerExportStatusStore>();
builder.Services.AddSingleton<IManufacturerExportWorkQueue, ManufacturerExportWorkQueue>();
builder.Services.AddHostedService<ManufacturerExportBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("WebApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
