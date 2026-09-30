using System.Globalization;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

public sealed record Punkt(double X, double Y);

public sealed record Ekstreemum(double X, double Y, string Tyyp);

public sealed class AnaluusiTulemus
{
    public required string ValemLatex { get; init; }
    public required string Maaramispiirkond { get; init; }
    public required string Nullkohad { get; init; }
    public required string Tuletis { get; init; }
    public required string TuletisLatex { get; init; }
    public required string KriitilisedPunktid { get; init; }
    public required string Ekstreemumid { get; init; }
    public required string Monotoonsus { get; init; }
    public required string Positiivsus { get; init; }
    public required string TeineTuletis { get; init; }
    public required string TeineTuletisLatex { get; init; }
    public required string Kaanupunktid { get; init; }
    public required string Kumerus { get; init; }

    /// <summary>Kas mõni tulemus on leitud numbriliselt ainult uuritavas vahemikus?</summary>
    public required bool Numbriline { get; init; }

    // graafiku jaoks
    public required IReadOnlyList<Punkt> NullkohaPunktid { get; init; }
    public required IReadOnlyList<Ekstreemum> EkstreemumPunktid { get; init; }
    public required IReadOnlyList<Punkt> KaanupunktiPunktid { get; init; }
    public required IReadOnlyList<double> Asumptoodid { get; init; }
    public required IReadOnlyList<double> Katkemised { get; init; }
    /// <summary>f, f' ja f'' määramispiirkonnad – graafikul katkestatakse iga joon ainult seal, kus see on määramata.</summary>
    public required Hulk PiirkondF { get; init; }
    public required Hulk PiirkondF1 { get; init; }
    public required Hulk PiirkondF2 { get; init; }
    public required Func<double, double> F { get; init; }
    public required Func<double, double> F1 { get; init; }
    public required Func<double, double> F2 { get; init; }
}

public sealed record Latexid(string Valem, string Tuletis, string TeineTuletis);

/// <summary>
/// Funktsiooni uurimine tuletise abil: määramispiirkond, nullkohad, positiivsus- ja negatiivsuspiirkond,
/// tuletis, kriitilised punktid, ekstreemumid, monotoonsus, teine tuletis, käänupunktid ja kumerus.
/// Sümbolarvutus (tuletised, lihtsustamine, polünoomid) – MathNet.Symbolics, numbrika – MathNet.Numerics.
/// </summary>
public sealed class FunktsiooniAnaluusija
{
    private const string Puuduvad = "puuduvad";

    /// <summary>Kontrollib valemit; tagastab veateate või null, kui valem on korras.</summary>
    public string? Kontrolli(string valem)
    {
        try
        {
            var f = ValemiParser.Parsi(valem).Avaldis;
            Avaldised.Tuletis(Avaldised.Tuletis(f));
            return null;
        }
        catch (ValemiViga viga)
        {
            return viga.Message;
        }
        catch (Exception)
        {
            return "Selle valemi tuletist ei õnnestunud leida – kontrolli valemi kuju.";
        }
    }

    public Latexid LeiaLatexid(string valem)
    {
        var f = ValemiParser.Parsi(valem).Avaldis;
        var f1 = Avaldised.Tuletis(f);
        return new Latexid(ValemiVormindaja.Latex(f), ValemiVormindaja.Latex(f1),
            ValemiVormindaja.Latex(Avaldised.Tuletis(f1)));
    }

