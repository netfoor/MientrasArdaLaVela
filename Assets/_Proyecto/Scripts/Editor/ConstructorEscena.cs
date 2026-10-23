using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Piezas comunes para construir escenas de recuerdo (F14 en adelante). Es la misma lógica
    /// que Mapa0Builder, pero con parámetros: el Mapa 0 sigue con su constructor propio, que ya
    /// está verificado y jugado; los recuerdos nuevos se arman con esto.
    /// </summary>
    public static class ConstructorEscena
    {
        public const int OrdenSuelo = 0, OrdenDeco = 1, OrdenMueble = 2, OrdenObjeto = 5,
                         OrdenAmbar = 10, OrdenNiebla = 20;

        static readonly Color ColFragmento = new Color(0.95f, 0.85f, 0.45f);
        static readonly Color ColNiebla = new Color(0.55f, 0.55f, 0.58f);

        // ============================================================ escena
        public static bool EscenaCorrecta(string ruta)
        {
            var activa = EditorSceneManager.GetActiveScene();
            if (activa.path == ruta) return true;
            if (Application.isBatchMode) { Debug.LogError($"[Escena] La escena abierta no es {ruta}."); return false; }
            string abierta = string.IsNullOrEmpty(activa.path) ? "una escena sin guardar" : activa.path;
            if (!EditorUtility.DisplayDialog("Escena equivocada",
                    $"Este comando construye en la escena abierta ({abierta}).\n\n¿Abrir {ruta} y construir ahí?",
                    "Abrir y construir", "Cancelar")) return false;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (System.IO.File.Exists(ruta)) EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            else { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ruta); }
            return true;
        }

        public static void AbrirOCrear(string ruta)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ruta));
            if (System.IO.File.Exists(ruta)) EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        /// <summary>B17: genera la geometría de los colliders antes de guardar la escena.</summary>
        public static void GenerarColisiones()
        {
            var grid = GameObject.Find("Grid");
            if (grid != null) ColisionesTilemap.Regenerar(grid);
        }

        public static void GuardarYRegistrar(string ruta)
        {
            GenerarColisiones();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ruta);
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
            var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!lista.Exists(e => e.path == ruta))
            {
                lista.Add(new EditorBuildSettingsScene(ruta, true));
                EditorBuildSettings.scenes = lista.ToArray();
            }
            AssetDatabase.SaveAssets();
        }

        // ============================================================ tilemaps
        public class Capas { public Tilemap Suelo, Paredes, Muebles, Deco; }

        public static Capas Tilemaps()
        {
            var grid = Raiz("Grid");
            Comp<Grid>(grid);
            Comp<ColisionesTilemap>(grid);   // B17
            return new Capas
            {
                Suelo = CapaTilemap(grid, "Suelo", OrdenSuelo, false),
                Paredes = CapaTilemap(grid, "Paredes", OrdenSuelo, true, composite: true),
                Muebles = CapaTilemap(grid, "Muebles", OrdenMueble, true),
                Deco = CapaTilemap(grid, "Decoracion", OrdenDeco, false),
            };
        }

        static Tilemap CapaTilemap(GameObject grid, string nombre, int orden, bool conCollider, bool composite = false)
        {
            var go = Hijo(grid.transform, nombre);
            var tm = Comp<Tilemap>(go);
            Comp<TilemapRenderer>(go).sortingOrder = orden;
            if (conCollider)
            {
                var col = Comp<TilemapCollider2D>(go);
                if (composite)
                {
                    Comp<Rigidbody2D>(go).bodyType = RigidbodyType2D.Static;
                    Comp<CompositeCollider2D>(go).geometryType = CompositeCollider2D.GeometryType.Polygons;
                    col.compositeOperation = Collider2D.CompositeOperation.Merge;
                }
            }
            return tm;
        }

        /// <summary>Tile del catálogo de TilesetLPC; si no hay arte, cuadro de color.</summary>
        public static TileBase Tile(string clave, Color relleno, bool collider = false)
        {
            if (TilesetLPC.HayArte())
            {
                var t = TilesetLPC.Tile(clave, collider);
                if (t != null) return t;
            }
            return PlaceholderArt.Tile(clave, relleno);
        }

        public static void Pintar(Tilemap tm, RectInt r, TileBase tile, HashSet<Vector2Int> registro = null)
        {
            Undo.RegisterCompleteObjectUndo(tm, "Pintar");
            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                {
                    tm.SetTile(new Vector3Int(x, y, 0), tile);
                    registro?.Add(new Vector2Int(x, y));
                }
        }

        /// <summary>Anillo de muro alrededor de lo transitable, con la cara vista en los muros del fondo.</summary>
        public static void Muros(Tilemap paredes, HashSet<Vector2Int> transitable, IEnumerable<Vector2Int> extra,
                                 TileBase arriba, TileBase cara)
        {
            Undo.RegisterCompleteObjectUndo(paredes, "Muros");
            var muro = new HashSet<Vector2Int>();
            foreach (var c in transitable)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var v = new Vector2Int(c.x + dx, c.y + dy);
                        if (!transitable.Contains(v)) muro.Add(v);
                    }
            if (extra != null) foreach (var v in extra) if (!transitable.Contains(v)) muro.Add(v);
            foreach (var v in muro)
                paredes.SetTile((Vector3Int)v, transitable.Contains(new Vector2Int(v.x, v.y - 1)) ? cara : arriba);
        }

        // ============================================================ luz
        public static void LuzGlobal(float intensidad, Color color)
        {
            var go = Raiz("Global Light 2D");
            var luz = Comp<Light2D>(go);
            luz.lightType = Light2D.LightType.Global;
            luz.intensity = intensidad;
            luz.color = color;
            EditorUtility.SetDirty(luz);
        }

        public static void LuzPuntual(Transform padre, string nombre, Vector3 pos, Color color, float intensidad, float radio)
        {
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var luz = Comp<Light2D>(go);
            luz.lightType = Light2D.LightType.Point;
            luz.color = color;
            luz.intensity = intensidad;
            luz.pointLightInnerRadius = radio * 0.2f;
            luz.pointLightOuterRadius = radio;
            EditorUtility.SetDirty(luz);
        }

        // ============================================================ Ámbar y Tizne
        public static GameObject Ambar(Vector3 pos)
        {
            var go = Raiz("Ambar");
            go.transform.position = pos;
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = SpriteHoja("ambar", "down", 0) ?? PlaceholderArt.Sprite("ambar", new Color(0.58f, 0.30f, 0.68f));
            sr.sortingOrder = OrdenAmbar;

            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Proyecto/Animations/Ambar/Ambar.controller");
            if (ctrl != null)
            {
                var anim = Comp<Animator>(go);
                anim.runtimeAnimatorController = ctrl;
                anim.applyRootMotion = false;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            var rb = Comp<Rigidbody2D>(go);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var cap = Comp<CapsuleCollider2D>(go);
            cap.direction = CapsuleDirection2D.Horizontal;
            cap.size = new Vector2(0.55f, 0.28f);
            cap.offset = new Vector2(0f, 0.14f);

            var farol = Hijo(go.transform, "Farol");
            farol.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            var luz = Comp<Light2D>(farol);
            luz.lightType = Light2D.LightType.Point;
            luz.color = new Color(1f, 0.78f, 0.42f);
            luz.intensity = 0.6f;
            luz.pointLightInnerRadius = 0.3f;
            luz.pointLightOuterRadius = 1.5f;
            EditorUtility.SetDirty(luz);

            var aviso = Hijo(go.transform, "Prompt");
            var tmp = Comp<TextMeshPro>(aviso);
            tmp.text = "[E] Examinar";
            tmp.fontSize = 5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(8f, 1.2f);
            tmp.sortingOrder = OrdenNiebla + 1;
            aviso.SetActive(false);

            Set(Comp<PlayerMovement>(go), ("speed", 3.2f), ("animator", go.GetComponent<Animator>()));
            Set(Comp<PlayerInteractor>(go),
                ("radius", 0.9f),
                ("interactableMask", (LayerMask)(1 << LayerMask.NameToLayer("Interactable"))),
                ("prompt", tmp),
                ("promptOffset", new Vector3(0f, 1.1f, 0f)));
            Set(Comp<CandleLight>(go), ("lanternLight", luz));
            return go;
        }

        public static void Tizne(GameObject ambar, Vector3 pos)
        {
            var go = Raiz("Tizne");
            go.transform.position = pos;
            var sr = Comp<SpriteRenderer>(go);
            var frente = HojaTizne.Cuadros("down");
            sr.sprite = frente != null ? frente[0] : PlaceholderArt.Sprite("tizne", new Color(0.18f, 0.16f, 0.20f));
            sr.sortingOrder = OrdenAmbar - 1;

            var ladrido = Hijo(go.transform, "Ladrido");
            ladrido.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            var tmp = Comp<TextMeshPro>(ladrido);
            tmp.text = "¡Guau!";
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.88f, 0.55f);
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
            tmp.sortingOrder = OrdenNiebla + 1;
            ladrido.SetActive(false);

            var t = Comp<Tizne>(go);
            Set(t, ("target", ambar.transform), ("spriteRenderer", sr), ("barkBubble", ladrido));
            foreach (var dir in new[] { "up", "left", "down", "right" })
            {
                var c = HojaTizne.Cuadros(dir);
                if (c != null) SetArray(t, "walk" + char.ToUpperInvariant(dir[0]) + dir.Substring(1), c);
            }
            var variantes = HojaTizne.CuadrosVariantes();
            if (variantes != null) SetArray(t, "variantes", variantes);
        }

        // ============================================================ cámara
        public static void Camara(GameObject objetivo, Rect limites)
        {
            var camGo = GameObject.Find("Main Camera") ?? Raiz("Main Camera");
            camGo.tag = "MainCamera";
            var cam = Comp<Camera>(camGo);
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.08f);
            cam.transform.position = new Vector3(objetivo.transform.position.x, objetivo.transform.position.y, -10f);
            var ppc = Comp<PixelPerfectCamera>(camGo);
            ppc.assetsPPU = 32; ppc.refResolutionX = 640; ppc.refResolutionY = 360;
            EditorUtility.SetDirty(ppc);
            Comp<CinemachineBrain>(camGo);

            var limite = Raiz("Limite del mapa");
            var poly = Comp<PolygonCollider2D>(limite);
            poly.isTrigger = true;
            poly.points = new[]
            {
                new Vector2(limites.xMin, limites.yMin), new Vector2(limites.xMax, limites.yMin),
                new Vector2(limites.xMax, limites.yMax), new Vector2(limites.xMin, limites.yMax),
            };

            var vcamGo = Raiz("Cinemachine Camera");
            var vcam = Comp<CinemachineCamera>(vcamGo);
            vcam.Target.TrackingTarget = objetivo.transform;
            var lente = vcam.Lens;
            lente.OrthographicSize = 5.625f;     // igual que la Main Camera (B02)
            vcam.Lens = lente;
            Comp<CinemachineFollow>(vcamGo).FollowOffset = new Vector3(0f, 0f, -10f);
            Comp<CinemachineConfiner2D>(vcamGo).BoundingShape2D = poly;
            EditorUtility.SetDirty(vcam);
        }

        // ============================================================ diálogo
        public static void DialogueSystem(params (string clave, string ruta)[] vinetas)
        {
            const string rutaProyecto = "Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject";
            // Un .yarn NUEVO no siempre hace que Yarn Spinner recompile el proyecto (pasó con
            // Recuerdo1_Panaderia.yarn: el archivo se importó pero sus nodos no existían).
            // Se fuerza la recompilación antes de asignarlo.
            AssetDatabase.ImportAsset(rutaProyecto, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            var runner = Object.FindAnyObjectByType<Yarn.Unity.DialogueRunner>();
            if (runner == null)
            {
                runner = Yarn.Unity.Editor.CreateDialogueRunnerMenuItem.CreateDialogueRunner();
                if (runner == null) { Debug.LogError("[Escena] No se pudo crear el Dialogue System."); return; }
                Undo.RegisterCreatedObjectUndo(runner.gameObject, "Crear Dialogue System");
                runner.gameObject.name = "Dialogue System";
            }
            var proyecto = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(rutaProyecto);
            if (proyecto != null) Set(runner, ("yarnProject", proyecto));
            else Debug.LogError($"[Escena] Falta el Yarn Project en {rutaProyecto}.");
            Comp<DialogueBridge>(runner.gameObject);

            // Cinemáticas (comandos de Yarn). La caja de diálogo va por encima de su velo.
            var cine = Comp<CinematicCommands>(runner.gameObject);
            var so = new SerializedObject(cine);
            var arr = so.FindProperty("vinetas");
            arr.arraySize = vinetas.Length;
            for (int i = 0; i < vinetas.Length; i++)
            {
                var e = arr.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("clave").stringValue = vinetas[i].clave;
                e.FindPropertyRelative("imagen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(vinetas[i].ruta);
            }
            so.ApplyModifiedProperties();

            TicDeTexto(runner);

            foreach (var c in runner.GetComponentsInChildren<Canvas>(true))
            {
                if (!c.isRootCanvas) continue;
                c.sortingOrder = 100;
                EditorUtility.SetDirty(c);
            }
        }

        /// <summary>
        /// Tic del texto (S05, F22). El presentador de líneas de Yarn Spinner llama a
        /// <see cref="SonidoTexto"/> por cada carácter que escribe, pero sólo si está en su lista
        /// de handlers, que es un campo privado: hay que registrarlo por SerializedObject.
        /// Lo usan todos los constructores, incluido el del Mapa 0, que monta su diálogo aparte.
        /// </summary>
        public static void TicDeTexto(Yarn.Unity.DialogueRunner runner)
        {
            var presentador = runner.GetComponentInChildren<Yarn.Unity.LinePresenter>(true);
            if (presentador == null)
            {
                Debug.LogWarning("[Escena] No se encontró el LinePresenter: el tic del texto no suena.");
                return;
            }

            var tic = Comp<SonidoTexto>(presentador.gameObject);
            var so = new SerializedObject(presentador);
            var handlers = so.FindProperty("eventHandlers");
            if (handlers == null)
            {
                Debug.LogWarning("[Escena] LinePresenter no tiene 'eventHandlers': el tic del texto no suena.");
                return;
            }

            for (int i = 0; i < handlers.arraySize; i++)
                if (handlers.GetArrayElementAtIndex(i).objectReferenceValue == tic) return;   // ya estaba

            handlers.arraySize++;
            handlers.GetArrayElementAtIndex(handlers.arraySize - 1).objectReferenceValue = tic;
            so.ApplyModifiedProperties();
        }

        // ============================================================ memoria
        public static void MemoryMap(string mapId, string prefijo, string nombre, int requeridos, string siguiente)
        {
            var mm = Comp<MientrasArdaLaVela.MemoryMap>(Raiz("MemoryMap"));
            mm.mapId = mapId;
            mm.fragmentPrefix = prefijo;
            mm.displayName = nombre;
            mm.requiredFragments = requeridos;
            mm.nextScene = siguiente;
            EditorUtility.SetDirty(mm);
        }

        /// <summary>Fragmento de memoria. Con sprite null usa un cuadro de color de relleno.</summary>
        public static GameObject Fragmento(Transform padre, string nombre, Vector3 pos, string id, string nodo,
                                           bool oculto, Sprite sprite = null)
        {
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Interactable");
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = sprite != null ? sprite : PlaceholderArt.Sprite("fragmento", ColFragmento);
            sr.sortingOrder = OrdenObjeto;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = Vector2.one;

            var destello = Hijo(go.transform, "Destello");
            destello.transform.localPosition = new Vector3(0f, sprite != null ? sprite.bounds.extents.y + 0.35f : 0.6f, 0f);
            destello.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var srd = Comp<SpriteRenderer>(destello);
            srd.sprite = Objeto("destello.png") ?? PlaceholderArt.Sprite("destello", Color.white, borde: false);
            srd.sortingOrder = OrdenObjeto + 1;

            Set(Comp<MemoryFragment>(go), ("fragmentId", id), ("yarnNode", nodo), ("sparkle", destello),
                ("promptText", "[E] Examinar"));
            if (oculto)
            {
                var rev = Comp<Revealable>(go);
                Set(rev, ("revealId", $"{id}_rev"), ("fadeTime", 0.6f));
                SetArray(rev, "renderers", new Object[] { sr, srd });
            }
            return go;
        }

        public const string RutaPetalo = "Assets/_Proyecto/Art/Resources/UI/petalo.png";

        /// <summary>
        /// Los cinco pétalos de cempasúchil coleccionables del mapa (F24): IDs &lt;mapa&gt;_petalo1..5.
        /// Se recogen al pasar encima (trigger) y brillan un poco para verse en la penumbra.
        /// </summary>
        public static void Petalos(string mapId, params Vector3[] posiciones)
        {
            var padre = Raiz("Petalos").transform;
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(RutaPetalo);
            if (sp == null) Debug.LogWarning($"[Escena] Falta el sprite del pétalo en {RutaPetalo}.");
            for (int i = 0; i < posiciones.Length; i++)
            {
                var go = Hijo(padre, $"Petalo_{i + 1}");
                go.transform.position = posiciones[i];
                var sr = Comp<SpriteRenderer>(go);
                sr.sprite = sp != null ? sp : PlaceholderArt.Sprite("petalo", new Color(0.95f, 0.55f, 0.15f));
                sr.sortingOrder = OrdenObjeto;
                var col = Comp<CircleCollider2D>(go);
                col.isTrigger = true;
                col.radius = 0.35f;

                var brillo = Hijo(go.transform, "Brillo");
                var luz = Comp<Light2D>(brillo);
                luz.lightType = Light2D.LightType.Point;
                luz.color = new Color(1f, 0.62f, 0.25f);
                luz.intensity = 0.5f;
                luz.pointLightInnerRadius = 0.1f;
                luz.pointLightOuterRadius = 0.9f;
                EditorUtility.SetDirty(luz);

                Set(Comp<Petalo>(go), ("petalId", $"{mapId}_petalo{i + 1}"), ("sprite", sr), ("brillo", luz));
            }
        }

        public static void Niebla(string nombre, Vector3 pos, Vector2 escala, OblivionFog.Unlock modo, string id)
        {
            var go = Raiz(nombre);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(escala.x, escala.y, 1f);
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = PlaceholderArt.Sprite("niebla", ColNiebla, borde: false);
            sr.color = new Color(1f, 1f, 1f, 0.82f);
            sr.sortingOrder = OrdenNiebla;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = false;
            col.size = Vector2.one;
            Set(Comp<OblivionFog>(go), ("unlockBy", modo), ("unlockId", id), ("fadeTime", 1.5f));
        }

        public static void Memento(string nombre, Vector3 pos, Sprite sprite, string bloqueado, string activar,
                                   string resuelto, string rutaMosaico, string titulo)
        {
            var go = Raiz(nombre);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Interactable");
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = sprite != null ? sprite : PlaceholderArt.Sprite("memento", new Color(0.90f, 0.70f, 0.35f));
            sr.sortingOrder = OrdenObjeto;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = Vector2.one;

            var brillo = Hijo(go.transform, "Brillo");
            var luz = Comp<Light2D>(brillo);
            luz.lightType = Light2D.LightType.Point;
            luz.color = new Color(1f, 0.85f, 0.5f);
            luz.intensity = 1.2f;
            luz.pointLightOuterRadius = 1.6f;
            brillo.SetActive(false);

            var mem = Comp<MientrasArdaLaVela.Memento>(go);
            var imagen = AssetDatabase.LoadAssetAtPath<Sprite>(rutaMosaico);
            if (imagen == null) Debug.LogWarning($"[Escena] Falta la imagen del mosaico en {rutaMosaico}.");
            Set(mem, ("lockedNode", bloqueado), ("activateNode", activar), ("solvedNode", resuelto),
                ("glow", brillo), ("promptText", "[E] Examinar"), ("mosaicTitle", titulo), ("mosaicImage", imagen));
        }

        /// <summary>
        /// Personaje del recuerdo: sprite LPC, actor de cinemática y, si se da un nodo, diálogo al
        /// hablarle. Devuelve el objeto para añadirle más cosas (por ejemplo, un MemoryFragment).
        /// </summary>
        public static GameObject Npc(string nombre, Vector3 pos, string hoja, string actorId, string mirando,
                                     string nodo = null, string aviso = "[E] Hablar", Color? tinte = null)
        {
            var go = Raiz(nombre);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Interactable");
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = SpriteHoja(hoja, "down", 0) ?? PlaceholderArt.Sprite(hoja, new Color(0.5f, 0.5f, 0.6f));
            sr.sortingOrder = OrdenObjeto;
            if (tinte.HasValue) sr.color = tinte.Value;
            // Collider sólido a los pies: no se le atraviesa. El trigger de arriba es para hablarle.
            var pies = Comp<CapsuleCollider2D>(go);
            pies.direction = CapsuleDirection2D.Horizontal;
            pies.size = new Vector2(0.55f, 0.28f);
            pies.offset = new Vector2(0f, 0.14f);

            var actor = Comp<CinematicActor>(go);
            Set(actor, ("actorId", actorId), ("spriteRenderer", sr), ("startFacing", mirando));
            foreach (var (campo, dir) in new[] { ("up", "up"), ("left", "left"), ("down", "down"), ("right", "right") })
            {
                var cuadros = Enumerable.Range(0, 9).Select(i => SpriteHoja(hoja, dir, i)).ToArray();
                if (cuadros.All(c => c != null)) SetArray(actor, campo, cuadros);
            }
            if (!string.IsNullOrEmpty(nodo))
                Set(Comp<DialogueInteractable>(go), ("yarnNode", nodo), ("promptText", aviso));
            return go;
        }

        public static void Eco(string nombre, Vector3 pos, string hoja, string echoId, string fragmento,
                               string nodoFalso, string nodoVerdad)
        {
            var go = Raiz(nombre);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Interactable");
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = SpriteHoja(hoja, "down", 0) ?? PlaceholderArt.Sprite("eco", new Color(0.6f, 0.6f, 0.62f));
            sr.color = new Color(0.62f, 0.62f, 0.66f, 0.6f);    // silueta gris al 60 % (documento: Ecos)
            sr.sortingOrder = OrdenObjeto;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = false;                              // bloquea el paso hasta desmentirlo
            col.size = new Vector2(1f, 1f);
            col.offset = new Vector2(0f, 0.5f);
            Set(Comp<Echo>(go), ("echoId", echoId), ("requiredFragment", fragmento),
                ("falseNode", nodoFalso), ("truthNode", nodoVerdad));
        }

        public static void Trigger(Transform padre, string nombre, Vector3 pos, Vector2 tam, string nodo, string saveId, bool alIniciar)
        {
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = tam;
            Set(Comp<DialogueTrigger>(go), ("yarnNode", nodo), ("saveId", saveId), ("playOnStart", alIniciar));
        }

        public static void Adorno(Transform padre, string nombre, string clave, Vector3 pos, int orden = OrdenMueble + 1,
                                  bool solido = false)
        {
            var sp = TilesetLPC.HayArte() ? TilesetLPC.Sprite(clave) : null;
            if (sp == null) return;
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = sp;
            sr.sortingOrder = orden;
            if (solido) Comp<BoxCollider2D>(go).size = new Vector2(0.9f, 0.6f);
        }

        /// <summary>Adorno con un sprite suelto del proyecto (arte propio, p. ej. papel picado).</summary>
        /// <summary>
        /// Arte propio de objetos (`Art/Objetos`, dibujado por `tools/arte/objetos_f25.py`).
        /// Devuelve null y avisa si falta, para que quien lo pida caiga en su relleno de color.
        /// </summary>
        public static Sprite Objeto(string archivo)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Proyecto/Art/Objetos/" + archivo);
            if (sp == null) Debug.LogWarning($"[Escena] Falta el arte Art/Objetos/{archivo}; se usa un cuadro de color.");
            return sp;
        }

        /// <summary>
        /// Convierte una pieza de Art/Objetos en un Tile reutilizable (sin collider), guardado
        /// en Art/Tiles. Es lo que permite pintar con arte propio una capa de decoración, en vez
        /// de depender de un recorte del tileset LPC.
        /// </summary>
        public static TileBase TileObjeto(string archivo)
        {
            string clave = System.IO.Path.GetFileNameWithoutExtension(archivo);
            string ruta = $"Assets/_Proyecto/Art/Tiles/{clave}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(ruta);
            var sp = Objeto(archivo);
            if (sp == null) return tile;

            bool nuevo = tile == null;
            if (nuevo) tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sp;
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
            if (nuevo) AssetDatabase.CreateAsset(tile, ruta);
            else EditorUtility.SetDirty(tile);
            return tile;
        }

        public static void AdornoSprite(Transform padre, string nombre, string ruta, Vector3 pos, int orden)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sp == null) { Debug.LogWarning($"[Escena] Falta el sprite {ruta}."); return; }
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = sp;
            sr.sortingOrder = orden;
        }

        // ============================================================ interfaz
        public static void HUD(string nombreMapa)
        {
            var go = Raiz("HUD");
            var canvas = Comp<Canvas>(go);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escala = Comp<CanvasScaler>(go);
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            Comp<GraphicRaycaster>(go);

            var panel = Hijo(go.transform, "ContadorFragmentos");
            var rt = Comp<RectTransform>(panel);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(40f, -40f);
            rt.sizeDelta = new Vector2(600f, 140f);
            // 900 px de ancho: "La panadería cerrada — 2015" no cabía en 560 y se partía en dos líneas.
            var titulo = Texto(panel.transform, "NombreMapa", nombreMapa, 44f, new Vector2(0f, 1f), new Vector2(0f, -10f),
                               new Vector2(900f, 56f), TextAlignmentOptions.Left, Color.white);
            titulo.textWrappingMode = TextWrappingModes.NoWrap;
            var contador = Texto(panel.transform, "Contador", "0/4", 36f, new Vector2(0f, 1f), new Vector2(0f, -70f),
                                 new Vector2(560f, 56f), TextAlignmentOptions.Left, Color.white);

            var rombos = new Object[4];
            // Rombos del HUD: arte propio (Art/UI), con el relleno de color como respaldo.
            var vacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Proyecto/Art/UI/rombo_vacio.png")
                        ?? PlaceholderArt.Sprite("rombo_vacio", new Color(0.35f, 0.35f, 0.40f), borde: false);
            var lleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Proyecto/Art/UI/rombo_lleno.png")
                        ?? PlaceholderArt.Sprite("rombo_lleno", ColFragmento, borde: false);
            for (int i = 0; i < 4; i++)
            {
                var r = Hijo(panel.transform, $"Rombo{i}");
                var rrt = Comp<RectTransform>(r);
                rrt.anchorMin = rrt.anchorMax = new Vector2(0f, 1f);
                rrt.pivot = new Vector2(0.5f, 0.5f);
                rrt.anchoredPosition = new Vector2(127f + i * 50f, -98f);
                rrt.sizeDelta = new Vector2(34f, 34f);
                rrt.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var img = Comp<Image>(r);
                img.sprite = vacio;
                rombos[i] = img;
            }
            // El Texto de arriba usa pivote (0,1) y lo necesita el contador; el TextoHUD original
            // de Mapa0Builder hace lo mismo.

            var pista = Texto(go.transform, "PistaTizne", "[H] Tizne: olfatear", 32f, new Vector2(0f, 0f),
                              new Vector2(40f, 36f), new Vector2(720f, 50f), TextAlignmentOptions.BottomLeft,
                              new Color(1f, 0.86f, 0.55f));
            Set(Comp<TizneHintUI>(go), ("label", pista));

            var ui = Comp<FragmentCounterUI>(go);
            Set(ui, ("mapNameText", titulo), ("counterText", contador), ("emptySlot", vacio), ("filledSlot", lleno));
            SetArray(ui, "slots", rombos);
        }

        public static void Mosaico()
        {
            var go = Raiz("Mosaico");
            var canvas = Comp<Canvas>(go);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var escala = Comp<CanvasScaler>(go);
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 1f;
            Comp<GraphicRaycaster>(go);

            var panel = Hijo(go.transform, "Panel");
            var prt = Comp<RectTransform>(panel);
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = prt.offsetMax = Vector2.zero;
            Comp<Image>(panel).color = new Color(0.04f, 0.03f, 0.06f, 0.94f);

            var tablero = Hijo(panel.transform, "Tablero");
            var trt = Comp<RectTransform>(tablero);
            trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0f, -6f);
            trt.sizeDelta = new Vector2(830f, 830f);

            var titulo = Texto(panel.transform, "Titulo", "", 50f, new Vector2(0.5f, 1f), new Vector2(0f, -18f),
                               new Vector2(1600f, 70f), TextAlignmentOptions.Center, new Color(1f, 0.86f, 0.55f));
            var estado = Texto(panel.transform, "Estado", "Piezas en su lugar: 0/16", 34f, new Vector2(0f, 0.5f),
                               new Vector2(60f, 40f), new Vector2(460f, 120f), TextAlignmentOptions.Left, Color.white);
            var pista = Texto(panel.transform, "Pista", "[H] Pista de Tizne", 30f, new Vector2(0f, 0.5f),
                              new Vector2(60f, -60f), new Vector2(460f, 120f), TextAlignmentOptions.Left, new Color(1f, 0.86f, 0.55f));
            Texto(panel.transform, "Controles",
                  "Flechas, WASD o mouse: elegir pieza   ·   Q / E o clic izq. / der.: girar   ·   R: reiniciar   ·   Esc: salir",
                  28f, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(1800f, 50f),
                  TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.85f));

            Set(Comp<MosaicPuzzle>(go), ("panel", panel), ("board", trt), ("titleText", titulo),
                ("statusText", estado), ("hintText", pista));
            panel.SetActive(false);
        }

        static TMP_Text Texto(Transform padre, string nombre, string texto, float tam, Vector2 ancla, Vector2 pos,
                              Vector2 tamRect, TextAlignmentOptions alin, Color color)
        {
            var go = Hijo(padre, nombre);
            var rt = Comp<RectTransform>(go);
            rt.anchorMin = rt.anchorMax = rt.pivot = ancla;
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamRect;
            var t = Comp<TextMeshProUGUI>(go);
            t.text = texto;
            t.fontSize = tam;
            t.alignment = alin;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        // ============================================================ utilidades
        public static Sprite SpriteHoja(string prefijo, string dir, int cuadro)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath($"Assets/_Proyecto/Art/Personajes/{prefijo}_walk.png"))
                if (o is Sprite s && s.name == $"{prefijo}_{dir}_{cuadro}") return s;
            return null;
        }

        public static GameObject Raiz(string nombre)
        {
            var existente = GameObject.Find(nombre);
            if (existente != null) return existente;
            var go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            return go;
        }

        public static GameObject Hijo(Transform padre, string nombre)
        {
            var t = padre.Find(nombre);
            if (t != null) { t.localScale = Vector3.one; return t.gameObject; }
            var go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            Undo.SetTransformParent(go.transform, padre, $"Anidar {nombre}");
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;    // SetTransformParent conserva la escala de mundo
            return go;
        }

        public static T Comp<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        public static void Set(Object destino, params (string prop, object valor)[] valores)
        {
            var so = new SerializedObject(destino);
            foreach (var (prop, valor) in valores)
            {
                var p = so.FindProperty(prop);
                if (p == null) { Debug.LogWarning($"[Escena] {destino.GetType().Name} no tiene el campo '{prop}'."); continue; }
                switch (valor)
                {
                    case string s: p.stringValue = s; break;
                    case bool b: p.boolValue = b; break;
                    case float f: p.floatValue = f; break;
                    case LayerMask lm: p.intValue = lm.value; break;
                    case System.Enum e: p.enumValueIndex = System.Convert.ToInt32(e); break;
                    case int i: p.intValue = i; break;
                    case Vector3 v: p.vector3Value = v; break;
                    case Vector2 v2: p.vector2Value = v2; break;
                    case Object o: p.objectReferenceValue = o; break;
                    case null: p.objectReferenceValue = null; break;
                    default: Debug.LogWarning($"[Escena] Tipo no soportado para '{prop}'."); break;
                }
            }
            so.ApplyModifiedProperties();
        }

        public static void SetArray(Object destino, string prop, Object[] valores)
        {
            var so = new SerializedObject(destino);
            var p = so.FindProperty(prop);
            if (p == null || !p.isArray) { Debug.LogWarning($"[Escena] {destino.GetType().Name} no tiene el arreglo '{prop}'."); return; }
            p.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            so.ApplyModifiedProperties();
        }
    }
}
