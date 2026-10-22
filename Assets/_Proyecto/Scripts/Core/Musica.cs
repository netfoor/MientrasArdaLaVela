using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Música de fondo (F22). Una pieza por escena según el Documento de Diseño, con fundido
    /// cruzado al cambiar: M01 menú, M02 ofrenda, M03 panadería, M04 boda, M05 horno,
    /// M06 campo; M07 durante el mosaico y M08 en el epílogo (comando &lt;&lt;musica M08&gt;&gt;).
    /// Las piezas están en Audio/Resources/Musica y las genera tools/arte/musica.py.
    /// Se crea sola y sobrevive a los cambios de escena. Volumen: Ajustes.VolumenMusica.
    /// </summary>
    public class Musica : MonoBehaviour
    {
        static Musica inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => inst = null;   // Reload Domain está desactivado

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (inst != null) return;
            var go = new GameObject("Música");
            DontDestroyOnLoad(go);
            go.AddComponent<Musica>();
        }

        static readonly Dictionary<string, string> PorEscena = new Dictionary<string, string>
        {
            [SceneNames.MainMenu] = "M01",
            [SceneNames.Ofrenda] = "M02",
            [SceneNames.Recuerdo1] = "M03",
            [SceneNames.Recuerdo2] = "M04",
            [SceneNames.Recuerdo3] = "M05",
            [SceneNames.Recuerdo4] = "M06",
        };

        AudioSource[] fuentes;
        int activa;
        string actual, previa;
        float[] nivel = { 0f, 0f };
        Coroutine fundido;

        void Awake()
        {
            inst = this;
            fuentes = new[] { gameObject.AddComponent<AudioSource>(), gameObject.AddComponent<AudioSource>() };
            foreach (var f in fuentes) { f.loop = true; f.playOnAwake = false; f.spatialBlend = 0f; f.volume = 0f; }
            SceneManager.sceneLoaded += AlCargar;
            Ajustes.Cambiaron += AplicarVolumen;
            AlCargar(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= AlCargar;
            Ajustes.Cambiaron -= AplicarVolumen;
            if (inst == this) inst = null;
        }

        void AlCargar(Scene escena, LoadSceneMode modo)
        {
            Sonidos.AsegurarOyente(gameObject);
            previa = null;
            if (PorEscena.TryGetValue(escena.name, out var clave)) Poner(clave, 2f);
        }

        /// <summary>Cambia la pieza con fundido cruzado. Si ya suena, no hace nada.</summary>
        public static void Tocar(string clave, float segundos = 2f)
        {
            if (inst == null) Crear();
            inst.Poner(clave, segundos);
        }

        /// <summary>Pieza pasajera (el mosaico); Restaurar() vuelve a la anterior.</summary>
        public static void Temporal(string clave)
        {
            if (inst == null) Crear();
            if (inst.previa == null) inst.previa = inst.actual;
            inst.Poner(clave, 1.2f);
        }

        public static void Restaurar()
        {
            if (inst == null || inst.previa == null) return;
            var p = inst.previa; inst.previa = null;
            inst.Poner(p, 1.5f);
        }

        void Poner(string clave, float segundos)
        {
            if (clave == actual) return;
            var clip = Resources.Load<AudioClip>("Musica/" + clave);
            if (clip == null) { Debug.LogWarning($"[Música] No existe Audio/Resources/Musica/{clave}."); return; }
            actual = clave;
            int nueva = 1 - activa;
            fuentes[nueva].clip = clip;
            fuentes[nueva].Play();
            if (fundido != null) StopCoroutine(fundido);
            fundido = StartCoroutine(Fundir(activa, nueva, segundos));
            activa = nueva;
        }

        IEnumerator Fundir(int sale, int entra, float segundos)
        {
            float desde = nivel[sale];
            for (float t = 0f; t < segundos; t += Time.unscaledDeltaTime)
            {
                float k = t / segundos;
                nivel[sale] = Mathf.Lerp(desde, 0f, k);
                nivel[entra] = Mathf.Lerp(0f, 1f, k);
                AplicarVolumen();
                yield return null;
            }
            nivel[sale] = 0f; nivel[entra] = 1f;
            fuentes[sale].Stop();
            AplicarVolumen();
        }

        void AplicarVolumen()
        {
            if (fuentes == null) return;
            for (int i = 0; i < 2; i++) fuentes[i].volume = nivel[i] * Ajustes.VolumenMusica * 0.7f;
        }
    }
}
