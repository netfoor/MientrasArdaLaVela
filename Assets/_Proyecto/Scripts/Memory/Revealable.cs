using System.Collections;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Objeto oculto que solo aparece al iluminarlo con la Luz de vela.
    /// Mientras está oculto, sus Interactables quedan deshabilitados.
    /// </summary>
    public class Revealable : MonoBehaviour
    {
        [Tooltip("ID único para recordar que ya fue revelado (p. ej. M0_foto_rev).")]
        [SerializeField] string revealId = "";
        [SerializeField] SpriteRenderer[] renderers;
        [SerializeField] float fadeTime = 0.6f;

        public bool Revealed { get; private set; }
        Interactable[] interactables;

        void Awake()
        {
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>();
            interactables = GetComponentsInChildren<Interactable>();
        }

        void Start()
        {
            bool already = !string.IsNullOrEmpty(revealId) && GameManager.Ensure().IsRevealed(revealId);
            SetVisible(already ? 1f : 0f);
            Revealed = already;
            foreach (var i in interactables) i.enabled = already;
            if (!already) CandleLight.Register(this);
        }

        void OnDestroy() => CandleLight.Unregister(this);

        public void Reveal()
        {
            if (Revealed) return;
            Revealed = true;
            CandleLight.Unregister(this);
            if (!string.IsNullOrEmpty(revealId)) GameManager.Instance.MarkRevealed(revealId);
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            for (float t = 0; t < fadeTime; t += Time.deltaTime) { SetVisible(t / fadeTime); yield return null; }
            SetVisible(1f);
            foreach (var i in interactables) i.enabled = true;
        }

        void SetVisible(float a)
        {
            foreach (var r in renderers) { if (r == null) continue; var c = r.color; c.a = a; r.color = c; }
        }
    }
}
