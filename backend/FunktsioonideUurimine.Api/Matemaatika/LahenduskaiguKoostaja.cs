namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Lahenduskäigu üks samm. Tekst võib sisaldada $…$ vahel LaTeX-it ja **paksu kirja**;
/// Valem kuvatakse eraldi real; Tabel on märgitabel.
/// </summary>
public sealed record Samm(string? Tekst = null, string? Valem = null, Margitabel? Tabel = null);

/// <summary>
/// Märgitabel: veerud on kordamööda vahemikud ja murdepunktid.
/// </summary>
/// <param name="MargiRida">Rea pealkiri LaTeX-is, nt "f'(x)".</param>
/// <param name="TahenduseRida">Teise rea pealkiri (nt "f(x)" noolte jaoks) või null.</param>
public sealed record Margitabel(string MargiRida, string? TahenduseRida, IReadOnlyList<TabeliVeerg> Veerud);

/// <param name="X">Vahemik või punkt LaTeX-is.</param>
/// <param name="Mark">"+", "−", "0" või "∄" (määramata).</param>
/// <param name="Tahendus">Teise rea sisu LaTeX-is, nt "\nearrow" või "\text{max}".</param>
/// <param name="Kontroll">Testpunkti arvutus LaTeX-is, nt "f'(-2) = 9".</param>
public sealed record TabeliVeerg(string X, bool Vahemik, string Mark, string? Tahendus, string? Kontroll);

/// <param name="Voti">Vastuse võti, mis vastab vastuste tabeli reale (nt "nullkohad").</param>
/// <param name="Vastused">Selle osa lõppvastused (samad tekstid, mis vastuste tabelis).</param>
/// <param name="Numbriline">Kas osa tulemused on leitud numbriliselt ainult uuritavas vahemikus?</param>
public sealed record LahenduseOsa(string Voti, string Pealkiri, IReadOnlyList<Samm> Sammud,
    IReadOnlyList<string> Vastused, bool Numbriline);

/// <summary>
/// Koostab analüüsi vahetulemustest samm-sammulise lahenduskäigu: kuidas iga vastus leiti.
/// </summary>
public static class LahenduskaiguKoostaja
{
    public static List<LahenduseOsa> Koosta(AnaluusiTulemus t)
    {
        var v = t.Vahe;
        return
        [
            Maaramispiirkond(t, v),
            Nullkohad(t, v),
            Positiivsus(t, v),
            new("tuletis", "Tuletis", TuletiseSelgitus.Sammud(v.Funktsioon, "f", "f'"), [t.Tuletis], false),
            Kriitilised(t, v),
            Monotoonsus(t, v),
            new("teineTuletis", "Teine tuletis", TuletiseSelgitus.Sammud(v.Tuletis, "f'", "f''"), [t.TeineTuletis], false),
            Kaanupunktid(t, v),
        ];
    }

    // ---------------------------------------------------------------- osad

    private static LahenduseOsa Maaramispiirkond(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>();
        if (v.Tingimused.Count == 0)
        {
            s.Add(new("Valemis ei ole muutujaga nimetajat, paarisjuurt, logaritmi ega muud piirangut – " +
                      "avaldise väärtuse saab arvutada iga reaalarvu $x$ korral."));
            s.Add(new(null, "X = \\mathbb{R} = (-\\infty;\\ \\infty)"));
        }
        else
        {
            s.Add(new("Määramispiirkond on kõigi nende $x$ väärtuste hulk, mille korral valem on arvutatav. " +
                      "Kirjutame välja tingimused:"));
            foreach (var tingimus in v.Tingimused)
            {
                var (hulk, _) = Piirkonnad.Maaramispiirkond([tingimus], v.A, v.B);
                var juured = Matemaatika.Nullkohad.Leia(tingimus.Avaldis, v.A, v.B);
                var tekst = Suurtaht(tingimus.Pohjus) + ".";
                if (juured.Vaartused.Count > 0)
                    tekst += $" Avaldis ${L(tingimus.Avaldis)}$ on null kohal ${VorrandiSelgitus.Loend(juured.Vaartused)}$" +
                             (tingimus.Liik == TingimuseLiik.NullistErinev
                                 ? "."
                                 : "; need punktid jagavad arvtelje vahemikeks, millest valime sobiva märgiga.");
                s.Add(new(tekst, $"{L(tingimus.Avaldis)} {Vordlus(tingimus.Liik)} 0 \\;\\Rightarrow\\; x \\in {HulkLatex(hulk)}"));
            }
            if (v.Tingimused.Count > 1)
                s.Add(new("Määramispiirkond on kõigi tingimuste ühisosa:"));
            s.Add(new(null, $"X = {HulkLatex(t.PiirkondF)}"));
        }
        if (!v.DTaielik)
            s.Add(new($"Tingimuste võrrandeid ei saanud algebraliselt lahendada, seepärast on määramispiirkond leitud " +
                      $"ainult lõigul {Loik(v.A, v.B)}."));
        return new("maaramispiirkond", "Määramispiirkond", s, [t.Maaramispiirkond], !v.DTaielik);
    }

