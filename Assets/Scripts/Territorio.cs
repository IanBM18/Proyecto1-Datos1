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
    public int tropas = 1;

    [Header("Vecinos")]
    public List<Territorio> vecinos = new List<Territorio>();

    public void AsignarPropietario(Jugador jugador)
    {
        propietario = jugador;
        tropas = Mathf.Max(1, tropas); // al asignar asegurar al menos 1
        ActualizarUI();
        MostrarFeedback();
    }

    public void ActualizarUI()
    {
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
        if (bordeSeleccionado != null) bordeSeleccionado.SetActive(activo);
    }

    public void OnClickTerritorio()
    {
        Debug.Log("Clic en territorio: " + name);
        if (GameManager.instancia != null) GameManager.instancia.ProcesarClicDeTerritorio(this);
    }

    public void MostrarFeedback()
    {
        // Si no tienes LeanTween, comenta la línea o implementa otra animación.
        #if LEANTWEEN_PRESENT
        LeanTween.scale(gameObject, Vector3.one * 1.08f, 0.18f).setEasePunch();
        #endif
    }
}
