using FunktsioonideUurimine.Api.Matemaatika;

namespace FunktsioonideUurimine.Tests;

/// <summary>Lahenduskäik peab näitama samu samme, mida õpilane käsitsi teeks.</summary>
public class LahenduskaiguTestid
{
    private readonly FunktsiooniAnaluusija _analuusija = new();

    private List<LahenduseOsa> Kaik(string valem, double a = -5, double b = 5) =>
        LahenduskaiguKoostaja.Koosta(_analuusija.Analuusi(valem, a, b));

    private static List<string> Valemid(LahenduseOsa osa) =>
        osa.Sammud.Select(s => s.Valem).OfType<string>().ToList();

    [Fact]
    public void Ruutvorrand_lahendatakse_diskriminandiga()
    {
        var nullkohad = Kaik("x^2 - 4x + 3").Single(o => o.Voti == "nullkohad");

        Assert.Contains(@"D = b^{2} - 4ac = \left(-4\right)^{2} - 4 \cdot 1 \cdot 3 = 4", Valemid(nullkohad));
        Assert.Contains(@"x_{1} = 1, \quad x_{2} = 3", Valemid(nullkohad));
    }

    [Fact]
    public void Kuupvorrand_ratsionaalse_juure_ja_Horneri_skeemiga()
    {
        var nullkohad = Kaik("x^3 - 6x^2 + 11x - 6").Single(o => o.Voti == "nullkohad");

        Assert.Contains(@"x^{3} - 6x^{2} + 11x - 6 = \left(x - 1\right)\left(x^{2} - 5x + 6\right) = 0", Valemid(nullkohad));
    }

    [Fact]
    public void Jagatise_tuletis_kirjutatakse_osadena_lahti()
    {
        var tuletis = Kaik("(x^2 - 1)/(x^2 + 1)").Single(o => o.Voti == "tuletis");
        var valemid = Valemid(tuletis);

        Assert.Contains(@"u = x^{2} - 1, \quad u' = 2x", valemid);
        Assert.Contains(@"v = x^{2} + 1, \quad v' = 2x", valemid);
        Assert.Equal(@"f'(x) = \frac{4x}{\left(x^{2} + 1\right)^{2}}", valemid[^1]);
    }

    [Fact]
    public void Margitabel_testpunktide_ja_ekstreemumitega()
    {
        var monotoonsus = Kaik("x^3 - 3x").Single(o => o.Voti == "monotoonsus");
        var tabel = monotoonsus.Sammud.Select(s => s.Tabel).OfType<Margitabel>().Single();

        Assert.Equal(["+", "0", "−", "0", "+"], tabel.Veerud.Select(v => v.Mark));
        Assert.Equal(["f'(-2) = 9", null, "f'(0) = -3", null, "f'(2) = 9"], tabel.Veerud.Select(v => v.Kontroll));
        Assert.Equal(@"\text{max}", tabel.Veerud[1].Tahendus);
        Assert.Contains(@"y_{\text{max}} = f(-1) = \left(-1\right)^{3} - 3 \cdot \left(-1\right) = 2", Valemid(monotoonsus));
    }

    [Fact]
    public void Maaramispiirkonna_tingimused_pohjustega()
    {
        var x = Kaik("sqrt(4 - x^2)/(x - 1)").Single(o => o.Voti == "maaramispiirkond");

        Assert.Contains(x.Sammud, s => s.Tekst?.StartsWith("Paarisjuurt saab võtta") == true);
        Assert.Contains(x.Sammud, s => s.Tekst?.StartsWith("Nulliga jagada ei saa") == true);
        Assert.Equal(@"X = [-2;\ 1) \cup (1;\ 2]", Valemid(x)[^1]);
    }

    [Fact]
    public void Numbriline_lahendamine_on_margitud()
    {
        var nullkohad = Kaik("sin(x)", -6.3, 6.3).Single(o => o.Voti == "nullkohad");

        Assert.True(nullkohad.Numbriline);
        Assert.Contains(nullkohad.Sammud, s => s.Tekst?.Contains("numbriliselt") == true);
    }

    [Theory]
    [InlineData("x^3 - 3x")]
    [InlineData("x*e^(-x)")]
    [InlineData("x^2*ln(x)")]
    [InlineData("x^(2/3)")]
    [InlineData("tan(x)")]
    [InlineData("1/(x^2 - 4)")]
    [InlineData("x^x")]
    [InlineData("arcsin(x)")]
    [InlineData("5")]
    public void Koostamine_ei_viska_erindit(string valem)
    {
        var osad = Kaik(valem);
        Assert.Equal(8, osad.Count);
        Assert.All(osad, o => Assert.NotEmpty(o.Sammud));
    }
}
