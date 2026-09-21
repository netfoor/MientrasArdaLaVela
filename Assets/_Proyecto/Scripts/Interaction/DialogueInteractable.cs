using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>Objeto o personaje que solo reproduce un nodo de Yarn (NPC, objeto decorativo).</summary>
    public class DialogueInteractable : Interactable
    {
        [SerializeField] string yarnNode = "";
        [Tooltip("Nodo alterno cuando ya se habló una vez (opcional).")]
        [SerializeField] string repeatNode = "";

        bool talked;

        public override void Interact(PlayerInteractor player)
        {
            string node = talked && !string.IsNullOrEmpty(repeatNode) ? repeatNode : yarnNode;
            if (DialogueBridge.Instance != null && DialogueBridge.Instance.Play(node)) talked = true;
        }
    }
}
