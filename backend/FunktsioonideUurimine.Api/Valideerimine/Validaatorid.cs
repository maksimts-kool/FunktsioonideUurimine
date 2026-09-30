using FluentValidation;
using FunktsioonideUurimine.Api.Matemaatika;
using FunktsioonideUurimine.Api.Mudelid;

namespace FunktsioonideUurimine.Api.Valideerimine;

public sealed class FunktsiooniPaeringuValidaator : AbstractValidator<FunktsiooniPaering>
{
    public FunktsiooniPaeringuValidaator(FunktsiooniAnaluusija analuusija)
    {
        RuleFor(p => p.Valem).Valem(analuusija);
        RuleFor(p => p.VahemikAlgus)
            .InclusiveBetween(Piirid.Min, Piirid.Max).WithMessage(Piirid.Teade)
            .LessThan(p => p.VahemikLopp).WithMessage("Vahemiku algus peab olema väiksem kui lõpp.");
        RuleFor(p => p.VahemikLopp)
            .InclusiveBetween(Piirid.Min, Piirid.Max).WithMessage(Piirid.Teade)
            .Must((p, lopp) => lopp - p.VahemikAlgus >= Piirid.MinPikkus)
            .WithMessage($"Vahemik peab olema vähemalt {Piirid.MinPikkus} pikkune.");
    }
}

public sealed class GraafikuPaeringuValidaator : AbstractValidator<GraafikuPaering>
{
    public GraafikuPaeringuValidaator(FunktsiooniAnaluusija analuusija)
    {
        RuleFor(p => p.Valem).Valem(analuusija);
        RuleFor(p => p.Algus)
            .InclusiveBetween(Piirid.Min, Piirid.Max).WithMessage(Piirid.Teade)
            .LessThan(p => p.Lopp).WithMessage("Vahemiku algus peab olema väiksem kui lõpp.");
        RuleFor(p => p.Lopp)
            .InclusiveBetween(Piirid.Min, Piirid.Max).WithMessage(Piirid.Teade)
            .Must((p, lopp) => lopp - p.Algus >= Piirid.MinPikkus)
            .WithMessage($"Vahemik peab olema vähemalt {Piirid.MinPikkus} pikkune.");
        RuleFor(p => p.Samm)
            .GreaterThanOrEqualTo(0.0001).WithMessage("Samm peab olema vähemalt 0.0001.")
            .Must((p, samm) => (p.Lopp - p.Algus) / samm <= Piirid.MaxPunkte)
            .WithMessage($"Liiga väike samm: graafikul võib olla kuni {Piirid.MaxPunkte} punkti.");
    }
}

internal static class Piirid
{
    public const double Min = -1000;
    public const double Max = 1000;
    public const double MinPikkus = 0.01;
    public const int MaxPunkte = 20000;
    public const string Teade = "Vahemiku otspunktid peavad olema lõigus [-1000; 1000].";

    public static IRuleBuilderOptionsConditions<T, string> Valem<T>(this IRuleBuilderInitial<T, string> reegel,
        FunktsiooniAnaluusija analuusija) =>
        reegel.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Sisesta funktsiooni valem.")
            .MaximumLength(200).WithMessage("Valem võib olla kuni 200 märki pikk.")
            .Custom((valem, kontekst) =>
            {
                if (analuusija.Kontrolli(valem) is { } viga) kontekst.AddFailure(viga);
            });
}
