using FunktsioonideUurimine.Api.Andmed;
using FunktsioonideUurimine.Api.Matemaatika;
using FunktsioonideUurimine.Api.Mudelid;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace FunktsioonideUurimine.Api.Otspunktid;

/// <summary>CRUD: salvestatud funktsioonide uurimised (EF Core kaudu).</summary>
public static class FunktsioonidOtspunktid
{
    public static void MapFunktsioonid(this IEndpointRouteBuilder app)
    {
        var grupp = app.MapGroup("/api/funktsioonid")
            .WithTags("Funktsioonid")
            .AddFluentValidationAutoValidation();

        grupp.MapGet("/", LoeKoikAsync).WithSummary("Kõik salvestatud funktsioonid (uuemad eespool)");
        grupp.MapGet("/{id:int}", LoeAsync).WithSummary("Üks funktsioon");
        grupp.MapPost("/", LooAsync).WithSummary("Uuri funktsiooni ja salvesta tulemus");
        grupp.MapPut("/{id:int}", MuudaAsync).WithSummary("Muuda valemit/vahemikku ja uuri uuesti");
        grupp.MapDelete("/{id:int}", KustutaAsync).WithSummary("Kustuta funktsioon");
    }

    private static async Task<Ok<List<FunktsiooniVastus>>> LoeKoikAsync(
        FunktsiooniKontekst _kontekst, FunktsiooniAnaluusija analuusija, CancellationToken ct)
    {
        var olemid = await _kontekst.FunktsiooniUurimised
            .AsNoTracking()
            .OrderByDescending(f => f.LuodudAeg)
            .ThenByDescending(f => f.Id)
            .ToListAsync(ct);
        return TypedResults.Ok(olemid.Select(o => Vastus(o, analuusija)).ToList());
    }

    private static async Task<Results<Ok<FunktsiooniVastus>, NotFound>> LoeAsync(
        int id, FunktsiooniKontekst _kontekst, FunktsiooniAnaluusija analuusija, CancellationToken ct)
    {
        var olem = await _kontekst.FunktsiooniUurimised.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
        return olem is null ? TypedResults.NotFound() : TypedResults.Ok(Vastus(olem, analuusija));
    }

    private static async Task<Created<FunktsiooniVastus>> LooAsync(
        FunktsiooniPaering paering, FunktsiooniKontekst _kontekst, FunktsiooniAnaluusija analuusija, CancellationToken ct)
    {
        var olem = new FunktsiooniUurimine();
        Kanna(paering, analuusija, olem);
        await _kontekst.FunktsiooniUurimised.AddAsync(olem, ct);
        await _kontekst.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/funktsioonid/{olem.Id}", Vastus(olem, analuusija));
    }

    private static async Task<Results<Ok<FunktsiooniVastus>, NotFound>> MuudaAsync(
        int id, FunktsiooniPaering paering, FunktsiooniKontekst _kontekst, FunktsiooniAnaluusija analuusija,
        CancellationToken ct)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync([id], ct);
        if (olem is null) return TypedResults.NotFound();
        Kanna(paering, analuusija, olem);
        olem.MuudetudAeg = DateTime.UtcNow;
        await _kontekst.SaveChangesAsync(ct);
        return TypedResults.Ok(Vastus(olem, analuusija));
    }

    private static async Task<Results<NoContent, NotFound>> KustutaAsync(
        int id, FunktsiooniKontekst _kontekst, CancellationToken ct)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync([id], ct);
        if (olem is null) return TypedResults.NotFound();
        _kontekst.FunktsiooniUurimised.Remove(olem);
        await _kontekst.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    /// <summary>Arvutab funktsiooni omadused ja kannab need olemile.</summary>
    public static void Kanna(FunktsiooniPaering paering, FunktsiooniAnaluusija analuusija, FunktsiooniUurimine olem)
    {
        var t = analuusija.Analuusi(paering.Valem, paering.VahemikAlgus, paering.VahemikLopp);
        olem.Valem = paering.Valem.Trim();
        olem.VahemikAlgus = paering.VahemikAlgus;
        olem.VahemikLopp = paering.VahemikLopp;
        olem.Maaramispiirkond = t.Maaramispiirkond;
        olem.Nullkohad = t.Nullkohad;
        olem.Positiivsus = t.Positiivsus;
        olem.Tuletis = t.Tuletis;
        olem.KriitilisedPunktid = t.KriitilisedPunktid;
        olem.Ekstreemumid = t.Ekstreemumid;
        olem.Monotoonsus = t.Monotoonsus;
        olem.TeineTuletis = t.TeineTuletis;
        olem.Kaanupunktid = t.Kaanupunktid;
        olem.Kumerus = t.Kumerus;
    }

    private static FunktsiooniVastus Vastus(FunktsiooniUurimine o, FunktsiooniAnaluusija analuusija)
    {
        Latexid? latex = null;
        try { latex = analuusija.LeiaLatexid(o.Valem); }
        catch (Exception) { /* vana/vigane kirje – kuvatakse tekstina */ }

        return new FunktsiooniVastus(o.Id, o.Valem, latex?.Valem, o.Maaramispiirkond, o.Nullkohad, o.Positiivsus,
            o.Tuletis, latex?.Tuletis, o.KriitilisedPunktid, o.Ekstreemumid, o.Monotoonsus, o.TeineTuletis,
            latex?.TeineTuletis, o.Kaanupunktid, o.Kumerus, o.VahemikAlgus, o.VahemikLopp, o.LuodudAeg,
            o.MuudetudAeg);
    }
}
