using FunktsioonideUurimine.Api.Matemaatika;

namespace FunktsioonideUurimine.Api.Mudelid;

/// <summary>Funktsiooni loomise, muutmise ja eelvaate päring.</summary>
/// <param name="Valem">Funktsiooni valem muutujaga x, nt "x^3 - 3*x".</param>
/// <param name="VahemikAlgus">Uuritava/joonestatava vahemiku algus.</param>
/// <param name="VahemikLopp">Uuritava/joonestatava vahemiku lõpp.</param>
public sealed record FunktsiooniPaering(string Valem, double VahemikAlgus = -5, double VahemikLopp = 5);

/// <summary>Graafiku punktide päring: y-väärtused arvutatakse serveris valemi põhjal.</summary>
public sealed record GraafikuPaering(string Valem, double Algus = -5, double Lopp = 5, double Samm = 0.05);

/// <summary>Funktsiooni uurimise tulemus (ilma salvestamata).</summary>
public sealed record AnaluusiVastus(
    string Valem,
    string ValemLatex,
    string Maaramispiirkond,
    string Nullkohad,
    string Positiivsus,
    string Tuletis,
    string TuletisLatex,
    string KriitilisedPunktid,
    string Ekstreemumid,
    string Monotoonsus,
    string TeineTuletis,
    string TeineTuletisLatex,
    string Kaanupunktid,
    string Kumerus,
    double VahemikAlgus,
    double VahemikLopp,
    bool Numbriline);

/// <summary>Andmebaasi salvestatud funktsiooni uurimine.</summary>
public sealed record FunktsiooniVastus(
    int Id,
    string Valem,
    string? ValemLatex,
    string Maaramispiirkond,
    string Nullkohad,
    string Positiivsus,
    string Tuletis,
    string? TuletisLatex,
    string KriitilisedPunktid,
    string Ekstreemumid,
    string Monotoonsus,
    string TeineTuletis,
    string? TeineTuletisLatex,
    string Kaanupunktid,
    string Kumerus,
    double VahemikAlgus,
    double VahemikLopp,
    DateTime LuodudAeg,
    DateTime? MuudetudAeg);

/// <summary>Graafiku andmed: ühine x-massiiv ning f(x), f'(x), f''(x) väärtused (null = määramata).</summary>
public sealed record GraafikuVastus(
    double[] X,
    double?[] Y,
    double?[] YTuletis,
    double?[] YTeineTuletis,
    IReadOnlyList<Punkt> Nullkohad,
    IReadOnlyList<Ekstreemum> Ekstreemumid,
    IReadOnlyList<Punkt> Kaanupunktid,
    IReadOnlyList<double> Asumptoodid,
    string ValemLatex,
    string TuletisLatex,
    string TeineTuletisLatex);

/// <summary>Samm-sammuline lahenduskäik: kuidas iga vastus leiti (tingimused, võrrandid, reeglid, märgitabelid).</summary>
public sealed record LahenduskaiguVastus(
    string Valem,
    string ValemLatex,
    double VahemikAlgus,
    double VahemikLopp,
    IReadOnlyList<LahenduseOsa> Osad);
