using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    public enum FaseJuego { AsignacionInicial, Refuerzo, Ataque, Movimiento }
    public FaseJuego faseActual = FaseJuego.AsignacionInicial;

    [Header("Territorios y Jugadores")]
    public List<Territorio> todosLosTerritorios;
    public List<Jugador> jugadores = new List<Jugador>();

    [Header("UI de Jugadores")]
    public TextMeshProUGUI nombreJugador1Text;
    public TextMeshProUGUI nombreJugador2Text;

    [Header("UI de Estado del Juego")]
    public TextMeshProUGUI textoTurno;
    public TextMeshProUGUI textoFase;
    public TextMeshProUGUI textoRefuerzos;
    public TextMeshProUGUI textoLog;

    public int territoriosLibresRestantes;
    public int indiceJugadorActual = 0;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Resetear territorios y UI
        ResetearTerritorios();

        // Crear jugadores de prueba
        Jugador j1 = new Jugador("Jugador 1", Color.red);
        Jugador j2 = new Jugador("Jugador 2", Color.blue);
        jugadores.Add(j1);
        jugadores.Add(j2);

        // Actualizar UI de nombres de jugador
        nombreJugador1Text.text = j1.nombre;
        nombreJugador1Text.color = j1.color;
        nombreJugador2Text.text = j2.nombre;
        nombreJugador2Text.color = j2.color;

        ActualizarUIPrincipal();

        RegistrarAccion("INICIO: Fase de Asignación Inicial.");
    }

    // ================== LOG DE ACCIONES ==================
    public void RegistrarAccion(string mensaje)
    {
        if (textoLog != null)
            textoLog.text += "\n" + mensaje;
        Debug.Log(mensaje);
    }

    void ActualizarUIPrincipal()
    {
        Jugador jugador = jugadores[indiceJugadorActual];
        if (textoTurno != null)
        {
            textoTurno.text = "Turno de: " + jugador.nombre;
            textoTurno.color = jugador.color;
        }
        if (textoFase != null)
            textoFase.text = "Fase actual: " + faseActual.ToString();
        if (textoRefuerzos != null)
            textoRefuerzos.text = "Tropas disponibles: " + jugador.tropasDisponibles;
    }

    // ================== CLIC EN TERRITORIOS ==================
    public void ProcesarClicDeTerritorio(Territorio t)
    {
        switch (faseActual)
        {
            case FaseJuego.AsignacionInicial:
                if (t.propietario == null)
                {
                    Jugador jugadorActual = jugadores[indiceJugadorActual];
                    t.AsignarPropietario(jugadorActual);
                    territoriosLibresRestantes--;

                    RegistrarAccion(jugadorActual.nombre + " reclama " + t.name);

                    // Animación simple
                    t.MostrarFeedback();

                    // Cambiar turno
                    indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;

                    // Si no quedan territorios libres, cambiar a fase Refuerzo
                    if (territoriosLibresRestantes <= 0)
                        CambiarAFaseRefuerzos();
                }
                else
                    RegistrarAccion("❌ Ese territorio ya está ocupado.");
                break;

            case FaseJuego.Refuerzo:
                if (t.propietario == jugadores[indiceJugadorActual])
                {
                    ColocarTropas(t, 1);
                }
                else
                {
                    RegistrarAccion("❌ Solo puedes reforzar tus territorios.");
                }
                break;

            case FaseJuego.Ataque:
                RegistrarAccion("⚔️ (Fase Ataque aún no implementada).");
                break;

            case FaseJuego.Movimiento:
                RegistrarAccion("🚛 (Fase Movimiento aún no implementada).");
                break;
        }

        ActualizarUIPrincipal();
    }

    // ================== FASES ==================
    void CambiarAFaseRefuerzos()
    {
        faseActual = FaseJuego.Refuerzo;
        indiceJugadorActual = 0;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);

        RegistrarAccion("🔄 Empieza la fase de Refuerzos. Turno de " + jugador.nombre);
        ActualizarUIPrincipal();
    }

    public void SiguienteTurno()
    {
        indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);

        RegistrarAccion("➡️ Cambio de turno. Ahora juega " + jugador.nombre);
        ActualizarUIPrincipal();
    }

    // ================== TERRITORIOS ==================
    public void ResetearTerritorios()
    {
        foreach (Territorio t in todosLosTerritorios)
        {
            t.propietario = null;
            t.tropas = 0;
            t.ActualizarUI();
        }

        territoriosLibresRestantes = todosLosTerritorios.Count;
        RegistrarAccion("♻️ Territorios reseteados. Total libres: " + territoriosLibresRestantes);
    }

    public void CalcularRefuerzos(Jugador jugador)
    {
        int refuerzosBase = Mathf.Max(3, TerritoriosControlados(jugador).Count / 3);
        jugador.tropasDisponibles = refuerzosBase;

        RegistrarAccion("➕ " + jugador.nombre + " recibe " + jugador.tropasDisponibles + " tropas de refuerzo.");
    }

    public List<Territorio> TerritoriosControlados(Jugador jugador)
    {
        List<Territorio> controlados = new List<Territorio>();
        foreach (Territorio t in todosLosTerritorios)
        {
            if (t.propietario == jugador)
                controlados.Add(t);
        }
        return controlados;
    }

    // ================== REFUERZOS ==================
    public void ColocarTropas(Territorio territorio, int cantidad)
    {
        Jugador jugadorActual = jugadores[indiceJugadorActual];

        if (territorio.propietario != jugadorActual)
        {
            RegistrarAccion("❌ No puedes colocar tropas en territorios ajenos.");
            return;
        }

        if (cantidad > jugadorActual.tropasDisponibles)
        {
            RegistrarAccion("❌ Tropas insuficientes.");
            return;
        }

        territorio.tropas += cantidad;
        jugadorActual.tropasDisponibles -= cantidad;

        territorio.ActualizarUI();
        territorio.MostrarFeedback();

        RegistrarAccion(jugadorActual.nombre + " coloca " + cantidad + " tropas en " + territorio.name + 
                        " (restan " + jugadorActual.tropasDisponibles + ").");

        ActualizarUIPrincipal();
    }
}