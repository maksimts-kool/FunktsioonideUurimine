using System.Globalization;
using System.Numerics;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

public sealed class ValemiViga(string sonum) : Exception(sonum);

public sealed record ParsitudValem(Expression Avaldis, IReadOnlyList<Tingimus> Tingimused);

/// <summary>
/// Loeb tekstilise valemi MathNet.Symbolics avaldiseks.
/// Oma parser on vajalik, sest MathNet'i Infix.Parse loeb "-x^2" kui (-x)^2, ei toeta kaudset
/// korrutamist (2x, 3(x+1)) ega kooli tähistusi (tg, ctg, log). Lisaks kogub parser määramispiirkonna
/// tingimused enne MathNet'i automaatset lihtsustamist (x/x → 1 kaotaks tingimuse x ≠ 0).
/// </summary>
public static class ValemiParser
{
    private static readonly Dictionary<string, Function> Funktsioonid = new()
    {
        ["sin"] = Function.Sin, ["cos"] = Function.Cos, ["tan"] = Function.Tan, ["tg"] = Function.Tan,
        ["cot"] = Function.Cot, ["ctg"] = Function.Cot, ["cotg"] = Function.Cot,
        ["sec"] = Function.Sec, ["csc"] = Function.Csc, ["cosec"] = Function.Csc,
        ["asin"] = Function.Asin, ["arcsin"] = Function.Asin,
        ["acos"] = Function.Acos, ["arccos"] = Function.Acos,
        ["atan"] = Function.Atan, ["arctan"] = Function.Atan, ["arctg"] = Function.Atan,
        ["acot"] = Function.Acot, ["arccot"] = Function.Acot, ["arcctg"] = Function.Acot,
        ["sinh"] = Function.Sinh, ["cosh"] = Function.Cosh, ["tanh"] = Function.Tanh,
        ["exp"] = Function.Exp, ["ln"] = Function.Ln, ["lg"] = Function.Lg, ["log"] = Function.Lg,
    };

    private static readonly string[] EriFunktsioonid = ["sqrt", "cbrt", "abs"];

    // pikim sobiv nimi eespool, et "sinh" ei loetaks "sin" + "h" ja "exp" mitte "e" + "x" + "p"
    private static readonly string[] Nimed = Funktsioonid.Keys
        .Concat(EriFunktsioonid)
        .Concat(["pi", "π", "x", "e"])
        .OrderByDescending(n => n.Length)
        .ToArray();

    public static ParsitudValem Parsi(string sisend)
    {
        if (string.IsNullOrWhiteSpace(sisend))
            throw new ValemiViga("Valem on tühi.");
        var lugeja = new Lugeja(Markerid(sisend));
        var avaldis = lugeja.LoeKoik();
        return new ParsitudValem(avaldis, lugeja.Tingimused.Koik);
    }

    // ---------------------------------------------------------------- tokeniseerimine

    private enum Liik { Arv, Nimi, Mark, Lopp }

    private readonly record struct Marker(Liik Liik, string Tekst, int Positsioon);

