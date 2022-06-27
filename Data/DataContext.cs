using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Models.Textures;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ApiVrEdu.Data;

public class DataContext : IdentityDbContext<User, Role, int>
{
    private readonly IWebHostEnvironment _env;

    public DataContext(DbContextOptions options, IWebHostEnvironment env) : base(options)
    {
        _env = env;
    }

    public DbSet<Texture> Textures { get; set; }
    public DbSet<TextureGroup> TextureGroups { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<ElementChildren> ElementChildren { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<ElementGroup> ElementGroups { get; set; }
    public DbSet<ElementType> ElementTypes { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Reactant> Reactants { get; set; }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        if (entity is not IModelImage et) return base.Remove(entity);
        FileManager.DeleteFile(et.Image ?? "", _env);
        return base.Remove(entity);
    }

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

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseModel && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((BaseModel)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added) ((BaseModel)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.IsActivated)
            .HasDefaultValueSql("false");

        modelBuilder.Entity<User>()
            .HasIndex(b => b.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(b => b.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(b => b.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<TextureGroup>()
            .HasIndex(b => b.Name)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}