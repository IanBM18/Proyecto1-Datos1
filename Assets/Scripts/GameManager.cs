using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Jugador
{
    public string nombre;
    public Color color;
    public int tropasDisponibles = 0;

    public List<Tarjeta> tarjetas = new List<Tarjeta>();

    public Jugador() { }

    public Jugador(string nombre, Color color)
    {
        this.nombre = nombre;
        this.color = color;
        this.tropasDisponibles = 0;
    }

    public bool TieneTrioDeTarjetas() => tarjetas.Count >= 3;

    public List<Tarjeta> UsarTrio()
    {
        List<Tarjeta> usadas = tarjetas.GetRange(0, 3);
        tarjetas.RemoveRange(0, 3);
        return usadas;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;
    public int NumeroIntercambios = 0;
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

    private int indiceJugadorActual = 0;           // Jugador activo
    private int tropasPendientesRefuerzo = 0;     // Tropas pendientes en refuerzo
    public int territoriosLibresRestantes = 0;
    public Territorio territorioSeleccionado = null;
    private Territorio territorioOrigenMovimiento = null;

    private TcpClientSimple cliente;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Crear jugadores por defecto si no hay en Inspector
        if (jugadores == null || jugadores.Count == 0)
        {
            jugadores = new List<Jugador>
            {
                new Jugador("Jugador 1", Color.red),
                new Jugador("Jugador 2", Color.blue)
            };
        }

        // Mostrar nombres y colores en UI
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

        // Conectar cliente TCP
        cliente = new TcpClientSimple();
        cliente.Connect("127.0.0.1", 7777);
        RegistrarAccion("📡 Cliente TCP conectado al servidor.");
    }

    // ---------- Logging ----------
    public void RegistrarAccion(string mensaje)
    {
        if (textoLog != null) textoLog.text += "\n" + mensaje;
        Debug.Log(mensaje);
    }

    public void EnviarAccion(string accion)
    {
        string json = $"{{\"jugador\":\"{jugadores[indiceJugadorActual].nombre}\", \"accion\":\"{accion}\"}}";
        cliente.Send(json);
        Debug.Log("📤 Mensaje enviado: " + json);
    }

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
        if (VerificarVictoria(jugadores[indiceJugadorActual]))
        {
            RegistrarAccion("🎉 ¡" + jugadores[indiceJugadorActual].nombre + " ha ganado!");
            EnviarAccion("¡" + jugadores[indiceJugadorActual].nombre + " ha ganado!");
            return;
        }

        indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);

        RegistrarAccion("➡️ Cambio de turno. Ahora juega " + jugador.nombre);
        EnviarAccion("Turno de " + jugador.nombre);
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
        EnviarAccion(jugador.nombre + " recibe " + jugador.tropasDisponibles + " tropas de refuerzo.");
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
        if (territorio.propietario != jugadorActual)
        {
            RegistrarAccion("❌ No puedes colocar en ajeno.");
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

        RegistrarAccion($"{jugadorActual.nombre} coloca {cantidad} tropas en {territorio.name} (restan {jugadorActual.tropasDisponibles}).");
        EnviarAccion($"{jugadorActual.nombre} coloca {cantidad} tropas en {territorio.name} (restan {jugadorActual.tropasDisponibles}).");

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
        EnviarAccion("Territorios reseteados. Libres: " + territoriosLibresRestantes);
    }

    // ---------- Procesar clic de territorio ----------
    public void ProcesarClicDeTerritorio(Territorio territorioClicado)
    {
        if (territorioClicado == null) return;

        switch (faseActual)
        {
            case FaseJuego.AsignacionInicial:
                FaseAsignacionInicial(territorioClicado);
                break;

            case FaseJuego.Refuerzo:
                FaseRefuerzo(territorioClicado);
                break;

            case FaseJuego.Ataque:
                FaseAtaque(territorioClicado);
                break;

            case FaseJuego.Movimiento:
                FaseMovimiento(territorioClicado);
                break;
        }

        ActualizarUIPrincipal();
    }

    // ---------- Fases específicas ----------
    private void FaseAsignacionInicial(Territorio territorio)
    {
        if (territorio.propietario == null)
        {
            Jugador jugadorActual = jugadores[indiceJugadorActual];
            territorio.AsignarPropietario(jugadorActual);
            territorio.tropas = 1;
            territoriosLibresRestantes--;

            RegistrarAccion($"{jugadorActual.nombre} reclama {territorio.name}. Libres: {territoriosLibresRestantes}");
            EnviarAccion($"{jugadorActual.nombre} reclama {territorio.name}");

            indiceJugadorActual = (indiceJugadorActual + 1) % jugadores.Count;
            if (territoriosLibresRestantes <= 0) CambiarAFaseRefuerzos();
        }
        else RegistrarAccion("❌ Ese territorio ya está ocupado.");
    }

    private void FaseRefuerzo(Territorio territorio)
    {
        Jugador jugadorRefuerzo = jugadores[indiceJugadorActual];
        if (territorio.propietario == jugadorRefuerzo)
        {
            if (jugadorRefuerzo.tropasDisponibles > 0)
            {
                territorio.tropas++;
                jugadorRefuerzo.tropasDisponibles--;
                territorio.ActualizarUI();
                RegistrarAccion($"{jugadorRefuerzo.nombre} coloca 1 tropa en {territorio.name}. Restan {jugadorRefuerzo.tropasDisponibles}");
                EnviarAccion($"{jugadorRefuerzo.nombre} coloca 1 tropa en {territorio.name}. Restan {jugadorRefuerzo.tropasDisponibles}");

                if (jugadorRefuerzo.tropasDisponibles <= 0)
                {
                    faseActual = FaseJuego.Ataque;
                    RegistrarAccion("⚔️ Fase Ataque comienza.");
                    EnviarAccion("Fase Ataque comienza");
                }
            }
            else RegistrarAccion("❌ No tienes tropas para colocar.");
        }
        else RegistrarAccion("❌ Solo puedes reforzar tus territorios.");
    }

    private void FaseAtaque(Territorio territorio)
    {
        Jugador atacante = jugadores[indiceJugadorActual];

        if (territorioSeleccionado == null)
        {
            // Seleccionar territorio de origen
            if (territorio.propietario == atacante && territorio.tropas > 1)
            {
                territorioSeleccionado = territorio;
                RegistrarAccion("Atacante seleccionado: " + territorio.name);

                // Resaltar territorios atacables
                List<Territorio> atacables = territorio.ObtenerVecinosAtacables();
                foreach (var t in todosLosTerritorios) t.Seleccionar(false);
                foreach (var t in atacables) t.Seleccionar(true);
            }
            else RegistrarAccion("❌ Selecciona un territorio propio con >1 tropa.");
        }
        else
        {
            // Seleccionar territorio destino
            Territorio origen = territorioSeleccionado;
            Territorio destino = territorio;

            foreach (var t in todosLosTerritorios) t.Seleccionar(false); // Limpiar resaltado

            if (origen.vecinos.Contains(destino))
            {
                if (destino.propietario != atacante)
                {
                    RegistrarAccion($"⚔️ Ataque {origen.name} -> {destino.name}");
                    EnviarAccion($"{atacante.nombre} atacó {destino.name} desde {origen.name}");

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
                    EnviarAccion($"Resultado: Atk -{atkLoss}, Def -{defLoss}");

                    if (destino.tropas <= 0)
                    {
                        destino.AsignarPropietario(atacante);

                        int mover = atkDice;
                        mover = Mathf.Min(mover, Mathf.Max(1, origen.tropas - 1));
                        origen.tropas -= mover;
                        destino.tropas = mover;

                        origen.ActualizarUI();
                        destino.ActualizarUI();

                        RegistrarAccion($"🎉 {atacante.nombre} conquistó {destino.name} y movió {mover} tropas.");
                        EnviarAccion($"{atacante.nombre} conquistó {destino.name} y movió {mover} tropas.");
                    }
                }
                else RegistrarAccion("❌ No puedes atacar tus propios territorios.");
            }
            else RegistrarAccion("❌ Destino no es vecino.");

            territorioSeleccionado = null;
        }
    }

    private void FaseMovimiento(Territorio territorio)
    {
        Jugador jugadorMovimiento = jugadores[indiceJugadorActual];

        if (territorioOrigenMovimiento == null)
        {
            if (territorio.propietario == jugadorMovimiento && territorio.tropas > 1)
            {
                territorioOrigenMovimiento = territorio;
                RegistrarAccion("Origen movimiento: " + territorio.name);
            }
            else RegistrarAccion("❌ Selecciona un territorio propio con >1 tropa.");
        }
        else
        {
            Territorio origen = territorioOrigenMovimiento;
            Territorio destino = territorio;

            if (origen.vecinos.Contains(destino) && destino.propietario == jugadorMovimiento)
            {
                int mover = Mathf.Max(1, origen.tropas - 1);
                origen.tropas -= mover;
                destino.tropas += mover;

                origen.ActualizarUI();
                destino.ActualizarUI();

                RegistrarAccion($"🚛 Movidas {mover} tropas de {origen.name} a {destino.name}");
                EnviarAccion($"Movidas {mover} tropas de {origen.name} a {destino.name}");

                territorioOrigenMovimiento = null;
                FinalizarMovimiento();
            }
            else
            {
                RegistrarAccion("❌ Destino debe ser tuyo y vecino del origen.");
                territorioOrigenMovimiento = null;
            }
        }
    }

    // ---------- Cambios de fase ----------
    void CambiarAFaseRefuerzos()
    {
        faseActual = FaseJuego.Refuerzo;
        indiceJugadorActual = 0;
        Jugador jugador = jugadores[indiceJugadorActual];
        CalcularRefuerzos(jugador);
        RegistrarAccion("🔄 Fase Refuerzos. Turno: " + jugador.nombre);
        EnviarAccion("Fase Refuerzos. Turno: " + jugador.nombre);
        ActualizarUIPrincipal();
    }

    public void FinalizarAtaque()
    {
        faseActual = FaseJuego.Movimiento;
        RegistrarAccion("➡️ Fin de Ataque. Ahora Movimiento.");
        EnviarAccion("Fin de Ataque. Ahora Movimiento");
        ActualizarUIPrincipal();
    }

    public void FinalizarMovimiento()
    {
        RegistrarAccion("✅ " + jugadores[indiceJugadorActual].nombre + " finalizó movimiento.");
        EnviarAccion(jugadores[indiceJugadorActual].nombre + " finalizó movimiento");
        SiguienteTurno();
    }

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
