using System.Collections;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Eco del Olvido (F08): silueta gris que repite una versión falsa.
    /// Sin el fragmento requerido reproduce falseNode; con él, truthNode y se desvanece.
    /// </summary>
    public class Echo : Interactable
    {
        [SerializeField] string echoId = "M0_eco";
        [SerializeField] string requiredFragment = "";
        [SerializeField] string falseNode = "";
        [SerializeField] string truthNode = "";
        [SerializeField] float fadeTime = 1f;

        public override string PromptText => "[E] Escuchar el eco";
        public override bool CanInteract => base.CanInteract && !GameManager.Instance.IsEchoResolved(echoId);

        void Start()
        {
            if (GameManager.Ensure().IsEchoResolved(echoId)) gameObject.SetActive(false);
        }

        public override void Interact(PlayerInteractor player)
        {
            if (DialogueBridge.Instance == null) return;
            if (GameManager.Instance.HasFragment(requiredFragment))
                DialogueBridge.Instance.Play(truthNode, () => StartCoroutine(Resolve()));
            else
                DialogueBridge.Instance.Play(falseNode);
        }

        IEnumerator Resolve()
        {
            GameManager.Instance.ResolveEcho(echoId);
            Sonidos.Play("S04");
            var srs = GetComponentsInChildren<SpriteRenderer>();
            foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                foreach (var r in srs) { var c = r.color; c.a = 1f - t / fadeTime; r.color = c; }
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
