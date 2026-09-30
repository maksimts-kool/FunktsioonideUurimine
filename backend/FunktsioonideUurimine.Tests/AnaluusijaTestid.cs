using FunktsioonideUurimine.Api.Matemaatika;

namespace FunktsioonideUurimine.Tests;

/// <summary>Matemaatiline täpsus: tulemused on kontrollitud käsitsi arvutades.</summary>
public class AnaluusijaTestid
{
    private readonly FunktsiooniAnaluusija _analuusija = new();

    [Fact]
    public void Kuupfunktsioon_ulesande_naide()
    {
        // f(x) = x³ - 3x: f'(x) = 3x² - 3 = 0 ⟺ x = ±1; f(-1) = 2, f(1) = -2
        var t = _analuusija.Analuusi("x^3 - 3*x", -3, 3);

        Assert.Equal("(-∞; ∞)", t.Maaramispiirkond);
        Assert.Equal("3*x^2 - 3", t.Tuletis);
        Assert.Equal("x₁ = -√3 ≈ -1.7321; x₂ = 0; x₃ = √3 ≈ 1.7321", t.Nullkohad);
        Assert.Equal("x₁ = -1; x₂ = 1", t.KriitilisedPunktid);
        Assert.Equal("max f(-1) = 2; min f(1) = -2", t.Ekstreemumid);
        Assert.Equal("X↑ = (-∞; -1) ∪ (1; ∞); X↓ = (-1; 1)", t.Monotoonsus);
        Assert.Equal("X⁺ = (-√3; 0) ∪ (√3; ∞); X⁻ = (-∞; -√3) ∪ (0; √3)", t.Positiivsus);
        Assert.Equal("6*x", t.TeineTuletis);
        Assert.Equal("K(0; 0)", t.Kaanupunktid);
        Assert.False(t.Numbriline);
    }

    [Fact]
    public void Ruutfunktsioon_graafikumalli_naide()
    {
        // f(x) = x² - 4x + 3 = (x - 1)(x - 3); haripunkt (2; -1)
        var t = _analuusija.Analuusi("x^2 - 4*x + 3", -2, 6);

        Assert.Equal("2*x - 4", t.Tuletis);
        Assert.Equal("x₁ = 1; x₂ = 3", t.Nullkohad);
        Assert.Equal("x = 2", t.KriitilisedPunktid);
        Assert.Equal("min f(2) = -1", t.Ekstreemumid);
        Assert.Equal("X∪ = (-∞; ∞); X∩ = ∅", t.Kumerus);
    }

    [Fact]
    public void Boonusulesande_naide()
    {
        // f(x) = x³ - 12x: nullkohad 0, ±2√3; f'(x) = 3x² - 12 = 0 ⟺ x = ±2; f(-2) = 16
        var t = _analuusija.Analuusi("x^3 - 12*x", -5, 5);

        Assert.Equal("x₁ = -2√3 ≈ -3.4641; x₂ = 0; x₃ = 2√3 ≈ 3.4641", t.Nullkohad);
        Assert.Equal("3*x^2 - 12", t.Tuletis);
        Assert.Equal("max f(-2) = 16; min f(2) = -16", t.Ekstreemumid);
    }

    [Fact]
    public void Neljanda_astme_polunoom_kolme_ekstreemumiga()
    {
        // f(x) = x⁴ - 2x²: f'(x) = 4x(x² - 1); käänupunktid x = ±1/√3, f = -5/9
        var t = _analuusija.Analuusi("x^4 - 2*x^2", -2.5, 2.5);

        Assert.Equal("x₁ = -√2 ≈ -1.4142; x₂ = 0; x₃ = √2 ≈ 1.4142", t.Nullkohad);
        Assert.Equal("min f(-1) = -1; max f(0) = 0; min f(1) = -1", t.Ekstreemumid);
        Assert.Equal("K₁(-√3/3; -5/9); K₂(√3/3; -5/9)", t.Kaanupunktid);
    }

