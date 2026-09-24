using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Pantalla de título y menú principal (F09).
    /// Paneles: Title (texto "Presiona cualquier tecla"), Menu (botones) y Credits.
    /// Conectar los botones a los métodos públicos desde el Inspector (OnClick).
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject titlePanel;
        [SerializeField] GameObject menuPanel;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] GameObject optionsPanel;   // se completa en el Sprint 2 (F17)
        [SerializeField] Button continueButton;
        [SerializeField] Button firstSelected;      // para navegación con gamepad

        void Start()
        {
            GameManager.Ensure();
            ShowTitle();
        }

        void Update()
        {
            if (titlePanel != null && titlePanel.activeSelf && GameInput.AnyKey.WasPressedThisFrame())
                ShowMenu();
            else if (creditsPanel != null && creditsPanel.activeSelf && GameInput.Pause.WasPressedThisFrame())
                ShowMenu();
        }

        void ShowTitle()
        {
            SetPanels(title: true);
        }

        public void ShowMenu()
        {
            SetPanels(menu: true);
            if (continueButton != null) continueButton.interactable = SaveSystem.HasSave;
            // La selección se pospone un cuadro a propósito: Espacio y Enter también son
            // "Submit" para la interfaz, y seleccionar el botón en el mismo cuadro en que se
            // pulsa la tecla hacía que se activara solo (se saltaba el menú y empezaba partida).
            if (firstSelected != null)
            {
                EventSystem.current?.SetSelectedGameObject(null);
                if (isActiveAndEnabled) StartCoroutine(SeleccionarSiguienteCuadro());
            }
        }

        IEnumerator SeleccionarSiguienteCuadro()
        {
            yield return null;
            if (firstSelected != null && menuPanel != null && menuPanel.activeSelf)
                EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
        }

        public void OnNewGame() => GameManager.Instance.NewGame();
        public void OnContinue() { if (SaveSystem.HasSave) GameManager.Instance.ContinueGame(); }
        public void OnOptions() { if (optionsPanel != null) SetPanels(options: true); }
        public void OnCredits() => SetPanels(credits: true);
        public void OnBack() => ShowMenu();

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void SetPanels(bool title = false, bool menu = false, bool credits = false, bool options = false)
        {
            if (titlePanel) titlePanel.SetActive(title);
            if (menuPanel) menuPanel.SetActive(menu);
            if (creditsPanel) creditsPanel.SetActive(credits);
            if (optionsPanel) optionsPanel.SetActive(options);
        }
    }
}
