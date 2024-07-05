using Azure.Storage.Blobs;
using ImageAnalysisAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog para el registro
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<PhotoResponsesService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<FileStorageAzureService>();
builder.Services.AddScoped<FileStorageLocalService>();
builder.Services.AddScoped<FormRecognizerService>();
builder.Services.AddScoped<GenuineCheckService>();
builder.Services.AddScoped<LiveCheckService>();
builder.Services.AddScoped<ModifiedCheckService>();
builder.Services.AddScoped<OCRDataExtractService>();
builder.Services.AddScoped<FaceRecognitionService>();

// Add Blob Service Client
builder.Services.AddSingleton(x =>
{
    var configuration = x.GetRequiredService<IConfiguration>();
    var connectionString = configuration["Azure:Storage:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentNullException("Azure:Storage:ConnectionString", "The Azure Storage connection string cannot be null or empty.");
    }
    return new BlobServiceClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
