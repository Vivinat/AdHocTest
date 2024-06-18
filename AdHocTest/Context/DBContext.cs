using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AdHocTest.Context;

public class DBContext : DbContext
{
    public DbSet<PlantSummary> plant { get; set; }
    public DbSet<PlantDetailsSummary> plant_details { get; set; }
    public DbSet<DangerousPlantsSummary> dangerous_plants { get; set; }
    public DbSet<CultivationSummary> cultivation { get; set; }

    static DBContext()
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Care_Level>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Growth_Rate>();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=PlantDB;Username=postgres;Password=admin; IncludeErrorDetail = true");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<Care_Level>();
        modelBuilder.HasPostgresEnum<Growth_Rate>();
        
        modelBuilder.Entity<PlantSummary>()
            .HasMany(p => p.PlantDetails)
            .WithOne(pd => pd.PlantSummary)
            .HasForeignKey(pd => pd.scientific_name);

        modelBuilder.Entity<PlantSummary>()
            .HasMany(p => p.PlantDangerous)
            .WithOne(pd => pd.PlantSummary)
            .HasForeignKey(pd => pd.scientific_name);

        modelBuilder.Entity<PlantSummary>()
            .HasMany(p => p.Cultivations)
            .WithOne(pd => pd.PlantSummary)
            .HasForeignKey(pd => pd.scientific_name);
    }
}