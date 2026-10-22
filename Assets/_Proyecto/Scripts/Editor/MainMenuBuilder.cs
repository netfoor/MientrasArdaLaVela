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

        /// <summary>Igual que en el Mapa 0: construye en la escena abierta, así que la comprueba.</summary>
        [MenuItem("Mientras arda la vela/Construir MainMenu")]
        public static void ConstruirDesdeMenu()
        {
            var activa = EditorSceneManager.GetActiveScene();
            if (activa.path != RutaEscena)
            {
                string abierta = string.IsNullOrEmpty(activa.path) ? "una escena sin guardar" : activa.path;
                if (Application.isBatchMode) { Debug.LogError($"[MainMenu] La escena abierta es {abierta}."); return; }
                if (!EditorUtility.DisplayDialog("Escena equivocada",
                        $"Este comando construye dentro de la escena abierta, y ahora lo está {abierta}.\n\n" +
                        "¿Abrir MainMenu y construir ahí?",
                        "Abrir MainMenu y construir", "Cancelar")) return;
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            }
            Construir();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[MainMenu] Escena reconstruida y guardada.");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();

            ConstruirCamara();
            var menu = ConstruirObjetoMenu();
            ConstruirGameManager();

            var canvas = ConstruirCanvas();
            var title = ConstruirTitlePanel(canvas.transform);
            var (menuPanel, btnNueva, btnContinuar, btnGaleria) = ConstruirMenuPanel(canvas.transform, menu);
            var credits = ConstruirCreditsPanel(canvas.transform, menu);
            var options = ConstruirPanelTexto(canvas.transform, "OptionsPanel", "Opciones",
                "Volumen, tamaño de texto y controles se agregan en el Sprint 2 (F17).", menu);

            Set(menu, ("titlePanel", title), ("menuPanel", menuPanel),
                      ("creditsPanel", credits), ("optionsPanel", options),
                      ("continueButton", btnContinuar), ("galleryButton", btnGaleria), ("firstSelected", btnNueva));

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

        static (GameObject, Button, Button, Button) ConstruirMenuPanel(Transform padre, MainMenu menu)
        {
            var panel = Panel(padre, "MenuPanel");
            Texto(panel.transform, "Titulo", "MIENTRAS ARDA LA VELA", 72f, ColAcento,
                  new Vector2(0f, 380f), new Vector2(1600f, 140f), TextAlignmentOptions.Center);

            // Siete botones con 92 px de separación (F23 añade Logros y Galería de bocetos).
            var nueva     = Boton(panel.transform, "BtnNuevaPartida", "Nueva partida",      new Vector2(0f,  230f), menu.OnNewGame);
            var continuar = Boton(panel.transform, "BtnContinuar",    "Continuar",          new Vector2(0f,  138f), menu.OnContinue);
            var opciones  = Boton(panel.transform, "BtnOpciones",     "Opciones",           new Vector2(0f,   46f), menu.OnOptions);
            var logros    = Boton(panel.transform, "BtnLogros",       "Logros",             new Vector2(0f,  -46f), menu.OnAchievements);
            var galeria   = Boton(panel.transform, "BtnGaleria",      "Galería de bocetos", new Vector2(0f, -138f), menu.OnGallery);
            var creditos  = Boton(panel.transform, "BtnCreditos",     "Créditos",           new Vector2(0f, -230f), menu.OnCredits);
            var salir     = Boton(panel.transform, "BtnSalir",        "Salir",              new Vector2(0f, -322f), menu.OnQuit);

            // Navegación explícita con gamepad/teclado
            Encadenar(new[] { nueva, continuar, opciones, logros, galeria, creditos, salir });
            return (panel, nueva, continuar, galeria);
        }


        /// <summary>
        /// Panel de créditos con ScrollRect. CREDITOS.txt ronda los 73 000 caracteres y un
        /// TextMeshPro no puede con eso de una pieza: el búfer de índices de una submalla es de
        /// 16 bits, o sea ~16 000 caracteres. Por eso el texto se reparte en bloques apilados
        /// dentro del contenido desplazable.
        /// </summary>
        static GameObject ConstruirCreditsPanel(Transform padre, MainMenu menu)
        {
            var panel = Panel(padre, "CreditsPanel");
            Texto(panel.transform, "Titulo", "Créditos", 72f, ColAcento,
                  new Vector2(0f, 430f), new Vector2(1600f, 120f), TextAlignmentOptions.Center);

            // --- ScrollRect ---
            var scroll = Hijo(panel.transform, "Scroll");
            var srt = Comp<RectTransform>(scroll);
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 10f);
            srt.sizeDelta = new Vector2(1500f, 700f);
            var fondo = Comp<Image>(scroll);
            fondo.color = new Color(0f, 0f, 0f, 0.25f);   // además hace de blanco para la rueda del ratón

            var rect = Comp<ScrollRect>(scroll);
            rect.horizontal = false;
            rect.vertical = true;
            rect.movementType = ScrollRect.MovementType.Clamped;
            rect.scrollSensitivity = 40f;

            // --- Viewport ---
            var viewport = Hijo(scroll.transform, "Viewport");
            var vrt = Comp<RectTransform>(viewport);
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = new Vector2(16f, 16f);
            vrt.offsetMax = new Vector2(-46f, -16f);      // hueco para la barra
            vrt.pivot = new Vector2(0f, 1f);
            Comp<RectMask2D>(viewport);

            // --- Contenido ---
            var content = Hijo(viewport.transform, "Content");
            var crt = Comp<RectTransform>(content);
            crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = new Vector2(0f, 0f); crt.offsetMax = new Vector2(0f, 0f);
            var vlg = Comp<VerticalLayoutGroup>(content);
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            vlg.spacing = 0f;
            var fitter = Comp<ContentSizeFitter>(content);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Limpiar bloques de una construcción anterior
            for (int i = content.transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(content.transform.GetChild(i).gameObject);

            var bloques = PartirTexto(LeerCreditos(), 6000);
            for (int i = 0; i < bloques.Count; i++)
            {
                var t = Hijo(content.transform, $"Bloque{i:D2}");
                Comp<RectTransform>(t);
                var tmp = Comp<TextMeshProUGUI>(t);
                tmp.text = bloques[i];
                tmp.fontSize = 24f;
                tmp.color = ColTexto;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
            }

            // --- Barra de desplazamiento ---
            var barra = Hijo(scroll.transform, "Scrollbar");
            var brt = Comp<RectTransform>(barra);
            brt.anchorMin = new Vector2(1f, 0f); brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot = new Vector2(1f, 1f);
            brt.offsetMin = new Vector2(-38f, 16f); brt.offsetMax = new Vector2(-8f, -16f);
            var bimg = Comp<Image>(barra);
            bimg.color = new Color(1f, 1f, 1f, 0.10f);
            var sb = Comp<Scrollbar>(barra);
            sb.direction = Scrollbar.Direction.BottomToTop;

            var area = Hijo(barra.transform, "Sliding Area");
            var art = Comp<RectTransform>(area);
            art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one;
            art.offsetMin = Vector2.zero; art.offsetMax = Vector2.zero;

            var handle = Hijo(area.transform, "Handle");
            var hrt = Comp<RectTransform>(handle);
            hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one;
            hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;
            var himg = Comp<Image>(handle);
            himg.color = ColAcento;
            sb.handleRect = hrt;
            sb.targetGraphic = himg;

            rect.content = crt;
            rect.viewport = vrt;
            rect.verticalScrollbar = sb;
            rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            var volver = Boton(panel.transform, "BtnVolver", "Volver", new Vector2(0f, -450f), menu.OnBack);
            // Con gamepad: del botón se sube a la barra y desde ahí se desplaza el texto.
            var nav = volver.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = sb;
            volver.navigation = nav;
            var navSb = sb.navigation;
            navSb.mode = Navigation.Mode.Explicit;
            navSb.selectOnDown = volver;
            sb.navigation = navSb;

            Debug.Log($"[MainMenu] Créditos: {LeerCreditos().Length} caracteres en {bloques.Count} bloques.");
            return panel;
        }

        /// <summary>Parte el texto en trozos de como mucho `maximo` caracteres, cortando por línea.</summary>
        static List<string> PartirTexto(string texto, int maximo)
        {
            var bloques = new List<string>();
            var actual = new System.Text.StringBuilder();
            foreach (var linea in texto.Replace("\r\n", "\n").Split('\n'))
            {
                if (actual.Length + linea.Length + 1 > maximo && actual.Length > 0)
                {
                    bloques.Add(actual.ToString());
                    actual.Clear();
                }
                actual.Append(linea).Append('\n');
            }
            if (actual.Length > 0) bloques.Add(actual.ToString());
            return bloques;
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
