using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

public enum TingimuseLiik
{
    NullistErinev, // g(x) ≠ 0
    MitteNegatiivne, // g(x) ≥ 0
    Positiivne // g(x) > 0
}

/// <summary>Määramispiirkonna tingimus kujul g(x) ≠ 0, g(x) ≥ 0 või g(x) > 0.</summary>
public sealed record Tingimus(Expression Avaldis, TingimuseLiik Liik)
{
    public Func<double, double> Funktsioon { get; } = Hindaja.Kompileeri(Avaldis);

    public bool Kehtib(double x)
    {
        var g = Funktsioon(x);
        return Liik switch
        {
            TingimuseLiik.NullistErinev => !double.IsNaN(g) && g != 0,
            TingimuseLiik.MitteNegatiivne => g >= 0,
            _ => g > 0
        };
    }
}

/// <summary>
/// Kogub avaldise määramispiirkonna tingimused: nimetaja ≠ 0, paarisjuure alune ≥ 0,
/// logaritmitav > 0, tan/cot koosinus/siinus ≠ 0, arcsin/arccos argument lõigus [-1; 1].
/// </summary>
public sealed class Tingimused
{
    private readonly Dictionary<string, Tingimus> _tingimused = new();

    public IReadOnlyList<Tingimus> Koik => _tingimused.Values.ToList();

    /// <summary>Kogub tingimused valmis avaldisest (kasutatakse tuletiste puhul).</summary>
    public static Tingimused Kogu(Expression e)
    {
        var t = new Tingimused();
        t.KoguRekursiivselt(e);
        return t;
    }

    private void KoguRekursiivselt(Expression e)
    {
        switch (e)
        {
            case Expression.Sum s:
                foreach (var osa in s.Item) KoguRekursiivselt(osa);
                break;
            case Expression.Product p:
                foreach (var osa in p.Item) KoguRekursiivselt(osa);
                break;
            case Expression.Power p:
                Astmele(p.Item1, p.Item2);
                KoguRekursiivselt(p.Item1);
                KoguRekursiivselt(p.Item2);
                break;
            case Expression.Function f:
                Funktsioonile(f.Item1, f.Item2);
                KoguRekursiivselt(f.Item2);
                break;
        }
    }

    /// <returns>false, kui tingimus on konstantne ja ei kehti (nt 1/0).</returns>
    public bool Jagamisele(Expression nimetaja) => Lisa(nimetaja, TingimuseLiik.NullistErinev);

    public bool Astmele(Expression alus, Expression astendaja)
    {
        if (Avaldised.OnRatsionaalarv(astendaja, out var q))
        {
            if (Avaldised.OnTaisarv(q))
                return !Avaldised.OnNegatiivne(q) || Lisa(alus, TingimuseLiik.NullistErinev);
            if (q.Denominator.IsEven)
                return Lisa(alus, Avaldised.OnNegatiivne(q) ? TingimuseLiik.Positiivne : TingimuseLiik.MitteNegatiivne);
            // paaritu juur on defineeritud kõigil reaalarvudel
            return !Avaldised.OnNegatiivne(q) || Lisa(alus, TingimuseLiik.NullistErinev);
        }
        // muutuv või irratsionaalne astendaja (x^x, 2^x, x^π): alus peab olema positiivne
        return Lisa(alus, TingimuseLiik.Positiivne);
    }

    public bool Funktsioonile(Function f, Expression argument)
    {
        if (f.IsLn || f.IsLg)
            return Lisa(argument, TingimuseLiik.Positiivne);
        if (f.IsTan || f.IsSec)
            return Lisa(Operators.cos(argument), TingimuseLiik.NullistErinev);
        if (f.IsCot || f.IsCsc)
            return Lisa(Operators.sin(argument), TingimuseLiik.NullistErinev);
        if (f.IsAsin || f.IsAcos)
            return Lisa(Operators.subtract(Expression.One, Operators.pow(argument, Avaldised.Taisarv(2))),
                TingimuseLiik.MitteNegatiivne);
        return true;
    }

    private bool Lisa(Expression avaldis, TingimuseLiik liik)
    {
        var tingimus = new Tingimus(avaldis, liik);
        if (!Avaldised.SisaldabX(avaldis))
            return tingimus.Kehtib(0); // konstantne tingimus: kas kehtib või mitte
        _tingimused.TryAdd($"{liik}:{avaldis}", tingimus);
        return true;
    }
}
