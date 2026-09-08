using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
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
});

// Generic CKAN client (from DataGovIL.Client), configured from the "DataGovIL" section.
builder.Services.AddDataGovIlClient(builder.Configuration);

// Resource ids for the specific datasets this Api exposes.
builder.Services.Configure<VehicleDataResourceOptions>(
    builder.Configuration.GetSection("VehicleDataResources"));
builder.Services.Configure<ExportOptions>(
    builder.Configuration.GetSection("ExportOptions"));

// App-specific services built on top of ICkanApiClient.
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IManufacturerCsvExportService, ManufacturerCsvExportService>();
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
