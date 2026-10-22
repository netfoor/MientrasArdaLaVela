using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Contador de fragmentos del HUD (F07): nombre del recuerdo y 4 rombos.
    /// El HUD completo (vela, pétalos) se termina en el Sprint 2 (F16).
    /// F24: a la derecha de los rombos, el contador de pétalos del mapa. Se crea en tiempo de
    /// ejecución copiando el texto del contador, así sirve para todas las escenas sin reconstruirlas.
    /// </summary>
    public class FragmentCounterUI : MonoBehaviour
    {
        [SerializeField] TMP_Text mapNameText;
        [SerializeField] TMP_Text counterText;             // "2/4"
        [SerializeField] Image[] slots;                    // 4 imágenes de rombo
        [SerializeField] Sprite emptySlot;
        [SerializeField] Sprite filledSlot;

        TMP_Text petalText;

        void Start()
        {
            var gm = GameManager.Ensure();
            gm.FragmentCollected += OnFragment;
            gm.PetalCollected += OnFragment;
            CrearContadorPetalos();
            Refresh();
        }

        void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.FragmentCollected -= OnFragment;
            GameManager.Instance.PetalCollected -= OnFragment;
        }

        void OnFragment(string id) => Refresh();

        void CrearContadorPetalos()
        {
            var map = MemoryMap.Current;
            if (counterText == null || map == null || System.Array.IndexOf(Logros.MapasConPetalos, map.mapId) < 0) return;
            petalText = Instantiate(counterText, counterText.transform.parent);
            petalText.name = "ContadorPetalos";
            var rt = petalText.rectTransform;
            rt.anchoredPosition = new Vector2(310f, -76f);
            rt.sizeDelta = new Vector2(320f, 44f);
            petalText.fontSize = 28f;
            petalText.color = new Color(1f, 0.66f, 0.28f);
            petalText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        void Refresh()
        {
            var map = MemoryMap.Current;
            if (map == null) return;
            int n = map.Collected;
            if (mapNameText) mapNameText.text = map.displayName;
            if (counterText) counterText.text = $"{n}/{map.requiredFragments}";
            if (petalText) petalText.text = $"Pétalos {GameManager.Instance.CountPetals(map.mapId + "_")}/{Logros.PetalosPorMapa}";
            for (int i = 0; i < slots.Length; i++)
                if (slots[i]) slots[i].sprite = i < n ? filledSlot : emptySlot;
        }
    }
}
