using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Ambiente en bucle por escena (F22, Tabla 27): el chisporroteo de la vela (S10) acompaña
    /// las tres escenas donde la vela está encendida delante del jugador — el menú, la ofrenda y
    /// el epílogo. Es un lecho continuo, no un efecto de un disparo, así que no pasa por
    /// <see cref="Sonidos"/>: tiene su propia fuente en bucle.
    ///
    /// Se crea sola al cargar la primera escena, igual que la música, y sobrevive a los cambios
    /// de escena. El crepitar del horno del Recuerdo 3 (S11) no está aquí a propósito: lo dispara
    /// el guion en su momento (&lt;&lt;sonido S11&gt;&gt;) y un bucle lo pisaría.
    /// </summary>
    public class Ambiente : MonoBehaviour
    {
        static Ambiente inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() => inst = null;   // Reload Domain está desactivado

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (inst != null) return;
            var go = new GameObject("Ambiente");
            DontDestroyOnLoad(go);
            go.AddComponent<Ambiente>();
        }

        static readonly Dictionary<string, string> PorEscena = new Dictionary<string, string>
        {
            [SceneNames.MainMenu] = "S10",
            [SceneNames.Ofrenda] = "S10",
            [SceneNames.Epilogo] = "S10",
        };

        const float Ganancia = 0.6f;   // es un lecho de fondo: nunca debe competir con la música

        AudioSource fuente;
        string actual;

        void Awake()
        {
            inst = this;
            fuente = gameObject.AddComponent<AudioSource>();
            fuente.loop = true;
            fuente.playOnAwake = false;
            fuente.spatialBlend = 0f;
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
            PorEscena.TryGetValue(escena.name, out var clave);
            Poner(clave);
        }

        void Poner(string clave)
        {
            if (clave == actual) return;
            actual = clave;
            if (clave == null) { fuente.Stop(); fuente.clip = null; return; }

            var clip = Sonidos.Clip(clave);
            if (clip == null) { fuente.Stop(); return; }   // sin clip, silencio: no es un error
            fuente.clip = clip;
            AplicarVolumen();
            fuente.Play();
        }

        void AplicarVolumen()
        {
            if (fuente != null) fuente.volume = Ganancia * Ajustes.VolumenEfectos;
        }
    }
}
