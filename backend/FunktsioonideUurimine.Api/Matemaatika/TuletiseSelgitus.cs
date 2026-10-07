using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Seletab, kuidas tuletis leiti: milline diferentseerimisreegel avaldise välimisele kujule sobib,
/// millised on osad (u, v) ja nende tuletised ning mis tuleb pärast asendamist. Tuletised ise arvutab
/// MathNet.Symbolics (Avaldised.Tuletis) – siin need ainult lahti kirjutatakse.
/// </summary>
public static class TuletiseSelgitus
{
    /// <param name="f">Diferentseeritav avaldis.</param>
    /// <param name="nimi">Funktsiooni tähis LaTeX-is, nt "f" või "f'".</param>
    /// <param name="tuletiseNimi">Tuletise tähis, nt "f'" või "f''".</param>
    public static List<Samm> Sammud(Expression f, string nimi, string tuletiseNimi)
    {
        var sammud = new List<Samm>
        {
            new($"Diferentseerime funktsiooni ${nimi}(x)$:", $"{nimi}(x) = {L(f)}")
        };
        var tulemus = Avaldised.Tuletis(f);

        if (!Avaldised.SisaldabX(f))
        {
            sammud.Add(new("Konstandi tuletis on null.", $"{tuletiseNimi}(x) = 0"));
            return sammud;
        }

        var asendatud = Seleta(f, sammud);
        if (asendatud is not null && asendatud != L(tulemus))
        {
            sammud.Add(new("Paneme osad kokku:", $"{tuletiseNimi}(x) = {asendatud}"));
            sammud.Add(new("Lihtsustame:", $"{tuletiseNimi}(x) = {L(tulemus)}"));
        }
        else
        {
            sammud.Add(new("**Seega:**", $"{tuletiseNimi}(x) = {L(tulemus)}"));
        }
        return sammud;
    }

    /// <summary>Lisab sammud välimise reegli kohta; tagastab lihtsustamata kokkupandud tuletise LaTeX-i.</summary>
    private static string? Seleta(Expression f, List<Samm> sammud)
    {
        switch (f)
        {
            case Expression.Sum summa:
            {
                var liikmed = ValemiVormindaja.Jarjesta(summa.Item);
                sammud.Add(Reegel("Summa tuletis on liidetavate tuletiste summa", "(u \\pm v)' = u' \\pm v'"));
                var tuletised = new List<string>();
                foreach (var liige in liikmed)
                {
                    var d = Avaldised.Tuletis(liige);
                    sammud.Add(new(LyhikeReegel(liige) is { } r ? $"{Suurtaht(r)}:" : null,
                        $"\\left({L(liige)}\\right)' = {L(d)}"));
                    if (!d.Equals(Expression.Zero)) tuletised.Add(L(d));
                }
                return tuletised.Count == 0 ? "0" : Summa(tuletised);
            }

            case Expression.Product korrutis:
                return SeletaKorrutis(korrutis.Item.ToList(), sammud);

            default:
                var (pealkiri, valem) = Pohireegel(f);
                sammud.Add(Reegel(pealkiri, valem));
                return Ahel(f, sammud);
        }
    }

    private static string? SeletaKorrutis(List<Expression> tegurid, List<Samm> sammud)
    {
        var konstandid = tegurid.Where(t => !Avaldised.SisaldabX(t)).ToList();
        var lugeja = tegurid.Where(t => Avaldised.SisaldabX(t) && !OnNimetajas(t)).ToList();
        var nimetaja = tegurid.Where(t => Avaldised.SisaldabX(t) && OnNimetajas(t))
            .Select(t => Operators.pow(((Expression.Power)t).Item1, Operators.negate(((Expression.Power)t).Item2)))
            .ToList();
        var muutuv = Korruta(lugeja.Concat(tegurid.Where(t => Avaldised.SisaldabX(t) && OnNimetajas(t))));

        var c = Korruta(konstandid);
        var cLatex = konstandid.Count == 0 ? "" : KordajaLatex(c);
        if (konstandid.Count > 0)
            sammud.Add(Reegel("Konstantse teguri võib tuua tuletise märgi ette", "(c \\cdot u)' = c \\cdot u'"));

        string? sisemine;
        if (nimetaja.Count > 0 && (lugeja.Count > 0 || konstandid.Count == 0))
        {
            var u = Korruta(lugeja);
            var v = Korruta(nimetaja);
            sammud.Add(Reegel("Jagatise tuletis", "\\left(\\frac{u}{v}\\right)' = \\frac{u' v - u v'}{v^{2}}"));
            sammud.AddRange(Osad(("u", u), ("v", v)));
            sisemine = $"\\frac{{{S(L(Avaldised.Tuletis(u)))} \\cdot {S(L(v))} - {S(L(u))} \\cdot {S(L(Avaldised.Tuletis(v)))}}}{{{Alus(L(v))}^{{2}}}}";
        }
        else if (lugeja.Count + nimetaja.Count >= 2)
        {
            var koik = tegurid.Where(Avaldised.SisaldabX).ToList();
            var u = koik[0];
            var v = Korruta(koik.Skip(1));
            sammud.Add(Reegel("Korrutise tuletis", "(u \\cdot v)' = u' v + u v'"));
            sammud.AddRange(Osad(("u", u), ("v", v)));
            sisemine = $"{S(L(Avaldised.Tuletis(u)))} \\cdot {S(L(v))} + {S(L(u))} \\cdot {S(L(Avaldised.Tuletis(v)))}";
        }
        else
        {
            // c · u, kus u on üks tegur: seletame u tuletise
            sisemine = Seleta(muutuv, sammud) ?? L(Avaldised.Tuletis(muutuv));
        }

        if (konstandid.Count == 0) return sisemine;
        return cLatex == "-" ? $"-{S(sisemine)}" : $"{cLatex} \\cdot {S(sisemine)}";
    }