    [Fact]
    public void Murdfunktsioon_puustasumptoodiga()
    {
        var t = _analuusija.Analuusi("1/(x - 2)", -3, 7);

        Assert.Equal("(-∞; 2) ∪ (2; ∞)", t.Maaramispiirkond);
        Assert.Equal("puuduvad", t.Nullkohad);
        Assert.Equal("-1/(x - 2)^2", t.Tuletis);
        Assert.Equal("puuduvad", t.Ekstreemumid);
        Assert.Equal("X↑ = ∅; X↓ = (-∞; 2) ∪ (2; ∞)", t.Monotoonsus);
        Assert.Equal([2.0], t.Asumptoodid);
    }

    [Fact]
    public void Ruutjuur_loigul()
    {
        // f(x) = √(4 - x²) – poolring: X = [-2; 2], max f(0) = 2
        var t = _analuusija.Analuusi("sqrt(4 - x^2)", -3, 3);

        Assert.Equal("[-2; 2]", t.Maaramispiirkond);
        Assert.Equal("x₁ = -2; x₂ = 2", t.Nullkohad);
        Assert.Equal("-x/sqrt(4 - x^2)", t.Tuletis);
        Assert.Equal("max f(0) = 2", t.Ekstreemumid);
        Assert.False(t.Numbriline);
    }

    [Fact]
    public void Eksponentfunktsioon_taeliku_kujuga()
    {
        // f(x) = x·e^(-x): f'(x) = (1 - x)e^(-x); max f(1) = 1/e; käänupunkt x = 2
        var t = _analuusija.Analuusi("x*e^(-x)", -1, 6);

        Assert.Equal("e^(-x) - x*e^(-x)", t.Tuletis);
        Assert.Equal("x = 0", t.Nullkohad);
        Assert.Equal("max f(1) = 1/e ≈ 0.3679", t.Ekstreemumid);
        Assert.Equal("K(2; 2/e²)", t.Kaanupunktid);
        Assert.False(t.Numbriline);
    }

    [Fact]
    public void Logaritm()
    {
        var t = _analuusija.Analuusi("x^2*ln(x)", 0, 2);

        Assert.Equal("(0; ∞)", t.Maaramispiirkond);
        Assert.Equal("x = 1", t.Nullkohad);
        // f'(x) = x(1 + 2 ln x) = 0 ⟺ x = e^(-1/2); f = -1/(2e)
        Assert.Equal("min f(1/√e) = -1/(2e) ≈ -0.1839", t.Ekstreemumid);
        Assert.Equal("K(1/(e√e); -3/(2e³))", t.Kaanupunktid);
        Assert.False(t.Numbriline);
    }

    [Fact]
    public void Ln_ja_eksponent_isoleeritakse()
    {
        Assert.Equal("x = e ≈ 2.7183", _analuusija.Analuusi("ln(x) - 1", 0, 5).Nullkohad);
        Assert.Equal("x = ln 2 ≈ 0.6931", _analuusija.Analuusi("e^x - 2", -2, 2).Nullkohad);
        Assert.Equal([0.0], _analuusija.Analuusi("ln(x)", -1, 5).Asumptoodid);
    }

    [Fact]
    public void Trigonomeetriline_funktsioon_uuritakse_vahemikus()
    {
        var t = _analuusija.Analuusi("sin(x)", -6.3, 6.3);

        Assert.True(t.Numbriline);
        Assert.Equal(
            "x₁ = -2π ≈ -6.2832; x₂ = -π ≈ -3.1416; x₃ = 0; x₄ = π ≈ 3.1416; x₅ = 2π ≈ 6.2832 (vahemikus [-6.3; 6.3])",
            t.Nullkohad);
        Assert.Equal(
            "max f(-3π/2) = 1; min f(-π/2) = -1; max f(π/2) = 1; min f(3π/2) = -1 (vahemikus [-6.3; 6.3])",
            t.Ekstreemumid);
    }

