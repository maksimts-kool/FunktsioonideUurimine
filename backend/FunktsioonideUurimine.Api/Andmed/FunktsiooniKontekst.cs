using FunktsioonideUurimine.Api.Mudelid;
using Microsoft.EntityFrameworkCore;

namespace FunktsioonideUurimine.Api.Andmed;

public class FunktsiooniKontekst(DbContextOptions<FunktsiooniKontekst> options) : DbContext(options)
{
    public DbSet<FunktsiooniUurimine> FunktsiooniUurimised => Set<FunktsiooniUurimine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var olem = modelBuilder.Entity<FunktsiooniUurimine>();
        olem.ToTable("FunktsiooniUurimised");
        olem.Property(f => f.Valem).IsRequired().HasMaxLength(200);

        foreach (var tekstivali in new[]
                 {
                     nameof(FunktsiooniUurimine.Maaramispiirkond), nameof(FunktsiooniUurimine.Tuletis),
                     nameof(FunktsiooniUurimine.KriitilisedPunktid), nameof(FunktsiooniUurimine.Ekstreemumid),
                     nameof(FunktsiooniUurimine.Nullkohad), nameof(FunktsiooniUurimine.Positiivsus),
                     nameof(FunktsiooniUurimine.Monotoonsus), nameof(FunktsiooniUurimine.TeineTuletis),
                     nameof(FunktsiooniUurimine.Kaanupunktid), nameof(FunktsiooniUurimine.Kumerus)
                 })
        {
            olem.Property<string>(tekstivali).IsRequired().HasMaxLength(2000);
        }

        // SQLite ei salvesta DateTime.Kind väärtust – loeme ajad tagasi UTC-na.
        olem.Property(f => f.LuodudAeg)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        olem.Property(f => f.MuudetudAeg)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
        olem.HasIndex(f => f.LuodudAeg);
    }
}
