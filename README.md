# Funktsioonide uurimise veebirakendus

C# (.NET 10) veebirakendus, kuhu kasutaja sisestab matemaatilise funktsiooni. Rakendus arvutab funktsiooni omadused
(määramispiirkond, nullkohad, tuletis, kriitilised punktid, ekstreemumid, monotoonsus, kumerus), salvestab need
EF Core abil andmebaasi ja joonestab funktsiooni ning selle tuletise graafiku.

- **Valemid on suvalised** – y-väärtused ja tuletised arvutab server tekstilise valemi põhjal (boonusülesanne).
- **Tulemused on täpsed** – polünoomide ja ratsionaalfunktsioonide nullkohad leitakse kogu arvteljel ning kuvatakse
  täpsel kujul: `x = -√3 ≈ -1.7321`, `max f(1) = 1/e ≈ 0.3679`, `x = 3π/2`.
- **Lahenduskäik** – „Näita lahenduskäiku“ näitab iga vastuse leidmist samm-sammult: määramispiirkonna tingimused,
  lahendatud võrrandid (tegurdamine, diskriminant, Horneri skeem), diferentseerimisreeglid ja märgitabelid
  (`POST /api/lahenduskaik`).
- **Teooria** – käsiraamatu stiilis leht (`/teooria`) funktsiooni uurimise, tuletiste tabeli ja uurimise skeemiga.

## Tehnoloogiad

