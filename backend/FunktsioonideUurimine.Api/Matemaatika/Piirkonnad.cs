namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>Määramispiirkonna ja märgipiirkondade (X⁺/X⁻, X↑/X↓, X∪/X∩) leidmine.</summary>
public static class Piirkonnad
{
    /// <summary>
    /// Määramispiirkond tingimuste põhjal. Tingimuste avaldiste nullkohad jaotavad arvtelje osadeks;
    /// igas vahemikus kontrollitakse tingimusi ühes testpunktis, murdepunktides eraldi.
    /// </summary>
    /// <returns>Hulk ja kas see kehtib kogu arvteljel (false – leitud ainult vahemikus [a; b]).</returns>
    public static (Hulk hulk, bool taielik) Maaramispiirkond(IReadOnlyList<Tingimus> tingimused, double a, double b)
    {
        if (tingimused.Count == 0) return (Hulk.Reaalarvud, true);

        var taielik = true;
        var juuredTingimuseti = new List<(Tingimus tingimus, Juured juured)>();
        foreach (var t in tingimused)
        {
            var juured = Nullkohad.Leia(t.Avaldis, a, b);
            if (juured.KoikPunktid)
            {
                // g ≡ 0: "≥ 0" kehtib kõikjal, "≠ 0" ja "> 0" mitte kuskil
                if (t.Liik == TingimuseLiik.MitteNegatiivne) continue;
                return (new Hulk([]), true);
            }
            taielik &= juured.Taielik;
            juuredTingimuseti.Add((t, juured));
        }

        var vasak = taielik ? double.NegativeInfinity : a;
        var parem = taielik ? double.PositiveInfinity : b;
        var murdepunktid = Sorteeritud(juuredTingimuseti.SelectMany(j => j.juured.Vaartused))
            .Where(p => p > vasak + Hulk.Tolerants && p < parem - Hulk.Tolerants)
            .ToList();

        bool PunktKehtib(double p) => juuredTingimuseti.All(j =>
            j.juured.Vaartused.Any(r => Math.Abs(r - p) <= Hulk.Tolerants)
                ? j.tingimus.Liik == TingimuseLiik.MitteNegatiivne
                : j.tingimus.Kehtib(p));

        bool VahemikKehtib(double u, double v)
        {
            var t = Testpunkt(u, v);
            return juuredTingimuseti.All(j => j.tingimus.Kehtib(t));
        }

        // tükid järjekorras: [a], (a; p1), p1, (p1; p2), ..., (pn; b), [b]
        var tukid = new List<(double u, double v, bool punkt, bool kehtib)>();
        if (!taielik) tukid.Add((a, a, true, PunktKehtib(a)));
        var piirid = new List<double> { vasak };
        piirid.AddRange(murdepunktid);
        piirid.Add(parem);
        for (var i = 0; i < piirid.Count - 1; i++)
        {
            if (i > 0) tukid.Add((piirid[i], piirid[i], true, PunktKehtib(piirid[i])));
            tukid.Add((piirid[i], piirid[i + 1], false, VahemikKehtib(piirid[i], piirid[i + 1])));
        }
        if (!taielik) tukid.Add((b, b, true, PunktKehtib(b)));

        var loigud = new List<Loik>();
        Loik? praegune = null;
        foreach (var (u, v, punkt, kehtib) in tukid)
        {
            if (!kehtib)
            {
                if (praegune is { } valmis) loigud.Add(valmis);
                praegune = null;
                continue;
            }
            praegune = praegune is { } p
                ? p with { Lopp = v, LoppKaasa = punkt }
                : new Loik(u, v, punkt, punkt);
        }
        if (praegune is { } viimane) loigud.Add(viimane);

        return (new Hulk(loigud), taielik);
    }

    /// <summary>
    /// Jaotab piirkonna osadeks, kus g on positiivne ja negatiivne.
    /// </summary>
    /// <param name="yhenda">Liida kõrvuti olevad sama märgiga osad (monotoonsus, kumerus: x³ kasvab kogu ℝ-il).</param>
    /// <param name="eiYhendaPunktides">Punktid, mille kohal ei liideta (x^(2/3) on nõgus kummalgi pool 0, aga mitte ℝ-il).</param>
    public static (Hulk positiivne, Hulk negatiivne) Margid(Func<double, double> g, Hulk piirkond,
        IEnumerable<double> murdepunktid, bool yhenda, IEnumerable<double>? eiYhendaPunktides = null)
    {
        var punktid = Sorteeritud(murdepunktid);
        var eraldajad = Sorteeritud(eiYhendaPunktides ?? []);
        var positiivne = new List<Loik>();
        var negatiivne = new List<Loik>();

        foreach (var loik in piirkond.Loigud.Where(l => !l.OnPunkt))
        {
            var piirid = new List<double> { loik.Algus };
            piirid.AddRange(punktid.Where(p => p > loik.Algus + Hulk.Tolerants && p < loik.Lopp - Hulk.Tolerants));
            piirid.Add(loik.Lopp);

            var osad = new List<(double u, double v, int mark)>();
            for (var i = 0; i < piirid.Count - 1; i++)
            {
                var y = g(Testpunkt(piirid[i], piirid[i + 1]));
                var mark = double.IsNaN(y) || y == 0 ? 0 : Math.Sign(y);
                var eraldab = eraldajad.Any(e => Math.Abs(e - piirid[i]) <= Hulk.Tolerants);
                if (yhenda && !eraldab && osad.Count > 0 && osad[^1].mark == mark && mark != 0)
                    osad[^1] = osad[^1] with { v = piirid[i + 1] };
                else
                    osad.Add((piirid[i], piirid[i + 1], mark));
            }

            foreach (var (u, v, mark) in osad)
            {
                if (mark > 0) positiivne.Add(new Loik(u, v, false, false));
                else if (mark < 0) negatiivne.Add(new Loik(u, v, false, false));
            }
        }
        return (new Hulk(positiivne), new Hulk(negatiivne));
    }

    private static double Testpunkt(double u, double v) =>
        double.IsNegativeInfinity(u) && double.IsPositiveInfinity(v) ? 0
        : double.IsNegativeInfinity(u) ? v - 1
        : double.IsPositiveInfinity(v) ? u + 1
        : (u + v) / 2;

    private static List<double> Sorteeritud(IEnumerable<double> punktid)
    {
        var tulemus = new List<double>();
        foreach (var p in punktid.Where(double.IsFinite).Order())
        {
            if (tulemus.Count == 0 || p - tulemus[^1] > Hulk.Tolerants) tulemus.Add(p);
        }
        return tulemus;
    }
}
