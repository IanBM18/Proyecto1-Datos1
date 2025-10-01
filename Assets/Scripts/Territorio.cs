using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Territorio : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoNombreJugador;
    public TextMeshProUGUI textoTropas;
    public Image fondoBoton;
    public GameObject bordeSeleccionado;

    [Header("Datos")]
    public Jugador propietario;
    public int tropas = 0;

    [Header("Vecinos")]
    public List<Territorio> vecinos;

    // ================== LÓGICA ==================
    public void AsignarPropietario(Jugador nuevoPropietario)
    {
        propietario = nuevoPropietario;
        tropas = 1; // Al reclamar
        ActualizarUI();
        MostrarFeedback();
    }

    public void ActualizarUI()
    {
        // Evitar errores si los elementos no están asignados
        if (textoNombreJugador != null)
        {
            textoNombreJugador.text = propietario != null ? propietario.nombre : "Libre";
            textoNombreJugador.color = propietario != null ? propietario.color : Color.black;
        }

        if (fondoBoton != null)
            fondoBoton.color = propietario != null ? propietario.color : Color.white;

        if (textoTropas != null)
            textoTropas.text = tropas.ToString();
    }

    public void Seleccionar(bool activo)
    {
        if (bordeSeleccionado != null)
            bordeSeleccionado.SetActive(activo);
    }

    public void OnClickTerritorio()
    {
        Debug.Log("Clic detectado en territorio: " + gameObject.name);
        if (GameManager.instancia != null)
            GameManager.instancia.ProcesarClicDeTerritorio(this);
    }

    public void MostrarFeedback()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.1f, 0.2f).setEasePunch();
    }
}