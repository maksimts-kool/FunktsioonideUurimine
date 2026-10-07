using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <param name="Vaartused">Leitud reaalarvulised nullkohad kasvavas järjekorras.</param>
/// <param name="Taielik">true – leitud kõik nullkohad (polünoom/ratsionaalfunktsioon); false – ainult antud vahemikus.</param>
/// <param name="KoikPunktid">Avaldis on samaselt null (nt konstantse funktsiooni tuletis).</param>
public sealed record Juured(IReadOnlyList<double> Vaartused, bool Taielik, bool KoikPunktid = false);

/// <summary>
/// Leiab avaldise g(x) nullkohad. Polünoomi ja ratsionaalfunktsiooni korral kõik reaalarvulised
/// nullkohad (kaasmaatriksi omaväärtused, MathNet.Numerics FindRoots.Polynomial), muul juhul numbriliselt
/// antud vahemikus (märgivahetus + Brenti meetod, puutuvad nullkohad tuletise kaudu).
/// </summary>
public static class Nullkohad
{
    private const int Samme = 4000;

    public static Juured Leia(Expression g, double a, double b, IEnumerable<double>? lisakandidaadid = null)
    {
        if (!Avaldised.SisaldabX(g))
        {
            var vaartus = Hindaja.Kompileeri(g)(0);
            return new Juured([], true, KoikPunktid: vaartus == 0);
        }

        // korrutis on null parajasti siis, kui mõni tegur on null: x·e^(-x) → {0} ∪ ∅
        if (Tegurid(g) is { } tegurid)
        {
            var osad = tegurid.Select(t => Leia(t, a, b, lisakandidaadid)).ToList();
            return new Juured(Uniq(osad.SelectMany(o => o.Vaartused)), osad.All(o => o.Taielik),
                osad.Any(o => o.KoikPunktid));
        }

        switch (g)
        {
            // u^p = 0 ⟺ u = 0 (p > 0); u^(-p) ja c^u (c > 0) ei ole kunagi nullid
            case Expression.Power { Item2: Expression.Number p } aste:
                return Avaldised.OnPositiivne(p.Item)
                    ? Leia(aste.Item1, a, b, lisakandidaadid)
                    : new Juured([], true);
            case Expression.Power { Item1: var alus } when !Avaldised.SisaldabX(alus):
                return new Juured([], true);
            case Expression.Function f when f.Item1.IsExp:
                return new Juured([], true);
            // ln u = 0 ⟺ u = 1
            case Expression.Function f when f.Item1.IsLn || f.Item1.IsLg:
                return Leia(Operators.subtract(f.Item2, Expression.One), a, b, lisakandidaadid);
            // k·ln u + c = 0 ⟺ u = e^(-c/k) jne
            case Expression.Sum summa when Isoleeri(summa) is { } isoleeritud:
                return Leia(isoleeritud, a, b, lisakandidaadid);
        }

        var sym = Avaldised.Sym(g);
        var x = Avaldised.Sym(Avaldised.X);

        if (Polunoomi(sym, x) is { } polunoomiJuured)
            return new Juured(polunoomiJuured, true);

        if (Avaldised.OnRatsionaalfunktsioon(g))
        {
            try
            {
                var ratsionaalne = sym.RationalSimplify(x);
                var lugeja = ratsionaalne.Numerator();
                var nimetaja = ratsionaalne.Denominator();
                if (lugeja.IsPolynomial(x) && nimetaja.IsPolynomial(x) && Polunoomi(lugeja, x) is { } lugejaJuured)
                {
                    var n = Hindaja.Kompileeri(nimetaja.Expression);
                    return new Juured(lugejaJuured.Where(r => Math.Abs(n(r)) > 1e-12).ToList(), true);
                }
            }
            catch (Exception)
            {
                // ei ole ratsionaalfunktsioon – jätkame numbriliselt
            }
        }

        var dg = Hindaja.Kompileeri(Avaldised.Sym(g).Differentiate(x).Expression);
        return new Juured(Numbriliselt(Hindaja.Kompileeri(g), dg, a, b, lisakandidaadid ?? []), false);
    }

