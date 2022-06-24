using System.Globalization;
using System.IO.Compression;
using System.Text.Json.Serialization;
using ApiVrEdu.Data;
using ApiVrEdu.Helpers;
using ApiVrEdu.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
// User ID =postgres;Password=password;Server=localhost;Port=5432;Database=vredu_chemistry; Integrated Security=true;Pooling=true;
var userId = Environment.GetEnvironmentVariable("DB_USER");
var pwd = Environment.GetEnvironmentVariable("DB_PASSWORD");
var host = Environment.GetEnvironmentVariable("DB_HOST");
var db = Environment.GetEnvironmentVariable("DB_NAME");

var connectionString =
    $"User ID ={userId};Password={pwd};Server={host};Port=5432;Database={db}; Integrated Security=true;Pooling=true;";

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors();

// PostgreSQL CONFIG
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TextureRepository>();
builder.Services.AddScoped<ElementRepository>();

// compression algorithm config
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 30 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
        ctx.Context.Response.Headers.Append("Expires",
            DateTime.UtcNow.AddDays(30).ToString("R", CultureInfo.InvariantCulture));
    }
});

app.UseCors(opt => opt
    .WithOrigins("http://localhost:3000", "http://localhost:8000", "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();