    [Fact]
    public void Teravik_on_kriitiline_punkt_kus_tuletis_puudub()
    {
        // f(x) = x^(2/3): f'(0) puudub, kuid f(0) = 0 on miinimum; nõgus kummalgi pool 0, mitte kogu ℝ-il
        var t = _analuusija.Analuusi("x^(2/3)", -3, 3);

        Assert.Equal("(-∞; ∞)", t.Maaramispiirkond);
        Assert.Equal("x = 0 (f' puudub)", t.KriitilisedPunktid);
        Assert.Equal("min f(0) = 0", t.Ekstreemumid);
        Assert.Equal("X∪ = ∅; X∩ = (-∞; 0) ∪ (0; ∞)", t.Kumerus);
    }

    [Fact]
    public void Kuupjuur_on_maaratud_ka_negatiivsetel()
    {
        var t = _analuusija.Analuusi("cbrt(x)", -3, 3);

        Assert.Equal("(-∞; ∞)", t.Maaramispiirkond);
        Assert.Equal(-2, t.F(-8), 12);
        Assert.Equal("X↑ = (-∞; ∞); X↓ = ∅", t.Monotoonsus);
        Assert.Equal("K(0; 0)", t.Kaanupunktid);
    }

    [Fact]
    public void Kuupfunktsioonil_puudub_ekstreemum_kuigi_tuletis_on_null()
    {
        var t = _analuusija.Analuusi("x^3", -2, 2);

        Assert.Equal("x = 0", t.KriitilisedPunktid);
        Assert.Equal("puuduvad", t.Ekstreemumid);
        Assert.Equal("X↑ = (-∞; ∞); X↓ = ∅", t.Monotoonsus);
    }

    [Fact]
    public void Lihtsustamine_ei_kaota_maaramispiirkonna_tingimust()
    {
        // MathNet lihtsustab x/x → 1, kuid x = 0 ei kuulu määramispiirkonda
        Assert.Equal("(-∞; 0) ∪ (0; ∞)", _analuusija.Analuusi("x/x", -3, 3).Maaramispiirkond);
    }

    [Fact]
    public void Ratsionaalfunktsiooni_tuletis_on_tegurdatud()
    {
        var t = _analuusija.Analuusi("(x^2-1)/(x^2+1)", -5, 5);

        Assert.Equal("4*x/(x^2 + 1)^2", t.Tuletis);
        Assert.Equal("min f(0) = -1", t.Ekstreemumid);
    }

    [Fact]
    public void Kuldloike_nullkohad()
    {
        Assert.Equal("x₁ = (1 - √5)/2 ≈ -0.618; x₂ = (1 + √5)/2 ≈ 1.618",
            _analuusija.Analuusi("x^2 - x - 1", -3, 3).Nullkohad);
    }

    [Fact]
    public void Keerulisem_avaldis_ei_jaa_rippuma()
    {
        // MathNet'i RationalSimplify jääb selle teise tuletise peal lõputult tööle – seda ei tohi välja kutsuda
        var tulemus = Task.Run(() => _analuusija.Analuusi("sqrt((x-1)/(x+2))", -5, 5));

        Assert.True(tulemus.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
        Assert.Equal("(-∞; -2) ∪ [1; ∞)", tulemus.Result.Maaramispiirkond);
    }

    [Theory]
    [InlineData("x^3 - 3*x", "x^{3} - 3x")]
    [InlineData("x*e^(-x)", "x e^{-x}")]
    [InlineData("(x^2-1)/(x^2+1)", "\\frac{x^{2} - 1}{x^{2} + 1}")]
    [InlineData("sqrt(4 - x^2)", "\\sqrt{4 - x^{2}}")]
    [InlineData("pi*x", "\\pi x")]
    [InlineData("sin(x)^2", "\\sin^{2} x")]
    public void Latex(string valem, string oodatud) =>
        Assert.Equal(oodatud, _analuusija.LeiaLatexid(valem).Valem);
}
