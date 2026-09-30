using System.Numerics;
using MathNet.Numerics;
using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>Väikesed abimeetodid MathNet.Symbolics avaldispuuga töötamiseks.</summary>
public static class Avaldised
{
    public static readonly Expression X = Expression.Symbol("x");

    public static bool SisaldabX(Expression e) => e switch
    {
        Expression.Identifier => true,
        Expression.Sum s => s.Item.Any(SisaldabX),
        Expression.Product p => p.Item.Any(SisaldabX),
        Expression.Power p => SisaldabX(p.Item1) || SisaldabX(p.Item2),
        Expression.Function f => SisaldabX(f.Item2),
        Expression.FunctionN f => f.Item2.Any(SisaldabX),
        _ => false
    };

    public static bool OnRatsionaalarv(Expression e, out BigRational arv)
    {
        if (e is Expression.Number n)
        {
            arv = n.Item;
            return true;
        }
        arv = BigRational.Zero;
        return false;
    }

    public static bool OnPositiivne(BigRational q) => q.Numerator.Sign > 0;
    public static bool OnNegatiivne(BigRational q) => q.Numerator.Sign < 0;
    public static bool OnTaisarv(BigRational q) => q.Denominator.IsOne;

    public static double Kahendarv(BigRational q) => (double)q.Numerator / (double)q.Denominator;

    public static Expression Ratsionaal(BigInteger lugeja, BigInteger nimetaja) =>
        SymbolicExpression.IntegerFraction(lugeja, nimetaja).Expression;

    public static Expression Taisarv(int n) => SymbolicExpression.Int32(n).Expression;

    public static SymbolicExpression Sym(Expression e) => new(e);

    /// <summary>
    /// Kas avaldis on x-i ratsionaalfunktsioon (ainult +, ·, täisarvulised astmed)?
    /// MathNet'i RationalSimplify võib muude avaldiste peal (nt ((x-1)/(x+2))^(3/2)) lõputult töötada.
    /// </summary>
    public static bool OnRatsionaalfunktsioon(Expression e) => e switch
    {
        Expression.Number or Expression.Identifier or Expression.Constant => true,
        Expression.Sum s => s.Item.All(OnRatsionaalfunktsioon),
        Expression.Product p => p.Item.All(OnRatsionaalfunktsioon),
        Expression.Power { Item2: Expression.Number n } p => OnTaisarv(n.Item) && OnRatsionaalfunktsioon(p.Item1),
        _ => !SisaldabX(e)
    };

    /// <summary>MathNet ei lihtsusta tuletist alati kõige loetavamale kujule – valime lühima variandi.</summary>
    public static Expression Lihtsusta(Expression e)
    {
        var sym = Sym(e);
        var kandidaadid = new List<Expression> { e };
        TryLisa(kandidaadid, () => sym.Expand().Expression);
        if (OnRatsionaalfunktsioon(e))
        {
            var x = Sym(X);
            TryLisa(kandidaadid, () => sym.RationalSimplify(x).Expression);
            // 4x/(x^4 + 2x^2 + 1) → 4x/(x^2 + 1)^2
            TryLisa(kandidaadid, () =>
            {
                var r = sym.RationalSimplify(x);
                return (r.Numerator().FactorSquareFree(x) / r.Denominator().FactorSquareFree(x)).Expression;
            });
        }
        return kandidaadid.MinBy(k => ValemiVormindaja.Tekst(k).Length)!;

        static void TryLisa(List<Expression> list, Func<Expression> tee)
        {
            try { list.Add(tee()); }
            catch (Exception) { /* teisendus ei sobi sellele avaldisele */ }
        }
    }

    public static Expression Tuletis(Expression e) => Lihtsusta(Sym(e).Differentiate(Sym(X)).Expression);
}
