using System.Globalization;
using System.Text;
using MathNet.Numerics;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Vormindab avaldise tekstiks ("3*x^2 - 3") või LaTeX-iks ("3x^{2} - 3").
/// MathNet'i enda vormindaja järjestab liikmed kasvavalt (-3 + 3*x^2) ja teeb vigu (\pix),
/// koolis on tavaks kahanev järjestus. Tekstikuju on meie parseriga uuesti loetav.
/// </summary>
public static class ValemiVormindaja
{
    public static string Tekst(Expression e) => new Kirjutaja(latex: false).V(e, 0);

    public static string Latex(Expression e) => new Kirjutaja(latex: true).V(e, 0);

    /// <summary>LaTeX, milles x asemel on arv: f(-1) = (-1)^{3} - 3 \cdot (-1). Lahenduskäigu jaoks.</summary>
    public static string LatexAsendusega(Expression e, double x)
    {
        var arv = Matemaatika.Arv.Latex(x);
        var lihtne = arv.All(char.IsDigit) || arv == "\\pi";
        return new Kirjutaja(latex: true, lihtne ? arv : $"\\left({arv}\\right)").V(e, 0);
    }

    /// <summary>Liidetavad samas järjekorras, nagu vormindaja need kirjutab (kahanev aste).</summary>
    public static List<Expression> Jarjesta(IEnumerable<Expression> liikmed) =>
        liikmed.Select((l, i) => (l, i)).OrderByDescending(t => Kirjutaja.MonoomiAste(t.l)).ThenBy(t => t.i)
            .Select(t => t.l).ToList();

    /// <summary>Sulud ümber LaTeX-i, kui see on (ülemisel tasemel) summa/vahe või algab miinusega.</summary>
    public static string Sulgudes(string latex)
    {
        if (latex.StartsWith('-')) return $"\\left({latex}\\right)";
        var sugavus = 0;
        for (var i = 0; i < latex.Length; i++)
        {
            if (latex[i] == '{' || latex.AsSpan(i).StartsWith("\\left")) sugavus++;
            else if (latex[i] == '}' || latex.AsSpan(i).StartsWith("\\right")) sugavus--;
            else if (sugavus == 0 && (latex.AsSpan(i).StartsWith(" + ") || latex.AsSpan(i).StartsWith(" - ")))
                return $"\\left({latex}\\right)";
        }
        return latex;
    }

    // prioriteedid: 1 summa/unaarne miinus, 2 korrutis/jagatis, 3 aste, 4 funktsioon, 5 aatom
    private sealed class Kirjutaja(bool latex, string? xAsendus = null)
    {
        public string V(Expression e, int noutud)
        {
            var (tekst, prioriteet) = Vorminda(e);
            if (prioriteet >= noutud) return tekst;
            return latex ? $"\\left({tekst}\\right)" : $"({tekst})";
        }

        private (string, int) Vorminda(Expression e) => e switch
        {
            Expression.Number n => Arv(n.Item),
            Expression.Approximation { Item: Approximation.Real r } =>
                (r.Item.ToString("G10", CultureInfo.InvariantCulture), r.Item < 0 ? 1 : 5),
            Expression.Identifier id => (xAsendus ?? id.Item.Item, 5),
            Expression.Constant c => (c.Item.IsPi ? (latex ? "\\pi" : "pi") : c.Item.IsE ? "e" : "i", 5),
            Expression.Sum s => (Summa(s.Item.ToList()), 1),
            Expression.Product p => Korrutis(p.Item.ToList()),
            Expression.Power p => Aste(p.Item1, p.Item2),
            Expression.Function f => (Funktsioon(f.Item1, f.Item2), 4),
            _ when e.IsPositiveInfinity => (latex ? "\\infty" : "∞", 5),
            _ when e.IsNegativeInfinity => (latex ? "-\\infty" : "-∞", 1),
            _ => (e.ToString(), 1)
        };

        private (string, int) Arv(BigRational q)
        {
            var negatiivne = Avaldised.OnNegatiivne(q);
            var lugeja = BigIntegerAbs(q.Numerator).ToString(CultureInfo.InvariantCulture);
            var margiga = negatiivne ? "-" : "";
            if (Avaldised.OnTaisarv(q)) return (margiga + lugeja, negatiivne ? 1 : 5);
            var nimetaja = q.Denominator.ToString(CultureInfo.InvariantCulture);
            return latex
                ? ($"{margiga}\\frac{{{lugeja}}}{{{nimetaja}}}", negatiivne ? 1 : 5)
                : ($"{margiga}{lugeja}/{nimetaja}", negatiivne ? 1 : 2);
        }

