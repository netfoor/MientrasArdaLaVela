using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Base de todo objeto con el que Ámbar puede interactuar.
    /// Requiere un Collider2D (marcar Is Trigger si no debe bloquear el paso).
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected string promptText = "[E] Examinar";

        public virtual string PromptText => promptText;
        public virtual bool CanInteract => isActiveAndEnabled;

        public abstract void Interact(PlayerInteractor player);
    }
}