    private static LahenduseOsa Nullkohad(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>
        {
            new("Nullkoht on argumendi väärtus, mille korral funktsiooni väärtus on null. Lahendame võrrandi $f(x) = 0$:")
        };
        s.AddRange(VorrandiSelgitus.Sammud(v.Funktsioon, "f(x)", v.J0, v.A, v.B));
        var valjas = v.J0.Vaartused.Where(x => !t.PiirkondF.Sisaldab(x)).ToList();
        if (valjas.Count > 0)
            s.Add(new($"Määramispiirkonda ei kuulu ${VorrandiSelgitus.Loend(valjas)}$ – see ei ole nullkoht."));
        var sobivad = v.J0.Vaartused.Where(t.PiirkondF.Sisaldab).Where(Arv.OnTapne).Take(3).ToList();
        if (sobivad.Count > 0 && !v.J0.KoikPunktid)
            s.Add(new("**Kontroll:**", string.Join(", \\quad ", sobivad.Select(x => $"f({Arv.Latex(x)}) = 0"))));
        return new("nullkohad", "Nullkohad", s, [t.Nullkohad], !v.NullTaielik);
    }

    private static LahenduseOsa Positiivsus(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>
        {
            new("Pidev funktsioon saab märki muuta ainult nullkohtades ja määramispiirkonna katkemiskohtades. " +
                "Need punktid jagavad määramispiirkonna vahemikeks, kus märk on muutumatu – piisab ühe " +
                "**testpunkti** kontrollimisest igas vahemikus (intervallmeetod):"),
            new(null, null, Tabel(t.F, "f", v.PositiivsusPiirkond, v.J0.Vaartused, null,
                mark => null, _ => null, LoikePiirid(v, v.NullTaielik))),
            new("Positiivsuspiirkond $X^{+}$ koosneb vahemikest, kus $f(x) > 0$, negatiivsuspiirkond $X^{-}$ – " +
                "vahemikest, kus $f(x) < 0$."),
        };
        return new("positiivsus", "Positiivsus- ja negatiivsuspiirkond", s, [t.Positiivsus], !v.NullTaielik);
    }

    private static LahenduseOsa Kriitilised(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>
        {
            new("Kriitilised punktid on määramispiirkonna sisepunktid, kus $f'(x) = 0$ (statsionaarsed punktid) " +
                "või kus tuletist ei eksisteeri. Lahendame võrrandi $f'(x) = 0$:", $"f'(x) = {L(v.Tuletis)}")
        };
        s.AddRange(VorrandiSelgitus.Sammud(v.Tuletis, "f'(x)", v.J1, v.A, v.B));

        var valjas = v.J1.Vaartused.Where(x => !v.Statsionaarsed.Contains(x)).ToList();
        if (valjas.Count > 0)
            s.Add(new($"${VorrandiSelgitus.Loend(valjas)}$ ei ole määramispiirkonna sisepunkt – jätame välja."));
        if (v.TuletisPuudub.Count > 0)
            s.Add(new("Tuletis ei ole määratud, kuigi funktsioon on – ka need on kriitilised punktid:",
                VorrandiSelgitus.Loend(v.TuletisPuudub)));
        if (v.Kriitilised.Count > 0)
            s.Add(new("**Kriitilised punktid:**", VorrandiSelgitus.Loend(v.Kriitilised)));
        return new("kriitilisedPunktid", "Kriitilised punktid", s, [t.KriitilisedPunktid], !v.TuletisTaielik);
    }

