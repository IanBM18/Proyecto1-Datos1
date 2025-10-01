using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Territorio : MonoBehaviour
{
    // 👇 Asigna estos en el Inspector de Unity
    public TextMeshProUGUI textoNombreJugador;  // para mostrar el dueño
    public TextMeshProUGUI textoTropas;         // para mostrar tropas (opcional)
    public Image fondoBoton;                    // color del botón según jugador

    // Propiedades del territorio
    public Jugador propietario;
    public int tropas = 1; // cada territorio empieza con 1 tropa por defecto

    // Vecinos (se asignan en el Inspector)
    public Territorio[] vecinos;

    // =====================================================
    // Asigna un jugador como dueño del territorio
    // =====================================================
    public void AsignarPropietario(Jugador jugador)
    {
        propietario = jugador;
        ActualizarUI();
    }

    // =====================================================
    // Actualiza UI (nombre, color y tropas)
    // =====================================================
    public void ActualizarUI()
    {
        if (textoNombreJugador != null) 
            textoNombreJugador.text = propietario != null ? propietario.nombre : "Neutral";

        if (fondoBoton != null) 
            fondoBoton.color = propietario != null ? propietario.color : Color.gray;

        if (textoTropas != null) 
            textoTropas.text = tropas.ToString();
    }

    // =====================================================
    // Cuando el jugador hace clic en el territorio
    // =====================================================
    public void OnClickTerritorio()
    {
        GameManager.instancia.ProcesarClicDeTerritorio(this);
    }
}