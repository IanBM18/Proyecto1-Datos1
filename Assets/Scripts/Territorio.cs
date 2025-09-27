using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // You'll need this for the List of neighbors

public class Territorio : MonoBehaviour
{
    // Connect these variables to your button in the Unity Inspector
    public TextMeshProUGUI textoNombreJugador;
    public Image fondoBoton;
    
    // This is where we'll store the owner's Jugador object
    public Jugador propietario;

    // This list will hold all the neighboring territories
    public List<Territorio> vecinos;
    
    // This method will be called from the GameManager to assign an owner
    public void AsignarPropietario(Jugador nuevoPropietario)
    {
        // Now you can assign the whole Jugador object at once
        propietario = nuevoPropietario;
        textoNombreJugador.text = nuevoPropietario.nombre;
        fondoBoton.color = nuevoPropietario.color;
    }

    // This function is activated when the button is clicked
    public void OnClickTerritorio()
    {
        // Llama al GameManager para procesar la lógica de clic para este territorio
        GameManager.instancia.ProcesarClicDeTerritorio(this);
    }
}