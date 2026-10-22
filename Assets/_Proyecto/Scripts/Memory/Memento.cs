using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Memento del mapa (pan de muerto, fotografía, caja de cerillos, canica, promesa) — F13.
    ///
    /// Flujo (Figura 1 del Documento de Diseño):
    ///   sin los 4 fragmentos → lockedNode;
    ///   con ellos → activateNode ("Anclar recuerdo") → Puzle de mosaico (F12);
    ///   mosaico resuelto → se completa el mapa y se autoguarda → solvedNode →
    ///   Transición de recuerdo (fundido a blanco) → MemoryMap.nextScene.
    /// Si el mapa ya estaba completado (por ejemplo, al Continuar), va directo a la transición.
    /// </summary>
    public class Memento : Interactable
    {
        public static Memento Current { get; private set; }

        [SerializeField] string lockedNode = "";     // "Aún falta algo..."
        [SerializeField] string activateNode = "";   // Aurelio reconoce el memento
        [SerializeField] string solvedNode = "";     // tras resolver el mosaico
        [SerializeField] GameObject glow;            // brillo cuando está listo
        [Header("Puzle de mosaico (F12)")]
        [SerializeField] Sprite mosaicImage;
        [SerializeField] string mosaicTitle = "";

        bool enCurso;

        public override string PromptText => MemoryMap.Current != null && MemoryMap.Current.AllFragments ? "[E] Recordar" : "[E] Examinar";
        public override bool CanInteract => base.CanInteract && !enCurso && !SceneTransition.Busy;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarEstaticos() => Current = null;   // Reload Domain está desactivado

        void Awake() => Current = this;
        void OnDestroy() { if (Current == this) Current = null; }

        void Update()
        {
            if (glow != null && MemoryMap.Current != null) glow.SetActive(MemoryMap.Current.AllFragments);
        }

        public override void Interact(PlayerInteractor player)
        {
            var map = MemoryMap.Current;
            if (map == null || DialogueBridge.Instance == null || enCurso) return;

            if (!map.AllFragments) { DialogueBridge.Instance.Play(lockedNode); return; }

            enCurso = true;
            if (GameManager.Instance.IsMapCompleted(map.mapId)) { Siguiente(map); return; }
            DialogueBridge.Instance.Play(activateNode, () => AbrirMosaico(map));
        }

        void AbrirMosaico(MemoryMap map)
        {
            var puzle = MosaicPuzzle.Instance;
            if (puzle == null || mosaicImage == null)
            {
                Debug.LogWarning("[Memento] No hay puzle de mosaico en la escena o falta la imagen; se omite.", this);
                Resuelto(map);
                return;
            }
            puzle.Open(mosaicImage, mosaicTitle, map.mapId, () => Resuelto(map), () => enCurso = false);
        }

        void Resuelto(MemoryMap map)
        {
            GameManager.Instance.CompleteMap(map.mapId);   // autoguarda (Guardar/Cargar: al completar un mosaico)
            if (string.IsNullOrEmpty(solvedNode)) Siguiente(map);
            else DialogueBridge.Instance.Play(solvedNode, () => Siguiente(map));
        }

        void Siguiente(MemoryMap map)
        {
            SceneTransition.Go(map.nextScene, () => enCurso = false);
        }
    }
}
