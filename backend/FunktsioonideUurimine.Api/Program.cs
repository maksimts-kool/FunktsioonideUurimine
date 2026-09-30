using FluentValidation;
using FunktsioonideUurimine.Api.Andmed;
using FunktsioonideUurimine.Api.Matemaatika;
using FunktsioonideUurimine.Api.Otspunktid;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FunktsiooniAnaluusija>();

// EF Core + SQLite; tühja andmebaasi lisatakse näidisfunktsioonid (EF Core 9+ seeding)
builder.Services.AddDbContext<FunktsiooniKontekst>((teenused, valikud) => valikud
    .UseSqlite(builder.Configuration.GetConnectionString("Andmebaas"))
    .UseSeeding((kontekst, _) =>
        Algandmed.Lisa(kontekst, teenused.GetRequiredService<FunktsiooniAnaluusija>()))
    .UseAsyncSeeding((kontekst, _, ct) =>
        Algandmed.LisaAsync(kontekst, teenused.GetRequiredService<FunktsiooniAnaluusija>(), ct)));

// sisendi valideerimine: FluentValidation + automaatne kontroll Minimal API otspunktides
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

await using (var skoop = app.Services.CreateAsyncScope())
{
    await skoop.ServiceProvider.GetRequiredService<FunktsiooniKontekst>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.MapScalarApiReference("/api/docs", valikud => valikud.WithTitle("Funktsioonide uurimine – API"));

// toodanguversioonis serveeritakse React-rakendust (npm run build → wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFunktsioonid();
app.MapAnaluus();
app.MapFallbackToFile("{*tee:regex(^(?!api/).*$)}", "index.html");

app.Run();

public partial class Program;
