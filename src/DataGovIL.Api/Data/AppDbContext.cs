using Microsoft.EntityFrameworkCore;

namespace DataGovIL.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ManufacturerModelEntity> ManufacturerModels => Set<ManufacturerModelEntity>();

    public DbSet<ManufacturerEntity> Manufacturers => Set<ManufacturerEntity>();

    public DbSet<TextEntity> Texts => Set<TextEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TextEntity>().HasKey(e => new { e.TableId, e.Id });

        modelBuilder.Entity<ManufacturerEntity>().HasKey(e => e.ManufacturerCode);
        modelBuilder.Entity<ManufacturerEntity>().Property(e => e.ManufacturerCode).ValueGeneratedNever();

        var manufacturer = modelBuilder.Entity<ManufacturerModelEntity>();

        manufacturer.ToTable("manufacturer_models");
        manufacturer.HasKey(e => new { e.ManufacturerCode, e.ModelCode, e.ModelYear, e.ModelType });

        // Source columns are CKAN "numeric" with mixed scale; keep Postgres numeric unconstrained.
        foreach (var property in manufacturer.Metadata.GetProperties().Where(p => p.ClrType == typeof(decimal?)))
            property.SetColumnType("numeric");
    }
}