| Kiht | Teek / tööriist |
|---|---|
| Backend | ASP.NET Core 10 Minimal API (`net10.0`) |
| ORM ja andmebaas | Entity Framework Core 10 + SQLite, migratsioonid, `UseSeeding` näidisandmed |
| Sümbolarvutus | [MathNet.Symbolics](https://symbolics.mathdotnet.com/) – tuletised, lihtsustamine, polünoomide SÜT |
| Numbrika | [MathNet.Numerics](https://numerics.mathdotnet.com/) – polünoomi juured (kaasmaatriks), Brenti meetod |
| Valideerimine | FluentValidation + SharpGrip.FluentValidation.AutoValidation.Endpoints |
| API dokumentatsioon | Microsoft.AspNetCore.OpenApi (OpenAPI 3.1) + Scalar |
| Frontend | React 19, Vite 8, TypeScript 7, React Router 8 |
| Kasutajaliides | Mantine 9, Tabler Icons |
| Andmete laadimine | TanStack Query 5 |
| Graafik | Apache ECharts 6 (joonestab `<canvas id="funktsiooniGraafik">` elemendile) |
| Valemid | KaTeX |
| Testid | xUnit v3 + Microsoft.Testing.Platform, `WebApplicationFactory` |

## Käivitamine

Vaja on **.NET 10 SDK** ja **Node.js 22+**.

```bash
npm install        # paigaldab ka frontend/ sõltuvused
npm run dev        # käivitab API (http://localhost:5176) ja Vite (http://localhost:5173)
```

Ava **http://localhost:5173**. Andmebaas `funktsioonid.db` luuakse esimesel käivitamisel migratsioonidest ja
täidetakse näidisfunktsioonidega. API dokumentatsioon: **http://localhost:5176/api/docs**.

Toodanguversioon (React ehitatakse `wwwroot` kausta ja ASP.NET serveerib kõike ühest pordist):

```bash
npm start          # → http://localhost:5176
```

Testid:

```bash
dotnet test
```

### EF Core migratsioonid

```bash
dotnet tool restore
dotnet ef migrations add <Nimi> -p backend/FunktsioonideUurimine.Api -o Andmed/Migratsioonid
dotnet ef database update -p backend/FunktsioonideUurimine.Api
```

## Hindamiskriteeriumid

### 1. EF Core ja andmebaas
- `FunktsiooniKontekst : DbContext` registreeritakse failis [Program.cs](backend/FunktsioonideUurimine.Api/Program.cs)
  (`AddDbContext` + `UseSqlite`), rakenduse käivitumisel rakendatakse migratsioonid (`MigrateAsync`).
- Migratsioon: [Andmed/Migratsioonid](backend/FunktsioonideUurimine.Api/Andmed/Migratsioonid) – tabel `FunktsiooniUurimised`
  (väljade pikkuspiirangud, indeks `LuodudAeg` järgi, UTC aegade teisendus).
- CRUD käib ORM-i kaudu eestikeelsete nimedega: `_kontekst.FunktsiooniUurimised.AddAsync(olem)`,
  `.ToListAsync()`, `FindAsync`, `Remove`, `SaveChangesAsync` – vt
  [FunktsioonidOtspunktid.cs](backend/FunktsioonideUurimine.Api/Otspunktid/FunktsioonidOtspunktid.cs).

Olem [`FunktsiooniUurimine`](backend/FunktsioonideUurimine.Api/Mudelid/FunktsiooniUurimine.cs) sisaldab ülesandes
antud välju (sh `LuodudAeg` täpselt ülesande nimega) ning lisaks: `Nullkohad`, `Positiivsus`, `Monotoonsus`,
`TeineTuletis`, `Kaanupunktid`, `Kumerus`, `VahemikAlgus`, `VahemikLopp`, `MuudetudAeg`.

### 2. Backend ja kontrollerid

| Meetod | Tee | Kirjeldus |
|---|---|---|
| GET | `/api/funktsioonid` | kõik salvestatud funktsioonid |
| GET | `/api/funktsioonid/{id}` | üks funktsioon (404, kui puudub) |
| POST | `/api/funktsioonid` | uuri valemit ja salvesta (201 + `Location`) |
| PUT | `/api/funktsioonid/{id}` | muuda valemit/vahemikku, omadused arvutatakse uuesti |
| DELETE | `/api/funktsioonid/{id}` | kustuta (204) |
| POST | `/api/analuus` | uuri ilma salvestamata (vormi eelvaade) |
| POST | `/api/graafik` | f(x), f'(x), f''(x) väärtused vahemikus antud sammuga + erilised punktid |

Sisend valideeritakse FluentValidationiga
([Validaatorid.cs](backend/FunktsioonideUurimine.Api/Valideerimine/Validaatorid.cs)): valem peab olema loetav ja
sisaldama ainult muutujat `x`, vahemik peab olema lõigus [-1000; 1000] ja algus < lõpp, graafikul kuni 20 000 punkti.
Vigane sisend annab vastuse `400 ValidationProblemDetails` eestikeelse veateatega, mida vorm näitab välja all.

### 3. Graafik ja kasutajaliides
- `f(x)` – pidev sinine joon, `f'(x)` – punane katkendjoon, `f''(x)` – roheline punktiir (legendist sisse lülitatav).
- Graafikul on nullkohad, ekstreemumid (max/min), käänupunktid ja püstasümptoodid (`x = 2`).
- Pooluse juures joon katkeb (harusid ei ühendata), y-telg skaleeritakse asümptootide korral mõistlikult.
- Vahemikku ja sammu saab muuta (vaikimisi samm 0.05); hiirerattaga suurendamine, pildina salvestamine.
- Uue funktsiooni vormis arvutatakse omadused ja graafik juba kirjutamise ajal.
- Kujundus toimib ka telefonis, olemas on hele ja tume teema.

### 4. Matemaatiline täpsus ja näidisandmed
Näidisandmete omadused arvutab sama analüsaator ([Algandmed.cs](backend/FunktsioonideUurimine.Api/Andmed/Algandmed.cs)),
seega vastavad need tegelikkusele. Käsitsi kontrollitud tulemused on kirjas testides
([AnaluusijaTestid.cs](backend/FunktsioonideUurimine.Tests/AnaluusijaTestid.cs)), näiteks:

| f(x) | Nullkohad | f'(x) | Ekstreemumid |
|---|---|---|---|
| x³ − 3x | −√3, 0, √3 | 3x² − 3 | max f(−1) = 2; min f(1) = −2 |
| x² − 4x + 3 | 1, 3 | 2x − 4 | min f(2) = −1 |
| x³ − 12x | −2√3, 0, 2√3 | 3x² − 12 | max f(−2) = 16; min f(2) = −16 |
| x⁴ − 2x² | −√2, 0, √2 | 4x³ − 4x | min f(±1) = −1; max f(0) = 0 |
| 1/(x − 2) | puuduvad | −1/(x − 2)² | puuduvad; X = (−∞; 2) ∪ (2; ∞) |
| √(4 − x²) | −2, 2 | −x/√(4 − x²) | max f(0) = 2; X = [−2; 2] |
| x·e^(−x) | 0 | e^(−x) − x·e^(−x) | max f(1) = 1/e |
| x²·ln x | 1 | x + 2x·ln x | min f(1/√e) = −1/(2e) |

### Boonus: suvalise valemi arvutamine serveris
Valem loetakse tekstist ([ValemiParser.cs](backend/FunktsioonideUurimine.Api/Matemaatika/ValemiParser.cs)), tuletised
leiab MathNet.Symbolics ja kõik y-väärtused (`/api/graafik`) arvutatakse serveris. Midagi pole koodis
kõvasti kodeeritud.

## Kuidas analüüs töötab

1. **Parser** ehitab valemist MathNet.Symbolics avaldise. MathNet'i enda `Infix.Parse` loeb `-x^2` kui `(-x)^2`
   ega toeta kaudset korrutamist, seepärast on kasutusel oma väike parser. Parser kogub ühtlasi määramispiirkonna
   tingimused enne MathNet'i automaatset lihtsustamist, et näiteks `x/x → 1` ei kaotaks tingimust `x ≠ 0`.
2. **Tuletised** leiab `SymbolicExpression.Differentiate`. Lihtsustamiseks proovitakse `Expand`, `RationalSimplify`
   ja tegurdamist ning valitakse lühim kuju, nt `4x/(x² + 1)²`.
3. **Nullkohad**:
   - polünoomi puhul kaasmaatriksi omaväärtused (`FindRoots.Polynomial`) ja Newtoni täpsustus;
   - korrutised jagatakse teguriteks, `k·ln u + c = 0` lahendatakse sümboolselt;
   - muudel juhtudel numbriliselt (märgivahetus ja Brenti meetod) valitud vahemikus.
4. **Määramispiirkond, monotoonsus, kumerus** leitakse märgianalüüsiga tingimuste ja tuletiste nullkohtade vahel.
5. **Täpne kuju** (`Arv.cs`) tunneb ära murrud, ruutjuured `(1 + √5)/2`, π kordsed ja e astmed.

Mittepolünoomiliste funktsioonide (nt `sin x`) omadused kehtivad uuritavas vahemikus. Tulemuse juures on siis
märkus „vahemikus [a; b]“.

### Valemi süntaks
`x`, `+ - * / ^`, sulud; kaudne korrutamine `2x`, `3(x+1)`; `sin cos tan/tg cot/ctg arcsin arccos arctan sinh cosh tanh
exp ln lg/log sqrt cbrt`; konstandid `pi`, `e`; kümnendmurd punkti või komaga (`0,5x^2`).

## Kaustad

```
backend/FunktsioonideUurimine.Api/
  Andmed/          DbContext, näidisandmed, migratsioonid
  Matemaatika/     parser, vormindaja, hindaja, nullkohad, piirkonnad, analüsaator
  Mudelid/         olem ja DTO-d
  Otspunktid/      Minimal API otspunktid
  Valideerimine/   FluentValidation validaatorid
backend/FunktsioonideUurimine.Tests/   xUnit testid (analüüs, parser, API)
frontend/src/
  api/             fetch-klient, tüübid, TanStack Query päringud
  komponendid/     graafik (ECharts), omaduste tabel, vorm, KaTeX
  lehed/           avaleht, detailvaade, loomine/muutmine
```
