using UnityEngine;

[System.Serializable]
public class Tarjeta
{
    public string territorioAsociado; // nombre del territorio
    public string tipo; // ejemplo: "Infantería", "Caballería", "Artillería"

    public Tarjeta(string territorio, string tipo)
    {
        territorioAsociado = territorio;
        this.tipo = tipo;
    }
}
