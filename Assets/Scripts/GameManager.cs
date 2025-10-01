using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ===============================================
// Clase Jugador
// ===============================================
[System.Serializable]
public class Jugador
{
    public string nombre;
    public Color color;

    // 🔹 Más adelante aquí agregarás tarjetas y lógica extra
    // public List<Tarjeta> tarjetas = new List<Tarjeta>();
}

// ===============================================
// Clase principal GameManager
// ===============================================
public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    // Fases del juego
    public enum FaseJuego { AsignacionInicial, Refuerzo, Ataque, Movimiento }
    public FaseJuego faseActual = FaseJuego.AsignacionInicial;

    // Referencias
    public int territoriosLibresRestantes;

    [Header("Territorios y Jugadores")]
    public List<Territorio> todosLosTerritorios;
    public List<Jugador> jugadores = new List<Jugador>();

    [Header("UI de Jugadores")]
    public TextMeshProUGUI nombreJugador1Text;
    public TextMeshProUGUI nombreJugador2Text;
    public List<Jugador> jugadores = new List<Jugador>();

    // Estado
    public Territorio territorioSeleccionado = null;

    // 🔹 NUEVO: variables para refuerzos y turnos
    private int jugadorActualIndex = 0;          // quién juega (0 o 1)
    private int tropasPendientesRefuerzo = 0;    // tropas que debe colocar en refuerzo
    private int contadorGlobalIntercambios = 0;  // contador Fibonacci global

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
        // Jugadores de prueba
        Jugador jugador1 = new Jugador();
        jugador1.nombre = "Jugador 1";
        jugador1.color = Color.red;

        Jugador jugador2 = new Jugador();
        jugador2.nombre = "Jugador 2";
        jugador2.color = Color.blue;

        jugadores.Add(jugador1);
        jugadores.Add(jugador2);

        // Actualizar UI de nombres de jugador
        nombreJugador1Text.text = j1.nombre;
        nombreJugador1Text.color = j1.color;
        nombreJugador2Text.text = j2.nombre;
        nombreJugador2Text.color = j2.color;

        ActualizarUIPrincipal();

        RegistrarAccion("INICIO: Fase de Asignación Inicial.");
        // Actualizar UI
        if (jugadores.Count > 0)
        {
            nombreJugador1Text.text = jugadores[0].nombre;
            nombreJugador1Text.color = jugadores[0].color;
        }

        if (jugadores.Count > 1)
        {
            nombreJugador2Text.text = jugadores[1].nombre;
            nombreJugador2Text.color = jugadores[1].color;
        }

        // Inicializar asignación de territorios
        territoriosLibresRestantes = todosLosTerritorios.Count;
        Debug.Log("INICIO: Fase de Asignación Inicial.");
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
        // Actualiza la UI principal del juego (implementación pendiente)
    }
    // ===============================================
    // Método para calcular refuerzos
    // ===============================================
    private int CalcularRefuerzos(Jugador jugador)
    {
        // Contar territorios del jugador
        int territoriosJugador = 0;
        foreach (var t in todosLosTerritorios)
        {
            if (t.propietario == jugador) territoriosJugador++;
        }

        // Base (mínimo 3)
        int refuerzosBase = Mathf.Max(3, territoriosJugador / 3);

        // Bonus por continente (más adelante)
        int bonusContinente = 0;

        // Bonus Fibonacci (si intercambia tarjetas)
        int bonusFibo = 0;
        // Aquí después usarás jugador.TieneTrioDeTarjetas()
        // Por ahora se deja en 0

        int total = refuerzosBase + bonusContinente + bonusFibo;
        return total;
    }

    private void SiguienteTurno()
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
        jugadorActualIndex = (jugadorActualIndex + 1) % jugadores.Count;

        faseActual = FaseJuego.Refuerzo;

        tropasPendientesRefuerzo = CalcularRefuerzos(jugadores[jugadorActualIndex]);

        Debug.Log("TURNO DE: " + jugadores[jugadorActualIndex].nombre +
                ". Tiene " + tropasPendientesRefuerzo + " tropas de refuerzo.");
    }

    // ===============================================
    // Reclamar un territorio (fase inicial)
    // ===============================================
    public void ReclamarTerritorio(Territorio territorioClicado)
    {
        if (territorioClicado.propietario == null)
        {
            territorioClicado.AsignarPropietario(jugadores[jugadorActualIndex]);
        }
    }

    // ===============================================
    // Procesar clics de territorios según fase
    // ===============================================
    public void ProcesarClicDeTerritorio(Territorio territorioClicado)
    {
        // Deselección
        if (territorioSeleccionado == territorioClicado)
        {
            territorioSeleccionado = null;
            Debug.Log("Territorio deseleccionado.");
            return;
        }

        switch (faseActual)
        {
            // ---------------------------------------
            // ASIGNACIÓN INICIAL
            // ---------------------------------------
            case FaseJuego.AsignacionInicial:
                if (territorioClicado.propietario == null)
                {
                    Jugador jugadorActual = jugadores[jugadorActualIndex];
                    territorioClicado.AsignarPropietario(jugadorActual);
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
                    Debug.Log(jugadorActual.nombre + " reclamó " +
                              territorioClicado.name + ". Libres: " + territoriosLibresRestantes);

                    if (territoriosLibresRestantes <= 0)
                    {
                        faseActual = FaseJuego.Refuerzo;
                        Debug.Log("FIN DE ASIGNACIÓN. COMIENZA REFUERZO.");

                        // Calcular tropas de refuerzo iniciales
                        tropasPendientesRefuerzo = CalcularRefuerzos(jugadores[jugadorActualIndex]);
                        Debug.Log("Jugador " + jugadores[jugadorActualIndex].nombre +
                                  " recibe " + tropasPendientesRefuerzo + " tropas.");
                    }
                }
                else
                {
                    Debug.Log("ERROR: Solo territorios LIBRES.");
                }
                break;

            case FaseJuego.Refuerzo:
                if (t.propietario == jugadores[indiceJugadorActual])
                {
                    // Aquí podrías colocar tropas, pero este bloque está duplicado y fuera de lugar.
                }
            // ---------------------------------------
            // REFUERZO
            // ---------------------------------------
            case FaseJuego.Refuerzo:
                Jugador jugadorTurno = jugadores[jugadorActualIndex];

                if (territorioClicado.propietario == jugadorTurno)
                {
                    if (tropasPendientesRefuerzo > 0)
                    {
                        territorioClicado.tropas++;
                        tropasPendientesRefuerzo--;

                        Debug.Log("Tropa colocada en " + territorioClicado.name +
                                  ". Quedan " + tropasPendientesRefuerzo);

                        if (tropasPendientesRefuerzo == 0)
                        {
                            faseActual = FaseJuego.Ataque;
                            Debug.Log("Jugador " + jugadorTurno.nombre +
                                      " terminó refuerzos. Ahora ATAQUE.");
                        }
                    }
                    else
                    {
                        Debug.Log("Ya colocaste todas tus tropas.");
                    }
                }
                else
                {
                    RegistrarAccion("❌ Solo puedes reforzar tus territorios.");
                    Debug.Log("Debes reforzar tus territorios.");
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
    

            // ---------------------------------------
            // PLACEHOLDERS (Ataque, Movimiento)
            // ---------------------------------------
            case FaseJuego.Ataque:
                Jugador jugadorTurnoAtaque = jugadores[jugadorActualIndex];

                if (territorioSeleccionado == null)
                {
                    // Selección del territorio atacante
                    if (territorioClicado.propietario == jugadorTurnoAtaque && territorioClicado.tropas > 1)
                    {
                        territorioSeleccionado = territorioClicado;
                        Debug.Log("Territorio atacante seleccionado: " + territorioClicado.name);
                    }
                    else
                    {
                        Debug.Log("Debes elegir un territorio tuyo con más de 1 tropa para atacar.");
                    }
                }
                else
                {
                    // Selección del territorio defensor
                    if (territorioSeleccionado.vecinos != null &&
                        System.Array.Exists(territorioSeleccionado.vecinos, t => t == territorioClicado))
                    {
                        if (territorioClicado.propietario != jugadorTurnoAtaque)
                        {
                            Debug.Log("Atacando desde " + territorioSeleccionado.name + " hacia " + territorioClicado.name);

                            // Determinar dados
                            int atkDice = Mathf.Min(3, territorioSeleccionado.tropas - 1);
                            int defDice = Mathf.Min(2, territorioClicado.tropas);

                            var (atkLoss, defLoss) = CombatResolver.Resolve(atkDice, defDice);

                            territorioSeleccionado.tropas -= atkLoss;
                            territorioClicado.tropas -= defLoss;

                            Debug.Log("Resultado batalla: Atacante -" + atkLoss + ", Defensor -" + defLoss);

                            // Conquista del territorio
                            if (territorioClicado.tropas <= 0)
                            {
                                territorioClicado.AsignarPropietario(jugadorTurnoAtaque);
                                int mover = atkDice; // por regla: al menos las tropas usadas en ataque
                                territorioSeleccionado.tropas -= mover;
                                territorioClicado.tropas = mover;

                                Debug.Log("¡" + jugadorTurnoAtaque.nombre + " conquistó " + territorioClicado.name + "!");
                            }
                        }
                        else
                        {
                            Debug.Log("No puedes atacar tus propios territorios.");
                        }
                    }
                    else
                    {
                        Debug.Log("El territorio defensor debe ser vecino.");
                    }

                    // Limpiar selección después del ataque
                    territorioSeleccionado = null;
                }
                break;

            case FaseJuego.Movimiento:
                Jugador jugadorMovimiento = jugadores[jugadorActualIndex];

                if (territorioSeleccionado == null)
                {
                    // Selección origen
                    if (territorioClicado.propietario == jugadorMovimiento && territorioClicado.tropas > 1)
                    {
                        territorioSeleccionado = territorioClicado;
                        Debug.Log("Territorio origen para mover: " + territorioClicado.name);
                    }
                    else
                    {
                        Debug.Log("Debes seleccionar un territorio tuyo con más de 1 tropa.");
                    }
                }
                else
                {
                    // Selección destino
                    if (territorioSeleccionado.vecinos != null &&
                        System.Array.Exists(territorioSeleccionado.vecinos, t => t == territorioClicado))
                    {
                        if (territorioClicado.propietario == jugadorMovimiento)
                        {
                            // Movimiento simple (mueve 1 tropa por clic)
                            territorioSeleccionado.tropas--;
                            territorioClicado.tropas++;
                            territorioSeleccionado.ActualizarUI();
                            territorioClicado.ActualizarUI();

                            Debug.Log("Movida 1 tropa de " + territorioSeleccionado.name +
                                    " a " + territorioClicado.name);

                            // 🔹 Fin del movimiento → cambio de turno
                            territorioSeleccionado = null;
                            SiguienteTurno();
                        }
                        else
                        {
                            Debug.Log("Debes mover a un territorio tuyo.");
                            territorioSeleccionado = null;
                        }
                    }
                    else
                    {
                        Debug.Log("El territorio destino debe ser vecino.");
                        territorioSeleccionado = null;
                    }
                }
                break;
        }
    }
    public void FinalizarAtaque()
    {
        faseActual = FaseJuego.Movimiento;
        Debug.Log("Jugador " + jugadores[jugadorActualIndex].nombre + " pasa a Fase MOVIMIENTO.");
    }
    public void FinalizarMovimiento()
    {
        Debug.Log("Jugador " + jugadores[jugadorActualIndex].nombre + " finalizó movimiento.");
        SiguienteTurno(); // 👈 pasa el turno al siguiente jugador
    }
}
