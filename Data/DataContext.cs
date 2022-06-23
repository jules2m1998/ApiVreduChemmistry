using ApiVrEdu.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<ElementGroup> ElementGroups { get; set; }
    public DbSet<ElementType> ElementTypes { get; set; }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseModel && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((BaseModel)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added) ((BaseModel)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.IsAdmin)
            .HasDefaultValueSql("false");

        modelBuilder.Entity<User>()
            .Property(b => b.IsActivated)
            .HasDefaultValueSql("false");

        modelBuilder.Entity<User>()
            .HasIndex(b => b.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(b => b.PhoneNumber)
            .IsUnique();
    }
}