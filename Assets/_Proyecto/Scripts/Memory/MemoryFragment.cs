using System.Collections;
using System.Collections.Generic;
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
        public bool Collected => GameManager.Instance != null && GameManager.Instance.HasFragment(fragmentId);

        // Registro de los fragmentos de la escena, para que Tizne olfatee el más cercano (F11).
        // Incluye los que siguen ocultos bajo un Revealable: justo esos son los que más cuesta encontrar.
        static readonly List<MemoryFragment> registrados = new List<MemoryFragment>();
        public static IReadOnlyList<MemoryFragment> All => registrados;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReiniciarRegistro() => registrados.Clear();   // Reload Domain está desactivado

        void Awake() => registrados.Add(this);
        void OnDestroy() => registrados.Remove(this);
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
            else Sonidos.Play("S03");   // destello cristalino ascendente
            if (sparkle != null) StartCoroutine(HideSparkle());
        }

        IEnumerator HideSparkle()
        {
            yield return new WaitForSeconds(0.25f);
            sparkle.SetActive(false);
        }
    }
}
