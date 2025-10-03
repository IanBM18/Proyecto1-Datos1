using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Jugador
{
    public string nombre;                 // Nombre del jugador
    public Color color;                   // Color asociado al jugador
    public int tropasDisponibles = 0;     // Tropas que puede colocar en su turno

    public List<Tarjeta> tarjetas = new List<Tarjeta>(); // Tarjetas que posee el jugador

    public Jugador() { } // Constructor vacío

    public Jugador(string nombre, Color color)
    {
        this.nombre = nombre;
        this.color = color;
        this.tropasDisponibles = 0;
    }

    // Verifica si tiene al menos 3 tarjetas para intercambio
    public bool TieneTrioDeTarjetas()
    {
        return tarjetas.Count >= 3;
    }

    // Usar las 3 primeras tarjetas y eliminarlas de la lista
    public List<Tarjeta> UsarTrio()
    {
        List<Tarjeta> usadas = tarjetas.GetRange(0, 3);
        tarjetas.RemoveRange(0, 3);
        return usadas;
    }

    // Intercambia tarjetas y devuelve refuerzos usando Fibonacci
    public int IntercambiarTarjetas()
    {
        if (!TieneTrioDeTarjetas()) return 0; // Si no hay trío, no hace nada

        var usadas = UsarTrio(); // Remueve las 3 primeras tarjetas

        // Obtener refuerzos según el número de intercambios consecutivos
        int exchangeNumber = GameManager.instancia.NumeroIntercambios;
        int refuerzos = FibonacciExchange.GetExchangeValue(exchangeNumber);

        GameManager.instancia.NumeroIntercambios++; // Incrementa contador global
        tropasDisponibles += refuerzos;             // Suma refuerzos al jugador

        GameManager.instancia.RegistrarAccion($"{nombre} intercambió 3 tarjetas y recibe {refuerzos} tropas.");
        return refuerzos;
    }
}