    public AnaluusiTulemus Analuusi(string valem, double a, double b)
    {
        var parsitud = ValemiParser.Parsi(valem);
        var f = parsitud.Avaldis;
        var f1 = Avaldised.Tuletis(f);
        var f2 = Avaldised.Tuletis(f1);
        var F = Hindaja.Kompileeri(f);
        var F1 = Hindaja.Kompileeri(f1);
        var F2 = Hindaja.Kompileeri(f2);

        // määramispiirkonnad: f, f' ja f''
        var (D, dTaielik) = Piirkonnad.Maaramispiirkond(parsitud.Tingimused, a, b);
        var (D1, d1Taielik) = Piirkonnad.Maaramispiirkond(Tingimused.Kogu(f1).Koik, a, b);
        var (D2, d2Taielik) = Piirkonnad.Maaramispiirkond(Tingimused.Kogu(f2).Koik, a, b);

        // Numbriline otsing on täielik, kui kogu (tõkestatud) määramispiirkond mahub vahemikku [a; b].
        var dLoigus = dTaielik && !D.OnTuhi && D.OnLoigus(a, b);
        bool Taielik(params bool[] osad) => dLoigus || osad.All(o => o);
        // mittetäieliku otsingu korral kehtivad märgipiirkonnad ainult uuritud vahemikus
        Hulk Piirkond(bool taielik) => taielik ? D : D.Loika(a, b);

        // nullkohad
        var j0 = Nullkohad.Leia(f, a, b, D.KinnisedOtspunktid());
        var nullTaielik = Taielik(dTaielik, j0.Taielik);
        var nullkohad = j0.Vaartused.Where(D.Sisaldab).ToList();

        // kriitilised punktid: f'(x) = 0 või f'(x) ei eksisteeri (määramispiirkonna sisepunktis)
        var j1 = Nullkohad.Leia(f1, a, b);
        var tuletisTaielik = Taielik(dTaielik, d1Taielik, j1.Taielik);
        var statsionaarsed = j1.Vaartused.Where(x => D.SisaldabSisepunktina(x) && D1.Sisaldab(x)).ToList();
        var tuletisPuudub = D1.Otspunktid().Where(x => D.SisaldabSisepunktina(x) && !D1.Sisaldab(x)).ToList();
        var kriitilised = statsionaarsed.Concat(tuletisPuudub).Distinct().Order().ToList();

        var ekstreemumid = new List<Ekstreemum>();
        var koikMurdepunktid = kriitilised.Concat(D.Otspunktid()).Concat(D1.Otspunktid()).ToList();
        foreach (var c in kriitilised)
        {
            var delta = Delta(c, koikMurdepunktid);
            var (vasak, parem) = (F1(c - delta), F1(c + delta));
            var y = Arv.Silu(F(c));
            if (double.IsNaN(y)) continue;
            if (vasak > 0 && parem < 0) ekstreemumid.Add(new Ekstreemum(c, y, "max"));
            else if (vasak < 0 && parem > 0) ekstreemumid.Add(new Ekstreemum(c, y, "min"));
        }

        // monotoonsus ja positiivsus
        var (kasvab, kahaneb) = Piirkonnad.Margid(F1, Piirkond(tuletisTaielik),
            j1.Vaartused.Concat(D1.Otspunktid()), yhenda: true);
        var (positiivne, negatiivne) = Piirkonnad.Margid(F, Piirkond(nullTaielik), j0.Vaartused, yhenda: false);

        // teine tuletis: käänupunktid ja kumerus
        var j2 = Nullkohad.Leia(f2, a, b);
        var teineTaielik = Taielik(dTaielik, d2Taielik, j2.Taielik);
        var kaanukandidaadid = j2.Vaartused.Concat(D2.Otspunktid())
            .Where(D.SisaldabSisepunktina).Distinct().Order().ToList();
        var kaanupunktid = new List<Punkt>();
        foreach (var k in kaanukandidaadid)
        {
            var delta = Delta(k, kaanukandidaadid.Concat(D.Otspunktid()));
            var (vasak, parem) = (F2(k - delta), F2(k + delta));
            var y = Arv.Silu(F(k));
            if (!double.IsNaN(y) && vasak * parem < 0) kaanupunktid.Add(new Punkt(k, y));
        }
        // kumerust ei liideta üle punkti, kus f' puudub (teravik)
        var (noges, kumer) = Piirkonnad.Margid(F2, Piirkond(teineTaielik),
            j2.Vaartused.Concat(D2.Otspunktid()), yhenda: true, eiYhendaPunktides: D1.Otspunktid());

        var vahemik = $" (vahemikus [{Arv.Kumnendkuju(a)}; {Arv.Kumnendkuju(b)}])";
        string Markus(bool taielik) => taielik ? "" : vahemik;

        return new AnaluusiTulemus
        {
            ValemLatex = ValemiVormindaja.Latex(f),
            Maaramispiirkond = D.Tekst() + Markus(dTaielik),
            Nullkohad = j0.KoikPunktid ? "kõik x ∈ X" : Loend(nullkohad, "x") + Markus(nullTaielik),
            Tuletis = ValemiVormindaja.Tekst(f1),
            TuletisLatex = ValemiVormindaja.Latex(f1),
            KriitilisedPunktid = j1.KoikPunktid
                ? "kõik x ∈ X (funktsioon on konstantne)"
                : KriitilisteLoend(kriitilised, tuletisPuudub) + Markus(tuletisTaielik),
            Ekstreemumid = ekstreemumid.Count == 0
                ? Puuduvad + Markus(tuletisTaielik)
                : string.Join("; ", ekstreemumid.Select(e => Arv.Vordus($"{e.Tyyp} f({Arv.Luhike(e.X)})", e.Y)))
                  + Markus(tuletisTaielik),
            Monotoonsus = $"X↑ = {kasvab.Tekst()}; X↓ = {kahaneb.Tekst()}" + Markus(tuletisTaielik),
            Positiivsus = $"X⁺ = {positiivne.Tekst()}; X⁻ = {negatiivne.Tekst()}" + Markus(nullTaielik),
            TeineTuletis = ValemiVormindaja.Tekst(f2),
            TeineTuletisLatex = ValemiVormindaja.Latex(f2),
            Kaanupunktid = (kaanupunktid.Count == 0
                ? Puuduvad
                : string.Join("; ", kaanupunktid.Select((p, i) =>
                    $"K{(kaanupunktid.Count > 1 ? Alaindeks(i + 1) : "")}({Arv.Luhike(p.X)}; {Arv.Luhike(p.Y)})")))
                + Markus(teineTaielik),
            Kumerus = $"X∪ = {noges.Tekst()}; X∩ = {kumer.Tekst()}" + Markus(teineTaielik),
            Numbriline = !(dTaielik && nullTaielik && tuletisTaielik && teineTaielik),

            NullkohaPunktid = nullkohad.Select(x => new Punkt(x, 0)).ToList(),
            EkstreemumPunktid = ekstreemumid,
            KaanupunktiPunktid = kaanupunktid,
            Asumptoodid = D.Otspunktid().Where(p => OnPystasumptoot(F, D, p)).ToList(),
            Katkemised = Uniq(D.Otspunktid().Concat(D1.Otspunktid()).Concat(D2.Otspunktid())
                .Where(p => !D.SisaldabSisepunktina(p) || !D1.Sisaldab(p) || !D2.Sisaldab(p))),
            PiirkondF = D,
            PiirkondF1 = D1,
            PiirkondF2 = D2,
            F = F,
            F1 = F1,
            F2 = F2
        };
    }