    private static List<Marker> Markerid(string s)
    {
        var tulemus = new List<Marker>();
        var i = 0;
        while (i < s.Length)
        {
            var c = s[i];
            var pos = i + 1;
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || (c is '.' or ',' && i + 1 < s.Length && char.IsDigit(s[i + 1])))
            {
                var algus = i;
                while (i < s.Length && char.IsDigit(s[i])) i++;
                if (i < s.Length && s[i] is '.' or ',' && i + 1 < s.Length && char.IsDigit(s[i + 1]))
                {
                    i++;
                    while (i < s.Length && char.IsDigit(s[i])) i++;
                }
                tulemus.Add(new Marker(Liik.Arv, s[algus..i].Replace(',', '.'), pos));
                continue;
            }

            if (char.IsLetter(c))
            {
                var algus = i;
                while (i < s.Length && char.IsLetter(s[i])) i++;
                TukeldaSona(s[algus..i].ToLowerInvariant(), algus, tulemus);
                continue;
            }

            switch (c)
            {
                case '+': tulemus.Add(new Marker(Liik.Mark, "+", pos)); break;
                case '-' or '−' or '–': tulemus.Add(new Marker(Liik.Mark, "-", pos)); break;
                case '*' or '·' or '×' or '⋅': tulemus.Add(new Marker(Liik.Mark, "*", pos)); break;
                case '/' or '÷' or ':': tulemus.Add(new Marker(Liik.Mark, "/", pos)); break;
                case '^': tulemus.Add(new Marker(Liik.Mark, "^", pos)); break;
                case '(' or '[' or '{': tulemus.Add(new Marker(Liik.Mark, "(", pos)); break;
                case ')' or ']' or '}': tulemus.Add(new Marker(Liik.Mark, ")", pos)); break;
                case '²':
                    tulemus.Add(new Marker(Liik.Mark, "^", pos));
                    tulemus.Add(new Marker(Liik.Arv, "2", pos));
                    break;
                case '³':
                    tulemus.Add(new Marker(Liik.Mark, "^", pos));
                    tulemus.Add(new Marker(Liik.Arv, "3", pos));
                    break;
                case '√': tulemus.Add(new Marker(Liik.Nimi, "sqrt", pos)); break;
                default: throw new ValemiViga($"Tundmatu märk '{c}' (positsioon {pos}).");
            }
            i++;
        }
        tulemus.Add(new Marker(Liik.Lopp, "", s.Length + 1));
        return tulemus;
    }

    /// <summary>Tükeldab tähejada teadaolevateks nimedeks: "xsinx" → x, sin, x.</summary>
    private static void TukeldaSona(string sona, int algus, List<Marker> tulemus)
    {
        var i = 0;
        while (i < sona.Length)
        {
            var nimi = Nimed.FirstOrDefault(n => string.CompareOrdinal(sona, i, n, 0, n.Length) == 0);
            if (nimi is null)
                throw new ValemiViga(
                    $"Tundmatu tähis '{sona[i..]}' (positsioon {algus + i + 1}). Muutuja peab olema x; lubatud funktsioonid: " +
                    "sin, cos, tan (tg), cot (ctg), arcsin, arccos, arctan, sinh, cosh, tanh, exp, ln, lg (log), sqrt, cbrt; konstandid pi ja e.");
            tulemus.Add(new Marker(Liik.Nimi, nimi == "π" ? "pi" : nimi, algus + i + 1));
            i += nimi.Length;
        }
    }

    // ---------------------------------------------------------------- rekursiivne laskumine

    private sealed class Lugeja(List<Marker> markerid)
    {
        private int _i;
        public Tingimused Tingimused { get; } = new();

        private Marker Praegune => markerid[_i];

        private bool On(string mark) => Praegune.Liik == Liik.Mark && Praegune.Tekst == mark;

        public Expression LoeKoik()
        {
            var avaldis = Summa();
            if (Praegune.Liik != Liik.Lopp)
                throw Viga(Praegune.Tekst == ")" ? "Liigne sulg ')'" : $"Ootamatu '{Praegune.Tekst}'");
            return avaldis;
        }

        // summa := liidetav (('+' | '-') liidetav)*
        private Expression Summa()
        {
            var v = Liidetav();
            while (On("+") || On("-"))
            {
                var miinus = On("-");
                _i++;
                var r = Liidetav();
                v = miinus ? Operators.subtract(v, r) : Operators.add(v, r);
            }
            return v;
        }

        // liidetav := unaarne (('*' | '/') unaarne | aste)*   – viimane on kaudne korrutamine: 2x, 3(x+1)
        private Expression Liidetav()
        {
            var v = Unaarne();
            while (true)
            {
                if (On("*"))
                {
                    _i++;
                    v = Operators.multiply(v, Unaarne());
                }
                else if (On("/"))
                {
                    _i++;
                    var positsioon = Praegune.Positsioon;
                    var nimetaja = Unaarne();
                    if (!Tingimused.Jagamisele(nimetaja))
                        throw new ValemiViga($"Jagamine nulliga (positsioon {positsioon}).");
                    v = Operators.divide(v, nimetaja);
                }
                else if (Praegune.Liik is Liik.Arv or Liik.Nimi || On("("))
                {
                    v = Operators.multiply(v, Aste());
                }
                else
                {
                    return v;
                }
            }
        }

        // unaarne := ('-' | '+') unaarne | aste     → -x^2 = -(x^2)
        private Expression Unaarne()
        {
            if (On("-"))
            {
                _i++;
                return Operators.negate(Unaarne());
            }
            if (On("+"))
            {
                _i++;
                return Unaarne();
            }
            return Aste();
        }

        // aste := primaarne ('^' unaarne)?          → paremassotsiatiivne, x^-2 lubatud
        private Expression Aste()
        {
            var alus = Primaarne();
            if (!On("^")) return alus;
            var positsioon = Praegune.Positsioon;
            _i++;
            var astendaja = Unaarne();
            return AsteTingimusega(alus, astendaja, positsioon);
        }

        private Expression AsteTingimusega(Expression alus, Expression astendaja, int positsioon)
        {
            if (!Tingimused.Astmele(alus, astendaja))
                throw new ValemiViga($"Aste ei ole reaalarvudes määratud (positsioon {positsioon}).");
            return Operators.pow(alus, astendaja);
        }

        private Expression Primaarne()
        {
            var m = Praegune;
            switch (m.Liik)
            {
                case Liik.Arv:
                    _i++;
                    return Arv(m.Tekst);
                case Liik.Mark when m.Tekst == "(":
                {
                    _i++;
                    var sees = Summa();
                    if (!On(")")) throw Viga("Puudub sulgev sulg ')'");
                    _i++;
                    return sees;
                }
                case Liik.Nimi:
                    _i++;
                    return m.Tekst switch
                    {
                        "x" => Avaldised.X,
                        "pi" => Expression.Pi,
                        "e" => Expression.E,
                        "abs" => throw new ValemiViga(
                            "abs() ei ole toetatud: absoluutväärtusega funktsiooni ei saa siin uurida."),
                        "sqrt" => AsteTingimusega(Argument(), Avaldised.Ratsionaal(1, 2), m.Positsioon),
                        "cbrt" => AsteTingimusega(Argument(), Avaldised.Ratsionaal(1, 3), m.Positsioon),
                        _ => FunktsioonTingimusega(Funktsioonid[m.Tekst], Argument(), m.Positsioon)
                    };
                case Liik.Lopp:
                    throw Viga("Valem lõppes ootamatult");
                default:
                    throw Viga($"Ootamatu '{m.Tekst}'");
            }
        }

        // funktsiooni argument: sin(x) või sulgudeta sin x, ln x^2 = ln(x^2);
        // sulgudes argumendi järel olev aste kuulub funktsioonile: sin(x)^2 = (sin x)^2
        private Expression Argument()
        {
            if (Praegune.Liik == Liik.Lopp || On(")") || On("+") || On("*") || On("/") || On("^"))
                throw Viga("Funktsioonil puudub argument");
            return On("(") ? Primaarne() : Aste();
        }

        private Expression FunktsioonTingimusega(Function f, Expression argument, int positsioon)
        {
            if (!Tingimused.Funktsioonile(f, argument))
                throw new ValemiViga($"Funktsioon ei ole selles punktis määratud (positsioon {positsioon}).");
            return Operators.apply(f, argument);
        }

        private static Expression Arv(string tekst)
        {
            var punkt = tekst.IndexOf('.');
            if (punkt < 0)
                return Avaldised.Ratsionaal(BigInteger.Parse(tekst, CultureInfo.InvariantCulture), 1);
            // kümnendmurd täpse ratsionaalarvuna: 0.25 → 1/4 (siis on ka tuletis täpne)
            var komakohti = tekst.Length - punkt - 1;
            var lugeja = BigInteger.Parse(tekst.Remove(punkt, 1), CultureInfo.InvariantCulture);
            return Avaldised.Ratsionaal(lugeja, BigInteger.Pow(10, komakohti));
        }

        private ValemiViga Viga(string sonum) => new($"{sonum} (positsioon {Praegune.Positsioon}).");
    }
}