        // ------------------------------------------------------------ summa

        private string Summa(List<Expression> liikmed)
        {
            // kahanev astendaja järgi; mittepolünoomilised liikmed polünoomiliste ja vabaliikme vahele
            var jarjestatud = liikmed
                .Select((l, i) => (l, i, aste: MonoomiAste(l)))
                .OrderByDescending(t => t.aste)
                .ThenBy(t => t.i)
                .Select(t => t.l)
                .ToList();

            // "4 - x^2" on loetavam kui "-x^2 + 4"
            if (jarjestatud.Count == 2 && OnNegatiivne(jarjestatud[0]) && !OnNegatiivne(jarjestatud[1]))
                jarjestatud.Reverse();

            var sb = new StringBuilder(V(jarjestatud[0], 1));
            foreach (var liige in jarjestatud.Skip(1))
            {
                if (OnNegatiivne(liige))
                    sb.Append(" - ").Append(V(Operators.negate(liige), 2));
                else
                    sb.Append(" + ").Append(V(liige, 1));
            }
            return sb.ToString();
        }

        internal static double MonoomiAste(Expression e) => e switch
        {
            Expression.Number or Expression.Constant => 0,
            Expression.Identifier => 1,
            Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number n } => Avaldised.Kahendarv(n.Item),
            Expression.Product p when p.Item.All(f => f is Expression.Number or Expression.Constant
                                                        || MonoomiAste(f) is not 0.5) =>
                p.Item.Sum(MonoomiAste),
            _ => Avaldised.SisaldabX(e) ? 0.5 : 0
        };

        private static bool OnNegatiivne(Expression e) => e switch
        {
            Expression.Number n => Avaldised.OnNegatiivne(n.Item),
            Expression.Approximation { Item: Approximation.Real r } => r.Item < 0,
            Expression.Product p => p.Item.Head is Expression.Number n && Avaldised.OnNegatiivne(n.Item),
            _ => false
        };

        // ------------------------------------------------------------ korrutis ja jagatis

        private (string, int) Korrutis(List<Expression> tegurid)
        {
            var kordaja = BigRational.One;
            var lugeja = new List<Expression>();
            var nimetaja = new List<Expression>();
            foreach (var t in tegurid)
            {
                if (t is Expression.Number n)
                    kordaja *= n.Item;
                else if (t is Expression.Power { Item2: Expression.Number a } p && Avaldised.OnNegatiivne(a.Item))
                    nimetaja.Add(Operators.pow(p.Item1, Expression.NewNumber(-a.Item)));
                else
                    lugeja.Add(t);
            }

            // kooli järjekord: konstandid, x astmed, sulud, funktsioonid – "x e^{-x}", mitte "e^{-x} x"
            lugeja = lugeja.OrderBy(TeguriJark).ToList();
            nimetaja = nimetaja.OrderBy(TeguriJark).ToList();

            var mark = Avaldised.OnNegatiivne(kordaja) ? "-" : "";
            var absLugeja = BigIntegerAbs(kordaja.Numerator);
            if (!absLugeja.IsOne || lugeja.Count == 0)
                lugeja.Insert(0, Expression.NewNumber(BigRational.FromBigInt(absLugeja)));
            if (!kordaja.Denominator.IsOne)
                nimetaja.Insert(0, Expression.NewNumber(BigRational.FromBigInt(kordaja.Denominator)));

            if (nimetaja.Count == 0)
                return (mark + Tegurid(lugeja), mark == "" ? 2 : 1);

            if (latex)
            {
                var lu = lugeja.Count == 1 ? V(lugeja[0], 0) : Tegurid(lugeja);
                var ni = nimetaja.Count == 1 ? V(nimetaja[0], 0) : Tegurid(nimetaja);
                return ($"{mark}\\frac{{{lu}}}{{{ni}}}", mark == "" ? 2 : 1);
            }

            var lugejaTekst = lugeja.Count == 1 ? V(lugeja[0], 3) : Tegurid(lugeja);
            var nimetajaTekst = nimetaja.Count == 1 ? V(nimetaja[0], 3) : $"({Tegurid(nimetaja)})";
            return ($"{mark}{lugejaTekst}/{nimetajaTekst}", mark == "" ? 2 : 1);
        }