    /// <summary>
    /// Jagab avaldise teguriteks: korrutise tegurid või summa ühine tegur
    /// (e^(-x) - x·e^(-x) = e^(-x)·(1 - x)). Tagastab null, kui tükeldada ei saa.
    /// </summary>
    internal static List<Expression>? Tegurid(Expression g)
    {
        if (g is Expression.Product korrutis)
        {
            // konstantsed tegurid (≠ 0) nullkohti ei mõjuta
            var xTegurid = korrutis.Item.Where(Avaldised.SisaldabX).ToList();
            return xTegurid.Count > 0 ? xTegurid : null;
        }

        if (g is not Expression.Sum summa) return null;
        var liidetavad = summa.Item.ToList();
        static IEnumerable<Expression> TeguridEraldi(Expression e) =>
            e is Expression.Product p ? p.Item : [e];

        var yhine = TeguridEraldi(liidetavad[0])
            .Where(Avaldised.SisaldabX)
            .FirstOrDefault(t => liidetavad.Skip(1).All(l => TeguridEraldi(l).Contains(t)));
        if (yhine is null) return null;

        var jagatis = liidetavad.Aggregate(Expression.Zero,
            (s, l) => Operators.add(s, Operators.divide(l, yhine)));
        return [yhine, jagatis];
    }

    /// <summary>
    /// Summa, milles ainult üks liidetav sõltub x-ist ja see on k·F(u), kus F on ln, lg, exp, c^u või √:
    /// võrrand k·F(u) + c = 0 teisendatakse kujule u - F⁻¹(-c/k) = 0. Lahendit pole → konstant 1.
    /// </summary>
    internal static Expression? Isoleeri(Expression.Sum summa)
    {
        var xLiikmed = summa.Item.Where(Avaldised.SisaldabX).ToList();
        if (xLiikmed.Count != 1) return null;
        var vabaliige = summa.Item.Where(l => !Avaldised.SisaldabX(l))
            .Aggregate(Expression.Zero, Operators.add);

        var liige = xLiikmed[0];
        var kordaja = Expression.One;
        var funktsioon = liige;
        if (liige is Expression.Product korrutis)
        {
            var xTegurid = korrutis.Item.Where(Avaldised.SisaldabX).ToList();
            if (xTegurid.Count != 1) return null;
            funktsioon = xTegurid[0];
            kordaja = korrutis.Item.Where(t => !Avaldised.SisaldabX(t)).Aggregate(Expression.One, Operators.multiply);
        }

        var r = Operators.divide(Operators.negate(vabaliige), kordaja); // F(u) = r
        var rVaartus = Hindaja.Kompileeri(r)(0);
        if (double.IsNaN(rVaartus)) return null;

        return funktsioon switch
        {
            Expression.Function f when f.Item1.IsLn => Operators.subtract(f.Item2, Operators.exp(r)),
            Expression.Function f when f.Item1.IsLg =>
                Operators.subtract(f.Item2, Operators.pow(Avaldised.Taisarv(10), r)),
            Expression.Function f when f.Item1.IsExp =>
                rVaartus > 0 ? Operators.subtract(f.Item2, Operators.ln(r)) : Expression.One,
            Expression.Power { Item1: var c, Item2: var u } when !Avaldised.SisaldabX(c) =>
                rVaartus > 0 ? Operators.subtract(u, Operators.divide(Operators.ln(r), Operators.ln(c))) : Expression.One,
            Expression.Power { Item2: Expression.Number p } aste when p.Item == BigRational.FromIntFraction(1, 2) =>
                rVaartus >= 0 ? Operators.subtract(aste.Item1, Operators.pow(r, Avaldised.Taisarv(2))) : Expression.One,
            _ => null
        };
    }

