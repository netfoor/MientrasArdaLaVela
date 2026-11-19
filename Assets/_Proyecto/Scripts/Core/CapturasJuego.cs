using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Capturas del juego en marcha, para la documentación y la entrega (F25/F26).
    ///
    /// No se activa nunca al jugar: hace falta arrancar el ejecutable con <c>-capturas</c>.
    /// Entonces recorre un guion de momentos —el menú, el prólogo en la ofrenda, cada recuerdo
    /// y el epílogo—, guarda un PNG de cada uno y cierra el juego.
    ///
    /// Es distinto de <c>Mapa0Captura</c>: aquél renderiza la escena desde el editor, sin jugar;
    /// esto enseña el juego de verdad, con su interfaz, su diálogo y su iluminación.
    ///
    /// Aquí no se toma la foto: el proyecto no incluye el módulo ScreenCapture de Unity y no
    /// merece la pena añadirlo a la entrega por esto. Este guía lleva al juego de un momento al
    /// siguiente y avisa por el log cuando la pantalla está lista; quien dispara la cámara es
    /// <c>tools/capturas-juego.sh</c>, que lee esos avisos.
    /// </summary>
    public class CapturasJuego : MonoBehaviour
    {
        const string Argumento = "-capturas";
        int n;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, Argumento);
            if (i < 0) return;

            Application.runInBackground = true;   // sin foco, Unity congelaría la partida
            var go = new GameObject("CapturasJuego");
            DontDestroyOnLoad(go);
            go.AddComponent<CapturasJuego>();
        }

        void Start() => StartCoroutine(Recorrido());

        IEnumerator Recorrido()
        {
            yield return Foto("menu", 3f);

            yield return Escena(SceneNames.Ofrenda, "ofrenda_prologo", 5f);
            // Unas cuantas pulsaciones para dejar atrás el prólogo y ver el mapa despejado.
            for (int i = 0; i < 14; i++)
            {
                Avanzar();
                yield return new WaitForSecondsRealtime(0.7f);
            }
            yield return Foto("ofrenda", 1.5f);

            yield return Escena(SceneNames.Recuerdo1, "recuerdo1_panaderia", 5f);
            yield return Escena(SceneNames.Recuerdo2, "recuerdo2_boda", 5f);
            yield return Escena(SceneNames.Recuerdo3, "recuerdo3_horno", 5f);
            yield return Escena(SceneNames.Recuerdo4, "recuerdo4_campo", 5f);
            yield return Escena(SceneNames.Epilogo, "epilogo", 5f);

            // El puzle de mosaico y el Diario: dos sistemas que no se ven recorriendo los mapas
            // y que son la mitad de la mecánica.
            yield return Escena(SceneNames.Recuerdo1, "_r1", 4f);
            var puzle = FindFirstObjectByType<MosaicPuzzle>(FindObjectsInactive.Include);
            // La imagen del mosaico la guarda el Memento de la escena en un campo privado; los
            // mosaicos no están en Resources, así que se toma de ahí.
            var memento = FindFirstObjectByType<Memento>(FindObjectsInactive.Include);
            Sprite imagen = null;
            if (memento != null)
            {
                var campo = typeof(Memento).GetField("mosaicImage",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                imagen = campo?.GetValue(memento) as Sprite;
            }
            if (puzle != null && imagen != null)
            {
                puzle.Open(imagen, "La fotografía de La Espiga", "demo_capturas", null, null);
                yield return Foto("puzle_mosaico", 2.5f);
                puzle.gameObject.SetActive(false);
            }
            else Debug.Log($"### AVISO: puzle={puzle != null} imagen={imagen != null} ###");

            InterfazGlobal.AbrirLogros(null);
            yield return Foto("logros", 2f);

            Debug.Log($"### FIN: {n} momentos ###");
            Application.Quit();
        }

        /// <summary>Pulsa lo mismo que pulsaría el jugador para pasar de línea.</summary>
        static void Avanzar()
        {
            var avanzador = FindFirstObjectByType<LineAdvancer>(FindObjectsInactive.Include);
            if (avanzador == null) return;
            avanzador.OnInputHurryUpLines();
            avanzador.OnInputHurryUpOptions();
        }

        IEnumerator Escena(string escena, string nombre, float espera)
        {
            SceneManager.LoadScene(escena);
            yield return Foto(nombre, espera);
        }

        /// <summary>
        /// Deja la escena asentada, avisa de que la pantalla está lista y aguanta ahí mientras
        /// el script de fuera dispara la foto.
        /// </summary>
        IEnumerator Foto(string nombre, float espera)
        {
            yield return new WaitForSecondsRealtime(espera);
            Debug.Log($"### LISTO {++n:00}_{nombre} ###");
            yield return new WaitForSecondsRealtime(4f);
        }
    }
}
