using TMPro;
using UnityEngine;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Aviso del HUD para la pista de Tizne (F11): "[H] Olfatear" cuando está lista y la cuenta
    /// atrás mientras espera. El HUD completo con la vela y los pétalos es F16.
    /// </summary>
    public class TizneHintUI : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] Color readyColor = new Color(1f, 0.86f, 0.55f, 1f);
        [SerializeField] Color waitColor = new Color(0.6f, 0.6f, 0.65f, 1f);

        int ultimoSegundo = -1;

        void Update()
        {
            if (label == null) return;
            var tizne = Tizne.Instance;
            // Se oculta en el puzle (tiene su propio aviso) y durante los diálogos, que ocupan
            // la parte baja de la pantalla.
            bool visible = tizne != null && !MosaicPuzzle.IsOpen && !DialogueBridge.IsBusy;
            if (label.gameObject.activeSelf != visible)
            {
                label.gameObject.SetActive(visible);
                ultimoSegundo = -1;
            }
            if (!visible) return;

            int s = Mathf.CeilToInt(tizne.CooldownRemaining);
            if (s == ultimoSegundo) return;
            ultimoSegundo = s;
            label.text = s <= 0 ? "[H] Tizne: olfatear" : $"Tizne descansa… {s} s";
            label.color = s <= 0 ? readyColor : waitColor;
        }
    }
}
