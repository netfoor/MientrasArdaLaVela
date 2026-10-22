using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Efectos de sonido de un disparo (F22). Los clips viven en
    /// Assets/_Proyecto/Audio/Resources/Audio/ con el código de la Tabla 27 del Documento de
    /// Diseño como nombre (S03 fragmento, S04 niebla, S06 pieza, S07 mosaico...). Si un clip no
    /// existe, no suena nada. El volumen sale de Ajustes.VolumenEfectos.
    ///
    /// Hay cuatro fuentes en rueda, no una, por dos motivos: el tono se varía por disparo (dos
    /// pasos seguidos sonarían idénticos con el mismo clip) y el tic del texto (S05) se dispara
    /// hasta 90 veces por segundo, así que un PlayOneShot no debe cortar al anterior.
    ///
    /// MEZCLA (F22). Los clips no están igualados entre sí, así que el volumen de cada llamada
    /// no es un gusto: sale de medir el clip y llevarlo a su nivel. Tres niveles, medidos en RMS
    /// sobre los .wav (la música ronda los -15 dB de media y pasa por su propia ganancia de 0.7):
    ///
    ///   Eventos narrativos  (-18 dB) 1.0   S03 fragmento, S07 mosaico, S08 transición,
    ///                                      S12 campanas, S13 campanilla, S15 latido
    ///   Eventos menores     (-22 dB) 0.8-1.0  S02 clic, S04 niebla, S06 pieza, S09 ladrido,
    ///                                      S14 brindis, S16 pétalo
    ///   Capas continuas     (-30 a   0.3-0.6  S01 pasos, S05 texto, S10 vela (Ambiente)
    ///                        -38 dB)
    ///
    /// Si se vuelve a generar un clip con otro nivel, hay que medirlo y rehacer su ganancia; la
    /// prueba SonidosTests sólo comprueba que el clip exista, no cómo suena.
    /// </summary>
    public static class Sonidos
    {
        const int Fuentes = 4;

        static AudioSource[] fuentes;
        static int siguiente;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() { fuentes = null; siguiente = 0; }   // Reload Domain está desactivado

        /// <param name="tono">Variación aleatoria del tono, en tanto por uno (0.08 = ±8 %).</param>
        public static void Play(string clave, float volumen = 1f, float tono = 0f)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + clave);
            if (clip == null) return;
            var f = Fuente();
            f.pitch = tono > 0f ? Random.Range(1f - tono, 1f + tono) : 1f;
            f.PlayOneShot(clip, volumen * Ajustes.VolumenEfectos);
        }

        /// <summary>Clip en bucle para un ambiente (S10, S11): lo gestiona quien lo pide.</summary>
        public static AudioClip Clip(string clave) => Resources.Load<AudioClip>("Audio/" + clave);

        static AudioSource Fuente()
        {
            if (fuentes == null)
            {
                var go = new GameObject("Sonidos");
                Object.DontDestroyOnLoad(go);
                fuentes = new AudioSource[Fuentes];
                for (int i = 0; i < Fuentes; i++)
                {
                    var a = go.AddComponent<AudioSource>();
                    a.playOnAwake = false;
                    a.spatialBlend = 0f;
                    fuentes[i] = a;
                }
            }
            AsegurarOyente(fuentes[0].gameObject);
            var elegida = fuentes[siguiente];
            siguiente = (siguiente + 1) % Fuentes;
            return elegida;
        }

        /// <summary>
        /// Las escenas se generan sin AudioListener: sin uno activo no se oiría nada. Pone uno
        /// en el objeto persistente indicado, salvo que la escena ya tenga el suyo.
        /// </summary>
        public static void AsegurarOyente(GameObject persistente)
        {
            var oyente = persistente.GetComponent<AudioListener>();
            bool hayOtro = false;
            foreach (var l in Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude))
                if (l != oyente && l.isActiveAndEnabled) hayOtro = true;
            if (!hayOtro && oyente == null) persistente.AddComponent<AudioListener>();
            else if (hayOtro && oyente != null) Object.Destroy(oyente);
        }
    }
}
