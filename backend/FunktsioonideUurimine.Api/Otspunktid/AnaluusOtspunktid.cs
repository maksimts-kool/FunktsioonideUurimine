using FunktsioonideUurimine.Api.Matemaatika;
using FunktsioonideUurimine.Api.Mudelid;
using Microsoft.AspNetCore.Http.HttpResults;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace FunktsioonideUurimine.Api.Otspunktid;

/// <summary>Arvutused ilma salvestamata: eelvaade vormis ja graafiku punktid.</summary>
public static class AnaluusOtspunktid
{
    public static void MapAnaluus(this IEndpointRouteBuilder app)
    {
        var grupp = app.MapGroup("/api")
            .WithTags("Analüüs")
            .AddFluentValidationAutoValidation();

        grupp.MapPost("/analuus", Analuusi)
            .WithSummary("Uuri funktsiooni ilma salvestamata (eelvaade)");
        grupp.MapPost("/graafik", Graafik)
            .WithSummary("f(x), f'(x) ja f''(x) väärtused vahemikus antud sammuga ning erilised punktid");
    }

    private static Ok<AnaluusiVastus> Analuusi(FunktsiooniPaering paering, FunktsiooniAnaluusija analuusija)
    {
        var t = analuusija.Analuusi(paering.Valem, paering.VahemikAlgus, paering.VahemikLopp);
        return TypedResults.Ok(new AnaluusiVastus(paering.Valem.Trim(), t.ValemLatex, t.Maaramispiirkond,
            t.Nullkohad, t.Positiivsus, t.Tuletis, t.TuletisLatex, t.KriitilisedPunktid, t.Ekstreemumid,
            t.Monotoonsus, t.TeineTuletis, t.TeineTuletisLatex, t.Kaanupunktid, t.Kumerus,
            paering.VahemikAlgus, paering.VahemikLopp, t.Numbriline));
    }

    private static Ok<GraafikuVastus> Graafik(GraafikuPaering paering, FunktsiooniAnaluusija analuusija)
    {
        var t = analuusija.Analuusi(paering.Valem, paering.Algus, paering.Lopp);
        return TypedResults.Ok(GraafikuKoostaja.Koosta(t, paering.Algus, paering.Lopp, paering.Samm));
    }
}
