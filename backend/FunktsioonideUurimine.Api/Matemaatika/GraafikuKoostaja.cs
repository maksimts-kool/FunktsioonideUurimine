using FunktsioonideUurimine.Api.Mudelid;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>Arvutab graafiku punktid valitud vahemikus antud sammuga (boonusülesanne: y arvutatakse serveris).</summary>
public static class GraafikuKoostaja
{
    public static GraafikuVastus Koosta(AnaluusiTulemus t, double algus, double lopp, double samm)
    {
        var n = (int)Math.Floor((lopp - algus) / samm + 1e-9);
        var xs = Enumerable.Range(0, n + 1).Select(i => Math.Round(algus + i * samm, 10)).ToList();
        if (xs[^1] < lopp - 1e-12) xs.Add(lopp);

        // Katkemispunktid (poolused, määramispiirkonna otsad) lisatakse alati punktideks. Joon katkestatakse
        // (null) ainult seal, kus just see funktsioon on määramata: √(4 - x²) graafik ulatub x = ±2-ni,
        // aga tuletise joon katkeb; 1/(x - 2) harusid ei ühendata ka siis, kui samm poolusest üle hüppab.
        var katkemised = t.Katkemised.Where(k => k > algus && k < lopp).ToList();
        var read = xs.Where(v => !katkemised.Any(k => Math.Abs(k - v) < 1e-9))
            .Concat(katkemised)
            .Order()
            .ToList();
        bool Katkeb(double x, Hulk piirkond) => katkemised.Contains(x) && !piirkond.Sisaldab(x);

        var x = read.ToArray();
        var y = new double?[x.Length];
        var y1 = new double?[x.Length];
        var y2 = new double?[x.Length];
        for (var i = 0; i < x.Length; i++)
        {
            y[i] = Katkeb(x[i], t.PiirkondF) ? null : Vaartus(t.F(x[i]));
            y1[i] = Katkeb(x[i], t.PiirkondF1) ? null : Vaartus(t.F1(x[i]));
            y2[i] = Katkeb(x[i], t.PiirkondF2) ? null : Vaartus(t.F2(x[i]));
        }

        bool Vahemikus(double v) => v >= algus - 1e-9 && v <= lopp + 1e-9;
        return new GraafikuVastus(
            x, y, y1, y2,
            t.NullkohaPunktid.Where(p => Vahemikus(p.X)).ToList(),
            t.EkstreemumPunktid.Where(p => Vahemikus(p.X)).ToList(),
            t.KaanupunktiPunktid.Where(p => Vahemikus(p.X)).ToList(),
            t.Asumptoodid.Where(Vahemikus).ToList(),
            t.ValemLatex, t.TuletisLatex, t.TeineTuletisLatex);
    }

    private static double? Vaartus(double v) => double.IsFinite(v) ? Math.Round(v, 10) : null;
}
