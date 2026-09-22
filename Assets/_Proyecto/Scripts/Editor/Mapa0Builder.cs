using System.Collections.Generic;
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
    /// Construye la jerarquía del Mapa 0 "La ofrenda" (F02–F08, F10) en la escena abierta,
    /// siguiendo la sección 4.2 de LEEME_SETUP.md. Es idempotente: si un objeto ya existe lo reutiliza.
    ///
    /// NOTA IMPORTANTE (paso 5 del LEEME): este builder NO crea el DialogueRunner.
    /// Después de construir hay que arrastrar a la escena el prefab "Dialogue System" de Yarn Spinner,
    /// asignarle el Yarn Project (Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject)
    /// y agregarle el componente DialogueBridge en ese mismo GameObject.
    ///
    /// El trazado de las zonas (sala, cocina, patio) es una aproximación a la Figura 8 del
    /// Documento de Diseño; ajusta las constantes de abajo cuando tengas el plano definitivo.
    /// </summary>
    public static class Mapa0Builder
    {
        // ---------- Trazado del mapa, en tiles (1 tile = 1 unidad = 32 px) ----------
        static readonly RectInt Sala    = new RectInt(2, 10, 16, 11);   // sala con la ofrenda
        static readonly RectInt Cocina  = new RectInt(19, 10, 16, 11);
        static readonly RectInt Puerta  = new RectInt(18, 14, 1, 3);    // vano sala <-> cocina
        static readonly RectInt Pasillo = new RectInt(16, 7, 4, 3);     // bajada al patio
        static readonly RectInt Patio   = new RectInt(6, 1, 24, 6);

        // ---------- Posiciones de los objetos (centro de tile) ----------
        static readonly Vector3 PosAmbar    = new Vector3( 9.5f, 15.5f, 0f);
        static readonly Vector3 PosMemento  = new Vector3( 4.5f, 19.5f, 0f);
        static readonly Vector3 PosFoto     = new Vector3( 6.5f, 19.5f, 0f);
        static readonly Vector3 PosDelantal = new Vector3(31.5f, 18.5f, 0f);
        static readonly Vector3 PosPetalos  = new Vector3(17.5f,  4.5f, 0f);   // en la bifurcación
        static readonly Vector3 PosSombrero = new Vector3( 3.5f, 11.5f, 0f);
        static readonly Vector3 PosLucia    = new Vector3(13.5f, 12.5f, 0f);
        static readonly Vector3 PosNiebla   = new Vector3(18.0f,  8.5f, 0f);

        // ---------- Colores de relleno ----------
        static readonly Color ColSuelo   = new Color(0.44f, 0.34f, 0.26f);
        static readonly Color ColPatio   = new Color(0.30f, 0.36f, 0.24f);
        static readonly Color ColPared   = new Color(0.26f, 0.21f, 0.19f);
        static readonly Color ColMueble  = new Color(0.52f, 0.36f, 0.22f);
        static readonly Color ColDeco    = new Color(0.85f, 0.45f, 0.10f);   // cempasúchil
        static readonly Color ColAmbar   = new Color(0.58f, 0.30f, 0.68f);   // rebozo morado
        static readonly Color ColLucia   = new Color(0.45f, 0.55f, 0.80f);
        static readonly Color ColFragmento = new Color(0.95f, 0.85f, 0.45f);
        static readonly Color ColMementoC  = new Color(0.90f, 0.70f, 0.35f);
        static readonly Color ColNiebla    = new Color(0.55f, 0.55f, 0.58f);

        const int OrdenSuelo = 0, OrdenDeco = 1, OrdenMueble = 2, OrdenObjeto = 5, OrdenAmbar = 10, OrdenNiebla = 20;

        [MenuItem("Mientras arda la vela/Construir jerarquía Mapa 0")]
        public static void Construir()
        {
            var escena = EditorSceneManager.GetActiveScene();
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();

            ConstruirTilemaps();
            ConstruirLuzGlobal();
            var ambar = ConstruirAmbar();
            ConstruirCamara(ambar);
            ConstruirMemoryMap();
            ConstruirFragmentos();
            ConstruirNiebla();
            ConstruirMemento();
            ConstruirLucia();
            ConstruirTriggers();
            ConstruirDecorado();
            ConstruirHUD();

            Undo.SetCurrentGroupName("Construir jerarquía Mapa 0");
            Undo.CollapseUndoOperations(grupo);
            EditorSceneManager.MarkSceneDirty(escena);

            Debug.Log("[Mapa0] Jerarquía construida. FALTA a mano: arrastra el prefab 'Dialogue System' " +
                      "de Yarn Spinner a la escena, asígnale el Yarn Project " +
                      "(Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject) y agrégale el script DialogueBridge.");
        }

        /// <summary>
        /// Crea (o abre) Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity, construye la jerarquía,
        /// guarda la escena y la registra en la lista de escenas. Pensado para batchmode.
        /// </summary>
        public static void CrearEscenaYConstruir()
        {
            const string ruta = "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity";
            System.IO.Directory.CreateDirectory("Assets/_Proyecto/Scenes");

            var escena = System.IO.File.Exists(ruta)
                ? EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Construir();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ruta);
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
            RegistrarEnBuild(ruta);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Mapa0] Escena guardada en {ruta}");
        }

        static void RegistrarEnBuild(string ruta)
        {
            var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (lista.Exists(e => e.path == ruta)) return;
            lista.Add(new EditorBuildSettingsScene(ruta, true));
            EditorBuildSettings.scenes = lista.ToArray();
        }

        // ============================ Tilemaps ============================
        static void ConstruirTilemaps()
        {
            var grid = Raiz("Grid");
            Comp<Grid>(grid);

            var suelo = CapaTilemap(grid, "Suelo", OrdenSuelo, conCollider: false);
            var paredes = CapaTilemap(grid, "Paredes", OrdenSuelo, conCollider: true, composite: true);
            var muebles = CapaTilemap(grid, "Muebles", OrdenMueble, conCollider: true);
            var deco = CapaTilemap(grid, "Decoracion", OrdenDeco, conCollider: false);

            // Arte real de los tilesets LPC; si todavía no está importado, cuadros de color.
            bool hayArte = TilesetLPC.HayArte();
            if (hayArte) TilesetLPC.Preparar();

            TileBase T(string clave, Color relleno, bool collider = false)
                => (hayArte ? TilesetLPC.Tile(clave, collider) : null) ?? PlaceholderArt.Tile(clave, relleno);

            var tSueloCasa  = T("suelo_casa",  ColSuelo);
            var tSueloPatio = T("suelo_patio", ColPatio);
            var tMuroArriba = T("muro_arriba", ColPared);
            var tMuroCara   = T("muro_cara",   ColPared);
            var tMueble     = T("mueble",      ColMueble);
            var tDeco       = T("petalos",     ColDeco);
            var tDecoRalo   = hayArte ? T("petalos_ralo", ColDeco) : tDeco;

            // --- Suelo: interior + patio ---
            var transitable = new HashSet<Vector2Int>();
            Pintar(suelo, Sala, tSueloCasa, transitable);
            Pintar(suelo, Cocina, tSueloCasa, transitable);
            Pintar(suelo, Puerta, tSueloCasa, transitable);
            Pintar(suelo, Pasillo, tSueloCasa, transitable);
            Pintar(suelo, Patio, tSueloPatio, transitable);

            // --- Paredes: anillo alrededor de todo lo transitable ---
            Undo.RegisterCompleteObjectUndo(paredes, "Paredes Mapa 0");
            var muro = new HashSet<Vector2Int>();
            foreach (var c in transitable)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var v = new Vector2Int(c.x + dx, c.y + dy);
                        if (!transitable.Contains(v)) muro.Add(v);
                    }
            // muro interior entre sala y cocina (deja el vano de la puerta)
            for (int y = Sala.yMin; y < Sala.yMax; y++)
            {
                var v = new Vector2Int(18, y);
                if (!transitable.Contains(v)) muro.Add(v);
            }
            // Un muro con suelo transitable justo debajo es el muro del fondo de la habitación:
            // el jugador le ve la cara. El resto muestra la textura de la coronación.
            foreach (var v in muro)
            {
                bool esCara = transitable.Contains(new Vector2Int(v.x, v.y - 1));
                paredes.SetTile((Vector3Int)v, esCara ? tMuroCara : tMuroArriba);
            }

            // --- Muebles: mesa de la cocina y repisa de la sala ---
            Undo.RegisterCompleteObjectUndo(muebles, "Muebles Mapa 0");
            Pintar(muebles, new RectInt(24, 15, 5, 2), tMueble, null);   // mesa de la cocina
            Pintar(muebles, new RectInt(3, 19, 4, 1), tMueble, null);    // base de la ofrenda

            // --- Decoración: camino de pétalos, del patio a la ofrenda (sin collider) ---
            // En el patio se bifurca: es lo que Ámbar señala en el nodo M0_Frag_Petalos
            // ("llega hasta el patio... pero se abre en dos").
            Undo.RegisterCompleteObjectUndo(deco, "Decoracion Mapa 0");
            TileBase Petalo(int n) => n % 2 == 0 ? tDeco : tDecoRalo;   // alterna para que no se note el patrón
            for (int x = 5; x <= 17; x++) deco.SetTile(new Vector3Int(x, 18, 0), Petalo(x));   // ofrenda -> centro
            for (int y = 4; y <= 17; y++) deco.SetTile(new Vector3Int(17, y, 0), Petalo(y));   // sala, pasillo y patio
            for (int i = 1; i <= 5; i++)                                                        // bifurcación en el patio
            {
                deco.SetTile(new Vector3Int(17 - i, 4 - i > 1 ? 4 - i : 2, 0), Petalo(i));
                deco.SetTile(new Vector3Int(17 + i, 4 - i > 1 ? 4 - i : 2, 0), Petalo(i + 1));
            }
        }

        static Tilemap CapaTilemap(GameObject grid, string nombre, int orden, bool conCollider, bool composite = false)
        {
            var go = Hijo(grid.transform, nombre);
            var tm = Comp<Tilemap>(go);
            var tr = Comp<TilemapRenderer>(go);
            tr.sortingOrder = orden;

            if (conCollider)
            {
                var col = Comp<TilemapCollider2D>(go);
                if (composite)
                {
                    var rb = Comp<Rigidbody2D>(go);
                    rb.bodyType = RigidbodyType2D.Static;
                    var comp = Comp<CompositeCollider2D>(go);
                    comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
                    col.compositeOperation = Collider2D.CompositeOperation.Merge;
                }
            }
            return tm;
        }

        static void Pintar(Tilemap tm, RectInt r, TileBase tile, HashSet<Vector2Int> registro)
        {
            Undo.RegisterCompleteObjectUndo(tm, "Pintar Mapa 0");
            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                {
                    tm.SetTile(new Vector3Int(x, y, 0), tile);
                    registro?.Add(new Vector2Int(x, y));
                }
        }

        // ============================ Luz global ============================
        static void ConstruirLuzGlobal()
        {
            var go = Raiz("Global Light 2D");
            var luz = Comp<Light2D>(go);
            luz.lightType = Light2D.LightType.Global;
            luz.intensity = 0.25f;                                  // noche (LEEME §4.2.2)
            luz.color = new Color(0.65f, 0.70f, 0.95f);
            EditorUtility.SetDirty(luz);
        }

        // ============================ Ámbar ============================
        static GameObject ConstruirAmbar()
        {
            var go = Raiz("Ambar");
            go.transform.position = PosAmbar;

            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = SpriteDeAmbar();          // arte real si ya está importado; si no, relleno
            sr.sortingOrder = OrdenAmbar;

            // Animator (F02): Blend Trees 2D con MoveX/MoveY/Speed, generados por AmbarAnimator.
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Proyecto/Animations/Ambar/Ambar.controller");
            if (ctrl != null)
            {
                var anim = Comp<Animator>(go);
                anim.runtimeAnimatorController = ctrl;
                anim.applyRootMotion = false;
                anim.updateMode = AnimatorUpdateMode.Normal;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            else
            {
                Debug.LogWarning("[Mapa0] Falta Ambar.controller. Corre 'Generar Animator de Ámbar' antes.");
            }

            var rb = Comp<Rigidbody2D>(go);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // El sprite LPC tiene el pivote en los pies, así que el origen del transform está
            // en el suelo y el collider va justo encima de él.
            var cap = Comp<CapsuleCollider2D>(go);
            cap.direction = CapsuleDirection2D.Horizontal;
            cap.size = new Vector2(0.55f, 0.28f);
            cap.offset = new Vector2(0f, 0.14f);

            // Farol: hijo con Light2D de tipo Point, color ámbar
            var farol = Hijo(go.transform, "Farol");
            farol.transform.localPosition = new Vector3(0f, 0.75f, 0f);   // altura de la mano
            var luz = Comp<Light2D>(farol);
            luz.lightType = Light2D.LightType.Point;
            luz.color = new Color(1f, 0.78f, 0.42f);
            luz.intensity = 0.6f;
            luz.pointLightInnerRadius = 0.3f;
            luz.pointLightOuterRadius = 1.5f;
            EditorUtility.SetDirty(luz);

            // Aviso "[E] Examinar" en World Space
            var aviso = Hijo(go.transform, "Prompt");
            var tmp = Comp<TextMeshPro>(aviso);
            tmp.text = "[E] Examinar";
            tmp.fontSize = 2.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(6f, 1f);
            tmp.sortingOrder = OrdenNiebla + 1;
            aviso.SetActive(false);

            var mov = Comp<PlayerMovement>(go);
            Set(mov, ("speed", 3.2f), ("animator", go.GetComponent<Animator>()));

            var inter = Comp<PlayerInteractor>(go);
            Set(inter,
                ("radius", 0.9f),
                ("interactableMask", (LayerMask)(1 << LayerMask.NameToLayer("Interactable"))),
                ("prompt", tmp),
                ("promptOffset", new Vector3(0f, 1.1f, 0f)));

            var vela = Comp<CandleLight>(go);
            Set(vela, ("lanternLight", luz));

            return go;
        }


        /// <summary>Primer cuadro de reposo mirando al frente; si no hay arte, un cuadro de color.</summary>
        static Sprite SpriteDeAmbar()
        {
            const string hoja = "Assets/_Proyecto/Art/Personajes/ambar_walk.png";
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(hoja))
                if (o is Sprite s && s.name == "ambar_down_0") return s;
            return PlaceholderArt.Sprite("ambar", ColAmbar);
        }


        /// <summary>
        /// Cuadro de reposo mirando al frente de una hoja de personaje. Devuelve null si la hoja
        /// todavía no está cortada en sprites (sólo Ámbar lo está, vía AmbarAnimator).
        /// </summary>
        static Sprite SpriteDeHoja(string hoja, string sprite)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath($"Assets/_Proyecto/Art/Personajes/{hoja}.png"))
                if (o is Sprite s && s.name == sprite) return s;
            return null;
        }

        /// <summary>Decorado ambiental con los objetos recortados de los tilesets LPC.</summary>
        static void ConstruirDecorado()
        {
            if (!TilesetLPC.HayArte()) return;
            var padre = Raiz("Decorado");

            Adorno(padre.transform, "Candelabro",  "velas",  new Vector3( 3.5f, 19.5f, 0f));
            Adorno(padre.transform, "Candelabro2", "velas",  new Vector3( 6.5f, 19.5f, 0f));
            Adorno(padre.transform, "Fogon",       "estufa", new Vector3(21.5f, 19.5f, 0f));
            Adorno(padre.transform, "Maceta",      "maceta", new Vector3( 9.5f,  3.5f, 0f));
            Adorno(padre.transform, "Maceta2",     "maceta", new Vector3(26.5f,  3.5f, 0f));
        }

        static void Adorno(Transform padre, string nombre, string clave, Vector3 pos)
        {
            var sp = TilesetLPC.Sprite(clave);
            if (sp == null) return;
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = sp;
            sr.sortingOrder = OrdenMueble + 1;
        }

        // ============================ Cámara ============================
        static void ConstruirCamara(GameObject objetivo)
        {
            var camGo = GameObject.Find("Main Camera") ?? Raiz("Main Camera");
            camGo.tag = "MainCamera";
            var cam = Comp<Camera>(camGo);
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;                  // 360 / 2 / 32
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.08f);
            cam.transform.position = new Vector3(PosAmbar.x, PosAmbar.y, -10f);

            var ppc = Comp<PixelPerfectCamera>(camGo);
            ppc.assetsPPU = 32;
            ppc.refResolutionX = 640;
            ppc.refResolutionY = 360;
            EditorUtility.SetDirty(ppc);

            Comp<CinemachineBrain>(camGo);

            // Límite del mapa para el Confiner 2D
            var limite = Raiz("Limite del mapa");
            var poly = Comp<PolygonCollider2D>(limite);
            poly.isTrigger = true;
            float x0 = Sala.xMin - 1, x1 = Cocina.xMax + 1;
            float y0 = Patio.yMin - 1, y1 = Sala.yMax + 1;
            poly.points = new[]
            {
                new Vector2(x0, y0), new Vector2(x1, y0),
                new Vector2(x1, y1), new Vector2(x0, y1),
            };

            var vcamGo = Raiz("Cinemachine Camera");
            var vcam = Comp<CinemachineCamera>(vcamGo);
            vcam.Target.TrackingTarget = objetivo.transform;
            var follow = Comp<CinemachineFollow>(vcamGo);
            follow.FollowOffset = new Vector3(0f, 0f, -10f);
            var confiner = Comp<CinemachineConfiner2D>(vcamGo);
            confiner.BoundingShape2D = poly;
            EditorUtility.SetDirty(vcam);
        }

        // ============================ MemoryMap ============================
        static void ConstruirMemoryMap()
        {
            var go = Raiz("MemoryMap");
            var mm = Comp<MemoryMap>(go);
            mm.mapId = "M0";
            mm.fragmentPrefix = "M0_";
            mm.displayName = "La ofrenda — 2025";
            mm.requiredFragments = 4;
            mm.nextScene = SceneNames.Recuerdo1;
            EditorUtility.SetDirty(mm);
        }

        // ============================ Fragmentos ============================
        static void ConstruirFragmentos()
        {
            var padre = Raiz("Fragmentos");
            Fragmento(padre.transform, "Frag_Foto", PosFoto, "M0_foto", "M0_Frag_Foto", oculto: false);
            Fragmento(padre.transform, "Frag_Delantal", PosDelantal, "M0_delantal", "M0_Frag_Delantal", oculto: false);
            Fragmento(padre.transform, "Frag_Petalos", PosPetalos, "M0_petalos", "M0_Frag_Petalos", oculto: false);
            Fragmento(padre.transform, "Frag_Sombrero", PosSombrero, "M0_sombrero", "M0_Frag_Sombrero", oculto: true);
        }

        static void Fragmento(Transform padre, string nombre, Vector3 pos, string id, string nodo, bool oculto)
        {
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Interactable");

            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = PlaceholderArt.Sprite("fragmento", ColFragmento);
            sr.sortingOrder = OrdenObjeto;

            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = Vector2.one;

            // Destello que se apaga al reunirlo
            var destello = Hijo(go.transform, "Destello");
            destello.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            destello.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var srd = Comp<SpriteRenderer>(destello);
            srd.sprite = PlaceholderArt.Sprite("destello", Color.white, borde: false);
            srd.sortingOrder = OrdenObjeto + 1;

            var frag = Comp<MemoryFragment>(go);
            Set(frag,
                ("fragmentId", id),
                ("yarnNode", nodo),
                ("sparkle", destello),
                ("promptText", "[E] Examinar"));

            if (oculto)
            {
                var rev = Comp<Revealable>(go);
                Set(rev, ("revealId", $"{id}_rev"), ("fadeTime", 0.6f));
                SetArray(rev, "renderers", new Object[] { sr, srd });
            }
        }

        // ============================ Niebla del Olvido ============================
        static void ConstruirNiebla()
        {
            var go = Raiz("Niebla del Olvido");
            go.transform.position = PosNiebla;
            go.transform.localScale = new Vector3(4f, 3f, 1f);

            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = PlaceholderArt.Sprite("niebla", ColNiebla, borde: false);
            sr.color = new Color(1f, 1f, 1f, 0.75f);
            sr.sortingOrder = OrdenNiebla;

            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = false;                 // bloquea el paso (LEEME §4.2.8)
            col.size = Vector2.one;

            var fog = Comp<OblivionFog>(go);
            Set(fog,
                ("unlockBy", OblivionFog.Unlock.Fragment),
                ("unlockId", "M0_delantal"),
                ("fadeTime", 1.5f));
        }

        // ============================ Memento ============================
        static void ConstruirMemento()
        {
            var go = Raiz("Memento_PanDeMuerto");
            go.transform.position = PosMemento;
            go.layer = LayerMask.NameToLayer("Interactable");

            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = (TilesetLPC.HayArte() ? TilesetLPC.Sprite("pan") : null)
                        ?? PlaceholderArt.Sprite("memento", ColMementoC);
            sr.sortingOrder = OrdenObjeto;

            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = Vector2.one;

            var brillo = Hijo(go.transform, "Brillo");
            brillo.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
            var luz = Comp<Light2D>(brillo);
            luz.lightType = Light2D.LightType.Point;
            luz.color = new Color(1f, 0.85f, 0.5f);
            luz.intensity = 1.2f;
            luz.pointLightOuterRadius = 1.6f;
            brillo.SetActive(false);

            var mem = Comp<Memento>(go);
            Set(mem,
                ("lockedNode", "M0_Memento_Bloqueado"),
                ("activateNode", "M0_Memento_Activar"),
                ("glow", brillo),
                ("promptText", "[E] Examinar"));
        }

        // ============================ Lucía ============================
        static void ConstruirLucia()
        {
            var go = Raiz("Lucia");
            go.transform.position = PosLucia;
            go.layer = LayerMask.NameToLayer("Interactable");

            var sr = Comp<SpriteRenderer>(go);
            sr.sprite = SpriteDeHoja("lucia_19_walk", "lucia_19_down_0")
                        ?? PlaceholderArt.Sprite("lucia", ColLucia);
            sr.sortingOrder = OrdenObjeto;

            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = Vector2.one;

            var dlg = Comp<DialogueInteractable>(go);
            Set(dlg, ("yarnNode", "M0_Lucia"), ("promptText", "[E] Mirar a Lucía"));
        }

        // ============================ DialogueTriggers ============================
        static void ConstruirTriggers()
        {
            var padre = Raiz("Triggers");
            Trigger(padre.transform, "Trigger_Prologo", PosAmbar, new Vector2(2f, 2f),
                    "M0_Prologo", "M0_prologo", playOnStart: true);
            Trigger(padre.transform, "Trigger_TutorialVela", new Vector3(4.5f, 12.5f, 0f), new Vector2(5f, 4f),
                    "M0_TutorialVela", "M0_tut_vela", playOnStart: false);
            Trigger(padre.transform, "Trigger_TutorialNiebla", new Vector3(18f, 11.5f, 0f), new Vector2(6f, 2f),
                    "M0_TutorialNiebla", "M0_tut_niebla", playOnStart: false);
        }

        static void Trigger(Transform padre, string nombre, Vector3 pos, Vector2 tam, string nodo, string saveId, bool playOnStart)
        {
            var go = Hijo(padre, nombre);
            go.transform.position = pos;
            var col = Comp<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = tam;

            var t = Comp<DialogueTrigger>(go);
            Set(t, ("yarnNode", nodo), ("saveId", saveId), ("playOnStart", playOnStart));
        }

        // ============================ HUD ============================
        static void ConstruirHUD()
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
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(40f, -40f);
            rt.sizeDelta = new Vector2(600f, 140f);

            var nombreMapa = TextoHUD(panel.transform, "NombreMapa", "La ofrenda — 2025", 44f, new Vector2(0f, -10f));
            var contador = TextoHUD(panel.transform, "Contador", "0/4", 36f, new Vector2(0f, -70f));

            var rombos = new Object[4];
            var vacio = PlaceholderArt.Sprite("rombo_vacio", new Color(0.35f, 0.35f, 0.40f), borde: false);
            var lleno = PlaceholderArt.Sprite("rombo_lleno", ColFragmento, borde: false);
            for (int i = 0; i < 4; i++)
            {
                var r = Hijo(panel.transform, $"Rombo{i}");
                var rrt = Comp<RectTransform>(r);
                rrt.anchorMin = rrt.anchorMax = new Vector2(0f, 1f);
                rrt.pivot = new Vector2(0f, 1f);
                rrt.anchoredPosition = new Vector2(110f + i * 50f, -62f);
                rrt.sizeDelta = new Vector2(40f, 40f);
                var img = Comp<Image>(r);
                img.sprite = vacio;
                rombos[i] = img;
            }

            var ui = Comp<FragmentCounterUI>(go);
            Set(ui, ("mapNameText", nombreMapa), ("counterText", contador),
                    ("emptySlot", vacio), ("filledSlot", lleno));
            SetArray(ui, "slots", rombos);
        }

        static TMP_Text TextoHUD(Transform padre, string nombre, string texto, float tam, Vector2 pos)
        {
            var go = Hijo(padre, nombre);
            var rt = Comp<RectTransform>(go);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(560f, 56f);
            var t = Comp<TextMeshProUGUI>(go);
            t.text = texto;
            t.fontSize = tam;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Left;
            return t;
        }

        // ============================ Utilidades ============================
        static GameObject Raiz(string nombre)
        {
            var existente = GameObject.Find(nombre);
            if (existente != null) return existente;
            var go = new GameObject(nombre);
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
            return go;
        }

        static T Comp<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        /// <summary>Asigna campos [SerializeField] privados por nombre.</summary>
        static void Set(Object destino, params (string prop, object valor)[] valores)
        {
            var so = new SerializedObject(destino);
            foreach (var (prop, valor) in valores)
            {
                var p = so.FindProperty(prop);
                if (p == null)
                {
                    Debug.LogWarning($"[Mapa0] {destino.GetType().Name} no tiene el campo '{prop}'.");
                    continue;
                }
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
                    default: Debug.LogWarning($"[Mapa0] Tipo no soportado para '{prop}'."); break;
                }
            }
            so.ApplyModifiedProperties();
        }

        static void SetArray(Object destino, string prop, Object[] valores)
        {
            var so = new SerializedObject(destino);
            var p = so.FindProperty(prop);
            if (p == null || !p.isArray)
            {
                Debug.LogWarning($"[Mapa0] {destino.GetType().Name} no tiene el arreglo '{prop}'.");
                return;
            }
            p.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            so.ApplyModifiedProperties();
        }
    }
}
