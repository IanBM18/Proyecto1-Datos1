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
    public List<Territorio> todosLosTerritorios;
    public TextMeshProUGUI nombreJugador1Text;
    public TextMeshProUGUI nombreJugador2Text;
    public List<Jugador> jugadores = new List<Jugador>();

    // Estado
    public Territorio territorioSeleccionado = null;

    // 🔹 NUEVO: variables para refuerzos y turnos
    private int jugadorActualIndex = 0;          // quién juega (0 o 1)
    private int tropasPendientesRefuerzo = 0;    // tropas que debe colocar en refuerzo
    private int contadorGlobalIntercambios = 0;  // contador Fibonacci global

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Jugadores de prueba
        Jugador jugador1 = new Jugador();
        jugador1.nombre = "Jugador 1";
        jugador1.color = Color.red;

        Jugador jugador2 = new Jugador();
        jugador2.nombre = "Jugador 2";
        jugador2.color = Color.blue;

        jugadores.Add(jugador1);
        jugadores.Add(jugador2);

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
                    Debug.Log("Debes reforzar tus territorios.");
                }
                break;

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