    private static LahenduseOsa Monotoonsus(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>
        {
            new("Kui vahemikus $f'(x) > 0$, siis funktsioon **kasvab** ($\\nearrow$); kui $f'(x) < 0$, siis " +
                "**kahaneb** ($\\searrow$). Kriitilised punktid jagavad määramispiirkonna vahemikeks; igas " +
                "vahemikus määrame tuletise märgi testpunkti abil:")
        };
        var tabel = Tabel(t.F1, "f'", v.MonotoonsusPiirkond, v.MonotoonsusMurdepunktid, "f(x)",
            mark => mark switch { "+" => "\\nearrow", "−" => "\\searrow", "0" => "\\rightarrow", _ => null },
            x => t.EkstreemumPunktid.FirstOrDefault(e => Math.Abs(e.X - x) <= Hulk.Tolerants) is { } e
                ? $"\\text{{{e.Tyyp}}}"
                : null, LoikePiirid(v, v.TuletisTaielik));
        s.Add(new(null, null, tabel));
        if (OnYhendatud(tabel))
            s.Add(new("Kõrvuti olevad sama märgiga vahemikud ühendame: tuletis ei muuda nende vahel märki, " +
                      "seega funktsioon kasvab (või kahaneb) ka üle selle punkti."));

        s.Add(new("**Ekstreemumi piisav tingimus:** kui $f'$ muudab kriitilises punktis märki $+ \\to -$, on seal " +
                  "maksimum; kui $- \\to +$, siis miinimum. Kui märk ei muutu, ekstreemumit ei ole."));
        foreach (var c in v.Kriitilised)
        {
            var e = t.EkstreemumPunktid.FirstOrDefault(p => Math.Abs(p.X - c) <= Hulk.Tolerants);
            if (e is null)
                s.Add(new($"Punktis $x = {Arv.Latex(c)}$ tuletis märki ei muuda – ekstreemumit ei ole."));
            else
                s.Add(new(e.Tyyp == "max" ? "Maksimumpunkt, funktsiooni väärtus:" : "Miinimumpunkt, funktsiooni väärtus:",
                    $"y_{{\\text{{{e.Tyyp}}}}} = {Vaartus(v.Funktsioon, c, e.Y)}"));
        }
        return new("monotoonsus", "Monotoonsus ja ekstreemumid", s, [t.Monotoonsus, t.Ekstreemumid], !v.TuletisTaielik);
    }

    private static LahenduseOsa Kaanupunktid(AnaluusiTulemus t, Vahetulemused v)
    {
        var s = new List<Samm>
        {
            new("Teise tuletise märk näitab graafiku kõverust: kui $f''(x) > 0$, on graafik **nõgus** ($\\cup$), " +
                "kui $f''(x) < 0$, siis **kumer** ($\\cap$). Käänupunktis muutub kõverus, st $f''$ muudab märki. " +
                "Lahendame võrrandi $f''(x) = 0$:", $"f''(x) = {L(v.TeineTuletis)}")
        };
        s.AddRange(VorrandiSelgitus.Sammud(v.TeineTuletis, "f''(x)", v.J2, v.A, v.B));
        var puudub = v.Kaanukandidaadid.Where(k => !v.J2.Vaartused.Any(j => Math.Abs(j - k) <= Hulk.Tolerants)).ToList();
        if (puudub.Count > 0)
            s.Add(new("Lisaks on kandidaadid punktid, kus $f''$ ei ole määratud:", VorrandiSelgitus.Loend(puudub)));

        var tabel = Tabel(t.F2, "f''", v.KumerusPiirkond, v.KumerusMurdepunktid, "f(x)",
            mark => mark switch { "+" => "\\cup", "−" => "\\cap", "0" => "—", _ => null },
            x => t.KaanupunktiPunktid.Any(k => Math.Abs(k.X - x) <= Hulk.Tolerants) ? "\\text{K}" : null,
            LoikePiirid(v, v.TeineTaielik));
        s.Add(new("Määrame teise tuletise märgi vahemikes:", null, tabel));

        foreach (var k in v.Kaanukandidaadid)
        {
            var kp = t.KaanupunktiPunktid.FirstOrDefault(p => Math.Abs(p.X - k) <= Hulk.Tolerants);
            if (kp is null)
                s.Add(new($"Punktis $x = {Arv.Latex(k)}$ teine tuletis märki ei muuda – käänupunkti ei ole."));
            else
                s.Add(new("Käänupunkt, funktsiooni väärtus:", Vaartus(v.Funktsioon, k, kp.Y)));
        }
        return new("kaanupunktid", "Käänupunktid, kumerus ja nõgusus", s, [t.Kaanupunktid, t.Kumerus], !v.TeineTaielik);
    }

