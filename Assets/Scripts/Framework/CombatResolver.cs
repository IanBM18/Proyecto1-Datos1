using System;
using UnityEngine;

public static class CombatResolver
{
    private static System.Random rng = new System.Random();

    public static (int atkLoss, int defLoss) Resolve(int atkDice, int defDice)
    {
        // Asegurar rangos válidos
        atkDice = Mathf.Clamp(atkDice, 1, 3);
        defDice = Mathf.Clamp(defDice, 1, 2);

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

        Debug.Log($"🎲 Ataque [{string.Join(",", a)}] vs Defensa [{string.Join(",", d)}] -> Atk -{atkLoss}, Def -{defLoss}");
        return (atkLoss, defLoss);
    }
}
