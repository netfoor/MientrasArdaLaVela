using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Configuración del mapa/recuerdo actual. Colocar UNO por escena.
    /// Los IDs de fragmentos del mapa deben iniciar con fragmentPrefix (p. ej. "M0_foto").
    /// </summary>
    public class MemoryMap : MonoBehaviour
    {
        public static MemoryMap Current { get; private set; }

        [Tooltip("Identificador del mapa: M0 (Ofrenda), R1, R2, R3, R4.")]
        public string mapId = "M0";
        [Tooltip("Prefijo de los IDs de fragmentos de este mapa.")]
        public string fragmentPrefix = "M0_";
        [Tooltip("Nombre visible en el HUD.")]
        public string displayName = "La ofrenda — 2025";
        public int requiredFragments = 4;
        [Tooltip("Escena a la que lleva el memento. Vacío = todavía no existe.")]
        public string nextScene = SceneNames.Recuerdo1;

        void Awake()
        {
            Current = this;
            GameManager.Ensure();
        }

        /// <summary>Evita que Current apunte a un mapa ya destruido mientras se está en el menú.</summary>
        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public int Collected => GameManager.Instance.CountFragments(fragmentPrefix);
        public bool AllFragments => Collected >= requiredFragments;
    }
}