    // ---------------------------------------------------------------- märgitabel

    /// <summary>
    /// Märgitabel piirkonnas: vahemikud murdepunktide vahel (märk testpunktist) ja punktid ise
    /// (0 või ∄). Määramispiirkonna lahtised otspunktid ja vahed märgitakse ∄-ga.
    /// </summary>
    private static Margitabel Tabel(Func<double, double> g, string tahis, Hulk piirkond, IEnumerable<double> murdepunktid,
        string? tahenduseRida, Func<string, string?> vahemikuTahendus, Func<double, string?> punktiTahendus,
        double[] loikePiirid)
    {
        // numbrilise otsingu korral on piirkond lõigatud uuritava lõiguga – need otspunktid pole murdepunktid
        bool OnLoikePiir(double x) => loikePiirid.Any(p => Math.Abs(p - x) <= Hulk.Tolerants);
        var punktid = Piirkonnad.Sorteeritud(murdepunktid);
        var veerud = new List<TabeliVeerg>();

        void Punkt(double x)
        {
            if (veerud.Count > 0 && !veerud[^1].Vahemik && veerud[^1].X == Arv.Latex(x)) return;
            var y = g(x);
            var mark = double.IsNaN(y) ? "∄" : Math.Abs(y) < 1e-9 ? "0" : y > 0 ? "+" : "−";
            veerud.Add(new(Arv.Latex(x), false, mark, punktiTahendus(x), null));
        }

        void Puudub(double x)
        {
            if (veerud.Count > 0 && !veerud[^1].Vahemik && veerud[^1].X == Arv.Latex(x)) return;
            veerud.Add(new(Arv.Latex(x), false, "∄", null, null));
        }

        void Vahemik(double u, double w)
        {
            var x = Testpunkt(u, w, g);
            var y = g(x);
            var mark = double.IsNaN(y) ? "∄" : Math.Abs(y) < 1e-12 ? "0" : y > 0 ? "+" : "−";
            var kontroll = double.IsNaN(y) ? null : Arv.VordusLatex($"{tahis}({Arv.Latex(x)})", Arv.Silu(y));
            veerud.Add(new(new Loik(u, w, false, false).Latex(), true, mark, vahemikuTahendus(mark), kontroll));
        }

        var loigud = piirkond.Loigud;
        for (var i = 0; i < loigud.Count; i++)
        {
            var l = loigud[i];
            if (l.OnPunkt)
            {
                Punkt(l.Algus);
                continue;
            }
            if (double.IsFinite(l.Algus) && !OnLoikePiir(l.Algus))
            {
                if (l.AlgusKaasa) Punkt(l.Algus);
                else Puudub(l.Algus);
            }

            var piirid = new List<double> { l.Algus };
            piirid.AddRange(punktid.Where(p => p > l.Algus + Hulk.Tolerants && p < l.Lopp - Hulk.Tolerants));
            piirid.Add(l.Lopp);
            for (var j = 0; j < piirid.Count - 1; j++)
            {
                if (j > 0) Punkt(piirid[j]);
                Vahemik(piirid[j], piirid[j + 1]);
            }

            if (double.IsFinite(l.Lopp) && !OnLoikePiir(l.Lopp))
            {
                if (l.LoppKaasa) Punkt(l.Lopp);
                else Puudub(l.Lopp);
            }
            if (i < loigud.Count - 1 && loigud[i + 1].Algus - l.Lopp > Hulk.Tolerants)
                veerud.Add(new(new Loik(l.Lopp, loigud[i + 1].Algus, false, false).Latex(), true, "∄", "\\notin X", null));
        }

        return new Margitabel($"{tahis}(x)", tahenduseRida, veerud);
    }

