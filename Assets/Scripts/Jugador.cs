using UnityEngine;

[System.Serializable]
public class Jugador
{
    public string nombre;
    public Color color;
    public int tropasDisponibles = 0;

    // Constructor vacío (para poder hacer "new Jugador()")
    public Jugador() {}

    // Constructor con parámetros (si quieres crear directo)
    public Jugador(string nombre, Color color)
    {
        this.nombre = nombre;
        this.color = color;
        this.tropasDisponibles = 0;
    }
}