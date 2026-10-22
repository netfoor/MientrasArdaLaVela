using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Pétalo de cempasúchil coleccionable (F24). Hay cinco por mapa, 25 en total, escondidos
    /// en rincones que no hacen falta para la historia. Se recoge con sólo pasar encima; brilla
    /// un poco para que se distinga en la penumbra. Reunirlos todos desbloquea el logro
    /// "Camino de pétalos" (escena adicional en los créditos).
    /// ID con el prefijo del mapa: M0_petalo1 ... R4_petalo5.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Petalo : MonoBehaviour
    {
        [SerializeField] string petalId = "M0_petalo1";
        [SerializeField] SpriteRenderer sprite;
        [SerializeField] Light2D brillo;

        public string PetalId => petalId;

        Vector3 basePos;
        float fase;
        bool recogido;

        void Start()
        {
            GameManager.Ensure();
            if (GameManager.Instance.HasPetal(petalId)) { gameObject.SetActive(false); return; }
            if (sprite == null) sprite = GetComponent<SpriteRenderer>();
            basePos = transform.position;
            fase = (petalId.GetHashCode() & 0xff) / 40f;
        }

        void Update()
        {
            if (recogido || sprite == null) return;
            // Flota y se mece un poco, como si lo moviera el aire de la vela.
            float t = Time.time + fase;
            transform.position = basePos + new Vector3(0f, Mathf.Sin(t * 2f) * 0.05f, 0f);
            sprite.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.3f) * 8f);
            if (brillo != null) brillo.intensity = 0.45f + 0.15f * Mathf.Sin(t * 3f);
        }

        void OnTriggerEnter2D(Collider2D otro)
        {
            if (recogido || otro.GetComponentInParent<PlayerMovement>() == null) return;
            recogido = true;
            GameManager.Instance.CollectPetal(petalId);
            Sonidos.Play("S16", 0.8f);
            StartCoroutine(Desvanecer());
        }

        IEnumerator Desvanecer()
        {
            var inicio = transform.position;
            var color = sprite != null ? sprite.color : Color.white;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                transform.position = inicio + new Vector3(0f, k * 0.6f, 0f);
                transform.localScale = Vector3.one * (1f + k * 0.4f);
                if (sprite != null) sprite.color = new Color(color.r, color.g, color.b, 1f - k);
                if (brillo != null) brillo.intensity = 0.8f * (1f - k);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
