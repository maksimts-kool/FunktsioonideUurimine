using MathNet.Symbolics;

namespace FunktsioonideUurimine.Api.Matemaatika;

/// <summary>
/// Kompileerib avaldise funktsiooniks x ↦ f(x). MathNet.Symbolics'i enda Compile kasutab Math.Pow-d,
/// mis annab ∛(-8) korral NaN – koolimatemaatikas on paaritu astme juur defineeritud kõigil reaalarvudel.
/// Määramata väärtuse korral tagastatakse double.NaN.
/// </summary>
public static class Hindaja
{
    public static Func<double, double> Kompileeri(Expression e)
    {
        var f = K(e);
        return x =>
        {
            var y = f(x);
            return double.IsFinite(y) ? y : double.NaN;
        };
    }

    private static Func<double, double> K(Expression e)
    {
        switch (e)
        {
            case Expression.Number n:
                var arv = Avaldised.Kahendarv(n.Item);
                return _ => arv;
            case Expression.Approximation { Item: Approximation.Real r }:
                var reaal = r.Item;
                return _ => reaal;
            case Expression.Identifier:
                return x => x;
            case Expression.Constant c:
                var konstant = c.Item.IsPi ? Math.PI : c.Item.IsE ? Math.E : double.NaN;
                return _ => konstant;
            case Expression.Sum s:
            {
                var osad = s.Item.Select(K).ToArray();
                return x =>
                {
                    var summa = 0.0;
                    foreach (var o in osad) summa += o(x);
                    return summa;
                };
            }
            case Expression.Product p:
            {
                var osad = p.Item.Select(K).ToArray();
                return x =>
                {
                    var korrutis = 1.0;
                    foreach (var o in osad) korrutis *= o(x);
                    return korrutis;
                };
            }
            case Expression.Power p:
                return Aste(p.Item1, p.Item2);
            case Expression.Function f:
                return Funktsioon(f.Item1, K(f.Item2));
            default:
                return _ => double.NaN;
        }
    }

    private static Func<double, double> Aste(Expression alus, Expression astendaja)
    {
        var b = K(alus);
        if (Avaldised.OnRatsionaalarv(astendaja, out var q))
        {
            var eksponent = Avaldised.Kahendarv(q);
            if (Avaldised.OnTaisarv(q))
            {
                var n = (int)q.Numerator;
                return x => Math.Pow(b(x), n);
            }
            if (!q.Denominator.IsEven)
            {
                // paaritu juur: ∛(-8) = -2, (-8)^(2/3) = 4
                var lugejaPaaritu = !q.Numerator.IsEven;
                return x =>
                {
                    var v = b(x);
                    var r = Math.Pow(Math.Abs(v), eksponent);
                    return v < 0 && lugejaPaaritu ? -r : r;
                };
            }
            return x => Math.Pow(b(x), eksponent);
        }
        var a = K(astendaja);
        return x => Math.Pow(b(x), a(x));
    }

    private static Func<double, double> Funktsioon(Function f, Func<double, double> a)
    {
        if (f.IsSin) return x => Math.Sin(a(x));
        if (f.IsCos) return x => Math.Cos(a(x));
        if (f.IsTan) return x => { var u = a(x); return Math.Abs(Math.Cos(u)) < 1e-15 ? double.NaN : Math.Tan(u); };
        if (f.IsCot) return x => { var u = a(x); var s = Math.Sin(u); return Math.Abs(s) < 1e-15 ? double.NaN : Math.Cos(u) / s; };
        if (f.IsSec) return x => 1 / Math.Cos(a(x));
        if (f.IsCsc) return x => 1 / Math.Sin(a(x));
        if (f.IsAsin) return x => Math.Asin(a(x));
        if (f.IsAcos) return x => Math.Acos(a(x));
        if (f.IsAtan) return x => Math.Atan(a(x));
        if (f.IsAcot) return x => Math.PI / 2 - Math.Atan(a(x));
        if (f.IsSinh) return x => Math.Sinh(a(x));
        if (f.IsCosh) return x => Math.Cosh(a(x));
        if (f.IsTanh) return x => Math.Tanh(a(x));
        if (f.IsExp) return x => Math.Exp(a(x));
        if (f.IsLn) return x => { var u = a(x); return u > 0 ? Math.Log(u) : double.NaN; };
        if (f.IsLg) return x => { var u = a(x); return u > 0 ? Math.Log10(u) : double.NaN; };
        if (f.IsAbs) return x => Math.Abs(a(x));
        return _ => double.NaN;
    }
}
