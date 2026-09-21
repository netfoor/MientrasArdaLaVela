using System.Collections;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Zona de niebla del Olvido (F08). Bloquea el paso y oculta lo que hay debajo
    /// hasta que se reúne el fragmento indicado o se desmiente el Eco indicado.
    /// Estructura sugerida: SpriteRenderer(s) o ParticleSystem + BoxCollider2D (no trigger).
    /// </summary>
    public class OblivionFog : MonoBehaviour
    {
        public enum Unlock { Fragment, Echo, AllMapFragments }

        [SerializeField] Unlock unlockBy = Unlock.Fragment;
        [Tooltip("ID del fragmento o del Eco que disipa esta niebla.")]
        [SerializeField] string unlockId = "";
        [SerializeField] float fadeTime = 1.5f;
        [SerializeField] AudioSource dissipateSfx; // S04

        SpriteRenderer[] renderers;
        ParticleSystem[] particles;
        Collider2D[] colliders;
        bool cleared;

        void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            particles = GetComponentsInChildren<ParticleSystem>();
            colliders = GetComponentsInChildren<Collider2D>();
        }

        void Start()
        {
            var gm = GameManager.Ensure();
            gm.FragmentCollected += OnFragment;
            gm.EchoResolved += OnEcho;
            if (IsUnlocked()) ClearInstant();
        }

        void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.FragmentCollected -= OnFragment;
            GameManager.Instance.EchoResolved -= OnEcho;
        }

        bool IsUnlocked()
        {
            var gm = GameManager.Instance;
            switch (unlockBy)
            {
                case Unlock.Fragment: return gm.HasFragment(unlockId);
                case Unlock.Echo: return gm.IsEchoResolved(unlockId);
                default: return MemoryMap.Current != null && MemoryMap.Current.AllFragments;
            }
        }

        void OnFragment(string id) { if (!cleared && IsUnlocked()) StartCoroutine(Dissipate()); }
        void OnEcho(string id) { if (!cleared && IsUnlocked()) StartCoroutine(Dissipate()); }

        IEnumerator Dissipate()
        {
            cleared = true;
            foreach (var c in colliders) c.enabled = false;
            foreach (var p in particles) p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (dissipateSfx != null) dissipateSfx.Play();

            var start = new float[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) start[i] = renderers[i].color.a;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                float k = 1f - t / fadeTime;
                for (int i = 0; i < renderers.Length; i++) { var c = renderers[i].color; c.a = start[i] * k; renderers[i].color = c; }
                yield return null;
            }
            gameObject.SetActive(false);
        }

        void ClearInstant()
        {
            cleared = true;
            gameObject.SetActive(false);
        }
    }
}
