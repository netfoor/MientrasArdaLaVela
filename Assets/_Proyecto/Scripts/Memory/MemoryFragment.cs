using System.Collections;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Fragmento de memoria (F07). Al examinarlo reproduce su diálogo y se registra.
    /// ID con el prefijo del mapa: M0_foto, M0_delantal, M0_petalos, M0_sombrero...
    /// </summary>
    public class MemoryFragment : Interactable
    {
        [SerializeField] string fragmentId = "M0_";
        [SerializeField] string yarnNode = "";
        [Tooltip("Destello que se apaga cuando el fragmento ya se reunió.")]
        [SerializeField] GameObject sparkle;
        [SerializeField] AudioSource collectSfx; // S03

        public string FragmentId => fragmentId;
        public override bool CanInteract => base.CanInteract && !GameManager.Instance.HasFragment(fragmentId);

        void Start()
        {
            GameManager.Ensure();
            if (GameManager.Instance.HasFragment(fragmentId) && sparkle != null) sparkle.SetActive(false);
        }

        public override void Interact(PlayerInteractor player)
        {
            if (DialogueBridge.Instance == null) { Collect(); return; }
            DialogueBridge.Instance.Play(yarnNode, Collect);
        }

        void Collect()
        {
            GameManager.Instance.CollectFragment(fragmentId);
            if (collectSfx != null) collectSfx.Play();
            if (sparkle != null) StartCoroutine(HideSparkle());
        }

        IEnumerator HideSparkle()
        {
            yield return new WaitForSeconds(0.25f);
            sparkle.SetActive(false);
        }
    }
}