    /// <summary>Samm märgi kontrolliks punkti ümbruses – väiksem kui kaugus lähima teise murdepunktini.</summary>
    private static double Delta(double c, IEnumerable<double> muudPunktid)
    {
        var lahim = muudPunktid.Where(p => Math.Abs(p - c) > Hulk.Tolerants)
            .Select(p => Math.Abs(p - c))
            .DefaultIfEmpty(1)
            .Min();
        return Math.Min(1e-4, lahim * 0.4);
    }

    /// <summary>Püstasümptoot: |f(x)| kasvab punktile lähenedes tõkestamatult (ka aeglaselt nagu ln x).</summary>
    private static bool OnPystasumptoot(Func<double, double> f, Hulk d, double p)
    {
        foreach (var suund in new[] { 1.0, -1.0 })
        {
            double lahedal = f(p + suund * 1e-7), kaugemal = f(p + suund * 1e-3);
            if (d.SisaldabSisepunktina(p + suund * 1e-7) && Math.Abs(lahedal) - Math.Abs(kaugemal) > 5)
                return true;
        }
        return false;
    }

    private static List<double> Uniq(IEnumerable<double> punktid)
    {
        var tulemus = new List<double>();
        foreach (var p in punktid.Order())
            if (tulemus.Count == 0 || p - tulemus[^1] > Hulk.Tolerants) tulemus.Add(p);
        return tulemus;
    }

    private static string Loend(IReadOnlyList<double> vaartused, string nimi) =>
        vaartused.Count == 0
            ? Puuduvad
            : string.Join("; ", vaartused.Select((v, i) =>
                Arv.Vordus(vaartused.Count > 1 ? nimi + Alaindeks(i + 1) : nimi, v)));

    private static string KriitilisteLoend(IReadOnlyList<double> kriitilised, IReadOnlyList<double> tuletisPuudub)
    {
        if (kriitilised.Count == 0) return Puuduvad;
        return string.Join("; ", kriitilised.Select((v, i) =>
            Arv.Vordus(kriitilised.Count > 1 ? "x" + Alaindeks(i + 1) : "x", v)
            + (tuletisPuudub.Any(t => Math.Abs(t - v) <= Hulk.Tolerants) ? " (f' puudub)" : "")));
    }

    private static string Alaindeks(int n) =>
        string.Concat(n.ToString(CultureInfo.InvariantCulture).Select(c => (char)('₀' + (c - '0'))));
}
