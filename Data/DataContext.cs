using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Data;

public class DataContext: DbContext
{
    public DbSet<WeatherForecast> Forecasts { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }
}