    /// <summary>Liitfunktsioon: kirjutab välja sisemise funktsiooni u ja u'. Tagastab null, kui u = x.</summary>
    private static string? Ahel(Expression f, List<Samm> sammud)
    {
        Expression? sisemine = f switch
        {
            Expression.Function fn => fn.Item2,
            Expression.Power { Item2: Expression.Number } p => p.Item1,
            Expression.Power p when !Avaldised.SisaldabX(p.Item1) => p.Item2,
            _ => null
        };
        if (sisemine is null || sisemine is Expression.Identifier) return null;

        var du = Avaldised.Tuletis(sisemine);
        sammud.Add(new($"Sisemine funktsioon $u = {L(sisemine)}$, selle tuletis:", $"u' = {L(du)}"));
        var valimine = f switch
        {
            Expression.Function fn => FunktsiooniTuletis(fn.Item1, L(sisemine)),
            Expression.Power { Item2: Expression.Number n } => AstmeTuletis(n.Item, L(sisemine)),
            Expression.Power { Item1: Expression.Constant { Item.IsE: true } } => L(f),
            Expression.Power p => $"{L(f)} \\ln {S(L(p.Item1))}",
            _ => L(f)
        };
        return $"{valimine} \\cdot {S(L(du))}";
    }

    // ---------------------------------------------------------------- reeglid

