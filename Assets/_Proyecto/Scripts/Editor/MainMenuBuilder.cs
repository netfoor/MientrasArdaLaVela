using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Construye la escena MainMenu (F09) según la sección 4.1 de LEEME_SETUP.md.
    /// Idempotente: reutiliza lo que ya exista.
    /// </summary>
    public static class MainMenuBuilder
    {
        const string RutaEscena = "Assets/_Proyecto/Scenes/MainMenu.unity";
        const string RutaCreditos = "CREDITOS.txt";   // en la raíz del proyecto, junto a Assets/

        static readonly Color ColFondo = new Color(0.06f, 0.05f, 0.09f);
        static readonly Color ColBoton = new Color(0.20f, 0.14f, 0.22f);
        static readonly Color ColTexto = new Color(0.96f, 0.92f, 0.85f);
        static readonly Color ColAcento = new Color(0.95f, 0.62f, 0.22f);   // ámbar de la vela

        [MenuItem("Mientras arda la vela/Construir MainMenu")]
        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();

            ConstruirCamara();
            var menu = ConstruirObjetoMenu();
            ConstruirGameManager();

            var canvas = ConstruirCanvas();
            var title = ConstruirTitlePanel(canvas.transform);
            var (menuPanel, btnNueva, btnContinuar) = ConstruirMenuPanel(canvas.transform, menu);
            var credits = ConstruirPanelTexto(canvas.transform, "CreditsPanel", "Créditos", LeerCreditos(), menu);
            var options = ConstruirPanelTexto(canvas.transform, "OptionsPanel", "Opciones",
                "Volumen, tamaño de texto y controles se agregan en el Sprint 2 (F17).", menu);

            Set(menu, ("titlePanel", title), ("menuPanel", menuPanel),
                      ("creditsPanel", credits), ("optionsPanel", options),
                      ("continueButton", btnContinuar), ("firstSelected", btnNueva));

            // Estado inicial: sólo el título. En juego lo fija MainMenu.Start(),
            // pero así la escena también se ve bien al abrirla en el editor.
            title.SetActive(true);
            menuPanel.SetActive(false);
            credits.SetActive(false);
            options.SetActive(false);

            Undo.SetCurrentGroupName("Construir MainMenu");
            Undo.CollapseUndoOperations(grupo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[MainMenu] Jerarquía construida.");
        }

        /// <summary>Crea/abre la escena, construye, guarda y la deja PRIMERA en la lista de escenas.</summary>
        public static void CrearEscenaYConstruir()
        {
            Directory.CreateDirectory("Assets/_Proyecto/Scenes");
            if (File.Exists(RutaEscena)) EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Construir();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), RutaEscena);
            AssetDatabase.ImportAsset(RutaEscena, ImportAssetOptions.ForceSynchronousImport);
            OrdenarEscenas();
            AssetDatabase.SaveAssets();
            Debug.Log($"[MainMenu] Escena guardada en {RutaEscena}");
        }

        /// <summary>MainMenu primero, Mapa0_Ofrenda después (LEEME §4).</summary>
        static void OrdenarEscenas()
        {
            const string mapa0 = "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity";
            var orden = new List<string> { RutaEscena };
            if (File.Exists(mapa0)) orden.Add(mapa0);
            foreach (var e in EditorBuildSettings.scenes)
                if (!orden.Contains(e.path) && File.Exists(e.path)) orden.Add(e.path);

            EditorBuildSettings.scenes = orden.ConvertAll(p => new EditorBuildSettingsScene(p, true)).ToArray();
            Debug.Log($"[MainMenu] Lista de escenas: {string.Join(" → ", orden)}");
        }

        // ---------------------------------------------------------------
        static void ConstruirCamara()
        {
            var go = GameObject.Find("Main Camera") ?? Raiz("Main Camera");
            go.tag = "MainCamera";
            var cam = Comp<Camera>(go);
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ColFondo;
            go.transform.position = new Vector3(0f, 0f, -10f);
        }

        static MainMenu ConstruirObjetoMenu() => Comp<MainMenu>(Raiz("MainMenu"));

        static void ConstruirGameManager() => Comp<GameManager>(Raiz("GameManager"));

        static Canvas ConstruirCanvas()
        {
            var go = Raiz("Canvas");
            var canvas = Comp<Canvas>(go);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escala = Comp<CanvasScaler>(go);
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 0.5f;
            Comp<GraphicRaycaster>(go);

            var ev = Raiz("EventSystem");
            Comp<EventSystem>(ev);
            Comp<InputSystemUIInputModule>(ev);    // Input System, no el módulo viejo
            return canvas;
        }

        static GameObject ConstruirTitlePanel(Transform padre)
        {
            var panel = Panel(padre, "TitlePanel");
            Texto(panel.transform, "Titulo", "MIENTRAS ARDA LA VELA", 110f, ColAcento,
                  new Vector2(0f, 120f), new Vector2(1600f, 200f), TextAlignmentOptions.Center);
            Texto(panel.transform, "Aviso", "Presiona cualquier tecla", 48f, ColTexto,
                  new Vector2(0f, -120f), new Vector2(1200f, 90f), TextAlignmentOptions.Center);
            return panel;
        }

        static (GameObject, Button, Button) ConstruirMenuPanel(Transform padre, MainMenu menu)
        {
            var panel = Panel(padre, "MenuPanel");
            Texto(panel.transform, "Titulo", "MIENTRAS ARDA LA VELA", 72f, ColAcento,
                  new Vector2(0f, 340f), new Vector2(1600f, 140f), TextAlignmentOptions.Center);

            var nueva     = Boton(panel.transform, "BtnNuevaPartida", "Nueva partida", new Vector2(0f,  140f), menu.OnNewGame);
            var continuar = Boton(panel.transform, "BtnContinuar",    "Continuar",     new Vector2(0f,   40f), menu.OnContinue);
            var opciones  = Boton(panel.transform, "BtnOpciones",     "Opciones",      new Vector2(0f,  -60f), menu.OnOptions);
            var creditos  = Boton(panel.transform, "BtnCreditos",     "Créditos",      new Vector2(0f, -160f), menu.OnCredits);
            var salir     = Boton(panel.transform, "BtnSalir",        "Salir",         new Vector2(0f, -260f), menu.OnQuit);

            // Navegación explícita con gamepad/teclado
            Encadenar(new[] { nueva, continuar, opciones, creditos, salir });
            return (panel, nueva, continuar);
        }

        static GameObject ConstruirPanelTexto(Transform padre, string nombre, string titulo, string cuerpo, MainMenu menu)
        {
            var panel = Panel(padre, nombre);
            Texto(panel.transform, "Titulo", titulo, 72f, ColAcento,
                  new Vector2(0f, 400f), new Vector2(1600f, 120f), TextAlignmentOptions.Center);
            Texto(panel.transform, "Cuerpo", cuerpo, 30f, ColTexto,
                  new Vector2(0f, 10f), new Vector2(1500f, 660f), TextAlignmentOptions.TopLeft);
            Boton(panel.transform, "BtnVolver", "Volver", new Vector2(0f, -420f), menu.OnBack);
            return panel;
        }

        // ---------------------------------------------------------------
        static GameObject Panel(Transform padre, string nombre)
        {
            var go = Hijo(padre, nombre);
            var rt = Comp<RectTransform>(go);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go;
        }

        static TMP_Text Texto(Transform padre, string nombre, string texto, float tam, Color color,
                              Vector2 pos, Vector2 tamRect, TextAlignmentOptions alineacion)
        {
            var go = Hijo(padre, nombre);
            var rt = Comp<RectTransform>(go);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamRect;
            var t = Comp<TextMeshProUGUI>(go);
            t.text = texto;
            t.fontSize = tam;
            t.color = color;
            t.alignment = alineacion;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        static Button Boton(Transform padre, string nombre, string etiqueta, Vector2 pos, UnityAction accion)
        {
            var go = Hijo(padre, nombre);
            var rt = Comp<RectTransform>(go);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(560f, 84f);

            var img = Comp<Image>(go);
            img.color = ColBoton;

            var btn = Comp<Button>(go);
            btn.targetGraphic = img;

            Texto(go.transform, "Etiqueta", etiqueta, 40f, ColTexto,
                  Vector2.zero, new Vector2(540f, 80f), TextAlignmentOptions.Center);

            // OnClick persistente (queda visible en el Inspector, como pide el LEEME §4.1.4)
            for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(btn.onClick, i);
            UnityEventTools.AddVoidPersistentListener(btn.onClick, accion);
            return btn;
        }

        static void Encadenar(Button[] botones)
        {
            for (int i = 0; i < botones.Length; i++)
            {
                var nav = botones[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = botones[(i - 1 + botones.Length) % botones.Length];
                nav.selectOnDown = botones[(i + 1) % botones.Length];
                botones[i].navigation = nav;
            }
        }

        static string LeerCreditos()
        {
            if (File.Exists(RutaCreditos)) return File.ReadAllText(RutaCreditos).TrimEnd();
            Debug.LogWarning($"[MainMenu] No se encontró {RutaCreditos}; el panel de créditos queda vacío.");
            return "(Pendiente: CREDITOS.txt)";
        }

        // ---------------------------------------------------------------
        static GameObject Raiz(string nombre)
        {
            var go = GameObject.Find(nombre);
            if (go != null) return go;
            go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            return go;
        }

        static GameObject Hijo(Transform padre, string nombre)
        {
            var t = padre.Find(nombre);
            if (t != null) return t.gameObject;
            var go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            Undo.SetTransformParent(go.transform, padre, $"Anidar {nombre}");
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            return go;
        }

        static T Comp<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        static void Set(Object destino, params (string prop, object valor)[] valores)
        {
            var so = new SerializedObject(destino);
            foreach (var (prop, valor) in valores)
            {
                var p = so.FindProperty(prop);
                if (p == null) { Debug.LogWarning($"[MainMenu] {destino.GetType().Name} no tiene '{prop}'."); continue; }
                p.objectReferenceValue = valor as Object;
            }
            so.ApplyModifiedProperties();
        }
    }
}
