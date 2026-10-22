using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MientrasArdaLaVela
{
    /// <summary>
    /// Pantalla de título y menú principal (F09).
    /// Paneles: Title (texto "Presiona cualquier tecla"), Menu (botones) y Credits.
    /// Conectar los botones a los métodos públicos desde el Inspector (OnClick).
    ///
    /// Códigos secretos (F23, Tabla 25): se teclean en el título o en el menú, sin campo de
    /// texto; una campanilla (S13) confirma el código y el efecto queda guardado en la configuración.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject titlePanel;
        [SerializeField] GameObject menuPanel;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] GameObject optionsPanel;   // se completa en el Sprint 2 (F17)
        [SerializeField] Button continueButton;
        [SerializeField] Button galleryButton;      // Galería de bocetos (F23): se abre con "Dos velas" o CEMPASUCHIL
        [SerializeField] Button firstSelected;      // para navegación con gamepad

        readonly System.Text.StringBuilder tecleado = new System.Text.StringBuilder();

        void OnEnable() { if (Keyboard.current != null) Keyboard.current.onTextInput += AlTeclear; }
        void OnDisable() { if (Keyboard.current != null) Keyboard.current.onTextInput -= AlTeclear; }

        void AlTeclear(char c)
        {
            bool enMenu = (titlePanel != null && titlePanel.activeSelf) || (menuPanel != null && menuPanel.activeSelf);
            if (!enMenu || !char.IsLetter(c)) return;
            tecleado.Append(char.ToUpperInvariant(c));
            if (tecleado.Length > 16) tecleado.Remove(0, tecleado.Length - 16);
            var r = Logros.ProbarCodigo(tecleado.ToString());
            if (r == null) return;
            tecleado.Clear();
            Sonidos.Play("S13");
            Avisos.Mostrar(r.Value.titulo, r.Value.mensaje);
            RefrescarBotones();
        }

        void RefrescarBotones()
        {
            if (continueButton != null) continueButton.interactable = SaveSystem.HasSave;
            if (galleryButton != null) galleryButton.interactable = Logros.GaleriaAbierta;
        }

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
            RefrescarBotones();
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
        /// <summary>Opciones reales (F17): las mismas del menú de pausa, en la interfaz global.</summary>
        public void OnOptions() => InterfazGlobal.AbrirOpciones(ShowMenu);
        public void OnAchievements() => InterfazGlobal.AbrirLogros(ShowMenu);
        public void OnGallery() { if (Logros.GaleriaAbierta) InterfazGlobal.AbrirGaleria(ShowMenu); }
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
