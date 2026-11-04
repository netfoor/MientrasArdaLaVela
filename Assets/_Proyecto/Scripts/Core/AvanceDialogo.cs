using UnityEngine;
using Yarn.Unity;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Avanza los diálogos con las teclas del juego (F05).
    ///
    /// POR QUÉ EXISTE: el prefab "Dialogue System" de Yarn Spinner trae un <c>LineAdvancer</c>
    /// con el modo de entrada en KeyCodes, pero el componente que de verdad lee el teclado
    /// (<c>LineAdvancerInput.KeyCodes</c>) lo añade <c>LineAdvancer.OnValidate</c>, que está
    /// dentro de <c>#if UNITY_EDITOR</c>. Como el prefab vive en el paquete y es de sólo
    /// lectura, ese componente nunca queda guardado: en el editor se añade al vuelo y todo
    /// funciona, pero **en una compilación nadie lo añade y ninguna tecla avanza el diálogo**.
    /// El juego se quedaba clavado en la primera línea (encontrado en el build del 2026-11-04).
    ///
    /// Aquí la entrada se lee de <see cref="GameInput.Interact"/>, que ya tiene E, Espacio,
    /// clic izquierdo y el botón del gamepad, así que se avanza con lo mismo con que se
    /// interactúa y no hace falta explicar una tecla aparte. No se usa <c>UnityEngine.Input</c>.
    ///
    /// Se crea solo al cargar la escena, como <see cref="Ambiente"/> y <see cref="Musica"/>:
    /// así vale para las siete escenas sin tocar sus constructores.
    /// </summary>
    public class AvanceDialogo : MonoBehaviour
    {
        static AvanceDialogo inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => inst = null;   // Reload Domain está desactivado

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (inst != null) return;
            var go = new GameObject("Avance de diálogo");
            DontDestroyOnLoad(go);
            go.AddComponent<AvanceDialogo>();
        }

        LineAdvancer avanzador;

        void Awake() => inst = this;

        void OnDestroy()
        {
            if (inst == this) inst = null;
        }

        void Update()
        {
            if (!DialogueBridge.IsBusy) return;
            if (GameInput.Interact == null || !GameInput.Interact.WasPressedThisFrame()) return;

            // El Dialogue System se crea con cada escena, así que el avanzador cambia; se busca
            // sólo cuando hace falta y se recuerda mientras siga vivo.
            if (avanzador == null)
                avanzador = FindFirstObjectByType<LineAdvancer>(FindObjectsInactive.Include);
            if (avanzador == null) return;

            // Hay que llamar a lo mismo que llamaría el componente de entrada de Yarn, no al
            // método público de nombre parecido: RequestLineHurryUp() sólo manda completar la
            // línea y nunca pasa a la siguiente, así que el texto salía entero pero el diálogo
            // se quedaba ahí clavado. OnInputHurryUpLines() mira en qué estado está la línea y
            // avanza cuando ya terminó de escribirse. (Primera versión de este arreglo usaba el
            // otro método y no servía de nada.)
            avanzador.OnInputHurryUpLines();
            avanzador.OnInputHurryUpOptions();   // mismo botón para apurar la lista de opciones
        }
    }
}
