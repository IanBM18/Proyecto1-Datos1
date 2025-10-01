using System;

public static class CombatResolver
{
    private static Random rng = new Random();

    // Devuelve (perdidasAtacante, perdidasDefensor)
    public static (int atkLoss, int defLoss) Resolve(int atkDice, int defDice)
    {
        int[] a = new int[atkDice];
        int[] d = new int[defDice];

        for (int i = 0; i < atkDice; i++) a[i] = rng.Next(1, 7);
        for (int i = 0; i < defDice; i++) d[i] = rng.Next(1, 7);

        Array.Sort(a); Array.Reverse(a);
        Array.Sort(d); Array.Reverse(d);

        int comps = Math.Min(atkDice, defDice);
        int atkLoss = 0, defLoss = 0;
        for (int i = 0; i < comps; i++)
        {
            if (a[i] > d[i]) defLoss++;
            else atkLoss++;
        }
        return (atkLoss, defLoss);
    }
}