namespace FunktsioonideUurimine.Api.Mudelid;

/// <summary>
/// Ühe funktsiooni uurimise tulemus. Esimesed väljad on ülesande andmemudelist,
/// ülejäänud on lisatud funktsiooni täielikuma uurimise jaoks.
/// </summary>
public class FunktsiooniUurimine
{
    public int Id { get; set; }
    public string Valem { get; set; } = string.Empty;              // nt "x^3 - 3*x"
    public string Maaramispiirkond { get; set; } = string.Empty;   // X = (-∞; ∞)
    public string Tuletis { get; set; } = string.Empty;            // f'(x) = 3*x^2 - 3
    public string KriitilisedPunktid { get; set; } = string.Empty; // x₁ = -1; x₂ = 1
    public string Ekstreemumid { get; set; } = string.Empty;       // max f(-1) = 2; min f(1) = -2
    public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;

    public string Nullkohad { get; set; } = string.Empty;          // x₁ = -√3 ≈ -1.7321; x₂ = 0; ...
    public string Positiivsus { get; set; } = string.Empty;        // X⁺ = ...; X⁻ = ...
    public string Monotoonsus { get; set; } = string.Empty;        // X↑ = ...; X↓ = ...
    public string TeineTuletis { get; set; } = string.Empty;       // f''(x) = 6*x
    public string Kaanupunktid { get; set; } = string.Empty;       // K(0; 0)
    public string Kumerus { get; set; } = string.Empty;            // X∪ = ...; X∩ = ...

    /// <summary>Vahemik, milles graafikut joonestatakse (ja mittepolünoomide korral numbriliselt uuritakse).</summary>
    public double VahemikAlgus { get; set; } = -5;
    public double VahemikLopp { get; set; } = 5;
    public DateTime? MuudetudAeg { get; set; }
}
