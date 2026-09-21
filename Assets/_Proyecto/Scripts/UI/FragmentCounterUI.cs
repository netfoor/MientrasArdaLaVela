using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Contador de fragmentos del HUD (F07): nombre del recuerdo y 4 rombos.
    /// El HUD completo (vela, pétalos) se termina en el Sprint 2 (F16).
    /// </summary>
    public class FragmentCounterUI : MonoBehaviour
    {
        [SerializeField] TMP_Text mapNameText;
        [SerializeField] TMP_Text counterText;             // "2/4"
        [SerializeField] Image[] slots;                    // 4 imágenes de rombo
        [SerializeField] Sprite emptySlot;
        [SerializeField] Sprite filledSlot;

        void Start()
        {
            GameManager.Ensure().FragmentCollected += OnFragment;
            Refresh();
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.FragmentCollected -= OnFragment;
        }

        void OnFragment(string id) => Refresh();

        void Refresh()
        {
            var map = MemoryMap.Current;
            if (map == null) return;
            int n = map.Collected;
            if (mapNameText) mapNameText.text = map.displayName;
            if (counterText) counterText.text = $"{n}/{map.requiredFragments}";
            for (int i = 0; i < slots.Length; i++)
                if (slots[i]) slots[i].sprite = i < n ? filledSlot : emptySlot;
        }
    }
}
