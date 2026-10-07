using System.Numerics;
using MathNet.Numerics;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Seletab võrrandi g(x) = 0 lahendamist samas järjekorras, nagu Nullkohad.Leia seda teeb:
/// tegurdamine, astme ja logaritmi lihtsustamine, lineaar- ja ruutvõrrand (diskriminant), bikvadraatvõrrand,
/// kõrgema astme polünoomi ratsionaalsed juured (Horneri skeem), murdvõrrand ja viimase võimalusena numbriline otsing.
/// Lõplikud lahendid võetakse alati Nullkohad.Leia tulemusest.
/// </summary>
public static class VorrandiSelgitus
{
    private const int MaxSugavus = 6;

    /// <param name="g">Avaldis, mille nullkohti otsitakse.</param>
    /// <param name="vasak">Võrrandi vasak pool LaTeX-is, nt "f(x)" või "f'(x)".</param>
    public static List<Samm> Sammud(Expression g, string vasak, Juured juured, double a, double b)
    {
        var sammud = new List<Samm> { new(null, $"{vasak} = 0 \\iff {L(g)} = 0") };
        Lahenda(g, sammud, a, b, 0);

        if (juured.KoikPunktid)
            sammud.Add(new("Samaselt null – kehtib iga $x$ korral."));
        else if (juured.Vaartused.Count == 0)
            sammud.Add(new("**Lahendid puuduvad.**" + (juured.Taielik ? "" : $" (Otsiti lõigul {Loik(a, b)}.)")));
        else if (sammud[^1].Valem != Loend(juured.Vaartused)) // ruutvõrrand jms annab sama loendi juba ise
            sammud.Add(new(juured.Taielik ? null : $"Lõigul {Loik(a, b)}:", Loend(juured.Vaartused)));
        return sammud;
    }

    /// <summary>Lahendite loend LaTeX-is: x_{1} = -\sqrt{3} \approx -1.7321, \quad x_{2} = 0.</summary>
    public static string Loend(IReadOnlyList<double> vaartused, string nimi = "x") =>
        string.Join(", \\quad ", vaartused.Select((v, i) =>
            Arv.VordusLatex(vaartused.Count > 1 ? $"{nimi}_{{{i + 1}}}" : nimi, v)));