        private static int TeguriJark(Expression t) => t switch
        {
            Expression.Number or Expression.Constant => 0,
            Expression.Identifier or Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number } => 1,
            Expression.Sum or Expression.Power { Item1: Expression.Sum } => 2,
            _ => 3
        };

        private string Tegurid(List<Expression> tegurid)
        {
            var sb = new StringBuilder();
            foreach (var t in tegurid)
            {
                var osa = V(t, 2);
                if (sb.Length > 0)
                {
                    if (!latex) sb.Append('*');
                    else if (char.IsDigit(osa[0])) sb.Append(" \\cdot "); // 2 · 3^x
                    else if (xAsendus is not null && char.IsDigit(sb[^1])) sb.Append(" \\cdot "); // 1 · e^{-1}
                    else if (char.IsLetter(sb[^1]) && char.IsLetter(osa[0])) sb.Append(' ');
                }
                sb.Append(osa);
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------ aste

        private (string, int) Aste(Expression alus, Expression astendaja)
        {
            if (Avaldised.OnRatsionaalarv(astendaja, out var q))
            {
                if (Avaldised.OnNegatiivne(q))
                {
                    var positiivne = Operators.pow(alus, Expression.NewNumber(-q));
                    return latex
                        ? ($"\\frac{{1}}{{{V(positiivne, 0)}}}", 2)
                        : ($"1/{V(positiivne, 3)}", 2);
                }
                if (q == BigRational.FromIntFraction(1, 2))
                    return (latex ? $"\\sqrt{{{V(alus, 0)}}}" : $"sqrt({V(alus, 0)})", 4);
                if (latex && q.Numerator.IsOne && q.Denominator < 10)
                    return ($"\\sqrt[{q.Denominator}]{{{V(alus, 0)}}}", 4);
            }

            // sin(x)^2 → \sin^{2} x
            if (latex && alus is Expression.Function f && !f.Item1.IsExp
                && Avaldised.OnRatsionaalarv(astendaja, out var n) && Avaldised.OnTaisarv(n))
                return ($"{FunktsiooniNimi(f.Item1)}^{{{n.Numerator}}}{Argument(f.Item2)}", 4);

            var alusTekst = V(alus, 5);
            if (latex) return ($"{alusTekst}^{{{V(astendaja, 0)}}}", 3);
            return ($"{alusTekst}^{V(astendaja, 5)}", 3);
        }

        // ------------------------------------------------------------ funktsioonid

        private string Funktsioon(Function f, Expression argument)
        {
            if (f.IsExp)
                return latex ? $"e^{{{V(argument, 0)}}}" : $"e^{V(argument, 5)}";
            if (!latex)
                return $"{FunktsiooniNimi(f)}({V(argument, 0)})";
            return FunktsiooniNimi(f) + Argument(argument);
        }

        private string Argument(Expression argument) =>
            argument is Expression.Identifier or Expression.Constant ||
            argument is Expression.Number n && !Avaldised.OnNegatiivne(n.Item) && Avaldised.OnTaisarv(n.Item)
                ? " " + V(argument, 0)
                : $"\\left({V(argument, 0)}\\right)";

        private string FunktsiooniNimi(Function f)
        {
            string nimi =
                f.IsSin ? "sin" : f.IsCos ? "cos" : f.IsTan ? "tan" : f.IsCot ? "cot" :
                f.IsSec ? "sec" : f.IsCsc ? "csc" :
                f.IsAsin ? "arcsin" : f.IsAcos ? "arccos" : f.IsAtan ? "arctan" : f.IsAcot ? "arccot" :
                f.IsSinh ? "sinh" : f.IsCosh ? "cosh" : f.IsTanh ? "tanh" :
                f.IsLn ? "ln" : f.IsLg ? "lg" : f.IsAbs ? "abs" : f.ToString().ToLowerInvariant();
            if (!latex) return nimi;
            return nimi is "arccot" or "abs" ? $"\\operatorname{{{nimi}}}" : "\\" + nimi;
        }

        private static System.Numerics.BigInteger BigIntegerAbs(System.Numerics.BigInteger v) =>
            System.Numerics.BigInteger.Abs(v);
    }
}