    /// <summary>Üldine reegel avaldise välimise kuju jaoks (pealkiri, valem).</summary>
    private static (string, string) Pohireegel(Expression f) => f switch
    {
        Expression.Identifier => ("Muutuja tuletis", "(x)' = 1"),
        Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number }
            => ("Astmefunktsiooni tuletis", "(x^{n})' = n x^{n - 1}"),
        Expression.Power { Item2: Expression.Number }
            => ("Liitfunktsiooni (astme) tuletis", "(u^{n})' = n u^{n - 1} \\cdot u'"),
        Expression.Power { Item1: Expression.Constant { Item.IsE: true } } p when p.Item2 is Expression.Identifier
            => ("Eksponentfunktsiooni tuletis", "(e^{x})' = e^{x}"),
        Expression.Power { Item1: Expression.Constant { Item.IsE: true } }
            => ("Liitfunktsiooni (eksponendi) tuletis", "(e^{u})' = e^{u} \\cdot u'"),
        Expression.Power p when !Avaldised.SisaldabX(p.Item1)
            => ("Eksponentfunktsiooni tuletis", p.Item2 is Expression.Identifier
                ? "(a^{x})' = a^{x} \\ln a"
                : "(a^{u})' = a^{u} \\ln a \\cdot u'"),
        Expression.Power
            => ("Muutuva astendajaga aste (logaritmiline diferentseerimine)",
                "(u^{v})' = u^{v} \\left(v' \\ln u + \\frac{v u'}{u}\\right)"),
        Expression.Function fn when fn.Item2 is Expression.Identifier
            => ("Tuletiste tabelist", $"({FunktsiooniLatex(fn.Item1, "x")})' = {FunktsiooniTuletis(fn.Item1, "x")}"),
        Expression.Function fn
            => ("Liitfunktsiooni tuletis (ahelreegel)",
                $"({FunktsiooniLatex(fn.Item1, "u")})' = {FunktsiooniTuletis(fn.Item1, "u")} \\cdot u'"),
        _ => ("Tuletis", $"\\left({L(f)}\\right)'")
    };

    /// <summary>Lühike reegli nimi liidetava kõrvale.</summary>
    private static string? LyhikeReegel(Expression f)
    {
        if (!Avaldised.SisaldabX(f)) return "konstandi tuletis on 0";
        if (f is Expression.Product p)
        {
            var muutuvad = p.Item.Where(Avaldised.SisaldabX).ToList();
            if (muutuvad.Any(OnNimetajas) && muutuvad.Any(t => !OnNimetajas(t))) return "jagatise tuletis";
            if (muutuvad.Count >= 2) return "korrutise tuletis";
            var reegel = LyhikeReegel(muutuvad[0]);
            return p.Item.Length > muutuvad.Count ? $"konstantne tegur ette, {reegel}" : reegel;
        }
        return f switch
        {
            Expression.Identifier => "muutuja tuletis on 1",
            Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number } => "astme tuletis",
            Expression.Power { Item2: Expression.Number } => "liitfunktsiooni tuletis",
            Expression.Power aste when !Avaldised.SisaldabX(aste.Item1) => "eksponentfunktsiooni tuletis",
            Expression.Function { Item2: Expression.Identifier } => "tuletiste tabelist",
            Expression.Function => "liitfunktsiooni tuletis",
            _ => null
        };
    }

    private static string FunktsiooniLatex(Function f, string u) =>
        f.IsExp ? $"e^{{{u}}}" : $"{FunktsiooniNimi(f)} {u}";

    /// <summary>Tuletiste tabel: (f(u))' ilma teguri u' -ta.</summary>
    private static string FunktsiooniTuletis(Function f, string u)
    {
        var su = u.Length == 1 ? $" {u}" : $"\\left({u}\\right)";
        if (f.IsSin) return $"\\cos{su}";
        if (f.IsCos) return $"-\\sin{su}";
        if (f.IsTan) return $"\\frac{{1}}{{\\cos^{{2}}{su}}}";
        if (f.IsCot) return $"-\\frac{{1}}{{\\sin^{{2}}{su}}}";
        if (f.IsSec) return $"\\frac{{\\sin{su}}}{{\\cos^{{2}}{su}}}";
        if (f.IsCsc) return $"-\\frac{{\\cos{su}}}{{\\sin^{{2}}{su}}}";
        if (f.IsAsin) return $"\\frac{{1}}{{\\sqrt{{1 - {S(u)}^{{2}}}}}}";
        if (f.IsAcos) return $"-\\frac{{1}}{{\\sqrt{{1 - {S(u)}^{{2}}}}}}";
        if (f.IsAtan) return $"\\frac{{1}}{{1 + {S(u)}^{{2}}}}";
        if (f.IsAcot) return $"-\\frac{{1}}{{1 + {S(u)}^{{2}}}}";
        if (f.IsSinh) return $"\\cosh{su}";
        if (f.IsCosh) return $"\\sinh{su}";
        if (f.IsTanh) return $"\\frac{{1}}{{\\cosh^{{2}}{su}}}";
        if (f.IsExp) return $"e^{{{u}}}";
        if (f.IsLn) return $"\\frac{{1}}{{{u}}}";
        if (f.IsLg) return $"\\frac{{1}}{{{S(u)} \\ln 10}}";
        return $"{FunktsiooniNimi(f)}'{su}";
    }

    private static string FunktsiooniNimi(Function f) =>
        f.IsSin ? "\\sin" : f.IsCos ? "\\cos" : f.IsTan ? "\\tan" : f.IsCot ? "\\cot" :
        f.IsSec ? "\\sec" : f.IsCsc ? "\\csc" : f.IsAsin ? "\\arcsin" : f.IsAcos ? "\\arccos" :
        f.IsAtan ? "\\arctan" : f.IsAcot ? "\\operatorname{arccot}" : f.IsSinh ? "\\sinh" :
        f.IsCosh ? "\\cosh" : f.IsTanh ? "\\tanh" : f.IsLn ? "\\ln" : f.IsLg ? "\\lg" : "\\exp";

    /// <summary>n u^{n-1} LaTeX-is (ilma teguri u' -ta).</summary>
    private static string AstmeTuletis(MathNet.Numerics.BigRational n, string u)
    {
        var uusAste = L(Expression.NewNumber(n - MathNet.Numerics.BigRational.One));
        return $"{S(L(Expression.NewNumber(n)))} \\cdot {Alus(u)}^{{{uusAste}}}";
    }

    // ---------------------------------------------------------------- abimeetodid

    private static Samm Reegel(string pealkiri, string valem) => new($"**{pealkiri}:**", valem);

    private static IEnumerable<Samm> Osad(params (string nimi, Expression avaldis)[] osad)
    {
        foreach (var (nimi, avaldis) in osad)
            yield return new(null, $"{nimi} = {L(avaldis)}, \\quad {nimi}' = {L(Avaldised.Tuletis(avaldis))}");
    }

    private static bool OnNimetajas(Expression t) =>
        t is Expression.Power { Item2: Expression.Number n } && Avaldised.OnNegatiivne(n.Item);

    private static Expression Korruta(IEnumerable<Expression> tegurid) =>
        tegurid.Aggregate(Expression.One, Operators.multiply);

    private static string KordajaLatex(Expression c)
    {
        var tekst = L(c);
        return tekst switch { "1" => "", "-1" => "-", _ => S(tekst) };
    }

    private static string L(Expression e) => ValemiVormindaja.Latex(e);

    private static string S(string latex) => ValemiVormindaja.Sulgudes(latex);

    /// <summary>Astme alus: sulud ümber kõige peale üksiku sümboli.</summary>
    private static string Alus(string latex) => latex.Length == 1 ? latex : $"\\left({latex}\\right)";

    private static string Summa(List<string> liikmed)
    {
        var tulemus = liikmed[0];
        foreach (var l in liikmed.Skip(1))
            tulemus += l.StartsWith('-') ? $" - {l[1..]}" : $" + {l}";
        return tulemus;
    }

    private static string Suurtaht(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
