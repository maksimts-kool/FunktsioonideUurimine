using System.Net;
using System.Net.Http.Json;
using FunktsioonideUurimine.Api.Mudelid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FunktsioonideUurimine.Tests;

/// <summary>Otspunktide testid päris SQLite andmebaasiga (ajutine fail).</summary>
public class ApiTestid : IClassFixture<ApiTestid.Rakendus>
{
    public sealed class Rakendus : WebApplicationFactory<Program>
    {
        private readonly string _fail = Path.Combine(Path.GetTempPath(), $"funktsioonid-test-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) =>
            builder.UseSetting("ConnectionStrings:Andmebaas", $"Data Source={_fail};Pooling=False");

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            File.Delete(_fail);
        }
    }

    private readonly HttpClient _klient;
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    public ApiTestid(Rakendus rakendus) => _klient = rakendus.CreateClient();

    [Fact]
    public async Task Naidisandmed_on_andmebaasis()
    {
        var koik = await _klient.GetFromJsonAsync<List<FunktsiooniVastus>>("/api/funktsioonid", Ct);

        Assert.NotNull(koik);
        var naide = Assert.Single(koik, f => f.Valem == "x^3 - 3*x");
        Assert.Equal("max f(-1) = 2; min f(1) = -2", naide.Ekstreemumid);
        Assert.Equal("x^{3} - 3x", naide.ValemLatex);
    }

    [Fact]
    public async Task Loomine_lugemine_muutmine_ja_kustutamine()
    {
        // Create
        var loomine = await _klient.PostAsJsonAsync("/api/funktsioonid",
            new FunktsiooniPaering("x^2 - 2*x", -2, 4), Ct);
        Assert.Equal(HttpStatusCode.Created, loomine.StatusCode);
        var loodud = (await loomine.Content.ReadFromJsonAsync<FunktsiooniVastus>(Ct))!;
        Assert.Equal("2*x - 2", loodud.Tuletis);
        Assert.Equal("min f(1) = -1", loodud.Ekstreemumid);
        Assert.Equal($"/api/funktsioonid/{loodud.Id}", loomine.Headers.Location?.ToString());

        // Read
        var loetud = await _klient.GetFromJsonAsync<FunktsiooniVastus>($"/api/funktsioonid/{loodud.Id}", Ct);
        Assert.Equal("x^2 - 2*x", loetud!.Valem);

        // Update – omadused arvutatakse uuesti
        var muutmine = await _klient.PutAsJsonAsync($"/api/funktsioonid/{loodud.Id}",
            new FunktsiooniPaering("x^3 - 3*x", -3, 3), Ct);
        Assert.Equal(HttpStatusCode.OK, muutmine.StatusCode);
        var muudetud = (await muutmine.Content.ReadFromJsonAsync<FunktsiooniVastus>(Ct))!;
        Assert.Equal("3*x^2 - 3", muudetud.Tuletis);
        Assert.NotNull(muudetud.MuudetudAeg);

        // Delete
        var kustutamine = await _klient.DeleteAsync($"/api/funktsioonid/{loodud.Id}", Ct);
        Assert.Equal(HttpStatusCode.NoContent, kustutamine.StatusCode);
        var puudub = await _klient.GetAsync($"/api/funktsioonid/{loodud.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, puudub.StatusCode);
    }

    [Theory]
    [InlineData("", -5, 5, "Valem")]
    [InlineData("x +* 2", -5, 5, "Valem")]
    [InlineData("y^2", -5, 5, "Valem")]
    [InlineData("x^2", 5, -5, "VahemikAlgus")]
    [InlineData("x^2", -5000, 5, "VahemikAlgus")]
    public async Task Vigane_sisend_annab_400(string valem, double algus, double lopp, string vali)
    {
        var vastus = await _klient.PostAsJsonAsync("/api/funktsioonid", new FunktsiooniPaering(valem, algus, lopp), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, vastus.StatusCode);
        var probleem = await vastus.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.Contains(vali, probleem!.Errors.Keys);
    }

    [Fact]
    public async Task Muutmine_ja_kustutamine_olematu_id_korral_annab_404()
    {
        var muutmine = await _klient.PutAsJsonAsync("/api/funktsioonid/999999", new FunktsiooniPaering("x"), Ct);
        var kustutamine = await _klient.DeleteAsync("/api/funktsioonid/999999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, muutmine.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, kustutamine.StatusCode);
    }

    [Fact]
    public async Task Graafiku_punktid_arvutatakse_serveris()
    {
        var vastus = await _klient.PostAsJsonAsync("/api/graafik", new GraafikuPaering("x^2 - 4*x + 3", -2, 6, 0.1), Ct);
        vastus.EnsureSuccessStatusCode();
        var graafik = (await vastus.Content.ReadFromJsonAsync<GraafikuVastus>(Ct))!;

        Assert.Equal(81, graafik.X.Length);
        Assert.Equal(-2, graafik.X[0]);
        Assert.Equal(15, graafik.Y[0]); // f(-2) = 4 + 8 + 3
        Assert.Equal(-8, graafik.YTuletis[0]); // f'(-2) = -4 - 4
        var min = Assert.Single(graafik.Ekstreemumid);
        Assert.Equal((2, -1, "min"), (min.X, min.Y, min.Tyyp));
    }

    [Fact]
    public async Task Graafik_katkeb_poolusel()
    {
        var vastus = await _klient.PostAsJsonAsync("/api/graafik", new GraafikuPaering("1/(x - 2)", 0, 4, 0.3), Ct);
        var graafik = (await vastus.Content.ReadFromJsonAsync<GraafikuVastus>(Ct))!;

        var i = Array.IndexOf(graafik.X, 2.0);
        Assert.True(i > 0, "poolusele x = 2 lisatakse tühi punkt, et harusid ei ühendataks");
        Assert.Null(graafik.Y[i]);
        Assert.Equal([2.0], graafik.Asumptoodid);
    }

    [Fact]
    public async Task Analuus_ei_salvesta()
    {
        var enne = (await _klient.GetFromJsonAsync<List<FunktsiooniVastus>>("/api/funktsioonid", Ct))!.Count;
        var vastus = await _klient.PostAsJsonAsync("/api/analuus", new FunktsiooniPaering("ln(x)"), Ct);
        var parast = (await _klient.GetFromJsonAsync<List<FunktsiooniVastus>>("/api/funktsioonid", Ct))!.Count;

        var analuus = (await vastus.Content.ReadFromJsonAsync<AnaluusiVastus>(Ct))!;
        Assert.Equal("(0; ∞)", analuus.Maaramispiirkond);
        Assert.Equal(enne, parast);
    }
}
