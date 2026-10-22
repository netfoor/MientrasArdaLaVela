using System;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Opciones del jugador (F17, pantalla Opciones del Documento de Diseño): volumen de música y
    /// efectos, velocidad del texto y pantalla completa. Se guardan en PlayerPrefs, aparte de la
    /// partida, porque son del equipo y no de la historia.
    /// </summary>
    public static class Ajustes
    {
        public static readonly string[] NombresVelocidad = { "Lenta", "Normal", "Rápida", "Instantánea" };
        static readonly int[] LetrasPorSegundo = { 25, 50, 90, 0 };   // 0 = sin máquina de escribir

        public static event Action Cambiaron;

        public static float VolumenMusica
        {
            get => PlayerPrefs.GetFloat("vol_musica", 0.8f);
            set { PlayerPrefs.SetFloat("vol_musica", Mathf.Clamp01(value)); Aplicar(); }
        }

        public static float VolumenEfectos
        {
            get => PlayerPrefs.GetFloat("vol_efectos", 0.8f);
            set { PlayerPrefs.SetFloat("vol_efectos", Mathf.Clamp01(value)); Aplicar(); }
        }

        /// <summary>Índice en NombresVelocidad.</summary>
        public static int VelocidadTexto
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("vel_texto", 1), 0, NombresVelocidad.Length - 1);
            set { PlayerPrefs.SetInt("vel_texto", Mathf.Clamp(value, 0, NombresVelocidad.Length - 1)); Aplicar(); }
        }

        public static bool PantallaCompleta
        {
            get => PlayerPrefs.GetInt("pantalla_completa", 1) == 1;
            set { PlayerPrefs.SetInt("pantalla_completa", value ? 1 : 0); Aplicar(); }
        }

        /// <summary>Aplica los ajustes a la escena actual (se llama también al cargar cada escena).</summary>
        public static void Aplicar()
        {
            PlayerPrefs.Save();
            if (!Application.isEditor) Screen.fullScreen = PantallaCompleta;   // en el editor no hace nada útil
            int lps = LetrasPorSegundo[VelocidadTexto];
            foreach (var lp in UnityEngine.Object.FindObjectsByType<Yarn.Unity.LinePresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                lp.lettersPerSecond = lps;
                // El presentador crea su máquina de escribir en Awake: se ajusta también la existente.
                // 0 letras por segundo = el texto aparece completo de inmediato.
                if (lp.Typewriter is Yarn.Unity.LetterTypewriter maquina) maquina.CharactersPerSecond = lps;
            }
            Cambiaron?.Invoke();
        }
    }
}