    private static void Lahenda(Expression g, List<Samm> s, double a, double b, int sugavus)
    {
        if (sugavus > MaxSugavus) return;

        if (!Avaldised.SisaldabX(g))
        {
            if (Hindaja.Kompileeri(g)(0) != 0)
                s.Add(new($"${L(g)} \\neq 0$."));
            return;
        }

        if (Nullkohad.Tegurid(g) is { } tegurid)
        {
            if (g is Expression.Sum)
                s.Add(new("Toome ühise teguri sulgude ette:",
                    $"{string.Join(" \\cdot ", tegurid.Select(t => Sulgudes(L(t))))} = 0"));

            // murd: lugeja = 0 (nimetaja ei ole kunagi null ega muuda lahendeid)
            var nimetajas = tegurid.Where(t => t is Expression.Power { Item2: Expression.Number n } && Avaldised.OnNegatiivne(n.Item)).ToList();
            if (nimetajas.Count > 0)
            {
                tegurid = tegurid.Except(nimetajas).ToList();
                if (tegurid.Count == 0)
                {
                    s.Add(new("Lugeja on konstant $\\neq 0$."));
                    return;
                }
                var lugeja = tegurid.Aggregate(Expression.One, Operators.multiply);
                s.Add(new("Murd $= 0 \\iff$ lugeja $= 0$:", $"{L(lugeja)} = 0"));
            }

            if (tegurid.Count > 1)
            {
                s.Add(new("Korrutis $= 0$, kui mõni tegur $= 0$:",
                    string.Join(" \\quad \\text{või} \\quad ", tegurid.Select(t => $"{L(t)} = 0"))));
                foreach (var t in tegurid)
                {
                    if (t is Expression.Identifier)
                    {
                        s.Add(new("$x = 0$"));
                        continue;
                    }
                    s.Add(new($"${L(t)} = 0$:"));
                    Lahenda(t, s, a, b, sugavus + 1);
                    OsaTulemus(t, s, a, b);
                }
            }
            else if (tegurid[0] is not Expression.Identifier)
            {
                Lahenda(tegurid[0], s, a, b, sugavus + 1);
            }
            return;
        }

        switch (g)
        {
            case Expression.Power { Item2: Expression.Number p } aste:
                if (Avaldised.OnPositiivne(p.Item))
                {
                    s.Add(new("Aste $= 0 \\iff$ alus $= 0$:", $"{L(aste.Item1)} = 0"));
                    if (aste.Item1 is not Expression.Identifier) Lahenda(aste.Item1, s, a, b, sugavus + 1);
                }
                else
                {
                    s.Add(new($"${L(g)} \\neq 0$ (lugeja on 1)."));
                }
                return;
            case Expression.Power { Item1: var alus } when !Avaldised.SisaldabX(alus):
            case Expression.Function f when f.Item1.IsExp:
                s.Add(new($"${L(g)} > 0$ alati."));
                return;
            case Expression.Function f when f.Item1.IsLn || f.Item1.IsLg:
                s.Add(new(null, $"{L(g)} = 0 \\iff {L(f.Item2)} = 1"));
                Lahenda(Operators.subtract(f.Item2, Expression.One), s, a, b, sugavus + 1);
                return;
            case Expression.Sum summa when Nullkohad.Isoleeri(summa) is { } isoleeritud:
                if (!Avaldised.SisaldabX(isoleeritud))
                {
                    s.Add(new("Eksponent/juur ei saa olla negatiivne."));
                    return;
                }
                s.Add(new("Pöördfunktsiooniga:", $"{L(isoleeritud)} = 0"));
                Lahenda(isoleeritud, s, a, b, sugavus + 1);
                return;
        }

        var sym = Avaldised.Sym(g);
        var x = Avaldised.Sym(Avaldised.X);

        if (sym.IsPolynomial(x))
        {
            Polunoom(sym, s, "x");
            return;
        }

        if (Avaldised.OnRatsionaalfunktsioon(g))
        {
            try
            {
                var r = sym.RationalSimplify(x);
                var lugeja = r.Numerator();
                var nimetaja = r.Denominator();
                if (lugeja.IsPolynomial(x) && nimetaja.IsPolynomial(x))
                {
                    s.Add(new(null,
                        $"\\frac{{{L(lugeja.Expression)}}}{{{L(nimetaja.Expression)}}} = 0 \\iff " +
                        $"{L(lugeja.Expression)} = 0, \\quad {L(nimetaja.Expression)} \\neq 0"));
                    Polunoom(lugeja, s, "x");
                    var n = Hindaja.Kompileeri(nimetaja.Expression);
                    var valja = Nullkohad.Leia(lugeja.Expression, a, b).Vaartused.Where(v => Math.Abs(n(v)) <= 1e-12).ToList();
                    if (valja.Count > 0)
                        s.Add(new($"${Loend(valja)}$ – nimetaja $= 0$, ei sobi."));
                    return;
                }
            }
            catch (Exception)
            {
                // jätkame numbrilise seletusega
            }
        }

        s.Add(new($"Algebraliselt ei lahendu – **numbriliselt** lõigul {Loik(a, b)} (märgimuutus + Brenti meetod)."));
    }

    /// <summary>Ühe teguri lahendid tegurdamise järel.</summary>
    private static void OsaTulemus(Expression t, List<Samm> s, double a, double b)
    {
        var j = Nullkohad.Leia(t, a, b);
        if (j.KoikPunktid) return;
        s.Add(new(j.Vaartused.Count == 0 ? "→ lahendid puuduvad" : $"→ ${Loend(j.Vaartused)}$"));
    }

    // ---------------------------------------------------------------- polünoomid

    private static void Polunoom(SymbolicExpression p, List<Samm> s, string muutuja)
    {
        var x = Avaldised.Sym(Avaldised.X);
        BigRational[] k;
        try
        {
            var kordajad = p.Coefficients(x);
            if (!kordajad.All(c => Avaldised.OnRatsionaalarv(c.Expression, out _)))
            {
                if (kordajad.Length == 2)
                {
                    var lahend = Operators.divide(Operators.negate(kordajad[0].Expression), kordajad[1].Expression);
                    s.Add(new(null, $"{L(p.Expression)} = 0 \\iff {muutuja} = {L(lahend)}"));
                    return;
                }
                s.Add(new("Irratsionaalsed kordajad – juured **numbriliselt** (kaasmaatriksi omaväärtused, Newton)."));
                return;
            }
            k = kordajad.Select(c => ((Expression.Number)c.Expression).Item).ToArray();
        }
        catch (Exception)
        {
            return;
        }
        PolunoomKordajatest(k, s, muutuja, 0);
    }