    /// <summary>
    /// Testpunkt, millega on mugav käsitsi arvutada: 0, täisarv või poolik, mis jääb vahemiku sisse.
    /// Vahemikus pole teisi murdepunkte, seega annab iga sisepunkt sama märgi.
    /// </summary>
    private static double Testpunkt(double u, double v, Func<double, double> g)
    {
        static bool Sees(double x, double u, double v) => x > u + 1e-6 && x < v - 1e-6;
        double valik;
        if (double.IsNegativeInfinity(u) && double.IsPositiveInfinity(v)) valik = 0;
        else if (double.IsNegativeInfinity(u)) valik = Math.Ceiling(v) - 1 >= v - 1e-6 ? Math.Ceiling(v) - 2 : Math.Ceiling(v) - 1;
        else if (double.IsPositiveInfinity(v)) valik = Math.Floor(u) + 1 <= u + 1e-6 ? Math.Floor(u) + 2 : Math.Floor(u) + 1;
        else
        {
            var kesk = (u + v) / 2;
            valik = new[] { 0, Math.Round(kesk), Math.Round(kesk * 2) / 2, Math.Round(kesk * 10) / 10, Math.Round(kesk * 100) / 100 }
                .FirstOrDefault(x => Sees(x, u, v), kesk);
        }
        return double.IsNaN(g(valik)) ? Piirkonnad.Testpunkt(u, v) : valik + 0.0;
    }

    private static double[] LoikePiirid(Vahetulemused v, bool taielik) => taielik ? [] : [v.A, v.B];

    /// <summary>Kas kaks sama märgiga vahemikku on ühendatud üle punkti (x³: + 0 +)?</summary>
    private static bool OnYhendatud(Margitabel tabel)
    {
        var veerud = tabel.Veerud;
        for (var i = 0; i + 2 < veerud.Count; i++)
        {
            if (veerud[i].Vahemik && !veerud[i + 1].Vahemik && veerud[i + 2].Vahemik
                && veerud[i].Mark == veerud[i + 2].Mark && veerud[i + 1].Mark == "0")
                return true;
        }
        return false;
    }

    // ---------------------------------------------------------------- abimeetodid

    /// <summary>"f(-1) = (-1)^{3} - 3 \cdot (-1) = 2" – asendus, kui x-il on täpne kuju.</summary>
    private static string Vaartus(MathNet.Symbolics.Expression f, double x, double y)
    {
        var tulemus = Arv.OnTapne(y) ? $"= {Arv.Latex(y)}" : $"\\approx {Arv.Kumnendkuju(y)}";
        if (!Arv.OnTapne(x)) return $"f({Arv.Latex(x)}) {tulemus}";
        return $"f({Arv.Latex(x)}) = {ValemiVormindaja.LatexAsendusega(f, x)} {tulemus}";
    }

    private static string HulkLatex(Hulk h) =>
        h.Loigud is [{ Algus: double.NegativeInfinity, Lopp: double.PositiveInfinity }] ? "\\mathbb{R}" : h.Latex();

    private static string Vordlus(TingimuseLiik liik) => liik switch
    {
        TingimuseLiik.NullistErinev => "\\neq",
        TingimuseLiik.MitteNegatiivne => "\\geq",
        _ => ">"
    };

    private static string L(MathNet.Symbolics.Expression e) => ValemiVormindaja.Latex(e);

    private static string Loik(double a, double b) => $"$[{Arv.Latex(a)};\\ {Arv.Latex(b)}]$";

    private static string Suurtaht(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
