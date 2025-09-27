using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Clase para guardar la información de un jugador
// [System.Serializable] hace que esta clase se pueda ver en el Inspector de Unity
[System.Serializable]
public class Jugador
{
    public string nombre;
    public Color color;
}

// Esta es la clase principal del GameManager
public class GameManager : MonoBehaviour
{
    // Una variable estática para que otros scripts puedan acceder a este GameManager
    public static GameManager instancia;

    // Listas y variables para gestionar jugadores y territorios
    // Dentro de la clase GameManager, al inicio
    public enum FaseJuego { AsignacionInicial, Refuerzo, Ataque, Movimiento }

    public FaseJuego faseActual = FaseJuego.AsignacionInicial;
    public int territoriosLibresRestantes; // Contador para saber cuándo termina la asignación
    public List<Territorio> todosLosTerritorios;
    public TextMeshProUGUI nombreJugador1Text;
    public TextMeshProUGUI nombreJugador2Text;
    public List<Jugador> jugadores = new List<Jugador>();

    // Esta variable guardará el territorio que el jugador ha seleccionado
    public Territorio territorioSeleccionado = null;

    void Awake()
    {
        // Se asegura de que haya solo una instancia de GameManager
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start se llama cuando el juego empieza
    void Start()
    {
        // --- AQUÍ SIMULAMOS QUE RECIBIMOS LOS DATOS DE LOS JUGADORES ---
        // En tu juego real, estos datos vendrán del socket o de un input field

        // Creamos un jugador de prueba y le damos un nombre y un color
        Jugador jugador1 = new Jugador();
        jugador1.nombre = "Jugador 1";
        jugador1.color = Color.red;

        // Hacemos lo mismo para el segundo jugador
        Jugador jugador2 = new Jugador();
        jugador2.nombre = "Jugador 2";
        jugador2.color = Color.blue;

        // Agregamos a los jugadores a nuestra lista
        jugadores.Add(jugador1);
        jugadores.Add(jugador2);

        // --- AHORA ACTUALIZAMOS LA INTERFAZ CON LA INFORMACIÓN ---
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


        // ¡ATENCIÓN! Hemos quitado la llamada a AsignarTerritoriosIniciales();
        
        // Inicializa el contador con el total de territorios
        territoriosLibresRestantes = todosLosTerritorios.Count;
        Debug.Log("INICIO: Fase de Asignación Inicial.");
    }

    // Lógica para que los jugadores reclamen territorios
    public void ReclamarTerritorio(Territorio territorioClicado)
    {
        // Si el territorio no tiene dueño, el jugador actual lo reclama.
        // Esto se ejecutará en la fase de 'asignación inicial'.
        if (territorioClicado.propietario == null)
        {
            // Asigna el territorio al Jugador 1 por ahora.
            // Más adelante, aquí irá la lógica del turno.
            territorioClicado.AsignarPropietario(jugadores[0]);
        }
    }

    // Lógica para que los jugadores muevan ejércitos entre territorios
    public void MoverEjercitos(Territorio territorioOrigen, Territorio territorioDestino)
    {
        // Verifica que el origen y destino pertenezcan al mismo jugador
        if (territorioOrigen.propietario == territorioDestino.propietario)
        {
            // Verifica que el destino sea vecino del origen
            if (territorioOrigen.vecinos.Contains(territorioDestino))
            {
                // Lógica para mover los ejércitos
                Debug.Log("Movimiento de ejércitos válido.");
            }
            else
            {
                Debug.Log("El territorio de destino no es vecino.");
            }
        }
        else
        {
            Debug.Log("Los territorios no pertenecen al mismo jugador.");
        }
    }

    public void ProcesarClicDeTerritorio(Territorio territorioClicado)
    {
    // Lógica para que el territorio se ilumine al hacer clic (selección visual)
    if (territorioSeleccionado != null)
    {
        // Desactiva la selección visual del anterior (si ya tenías la lógica visual)
        // territorioSeleccionado.bordeSeleccionado.SetActive(false);
    }
    
    // Si el territorio clicado ya está seleccionado (para deseleccionar)
    if (territorioClicado == territorioSeleccionado)
    {
        territorioSeleccionado = null;
        // territorioClicado.bordeSeleccionado.SetActive(false);
        Debug.Log("Territorio deseleccionado.");
        return;
    }

    switch (faseActual)
    {
        case FaseJuego.AsignacionInicial:
            // --------------------------------------------------------
            // LÓGICA DE ASIGNACIÓN INICIAL (SOLO LIBRES)
            // --------------------------------------------------------
            
            // Si el territorio NO tiene dueño (está libre)
            if (territorioClicado.propietario == null)
            {
                // **FALTA IMPLEMENTAR:** Lógica del turno para saber quién reclama
                Jugador jugadorActual = jugadores[0]; // Por ahora, asignamos al Jugador 1

                territorioClicado.AsignarPropietario(jugadorActual);
                territoriosLibresRestantes--;

                Debug.Log(territorioClicado.propietario.nombre + " ha reclamado un territorio. Libres restantes: " + territoriosLibresRestantes);

                // Comprobar si termina la fase de asignación
                if (territoriosLibresRestantes <= 0)
                {
                    faseActual = FaseJuego.Refuerzo; // Pasa a la siguiente fase
                    Debug.Log("FIN DE ASIGNACIÓN INICIAL. COMIENZA FASE DE REFUERZO.");
                }
            }
            else
            {
                Debug.Log("ERROR: Solo puedes reclamar territorios LIBRES en esta fase.");
            }
            break;

        case FaseJuego.Refuerzo:
            // --------------------------------------------------------
            // LÓGICA DE MOVIMIENTO/ATAQUE (Necesita lógica de selección)
            // --------------------------------------------------------
            
            // Lógica de Selección de Origen
            if (territorioSeleccionado == null)
            {
                if (territorioClicado.propietario == jugadores[0]) // Si es del jugador actual
                {
                    territorioSeleccionado = territorioClicado;
                    // territorioClicado.bordeSeleccionado.SetActive(true);
                    Debug.Log("Origen seleccionado para refuerzo: " + territorioClicado.propietario.nombre);
                }
                else
                {
                    Debug.Log("ERROR: Debes seleccionar uno de tus territorios.");
                }
            }
            // Lógica de Selección de Destino (Solo vecinos)
            else
            {
                if (territorioSeleccionado.vecinos.Contains(territorioClicado))
                {
                    // Lógica para refuerzo o ataque
                    if (territorioSeleccionado.propietario == territorioClicado.propietario)
                    {
                        Debug.Log("Refuerzo válido. Moviendo tropas.");
                        // ... Implementar lógica de mover N tropas ...
                    }
                    else
                    {
                        Debug.Log("¡Ataque! Lógica de batalla pendiente.");
                        // ... Implementar lógica de ataque ...
                    }
                }
                else
                {
                    Debug.Log("ERROR: El destino debe ser un territorio vecino.");
                }
                
                // Limpiar selección después de la acción
                territorioSeleccionado = null;
            }
            break;
            
        // Puedes añadir más fases aquí (Ataque, Movimiento)
        default:
            Debug.Log("Fase de juego no implementada: " + faseActual.ToString());
            break;
    }
    }

}