    private static List<double>? Polunoomi(SymbolicExpression p, SymbolicExpression x)
    {
        if (!p.IsPolynomial(x)) return null;

        // ruutvaba osa p / syt(p, p') – kordsed nullkohad muutuvad lihtsateks ja omaväärtused täpseks
        var ruutvaba = p;
        try
        {
            var syt = p.PolynomialGcd(x, p.Differentiate(x));
            if (Aste(syt, x) > 0) ruutvaba = p.PolynomialQuotient(x, syt);
        }
        catch (Exception)
        {
            // irratsionaalsete kordajatega polünoom (nt π·x) – kasutame algset
        }

        var kordajad = ruutvaba.Coefficients(x).Select(k => Hindaja.Kompileeri(k.Expression)(0)).ToArray();
        if (kordajad.Any(double.IsNaN)) return null;
        var aste = kordajad.Length - 1;
        while (aste > 0 && kordajad[aste] == 0) aste--;
        if (aste == 0) return [];
        kordajad = kordajad[..(aste + 1)];

        var juured = new List<double>();
        foreach (var z in FindRoots.Polynomial(kordajad))
        {
            if (Math.Abs(z.Imaginary) > 1e-7 * Math.Max(1, Math.Abs(z.Real))) continue;
            juured.Add(Arv.Silu(Newton(kordajad, z.Real)));
        }
        return Uniq(juured);
    }

    private static int Aste(SymbolicExpression p, SymbolicExpression x)
    {
        var kordajad = p.Coefficients(x);
        return kordajad.Length - 1;
    }

    /// <summary>Täpsustab omaväärtusest saadud juurt Newtoni meetodiga (Horneri skeem).</summary>
    private static double Newton(double[] k, double x)
    {
        for (var i = 0; i < 8; i++)
        {
            double p = 0, dp = 0;
            for (var j = k.Length - 1; j >= 0; j--)
            {
                dp = dp * x + p;
                p = p * x + k[j];
            }
            if (dp == 0 || !double.IsFinite(p / dp)) break;
            var uus = x - p / dp;
            if (Math.Abs(uus - x) > 1e-3 * Math.Max(1, Math.Abs(x))) break; // ei kaldu kõrvale
            x = uus;
        }
        return x;
    }

    private static List<double> Numbriliselt(Func<double, double> g, Func<double, double> dg, double a, double b,
        IEnumerable<double> lisakandidaadid)
    {
        var h = (b - a) / Samme;
        var xs = new double[Samme + 1];
        var ys = new double[Samme + 1];
        for (var i = 0; i <= Samme; i++)
        {
            xs[i] = i == Samme ? b : a + i * h;
            ys[i] = g(xs[i]);
        }

        var juured = new List<double>();
        for (var i = 0; i <= Samme; i++)
        {
            if (ys[i] == 0)
            {
                juured.Add(xs[i]);
                continue;
            }

            // märgivahetus: Brenti meetod; poolustel (tan x) on |g| suur ja need jäetakse välja
            if (i < Samme && double.IsFinite(ys[i]) && double.IsFinite(ys[i + 1]) && ys[i] * ys[i + 1] < 0
                && Brent.TryFindRoot(g, xs[i], xs[i + 1], 1e-14, 200, out var juur) && Math.Abs(g(juur)) < 1e-7)
            {
                juured.Add(juur);
            }

            // puutuv nullkoht (nt sin²x): |g| lokaalne miinimum, täpsustame g'(x) = 0 kaudu
            if (i > 0 && i < Samme && double.IsFinite(ys[i]) && double.IsFinite(ys[i - 1]) && double.IsFinite(ys[i + 1])
                && Math.Sign(ys[i - 1]) == Math.Sign(ys[i]) && Math.Sign(ys[i + 1]) == Math.Sign(ys[i])
                && Math.Abs(ys[i]) <= Math.Abs(ys[i - 1]) && Math.Abs(ys[i]) <= Math.Abs(ys[i + 1])
                && Math.Abs(ys[i]) < 1e-3
                && Brent.TryFindRoot(dg, xs[i - 1], xs[i + 1], 1e-14, 200, out var puutuv)
                && Math.Abs(g(puutuv)) < 1e-10)
            {
                juured.Add(puutuv);
            }
        }

        foreach (var k in lisakandidaadid)
        {
            if (k >= a - 1e-9 && k <= b + 1e-9 && Math.Abs(g(k)) < 1e-10)
                juured.Add(k);
        }

        return Uniq(juured.Select(Arv.Silu));
    }

    private static List<double> Uniq(IEnumerable<double> vaartused)
    {
        var tulemus = new List<double>();
        foreach (var v in vaartused.Order())
        {
            if (tulemus.Count == 0 || Math.Abs(v - tulemus[^1]) > 1e-7 * Math.Max(1, Math.Abs(v)))
                tulemus.Add(v);
        }
        return tulemus;
    }
}