    /// <param name="k">Kordajad kasvava astme järgi: k[0] + k[1]·x + … </param>
    private static void PolunoomKordajatest(BigRational[] k, List<Samm> s, string muutuja, int sugavus)
    {
        var aste = k.Length - 1;
        while (aste > 0 && k[aste].IsZero) aste--;
        k = k[..(aste + 1)];
        if (aste == 0) return;

        // x^m sulgude ette: x³ - 3x = x(x² - 3)
        var m = 0;
        while (k[m].IsZero) m++;
        if (m > 0)
        {
            var rest = k[m..];
            var xm = m == 1 ? muutuja : $"{muutuja}^{{{m}}}";
            if (rest.Length == 1)
            {
                if (Pol(k, muutuja) != muutuja)
                    s.Add(new($"${Pol(k, muutuja)} = 0 \\iff {muutuja} = 0$."));
                return;
            }
            s.Add(new($"${xm}$ sulgude ette:",
                $"{xm} \\left({Pol(rest, muutuja)}\\right) = 0 \\iff {muutuja} = 0 \\quad \\text{{või}} \\quad {Pol(rest, muutuja)} = 0"));
            k = rest;
            aste = k.Length - 1;
        }

        switch (aste)
        {
            case 1:
                Lineaar(k, s, muutuja);
                return;
            case 2:
                Ruut(k, s, muutuja);
                return;
            case 4 when k[1].IsZero && k[3].IsZero:
                Bikvadraat(k, s, muutuja);
                return;
        }

        if (sugavus < 4 && RatsionaalneJuur(k) is { } r)
        {
            var jagatis = Horner(k, r);
            var rL = L(Expression.NewNumber(r));
            s.Add(new($"Ratsionaalne juur (vabaliikme jagajate seast): $P({rL}) = 0$."));
            var tegur = Pol([-r, BigRational.One], muutuja);
            s.Add(new($"Horneri skeem, jagame $({tegur})$-ga:",
                $"{Pol(k, muutuja)} = \\left({tegur}\\right)\\left({Pol(jagatis, muutuja)}\\right) = 0"));
            if (jagatis.Length > 1)
            {
                PolunoomKordajatest(jagatis, s, muutuja, sugavus + 1);
            }
            return;
        }

        s.Add(new($"{aste}. aste, ratsionaalseid juuri pole – **numbriliselt** (kaasmaatriksi omaväärtused, Newton)."));
    }

    private static void Lineaar(BigRational[] k, List<Samm> s, string muutuja)
    {
        var lahend = -k[0] / k[1];
        var parem = L(Expression.NewNumber(-k[0]));
        var tulemus = L(Expression.NewNumber(lahend));
        s.Add(new(null, k[1] == BigRational.One
            ? $"{Pol(k, muutuja)} = 0 \\iff {muutuja} = {parem}"
            : $"{Pol(k, muutuja)} = 0 \\iff {Pol([BigRational.Zero, k[1]], muutuja)} = {parem} \\iff {muutuja} = {tulemus}"));
    }

    private static void Ruut(BigRational[] k, List<Samm> s, string muutuja)
    {
        var (c, b, a) = (k[0], k[1], k[2]);
        string N(BigRational q) => L(Expression.NewNumber(q));
        string NS(BigRational q) => Sulgudes(N(q));

        if (b.IsZero)
        {
            var parem = -c / a;
            s.Add(new(null,
                $"{Pol(k, muutuja)} = 0 \\iff {muutuja}^{{2}} = {N(parem)}"));
            if (Avaldised.OnNegatiivne(parem))
                s.Add(new("Ruut ei saa olla negatiivne – lahendid puuduvad."));
            else
            {
                var juur = Arv.Latex(Math.Sqrt(Avaldised.Kahendarv(parem)));
                var juurKuju = $"\\sqrt{{{N(parem)}}}";
                s.Add(new(null, juur == juurKuju ? $"{muutuja} = \\pm{juurKuju}" : $"{muutuja} = \\pm{juurKuju} = \\pm {juur}"));
            }
            return;
        }

        var d = b * b - Arv4 * a * c;
        s.Add(new($"$a = {N(a)},\\ b = {N(b)},\\ c = {N(c)}$:",
            $"D = b^{{2}} - 4ac = {NS(b)}^{{2}} - 4 \\cdot {NS(a)} \\cdot {NS(c)} = {N(d)}"));

        if (Avaldised.OnNegatiivne(d))
        {
            s.Add(new("$D < 0$ – lahendid puuduvad."));
            return;
        }
        if (d.IsZero)
        {
            var x0 = -b / (Arv2 * a);
            s.Add(new("$D = 0$:",
                $"{muutuja} = -\\frac{{b}}{{2a}} = -\\frac{{{N(b)}}}{{{N(Arv2 * a)}}} = {N(x0)}"));
            return;
        }

        var sqrtD = Math.Sqrt(Avaldised.Kahendarv(d));
        var (ad, bd) = (Avaldised.Kahendarv(a), Avaldised.Kahendarv(b));
        var lahendid = new[] { (-bd - sqrtD) / (2 * ad), (-bd + sqrtD) / (2 * ad) }.Order().ToList();
        s.Add(new("$D > 0$:",
            $"{muutuja}_{{1,2}} = \\frac{{-b \\pm \\sqrt{{D}}}}{{2a}} = \\frac{{{N(-b)} \\pm \\sqrt{{{N(d)}}}}}{{{N(Arv2 * a)}}}"));
        s.Add(new(null, Loend(lahendid.Select(Arv.Silu).ToList(), muutuja)));
    }

