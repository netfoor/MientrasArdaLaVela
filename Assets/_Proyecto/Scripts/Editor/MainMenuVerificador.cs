using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>Comprueba la escena MainMenu contra la sección 4.1 de LEEME_SETUP.md.</summary>
    public static class MainMenuVerificador
    {
        static int fallos;

        [MenuItem("Mientras arda la vela/Verificar MainMenu")]
        public static void Verificar()
        {
            fallos = 0;
            const string ruta = "Assets/_Proyecto/Scenes/MainMenu.unity";
            if (EditorSceneManager.GetActiveScene().path != ruta)
                EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            // --- Canvas y EventSystem ---
            var canvas = GameObject.Find("Canvas");
            if (Ok("Canvas existe", canvas != null))
            {
                var c = canvas.GetComponent<Canvas>();
                Ok("Canvas en Screen Space - Overlay", c != null && c.renderMode == RenderMode.ScreenSpaceOverlay);
                var esc = canvas.GetComponent<CanvasScaler>();
                Ok("Canvas Scaler 1920×1080 'Scale With Screen Size'",
                   esc != null && esc.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                   esc.referenceResolution == new Vector2(1920f, 1080f));
            }
            var ev = GameObject.Find("EventSystem");
            if (Ok("EventSystem existe", ev != null))
            {
                Ok("EventSystem con Input System UI Input Module",
                   ev.GetComponent<InputSystemUIInputModule>() != null);
                Ok("EventSystem sin el módulo antiguo",
                   ev.GetComponent<StandaloneInputModule>() == null);
            }

            // --- MainMenu y sus referencias ---
            var menuGo = GameObject.Find("MainMenu");
            if (Ok("Objeto MainMenu con el script", menuGo != null && menuGo.GetComponent<MainMenu>() != null))
            {
                var so = new SerializedObject(menuGo.GetComponent<MainMenu>());
                foreach (var campo in new[] { "titlePanel", "menuPanel", "creditsPanel", "optionsPanel",
                                              "continueButton", "galleryButton", "firstSelected" })
                    Ok($"  '{campo}' asignado", so.FindProperty(campo).objectReferenceValue != null);
            }
            Ok("Objeto GameManager con el script",
               GameObject.Find("GameManager")?.GetComponent<GameManager>() != null);

            // --- Paneles ---
            foreach (var (nombre, activo) in new[] { ("TitlePanel", true), ("MenuPanel", false),
                                                     ("CreditsPanel", false), ("OptionsPanel", false) })
            {
                var p = canvas?.transform.Find(nombre);
                if (!Ok($"Panel '{nombre}'", p != null)) continue;
                Ok($"  '{nombre}' activo al abrir = {activo}", p.gameObject.activeSelf == activo);
            }

            // --- Botones y sus OnClick ---
            var esperados = new (string obj, string metodo)[]
            {
                ("MenuPanel/BtnNuevaPartida", "OnNewGame"),
                ("MenuPanel/BtnContinuar",    "OnContinue"),
                ("MenuPanel/BtnOpciones",     "OnOptions"),
                ("MenuPanel/BtnLogros",       "OnAchievements"),
                ("MenuPanel/BtnGaleria",      "OnGallery"),
                ("MenuPanel/BtnCreditos",     "OnCredits"),
                ("MenuPanel/BtnSalir",        "OnQuit"),
                ("CreditsPanel/BtnVolver",    "OnBack"),
                ("OptionsPanel/BtnVolver",    "OnBack"),
            };
            foreach (var (obj, metodo) in esperados)
            {
                var t = canvas?.transform.Find(obj);
                var b = t != null ? t.GetComponent<Button>() : null;
                if (!Ok($"Botón '{obj}'", b != null)) continue;
                bool encontrado = false;
                for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                    if (b.onClick.GetPersistentMethodName(i) == metodo &&
                        b.onClick.GetPersistentTarget(i) is MainMenu) encontrado = true;
                Ok($"  '{obj}' → MainMenu.{metodo} (persistente)", encontrado);
            }

            // --- Créditos con scroll ---
            var scroll = canvas?.transform.Find("CreditsPanel/Scroll");
            var rect = scroll != null ? scroll.GetComponent<ScrollRect>() : null;
            if (Ok("CreditsPanel con ScrollRect", rect != null))
            {
                Ok("  scroll sólo vertical", rect.vertical && !rect.horizontal);
                Ok("  viewport asignado", rect.viewport != null);
                Ok("  content asignado", rect.content != null);
                Ok("  barra vertical asignada", rect.verticalScrollbar != null);
                Ok("  viewport recorta (RectMask2D)",
                   rect.viewport != null && rect.viewport.GetComponent<RectMask2D>() != null);
                if (rect.content != null)
                {
                    Ok("  content con ContentSizeFitter vertical",
                       rect.content.GetComponent<ContentSizeFitter>() is { } f &&
                       f.verticalFit == ContentSizeFitter.FitMode.PreferredSize);
                    Ok("  content con VerticalLayoutGroup",
                       rect.content.GetComponent<VerticalLayoutGroup>() != null);

                    var bloques = rect.content.GetComponentsInChildren<TMP_Text>(true);
                    Ok("  el texto está partido en varios bloques", bloques.Length > 1, $"{bloques.Length}");
                    int total = bloques.Sum(b => b.text.Length);
                    int mayor = bloques.Length > 0 ? bloques.Max(b => b.text.Length) : 0;
                    Ok($"  ningún bloque supera los 16 000 caracteres (mayor: {mayor})", mayor <= 16000);

                    var enDisco = System.IO.File.Exists("CREDITOS.txt")
                        ? System.IO.File.ReadAllText("CREDITOS.txt").TrimEnd().Length : -1;
                    Ok($"  el texto mostrado coincide con CREDITOS.txt ({total} vs {enDisco})",
                       enDisco > 0 && Mathf.Abs(total - enDisco) <= bloques.Length + 2);
                }
            }

            // --- Lista de escenas ---
            var escenas = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            Ok("MainMenu es la primera escena del Build",
               escenas.Length > 0 && escenas[0] == ruta,
               escenas.Length > 0 ? escenas[0] : "(vacío)");
            Ok("Mapa0_Ofrenda es la segunda",
               escenas.Length > 1 && escenas[1] == "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity");

            Debug.Log($"### MAINMENU: {fallos} fallos ###");
        }

        static bool Ok(string desc, bool cond, string detalle = null)
        {
            if (cond) Debug.Log($"  ok   · {desc}");
            else { fallos++; Debug.LogError($"FALLA  · {desc}{(detalle != null ? $" (valor: '{detalle}')" : "")}"); }
            return cond;
        }
    }
}
