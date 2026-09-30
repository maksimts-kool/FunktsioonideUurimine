using System.Globalization;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Tunneb numbriliselt leitud arvus ära "ilusa" täpse kuju: täisarv, murd, ruutjuur (-√3, (1 + √5)/2),
/// π kordne (3π/2) või e aste (1/e). Nii saab kirjutada "x = -√3 ≈ -1.7321" mitte ainult "x ≈ -1.7321".
/// </summary>
public static class Arv
{
    private const double Tapsus = 1e-9;

    public static string Kumnendkuju(double v)
    {
        var tekst = Math.Round(v, 4).ToString("0.####", CultureInfo.InvariantCulture);
        return tekst == "-0" ? "0" : tekst;
    }

    /// <summary>Lühike kuju intervallide jaoks: täpne kuju, kui see on olemas, muidu kümnendmurd.</summary>
    public static string Luhike(double v)
    {
        if (double.IsPositiveInfinity(v)) return "∞";
        if (double.IsNegativeInfinity(v)) return "-∞";
        return TapneKuju(v) ?? Kumnendkuju(v);
    }

    /// <summary>"x = 2", "x = -√3 ≈ -1.7321" või "x ≈ 1.2346".</summary>
    public static string Vordus(string nimi, double v)
    {
        var tapne = TapneKuju(v);
        var kumnend = Kumnendkuju(v);
        if (tapne is null) return $"{nimi} ≈ {kumnend}";
        return tapne == kumnend ? $"{nimi} = {tapne}" : $"{nimi} = {tapne} ≈ {kumnend}";
    }

    /// <summary>Ümardab arvu täpse kuju väärtuseks (nt 1.9999999999 → 2), et vältida müra.</summary>
    public static double Silu(double v)
    {
        var lahim = Math.Round(v);
        // + 0.0 muudab -0 nulliks
        return (Math.Abs(v - lahim) < Tapsus * Math.Max(1, Math.Abs(v)) ? lahim : v) + 0.0;
    }

    public static string? TapneKuju(double v)
    {
        if (!double.IsFinite(v)) return null;
        var tol = Tapsus * Math.Max(1, Math.Abs(v));

        if (Math.Abs(v - Math.Round(v)) < tol)
            return Kumnendkuju(Math.Round(v));

        // lõplik kümnendmurd (0.5, -1.25, 6.3) on loetavam kui harilik murd
        if (Math.Abs(v * 1e4 - Math.Round(v * 1e4)) < tol * 1e4)
            return Kumnendkuju(v);

        if (Murd(v, tol) is { } murd)
            return MurruTekst(murd.p, murd.q, "", "");

        // π kordsed: π/2, -3π/4
        if (Murd(v / Math.PI, tol / Math.PI, maxLugeja: 60) is { } pi)
            return MurruTekst(pi.p, pi.q, "π", "");

        if (Juur(v, tol) is { } juur)
            return juur;

        // e astmed: e, 1/e, √e, 1/(2e), e√e
        foreach (var (aste, tekst) in EAstmed)
        {
            if (Murd(v / Math.Exp(aste), tol) is { } m)
                return aste > 0 ? MurruTekst(m.p, m.q, tekst, "") : MurruTekst(m.p, m.q, "", tekst);
        }

        // naturaallogaritmid: ln 2, ln(3/2), -ln 3
        var eAste = Math.Exp(Math.Abs(v));
        if (Math.Abs(v) < 20 && Murd(eAste, tol * eAste, maxLugeja: 100) is { } ln)
            return (v < 0 ? "-" : "") + (ln.q == 1 ? $"ln {ln.p}" : $"ln({ln.p}/{ln.q})");
        return null;
    }

    private static readonly (double aste, string tekst)[] EAstmed =
        new[] { (1.0, "e"), (0.5, "√e"), (2.0, "e²"), (1.5, "e√e"), (3.0, "e³") }
            .SelectMany(a => new[] { a, (-a.Item1, a.Item2) })
            .ToArray();

    /// <summary>Taandatud murd p/q (väikseim sobiv nimetaja leitakse esimesena).</summary>
    private static (long p, long q)? Murd(double v, double tol, int maxNimetaja = 12, int maxLugeja = 1000)
    {
        for (long q = 1; q <= maxNimetaja; q++)
        {
            var p = Math.Round(v * q);
            if (Math.Abs(p) <= maxLugeja && p != 0 && Math.Abs(v - p / q) < tol)
                return ((long)p, q);
        }
        return null;
    }

    /// <summary>Kujul (p + s√m)/c, kus m ei ole täisruut. Piiratud otsinguruum hoiab valepositiivsed ära.</summary>
    private static string? Juur(double v, double tol)
    {
        foreach (var c in new long[] { 1, 2, 3, 4, 6 })
        {
            for (var k = 0; k <= 24; k++)
            {
                long p = k % 2 == 0 ? k / 2 : -(k + 1) / 2; // 0, -1, 1, -2, 2, ...
                var t = v * c - p;
                var m = Math.Round(t * t);
                if (m < 2 || m > 1000) continue;
                var ruut = Math.Sqrt(m);
                if (ruut == Math.Floor(ruut)) continue;
                if (Math.Abs(Math.Abs(t) - ruut) >= tol * c) continue;

                var (kordaja, juurealune) = EraldaRuut((long)m);
                var s = Math.Sign(t) * kordaja;
                var jaguja = Syt(Syt(Math.Abs(p), Math.Abs(s)), c);
                p /= jaguja;
                s /= jaguja;
                var nimetaja = c / jaguja;

                var juureosa = (Math.Abs(s) == 1 ? "" : Math.Abs(s).ToString(CultureInfo.InvariantCulture)) + "√" + juurealune;
                string lugeja = p == 0
                    ? (s < 0 ? "-" : "") + juureosa
                    : $"{p} {(s < 0 ? "-" : "+")} {juureosa}";
                if (nimetaja == 1) return lugeja;
                return p == 0 ? $"{lugeja}/{nimetaja}" : $"({lugeja})/{nimetaja}";
            }
        }
        return null;
    }

    private static (long kordaja, long juurealune) EraldaRuut(long m)
    {
        long kordaja = 1;
        for (long i = 2; i * i <= m; i++)
        {
            while (m % (i * i) == 0)
            {
                m /= i * i;
                kordaja *= i;
            }
        }
        return (kordaja, m);
    }

    private static long Syt(long a, long b) => b == 0 ? Math.Max(a, 1) : Syt(b, a % b);

    /// <summary>p·A/(q·B) tekstina: "3π/2", "-1/(2e)", "e/2", "2/e".</summary>
    private static string MurruTekst(long p, long q, string lugejaKonstant, string nimetajaKonstant)
    {
        var mark = p < 0 ? "-" : "";
        var ap = Math.Abs(p);
        var lugeja = lugejaKonstant == "" ? ap.ToString(CultureInfo.InvariantCulture)
            : ap == 1 ? lugejaKonstant : $"{ap}{lugejaKonstant}";
        var nimetaja = nimetajaKonstant == "" ? (q == 1 ? "" : q.ToString(CultureInfo.InvariantCulture))
            : q == 1 ? (nimetajaKonstant.Length > 2 ? $"({nimetajaKonstant})" : nimetajaKonstant)
            : $"({q}{nimetajaKonstant})";
        return nimetaja == "" ? mark + lugeja : $"{mark}{lugeja}/{nimetaja}";
    }
}
