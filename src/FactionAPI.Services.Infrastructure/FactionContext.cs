using FactionAPI.Services.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FactionAPI.Services.Infrastructure;

public class FactionContextDesignTimeFactory : IDesignTimeDbContextFactory<FactionContext>
{
    public FactionContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FactionContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=factionapi;Username=postgres;Password=postgres");
        return new FactionContext(optionsBuilder.Options);
    }
}
public class FactionContext(DbContextOptions<FactionContext> options) : DbContext(options)
{
    
    public DbSet<Supporter> Supporters { get; set; }
    public DbSet<TelemetryEntry> TelemetryEntries { get; set; }
    public DbSet<ConfigValue> ConfigValues { get; set; }
    public DbSet<ApiToken> ApiTokens { get; set; }
    public DbSet<TelemetryDependingMod> TelemetryDependingMods { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var supporter = modelBuilder.Entity<Supporter>();
        supporter.HasKey(s => s.Id);
        supporter.HasMany(x => x.Appearances)
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        var supporterAppearance = modelBuilder.Entity<SupporterAppearance>();
        supporterAppearance.HasKey(s => new {s.PlayerId, s.Key});

        var telemetryEntry = modelBuilder.Entity<TelemetryEntry>();
        telemetryEntry.HasKey(t => t.Timestamp);

        var dependingMod = modelBuilder.Entity<TelemetryDependingMod>();
        dependingMod.HasKey(d => new { d.Timestamp, d.DependingModId });
        dependingMod.HasOne<TelemetryEntry>()
            .WithMany()
            .HasForeignKey(d => d.Timestamp)
            .OnDelete(DeleteBehavior.Cascade);
        dependingMod.HasIndex(d => new { d.ModId, d.DependingModId });

        modelBuilder.Entity<ConfigValue>().HasKey(x => x.Key);

        modelBuilder.Entity<ApiToken>().HasKey(t => t.Name);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<ResourceLocation>()
            .HaveConversion<ResourceLocationTypeConverter>();
    }

}

public class ResourceLocationTypeConverter() : ValueConverter<ResourceLocation, string>(location => location, str => str);
