using AdHocTest.Types;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AdHocTest.Context;

public class DBContext : DbContext
{
    public DbSet<plant> plant { get; set; }
    public DbSet<plant_details> plant_details { get; set; }
    public DbSet<dangerous_plants> dangerous_plants { get; set; }
    public DbSet<cultivation> cultivation { get; set; }

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
        
        modelBuilder.Entity<plant>()
            .HasMany(p => p.PlantDetails)
            .WithOne(pd => pd.Plant)
            .HasForeignKey(pd => pd.scientific_name);

        modelBuilder.Entity<plant>()
            .HasMany(p => p.PlantDangerous)
            .WithOne(pd => pd.Plant)
            .HasForeignKey(pd => pd.scientific_name);

        modelBuilder.Entity<plant>()
            .HasMany(p => p.Cultivations)
            .WithOne(pd => pd.Plant)
            .HasForeignKey(pd => pd.scientific_name);
    }
}