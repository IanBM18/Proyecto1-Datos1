using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Territorio : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoNombreJugador; // Muestra el nombre del jugador que controla el territorio
    public TextMeshProUGUI textoTropas;        // Muestra la cantidad de tropas en el territorio
    public Image fondoBoton;                   // Fondo del botón para colorear según propietario
    public GameObject bordeSeleccionado;       // Borde que indica selección de territorio

    [Header("Datos")]
    public Jugador propietario;               // Jugador dueño del territorio
    public int tropas = 1;                     // Tropas actuales en el territorio

    [Header("Vecinos")]
    public List<Territorio> vecinos = new List<Territorio>(); // Territorios adyacentes

    // Asigna un propietario al territorio y actualiza UI
    public void AsignarPropietario(Jugador jugador)
    {
        propietario = jugador;  
        tropas = Mathf.Max(1, tropas); // Asegura al menos 1 tropa al asignar
        ActualizarUI();                // Refresca la interfaz
        MostrarFeedback();             // Animación visual
    }

    // Actualiza la UI del territorio (nombre, color, tropas)
    public void ActualizarUI()
    {
        if (textoNombreJugador != null)
        {
            textoNombreJugador.text = propietario != null ? propietario.Nombre : "Libre";
            textoNombreJugador.color = propietario != null ? propietario.Color : Color.black;
        }

        if (fondoBoton != null)
            fondoBoton.color = propietario != null ? propietario.Color : Color.white;

        if (textoTropas != null)
            textoTropas.text = tropas.ToString();
    }

    // Activa o desactiva el borde de selección
    public void Seleccionar(bool activo)
    {
        if (bordeSeleccionado != null) bordeSeleccionado.SetActive(activo);
    }

    // Método que se llama al hacer clic en el territorio (botón)
    public void OnClickTerritorio()
    {
        Debug.Log("Clic en territorio: " + name);
        if (GameManager.instancia != null)
            GameManager.instancia.ProcesarClicDeTerritorio(this);
    }

    // Animación visual opcional al interactuar
    public void MostrarFeedback()
    {
        // Si no tienes LeanTween, comenta la línea o implementa otra animación.
        #if LEANTWEEN_PRESENT
        LeanTween.scale(gameObject, Vector3.one * 1.08f, 0.18f).setEasePunch();
        #endif
    }

    // Devuelve lista de territorios vecinos que pueden ser atacados
    public List<Territorio> ObtenerVecinosAtacables()
    {
        List<Territorio> atacables = new List<Territorio>();
        foreach (var vecino in vecinos)
        {
            // Solo los que no son del mismo propietario
            if (vecino.propietario != propietario) 
                atacables.Add(vecino);
        }
        return atacables;
    }
}
