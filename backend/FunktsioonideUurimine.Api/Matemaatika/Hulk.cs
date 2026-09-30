namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>Arvtelje lõik/vahemik; Algus == Lopp tähendab üksikpunkti.</summary>
public readonly record struct Loik(double Algus, double Lopp, bool AlgusKaasa, bool LoppKaasa)
{
    public bool OnPunkt => Algus == Lopp;

    public string Tekst()
    {
        if (OnPunkt) return $"{{{Arv.Luhike(Algus)}}}";
        return $"{(AlgusKaasa ? "[" : "(")}{Arv.Luhike(Algus)}; {Arv.Luhike(Lopp)}{(LoppKaasa ? "]" : ")")}";
    }
}

/// <summary>Lõikude ühend, nt määramispiirkond (-∞; 2) ∪ (2; ∞).</summary>
public sealed class Hulk(IReadOnlyList<Loik> loigud)
{
    public const double Tolerants = 1e-9;

    public static readonly Hulk Reaalarvud = new([new Loik(double.NegativeInfinity, double.PositiveInfinity, false, false)]);

    public IReadOnlyList<Loik> Loigud { get; } = loigud;

    public bool OnTuhi => Loigud.Count == 0;

    public bool Sisaldab(double x) => Loigud.Any(l =>
        (x > l.Algus + Tolerants || (l.AlgusKaasa && Math.Abs(x - l.Algus) <= Tolerants)) &&
        (x < l.Lopp - Tolerants || (l.LoppKaasa && Math.Abs(x - l.Lopp) <= Tolerants)));

    /// <summary>Kas x on mõne lõigu sisepunkt (mitte otspunkt)?</summary>
    public bool SisaldabSisepunktina(double x) =>
        Loigud.Any(l => x > l.Algus + Tolerants && x < l.Lopp - Tolerants);

    /// <summary>Kinnised otspunktid – kandidaadid nullkohtadeks, nt √(4 - x²) korral ±2.</summary>
    public IEnumerable<double> KinnisedOtspunktid() => Loigud
        .SelectMany(l => new[] { (l.Algus, l.AlgusKaasa), (l.Lopp, l.LoppKaasa) })
        .Where(o => o.Item2 && double.IsFinite(o.Item1))
        .Select(o => o.Item1)
        .Distinct();

    /// <summary>Kõik lõplikud otspunktid (lahtised ja kinnised).</summary>
    public IEnumerable<double> Otspunktid() => Loigud
        .SelectMany(l => new[] { l.Algus, l.Lopp })
        .Where(double.IsFinite)
        .Distinct();

    /// <summary>Ühisosa lõiguga [a; b].</summary>
    public Hulk Loika(double a, double b)
    {
        var tulemus = new List<Loik>();
        foreach (var l in Loigud)
        {
            var algus = Math.Max(l.Algus, a);
            var lopp = Math.Min(l.Lopp, b);
            var algusKaasa = algus == l.Algus ? l.AlgusKaasa : true;
            var loppKaasa = lopp == l.Lopp ? l.LoppKaasa : true;
            if (algus < lopp || (algus == lopp && algusKaasa && loppKaasa))
                tulemus.Add(new Loik(algus, lopp, algusKaasa, loppKaasa));
        }
        return new Hulk(tulemus);
    }

    /// <summary>Kas kõik lõigud mahuvad lõiku [a; b]?</summary>
    public bool OnLoigus(double a, double b) => Loigud.All(l => l.Algus >= a - Tolerants && l.Lopp <= b + Tolerants);

    public string Tekst() => OnTuhi ? "∅" : string.Join(" ∪ ", Loigud.Select(l => l.Tekst()));
}