    private static void Bikvadraat(BigRational[] k, List<Samm> s, string muutuja)
    {
        var t = new[] { k[0], k[2], k[4] };
        s.Add(new($"Asendus $t = {muutuja}^{{2}} \\geq 0$:", $"{Pol(t, "t")} = 0"));
        Ruut(t, s, "t");

        var (c, b, a) = (Avaldised.Kahendarv(t[0]), Avaldised.Kahendarv(t[1]), Avaldised.Kahendarv(t[2]));
        var d = b * b - 4 * a * c;
        if (d < 0) return;
        var tJuured = d == 0 ? new[] { -b / (2 * a) } : [(-b - Math.Sqrt(d)) / (2 * a), (-b + Math.Sqrt(d)) / (2 * a)];
        foreach (var tj in tJuured.Select(Arv.Silu).Distinct())
        {
            s.Add(new(tj < 0
                ? $"$t = {Arv.Latex(tj)} < 0$ – ei sobi."
                : $"${muutuja}^{{2}} = {Arv.Latex(tj)} \\Rightarrow {muutuja} = \\pm {Arv.Latex(Math.Sqrt(tj))}$"));
        }
    }

    /// <summary>Ratsionaalsete juurte teoreem: ±p/q, p | a₀, q | aₙ (kordajad korrutatakse täisarvudeks).</summary>
    private static BigRational? RatsionaalneJuur(BigRational[] k)
    {
        var vjk = k.Aggregate(BigInteger.One, (acc, q) => acc * q.Denominator / BigInteger.GreatestCommonDivisor(acc, q.Denominator));
        var t = k.Select(q => q.Numerator * (vjk / q.Denominator)).ToArray();
        var (a0, an) = (BigInteger.Abs(t[0]), BigInteger.Abs(t[^1]));
        if (a0 > 1_000_000 || an > 1_000_000) return null;

        var nimetajad = Jagajad((long)an);
        foreach (var p in Jagajad((long)a0))
        foreach (var q in nimetajad)
        foreach (var mark in new[] { 1, -1 })
        {
            var r = BigRational.FromBigIntFraction(mark * p, q);
            if (Vaartus(k, r).IsZero) return r;
        }
        return null;
    }

    private static List<long> Jagajad(long n)
    {
        var jagajad = new List<long>();
        for (long i = 1; i * i <= n; i++)
        {
            if (n % i != 0) continue;
            jagajad.Add(i);
            if (i * i != n) jagajad.Add(n / i);
        }
        return jagajad.Order().ToList();
    }

    private static BigRational Vaartus(BigRational[] k, BigRational x)
    {
        var tulemus = BigRational.Zero;
        for (var i = k.Length - 1; i >= 0; i--) tulemus = tulemus * x + k[i];
        return tulemus;
    }

    /// <summary>Horneri skeem: P(x) / (x - r), kordajad kasvava astme järgi.</summary>
    private static BigRational[] Horner(BigRational[] k, BigRational r)
    {
        var n = k.Length - 1;
        var q = new BigRational[n];
        var jaak = k[n];
        for (var i = n - 1; i >= 0; i--)
        {
            q[i] = jaak;
            jaak = jaak * r + k[i];
        }
        return q;
    }

    private static string Pol(BigRational[] k, string muutuja)
    {
        var m = Expression.Symbol(muutuja);
        var avaldis = k.Select((c, i) => Operators.multiply(Expression.NewNumber(c), Operators.pow(m, Avaldised.Taisarv(i))))
            .Aggregate(Expression.Zero, Operators.add);
        return L(avaldis);
    }

    private static readonly BigRational Arv2 = BigRational.FromInt(2);
    private static readonly BigRational Arv4 = BigRational.FromInt(4);

    private static string L(Expression e) => ValemiVormindaja.Latex(e);

    private static string Sulgudes(string latex) => ValemiVormindaja.Sulgudes(latex);

    private static string Loik(double a, double b) => $"$[{Arv.Latex(a)};\\ {Arv.Latex(b)}]$";
}
