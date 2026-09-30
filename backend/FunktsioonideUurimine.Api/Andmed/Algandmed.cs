using FunktsioonideUurimine.Api.Matemaatika;
using FunktsioonideUurimine.Api.Mudelid;
using FunktsioonideUurimine.Api.Otspunktid;
using Microsoft.EntityFrameworkCore;

namespace FunktsioonideUurimine.Api.Andmed;

/// <summary>
/// Näidisandmed, mis lisatakse tühja andmebaasi (EF Core UseSeeding/UseAsyncSeeding).
/// Omadusi ei kirjutata käsitsi – need arvutab sama analüsaator, mistõttu vastavad tegelikkusele.
/// </summary>
public static class Algandmed
{
    public static readonly FunktsiooniPaering[] Naited =
    [
        new("x^3 - 3*x", -3, 3),
        new("x^2 - 4*x + 3", -2, 6),
        new("x^3 - 12*x", -5, 5),
        new("x^4 - 2*x^2", -2.5, 2.5),
        new("1/(x - 2)", -3, 7),
        new("sqrt(4 - x^2)", -3, 3),
        new("x*e^(-x)", -1, 6),
        new("sin(x)", -6.3, 6.3),
    ];

    public static async Task LisaAsync(DbContext kontekst, FunktsiooniAnaluusija analuusija, CancellationToken ct)
    {
        var tabel = kontekst.Set<FunktsiooniUurimine>();
        if (await tabel.AnyAsync(ct)) return;
        await tabel.AddRangeAsync(Olemid(analuusija), ct);
        await kontekst.SaveChangesAsync(ct);
    }

    public static void Lisa(DbContext kontekst, FunktsiooniAnaluusija analuusija)
    {
        var tabel = kontekst.Set<FunktsiooniUurimine>();
        if (tabel.Any()) return;
        tabel.AddRange(Olemid(analuusija));
        kontekst.SaveChanges();
    }

    private static IEnumerable<FunktsiooniUurimine> Olemid(FunktsiooniAnaluusija analuusija)
    {
        var aeg = DateTime.UtcNow;
        // vanimad ees, et loendis (uuemad eespool) oleks esimene näide ülal
        for (var i = Naited.Length - 1; i >= 0; i--)
        {
            var olem = new FunktsiooniUurimine { LuodudAeg = aeg.AddSeconds(-i) };
            FunktsioonidOtspunktid.Kanna(Naited[i], analuusija, olem);
            yield return olem;
        }
    }
}
