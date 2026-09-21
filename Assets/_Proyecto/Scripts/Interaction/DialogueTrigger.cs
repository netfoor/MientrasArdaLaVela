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
            if (!string.IsNullOrEmpty(saveId) && GameManager.Ensure().IsRevealed(saveId)) done = true;
            if (playOnStart) Invoke(nameof(Fire), 0.3f);
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
                if (!string.IsNullOrEmpty(saveId)) GameManager.Instance.MarkRevealed(saveId);
            }
        }
    }
}
