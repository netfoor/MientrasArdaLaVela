using TMPro;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Detecta el Interactable más cercano y lo activa con E / Espacio / clic / botón A (F04).
    /// Muestra el aviso "[E] Examinar" sobre el objeto.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float radius = 0.9f;
        [SerializeField] LayerMask interactableMask = ~0;
        [Tooltip("Texto TMP en World Space (hijo del jugador o suelto en escena).")]
        [SerializeField] TMP_Text prompt;
        [SerializeField] Vector3 promptOffset = new Vector3(0f, 0.9f, 0f);

        readonly System.Collections.Generic.List<Collider2D> hits = new System.Collections.Generic.List<Collider2D>();
        Interactable current;

        void Update()
        {
            current = DialogueBridge.IsBusy || PlayerMovement.Locked ? null : FindNearest();

            if (prompt != null)
            {
                bool show = current != null;
                prompt.gameObject.SetActive(show);
                if (show)
                {
                    // <mark> pinta una banda oscura detrás: el aviso va sobre el mapa,
                    // que cambia de color, y en blanco solo se lee a ratos.
                    prompt.text = $"<mark=#140F20DD padding=\"12,12,4,4\">{current.PromptText}</mark>";
                    prompt.transform.position = current.transform.position + promptOffset;
                }
            }

            if (current != null && GameInput.Interact.WasPressedThisFrame())
                current.Interact(this);
        }

        Interactable FindNearest()
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = interactableMask, useTriggers = true };
            int n = Physics2D.OverlapCircle(transform.position, radius, filter, hits);
            n = hits.Count;
            Interactable best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var it = hits[i].GetComponentInParent<Interactable>();
                if (it == null || !it.CanInteract) continue;
                float d = (it.transform.position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
