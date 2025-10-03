using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Jugador
{
    public string nombre;
    public Color color;
    public int tropasDisponibles = 0;

    public Jugador() { }
    public Jugador(string nombre, Color color)
    {
        this.nombre = nombre;
        this.color = color;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    public enum FaseJuego { AsignacionInicial, Refuerzo, Ataque, Movimiento }
    public FaseJuego faseActual = FaseJuego.AsignacionInicial;

    [Header("Territorios y Jugadores")]
    public List<Territorio> todosLosTerritorios = new List<Territorio>();
    public List<Jugador> jugadores = new List<Jugador>();

    [Header("UI de Jugadores")]
    public TextMeshProUGUI nombreJugador1Text;
    public TextMeshProUGUI nombreJugador2Text;

    [Header("UI de Estado del Juego")]
    public TextMeshProUGUI textoTurno;
    public TextMeshProUGUI textoFase;
    public TextMeshProUGUI textoRefuerzos;
    public TextMeshProUGUI textoLog;

    // Estado
    private int indiceJugadorActual = 0;
    private int tropasPendientesRefuerzo = 0;
    public int territoriosLibresRestantes = 0;
    public Territorio territorioSeleccionado = null;
    private Territorio territorioOrigenMovimiento = null;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Si no hay jugadores en Inspector, crear dos por defecto
        if (jugadores == null || jugadores.Count == 0)
        {
            jugadores = new List<Jugador> { new Jugador("Jugador 1", Color.red), new Jugador("Jugador 2", Color.blue) };
        }

        // Mostrar nombres en UI si están asignados
        if (nombreJugador1Text != null && jugadores.Count > 0)
        {
            nombreJugador1Text.text = jugadores[0].nombre;
            nombreJugador1Text.color = jugadores[0].color;
        }
        if (nombreJugador2Text != null && jugadores.Count > 1)
        {
            nombreJugador2Text.text = jugadores[1].nombre;
            nombreJugador2Text.color = jugadores[1].color;
        }

        ResetearTerritorios();
        territoriosLibresRestantes = todosLosTerritorios.Count;

        RegistrarAccion("INICIO: Fase de Asignación Inicial.");
        ActualizarUIPrincipal();
    }

    // ---------- Logging ----------
    public void RegistrarAccion(string mensaje)
    {
        if (textoLog != null) textoLog.text += "\n" + mensaje;
        Debug.Log(mensaje);
    }

    // ---------- UI ----------
    private void ActualizarUIPrincipal()
    {
        if (jugadores == null || jugadores.Count == 0) return;
        Jugador jugador = jugadores[indiceJugadorActual];

        if (textoTurno != null)
        {
            textoTurno.text = "Turno: " + jugador.nombre;
            textoTurno.color = jugador.color;
        }
        if (textoFase != null) textoFase.text = "Fase: " + faseActual.ToString();
        if (textoRefuerzos != null) textoRefuerzos.text = "Tropas disponibles: " + jugador.tropasDisponibles;
    }

    // ---------- Turnos ----------
    public void SiguienteTurno()
    {
        // Verificar victoria del jugador que acaba de jugar (opcional)
        if (VerificarVictoria(jugadores[indiceJugadorActual]))
        {
            RegistrarAccion("🎉 ¡" + jugadores[indiceJugadorActual].nombre + " ha ganado!");
            // Aquí podrías pausar el juego, mostrar panel, etc.
            return;
        }

        indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);

        RegistrarAccion("➡️ Cambio de turno. Ahora juega " + jugador.nombre);
        ActualizarUIPrincipal();
    }

    // ---------- Refuerzos ----------
    private void CalcularRefuerzos(Jugador jugador)
    {
        int territorios = TerritoriosControlados(jugador).Count;
        int refuerzosBase = Mathf.Max(3, territorios / 3);
        jugador.tropasDisponibles = refuerzosBase;
        tropasPendientesRefuerzo = refuerzosBase;
        RegistrarAccion("➕ " + jugador.nombre + " recibe " + jugador.tropasDisponibles + " tropas de refuerzo.");
    }

    public List<Territorio> TerritoriosControlados(Jugador jugador)
    {
        List<Territorio> lista = new List<Territorio>();
        foreach (var t in todosLosTerritorios) if (t != null && t.propietario == jugador) lista.Add(t);
        return lista;
    }

    public void ColocarTropas(Territorio territorio, int cantidad)
    {
        Jugador jugadorActual = jugadores[indiceJugadorActual];
        if (territorio.propietario != jugadorActual) { RegistrarAccion("❌ No puedes colocar en ajeno."); return; }
        if (cantidad > jugadorActual.tropasDisponibles) { RegistrarAccion("❌ Tropas insuficientes."); return; }

        territorio.tropas += cantidad;
        jugadorActual.tropasDisponibles -= cantidad;
        territorio.ActualizarUI();
        territorio.MostrarFeedback();
        RegistrarAccion($"{jugadorActual.nombre} coloca {cantidad} tropas en {territorio.name} (restan {jugadorActual.tropasDisponibles}).");
        ActualizarUIPrincipal();
    }

    // ---------- Territorios ----------
    public void ResetearTerritorios()
    {
        if (todosLosTerritorios == null) todosLosTerritorios = new List<Territorio>();
        foreach (Territorio t in todosLosTerritorios)
        {
            if (t == null) continue;
            t.propietario = null;
            t.tropas = 0;
            t.ActualizarUI();
        }
        territoriosLibresRestantes = todosLosTerritorios.Count;
        RegistrarAccion("♻️ Territorios reseteados. Libres: " + territoriosLibresRestantes);
    }

    // ---------- Flujo principal (clics) ----------
    public void ProcesarClicDeTerritorio(Territorio territorioClicado)
    {
        if (territorioClicado == null) return;

        switch (faseActual)
        {
            // ASIGNACIÓN INICIAL
            case FaseJuego.AsignacionInicial:
                if (territorioClicado.propietario == null)
                {
                    Jugador jugadorActual = jugadores[indiceJugadorActual];
                    territorioClicado.AsignarPropietario(jugadorActual);
                    territorioClicado.tropas = 1;
                    territorioClicado.ActualizarUI();

                    territoriosLibresRestantes--;
                    RegistrarAccion($"{jugadorActual.nombre} reclama {territorioClicado.name}. Libres: {territoriosLibresRestantes}");

                    indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;
                    if (territoriosLibresRestantes <= 0) CambiarAFaseRefuerzos();
                }
                else RegistrarAccion("❌ Ese territorio ya está ocupado.");
                break;

            // REFUERZO
            case FaseJuego.Refuerzo:
                Jugador jugadorRefuerzo = jugadores[indiceJugadorActual];
                if (territorioClicado.propietario == jugadorRefuerzo)
                {
                    if (jugadorRefuerzo.tropasDisponibles > 0)
                    {
                        territorioClicado.tropas++;
                        jugadorRefuerzo.tropasDisponibles--;
                        territorioClicado.ActualizarUI();
                        RegistrarAccion($"{jugadorRefuerzo.nombre} coloca 1 tropa en {territorioClicado.name}. Restan {jugadorRefuerzo.tropasDisponibles}");
                        if (jugadorRefuerzo.tropasDisponibles <= 0)
                        {
                            faseActual = FaseJuego.Ataque;
                            RegistrarAccion("⚔️ Fase Ataque comienza.");
                        }
                    }
                    else RegistrarAccion("❌ No tienes tropas para colocar.");
                }
                else RegistrarAccion("❌ Solo puedes reforzar tus territorios.");
                break;

            // ATAQUE
            case FaseJuego.Ataque:
                Jugador atacante = jugadores[indiceJugadorActual];
                if (territorioSeleccionado == null)
                {
                    if (territorioClicado.propietario == atacante && territorioClicado.tropas > 1)
                    {
                        territorioSeleccionado = territorioClicado;
                        RegistrarAccion("Atacante seleccionado: " + territorioClicado.name);
                    }
                    else RegistrarAccion("❌ Selecciona un territorio propio con >1 tropa.");
                }
                else
                {
                    Territorio origen = territorioSeleccionado;
                    Territorio destino = territorioClicado;

                    if (origen.vecinos.Contains(destino))
                    {
                        if (destino.propietario != atacante)
                        {
                            RegistrarAccion($"⚔️ Ataque {origen.name} -> {destino.name}");

                            int atkDice = Mathf.Min(3, origen.tropas - 1);
                            int defDice = Mathf.Min(2, destino.tropas);

                            var (atkLoss, defLoss) = CombatResolver.Resolve(atkDice, defDice);

                            origen.tropas -= atkLoss;
                            destino.tropas -= defLoss;
                            if (origen.tropas < 1) origen.tropas = 1;
                            if (destino.tropas < 0) destino.tropas = 0;

                            origen.ActualizarUI();
                            destino.ActualizarUI();

                            RegistrarAccion($"Resultado: Atk -{atkLoss}, Def -{defLoss}");

                            if (destino.tropas <= 0)
                            {
                                destino.AsignarPropietario(atacante);

                                int mover = atkDice;
                                mover = Mathf.Min(mover, Mathf.Max(1, origen.tropas - 1)); // asegurar mover válido
                                origen.tropas -= mover;
                                destino.tropas = mover;

                                origen.ActualizarUI();
                                destino.ActualizarUI();

                                RegistrarAccion($"🎉 {atacante.nombre} conquistó {destino.name} y movió {mover} tropas.");
                            }
                        }
                        else RegistrarAccion("❌ No puedes atacar tus propios territorios.");
                    }
                    else RegistrarAccion("❌ Destino no es vecino.");

                    territorioSeleccionado = null;
                }
                break;

            // MOVIMIENTO
            case FaseJuego.Movimiento:
                Jugador jugadorMovimiento = jugadores[indiceJugadorActual];

                if (territorioOrigenMovimiento == null)
                {
                    if (territorioClicado.propietario == jugadorMovimiento && territorioClicado.tropas > 1)
                    {
                        territorioOrigenMovimiento = territorioClicado;
                        RegistrarAccion("Origen movimiento: " + territorioClicado.name);
                    }
                    else RegistrarAccion("❌ Selecciona un territorio propio con >1 tropa.");
                }
                else
                {
                    Territorio origen = territorioOrigenMovimiento;
                    Territorio destino = territorioClicado;

                    if (origen.vecinos.Contains(destino) && destino.propietario == jugadorMovimiento)
                    {
                        int mover = Mathf.Max(1, origen.tropas - 1); // mueve todas menos 1
                        origen.tropas -= mover;
                        destino.tropas += mover;

                        origen.ActualizarUI();
                        destino.ActualizarUI();

                        RegistrarAccion($"🚛 Movidas {mover} tropas de {origen.name} a {destino.name}");

                        territorioOrigenMovimiento = null;
                        FinalizarMovimiento();
                    }
                    else
                    {
                        RegistrarAccion("❌ Destino debe ser tuyo y vecino del origen.");
                        territorioOrigenMovimiento = null;
                    }
                }
                break;
        }

        ActualizarUIPrincipal();
    }

    // ---------- Cambios de fase ----------
    void CambiarAFaseRefuerzos()
    {
        faseActual = FaseJuego.Refuerzo;
        indiceJugadorActual = 0;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);
        RegistrarAccion("🔄 Fase Refuerzos. Turno: " + jugador.nombre);
        ActualizarUIPrincipal();
    }

    public void FinalizarAtaque()
    {
        faseActual = FaseJuego.Movimiento;
        RegistrarAccion("➡️ Fin de Ataque. Ahora Movimiento.");
        ActualizarUIPrincipal();
    }

    public void FinalizarMovimiento()
    {
        RegistrarAccion("✅ " + jugadores[indiceJugadorActual].nombre + " finalizó movimiento.");
        SiguienteTurno();
    }

    // ---------- Victoria ----------
    private bool VerificarVictoria(Jugador jugador)
    {
        foreach (var t in todosLosTerritorios)
        {
            if (t == null) continue;
            if (t.propietario != jugador) return false;
        }
        return true;
    }
}
