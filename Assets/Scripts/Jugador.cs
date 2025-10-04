using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Jugador
{
    public string Nombre { get; private set; }
    public Color Color { get; private set; }
    public int TropasDisponibles { get; set; }
    public List<Tarjeta> Tarjetas { get; } = new List<Tarjeta>();

    public Jugador() { }

    public Jugador(string nombre, Color color)
    {
        Nombre = nombre;
        Color = color;
        TropasDisponibles = 0;
    }

    // ¡NOMBRE DE FUNCIÓN CAMBIADO!
    public bool PuedeIntercambiar() => Tarjetas.Count >= 3;

    // ¡NOMBRE DE FUNCIÓN CAMBIADO!
    public List<Tarjeta> ObtenerUsadas()
    {
        if (Tarjetas.Count < 3) return new List<Tarjeta>();
        var usadas = Tarjetas.GetRange(0, 3);
        Tarjetas.RemoveRange(0, 3);
        return usadas;
    }

    public int IntercambiarTarjetas()
    {
        // USO CORREGIDO
        if (!this.PuedeIntercambiar()) return 0;

        // USO CORREGIDO
        var usadas = this.ObtenerUsadas();
        int exchangeNumber = GameManager.instancia.NumeroIntercambios;
        int refuerzos = FibonacciExchange.GetExchangeValue(exchangeNumber);

        GameManager.instancia.NumeroIntercambios++;
        TropasDisponibles += refuerzos;

        GameManager.instancia.RegistrarAccion($"{Nombre} intercambió 3 tarjetas y recibe {refuerzos} tropas.");
        return refuerzos;
    }
}
