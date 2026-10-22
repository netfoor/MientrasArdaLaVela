using System.Collections;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Zona que inicia un diálogo al entrar el jugador (tutorial, prólogo).
    /// Se reproduce una sola vez por partida si se indica un saveId.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] string yarnNode = "";
        [Tooltip("ID único para no repetir el diálogo tras guardar (vacío = una vez por sesión).")]
        [SerializeField] string saveId = "";
        [SerializeField] bool playOnStart = false;

        bool done;

        void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (!string.IsNullOrEmpty(saveId) && GameManager.Ensure().HasSeenDialogue(saveId)) done = true;
            if (playOnStart) StartCoroutine(DispararCuandoHayaDialogo());
        }

        /// <summary>
        /// Espera a que exista el DialogueBridge en vez de apostar a un retraso fijo. Ámbar
        /// nace DENTRO del trigger del prólogo, así que OnTriggerEnter2D nunca se dispara:
        /// si este intento fallaba, el prólogo no se reproducía nunca y sin error visible.
        /// </summary>
        IEnumerator DispararCuandoHayaDialogo()
        {
            float limite = Time.time + 5f;
            while (DialogueBridge.Instance == null && Time.time < limite) yield return null;
            if (DialogueBridge.Instance == null)
            {
                Debug.LogError($"[DialogueTrigger] No apareció ningún DialogueBridge; '{yarnNode}' no se reproducirá.", this);
                yield break;
            }
            // Si venimos de una transición de recuerdo, espera a que termine de abrirse.
            while (SceneTransition.Busy) yield return null;
            // Un cuadro más para que el DialogueRunner termine de montar sus presentadores.
            yield return null;
            Fire();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovement>() != null) Fire();
        }

        void Fire()
        {
            if (done || DialogueBridge.Instance == null) return;
            if (DialogueBridge.Instance.Play(yarnNode))
            {
                done = true;
                if (!string.IsNullOrEmpty(saveId)) GameManager.Instance.MarkDialogueSeen(saveId);
            }
        }
    }
}
