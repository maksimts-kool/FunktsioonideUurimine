using FunktsioonideUurimine.Api.Matemaatika;

namespace FunktsioonideUurimine.Tests;

public class ParseriTestid
{
    private static double Vaartus(string valem, double x) =>
        Hindaja.Kompileeri(ValemiParser.Parsi(valem).Avaldis)(x);

    [Theory]
    [InlineData("-x^2 + 1", 2, -3)] // MathNet'i Infix.Parse annaks (-x)^2 + 1 = 5
    [InlineData("2x^3 - 3x", 2, 10)] // kaudne korrutamine
    [InlineData("3(x + 1)", 1, 6)]
    [InlineData("(x + 1)(x - 1)", 3, 8)]
    [InlineData("x^-2", 2, 0.25)]
    [InlineData("2^-x", 1, 0.5)]
    [InlineData("0,5x^2", 2, 2)] // koma kümnenderaldajana
    [InlineData("x²", 3, 9)]
    [InlineData("√x", 9, 3)]
    [InlineData("sin x", 0, 0)]
    [InlineData("tg(x) + ctg(x)", Math.PI / 4, 2)]
    [InlineData("log(x)", 100, 2)] // log = lg
    [InlineData("xsinx", Math.PI / 2, Math.PI / 2)]
    [InlineData("e^x", 1, Math.E)]
    [InlineData("pi*x", 1, Math.PI)]
    [InlineData("arcsin(x)", 1, Math.PI / 2)]
    [InlineData("x^(1/3)", -8, -2)]
    public void Arvutab_oige_vaartuse(string valem, double x, double oodatud) =>
        Assert.Equal(oodatud, Vaartus(valem, x), 10);

    [Theory]
    [InlineData("y + 1", "Tundmatu tähis")]
    [InlineData("x +", "lõppes ootamatult")]
    [InlineData("(x + 1", "sulgev sulg")]
    [InlineData("x + 1)", "Liigne sulg")]
    [InlineData("1/0", "Jagamine nulliga")]
    [InlineData("sqrt(-1)", "ei ole reaalarvudes")]
    [InlineData("abs(x)", "abs() ei ole toetatud")]
    [InlineData("x $ 2", "Tundmatu märk")]
    [InlineData("", "tühi")]
    public void Annab_arusaadava_veateate(string valem, string oodatudOsa)
    {
        var viga = Assert.Throws<ValemiViga>(() => ValemiParser.Parsi(valem));
        Assert.Contains(oodatudOsa, viga.Message);
    }

    [Theory]
    [InlineData("1/(x-2)", "x - 2 NullistErinev")]
    [InlineData("sqrt(x)", "x MitteNegatiivne")]
    [InlineData("ln(x+1)", "x + 1 Positiivne")]
    [InlineData("x^(-1/2)", "x Positiivne")]
    [InlineData("tan(x)", "cos(x) NullistErinev")]
    public void Kogub_maaramispiirkonna_tingimused(string valem, string oodatud)
    {
        var tingimused = ValemiParser.Parsi(valem).Tingimused
            .Select(t => $"{ValemiVormindaja.Tekst(t.Avaldis)} {t.Liik}");
        Assert.Contains(oodatud, tingimused);
    }
}
