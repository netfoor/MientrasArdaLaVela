using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Memento del mapa (pan de muerto, fotografía, caja de cerillos, canica, promesa).
    /// Se habilita al reunir los fragmentos del mapa. En el Sprint 2 abrirá el Puzle de mosaico (F12);
    /// por ahora reproduce su diálogo, marca el mapa como completado y carga la siguiente escena si existe.
    /// </summary>
    public class Memento : Interactable
    {
        [SerializeField] string lockedNode = "";     // "Aún falta algo..."
        [SerializeField] string activateNode = "";   // Aurelio reconoce el memento
        [SerializeField] GameObject glow;            // brillo cuando está listo

        public override string PromptText => MemoryMap.Current != null && MemoryMap.Current.AllFragments ? "[E] Recordar" : "[E] Examinar";

        void Update()
        {
            if (glow != null && MemoryMap.Current != null) glow.SetActive(MemoryMap.Current.AllFragments);
        }

        public override void Interact(PlayerInteractor player)
        {
            var map = MemoryMap.Current;
            if (map == null || DialogueBridge.Instance == null) return;

            if (!map.AllFragments) { DialogueBridge.Instance.Play(lockedNode); return; }

            DialogueBridge.Instance.Play(activateNode, () =>
            {
                GameManager.Instance.CompleteMap(map.mapId);
                // TODO Sprint 2 (F12/F13): abrir MosaicPuzzle y, al resolverlo, la transición.
                if (!string.IsNullOrEmpty(map.nextScene) && Application.CanStreamedLevelBeLoaded(map.nextScene))
                    GameManager.Instance.LoadScene(map.nextScene);
                else
                    Debug.Log($"[Memento] Mapa {map.mapId} completado. La escena '{map.nextScene}' se construye en el Sprint 2.");
            });
        }
    }
}
