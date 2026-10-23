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

        // ---------- Camino de pétalos (sección 5.1 del Documento de Diseño) ----------
        // Se escribe tile a tile, cada uno compartiendo lado con el anterior: trazadas en
        // diagonal pura, las ramas quedaban con huecos y la bifurcación se leía como dos
        // rayas sueltas en lugar de un camino que se abre.
        static readonly Vector2Int[] RamaIzquierda = Tiles((16, 4), (16, 3), (15, 3), (14, 3), (14, 2),
                                                           (13, 2), (12, 2), (12, 1), (11, 1), (10, 1));
        static readonly Vector2Int[] RamaDerecha   = Tiles((18, 4), (18, 3), (19, 3), (20, 3), (20, 2),
                                                           (21, 2), (22, 2), (22, 1), (23, 1), (24, 1));
        // Cruz de pétalos al pie de la ofrenda, como se pone en una casa el 1 de noviembre.
        static readonly Vector2Int[] CruzOfrenda   = Tiles((5, 18), (5, 17), (5, 16), (5, 15), (5, 14),
                                                           (3, 16), (4, 16), (6, 16), (7, 16));

        static Vector2Int[] Tiles(params (int x, int y)[] puntos)
        {
            var r = new Vector2Int[puntos.Length];
            for (int i = 0; i < puntos.Length; i++) r[i] = new Vector2Int(puntos[i].x, puntos[i].y);
            return r;
        }

        // ---------- Posiciones de los objetos (centro de tile) ----------
        static readonly Vector3 PosAmbar    = new Vector3( 9.5f, 15.5f, 0f);
        static readonly Vector3 PosMemento  = new Vector3( 4.5f, 19.5f, 0f);
        static readonly Vector3 PosFoto     = new Vector3( 6.5f, 19.5f, 0f);
        static readonly Vector3 PosDelantal = new Vector3(31.5f, 18.5f, 0f);
        static readonly Vector3 PosPetalos  = new Vector3(17.5f,  4.5f, 0f);   // en la bifurcación
        static readonly Vector3 PosSombrero = new Vector3( 3.5f, 11.5f, 0f);
        static readonly Vector3 PosLucia    = new Vector3(13.5f, 12.5f, 0f);
        static readonly Vector3 PosNiebla   = new Vector3(18.0f,  8.5f, 0f);
        static readonly Vector3 PosTizne    = new Vector3( 8.3f, 15.3f, 0f);   // a la izquierda de Ámbar

        const string RutaMosaico = "Assets/_Proyecto/Art/Mosaicos/mosaico_M0.png";

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
        static readonly Color ColTizne     = new Color(0.18f, 0.16f, 0.20f);

        /// <summary>
        /// Intensidad de la luz global (noche). El LEEME §4.2.2 fija 0.25, valor pensado cuando
        /// los tiles eran cuadros de color oscuros; con la duela y la piedra definitivas, que son
        /// mucho más claras, a 0.25 la escena se lee como una habitación iluminada y el charco
        /// del farol se pierde. El Documento de Diseño v1.2 y el LEEME ya recogen el 0.14.
        /// </summary>
        const float LuzNocturna = 0.14f;

        const int OrdenSuelo = 0, OrdenDeco = 1, OrdenMueble = 2, OrdenObjeto = 5, OrdenAmbar = 10, OrdenNiebla = 20;

        const string RutaEscena = "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity";

        /// <summary>
        /// Punto de entrada del menú. Construye SIEMPRE en la escena abierta, así que primero
        /// comprueba que sea la correcta: ejecutarlo con MainMenu abierta metía el mapa entero
        /// dentro del menú.
        /// </summary>
        [MenuItem("Mientras arda la vela/Construir jerarquía Mapa 0")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta()) return;
            Construir();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Mapa0] Escena reconstruida y guardada.");
        }

        static bool EscenaCorrecta()
        {
            var activa = EditorSceneManager.GetActiveScene();
            if (activa.path == RutaEscena) return true;

            string abierta = string.IsNullOrEmpty(activa.path) ? "una escena sin guardar" : activa.path;
            if (Application.isBatchMode)
            {
                Debug.LogError($"[Mapa0] La escena abierta es {abierta}, no {RutaEscena}.");
                return false;
            }
            bool abrir = EditorUtility.DisplayDialog(
                "Escena equivocada",
                $"Este comando construye dentro de la escena que esté abierta, y ahora mismo lo está {abierta}.\n\n" +
                "Si continúas ahí, el Mapa 0 se metería dentro de esa escena.\n\n" +
                "¿Abrir Mapa0_Ofrenda y construir ahí?",
                "Abrir Mapa 0 y construir", "Cancelar");
            if (!abrir) return false;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            return true;
        }

        public static void Construir()
        {
            var escena = EditorSceneManager.GetActiveScene();
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();

            ConstruirTilemaps();
            ConstruirLuzGlobal();
            var ambar = ConstruirAmbar();
            ConstruirTizne(ambar);
            ConstruirCamara(ambar);
            ConstruirMemoryMap();
            ConstruirFragmentos();
            // Pétalos coleccionables (F24): rincones de la sala, la cocina y el patio.
            ConstructorEscena.Petalos("M0", new Vector3(15.5f, 19.5f, 0f), new Vector3(33.5f, 11.5f, 0f), new Vector3(26.5f, 13.5f, 0f), new Vector3(7.5f, 2.5f, 0f), new Vector3(28.5f, 5.5f, 0f));
            ConstruirNiebla();
            ConstruirMemento();
            ConstruirLucia();
            ConstruirTriggers();
            ConstruirDecorado();
            ConstruirHUD();
            ConstruirMosaico();
            ConstruirDialogueSystem();

            ColisionesTilemap.Regenerar(GameObject.Find("Grid"));   // B17: geometría antes de guardar
            Undo.SetCurrentGroupName("Construir jerarquía Mapa 0");
            Undo.CollapseUndoOperations(grupo);
            EditorSceneManager.MarkSceneDirty(escena);

            Debug.Log("[Mapa0] Jerarquía construida, Dialogue System incluido.");
        }

        /// <summary>
        /// Crea (o abre) Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity, construye la jerarquía,
        /// guarda la escena y la registra en la lista de escenas. Pensado para batchmode.
        /// </summary>
        public static void CrearEscenaYConstruir()
        {
            const string ruta = RutaEscena;
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
            Comp<ColisionesTilemap>(grid);   // B17: colliders reconstruidos al cargar

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
            var tMuroArriba = T("muro_arriba", ColPared, collider: true);   // sin esto los muros no bloquean
            var tMuroCara   = T("muro_cara",   ColPared, collider: true);
            // La capa Muebles lleva TilemapCollider2D, pero sin esto el tile pedía ColliderType.None
            // y la mesa de la cocina y la base de la ofrenda se podían atravesar (ver B17).
            var tMueble     = T("mueble",      ColMueble, collider: true);
            // Pétalos: arte propio (tools/arte/camino_petalos.py). El recorte del tileset de
            // plantas que se usaba antes parecía hierba suelta y repetía siempre el mismo cuadro.
            var tPetalos = new[]
            {
                ConstructorEscena.TileObjeto("camino_denso.png") ?? PlaceholderArt.Tile("petalos", ColDeco),
                ConstructorEscena.TileObjeto("camino_medio.png") ?? PlaceholderArt.Tile("petalos", ColDeco),
                ConstructorEscena.TileObjeto("camino_ralo.png")  ?? PlaceholderArt.Tile("petalos", ColDeco),
            };
            var tFlor = ConstructorEscena.TileObjeto("camino_flor.png") ?? tPetalos[0];

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

            // --- Decoración: el camino de pétalos ---
            // Entra desde el patio, donde se abre en dos (es lo que Ámbar señala en M0_Frag_Petalos:
            // "Lucía siempre pone uno solo. ¿Para quién será el otro?"), sube por el pasillo, cruza
            // la sala y termina en la cruz de pétalos al pie de la ofrenda.
            Undo.RegisterCompleteObjectUndo(deco, "Decoracion Mapa 0");
            int grano = 0;
            void Petalos(int x, int y) => deco.SetTile(new Vector3Int(x, y, 0), tPetalos[grano++ % tPetalos.Length]);
            void Flor(int x, int y) => deco.SetTile(new Vector3Int(x, y, 0), tFlor);

            for (int x = 5; x <= 17; x++) Petalos(x, 18);                 // de la ofrenda al centro de la sala
            for (int y = 4; y <= 17; y++) Petalos(17, y);                 // sala, pasillo y patio
            foreach (var t in RamaIzquierda) Petalos(t.x, t.y);
            foreach (var t in RamaDerecha) Petalos(t.x, t.y);
            // La cruz va toda con el tile denso: con la densidad alterna del camino se perdía
            // su forma, que es lo único que la distingue del tramo que llega hasta ella.
            foreach (var t in CruzOfrenda) deco.SetTile(new Vector3Int(t.x, t.y, 0), tPetalos[0]);

            // Flores enteras donde la escena las pide: el punto en que el camino se parte (ahí
            // está el fragmento), el centro de la cruz y el final de cada rama.
            Flor(17, 4); Flor(5, 16); Flor(10, 1); Flor(24, 1);
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
            luz.intensity = LuzNocturna;
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
            tmp.fontSize = 5f;            // ~16 px en pantalla a PPU 32
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(8f, 1.2f);
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
            // El lente del vcam DEBE coincidir con el de la Main Camera: el Confiner 2D calcula
            // el recorte con este valor, y el brain se lo impone a la cámara. Con el 10 que trae
            // por defecto, el encuadre calculado mide casi todo el mapa y la cámara deja de seguir
            // a Ámbar, además de pelearse con el Pixel Perfect Camera.
            var lente = vcam.Lens;
            lente.OrthographicSize = 5.625f;          // 360 / 2 / 32 PPU
            vcam.Lens = lente;
            var follow = Comp<CinemachineFollow>(vcamGo);
            follow.FollowOffset = new Vector3(0f, 0f, -10f);
            var confiner = Comp<CinemachineConfiner2D>(vcamGo);
            confiner.BoundingShape2D = poly;
            EditorUtility.SetDirty(vcam);
        }


        // ============================ Dialogue System ============================
        /// <summary>
        /// Instancia el prefab "Dialogue System" de Yarn Spinner, le asigna el Yarn Project
        /// y le añade el DialogueBridge (LEEME §4.2.5).
        ///
        /// El P3 original decía dejar esto para hacerlo a mano, pero Yarn Spinner expone
        /// Yarn.Unity.Editor.CreateDialogueRunnerMenuItem.CreateDialogueRunner() como público:
        /// es exactamente lo que ejecuta el menú GameObject > Yarn Spinner > Dialogue System.
        /// </summary>
        static void ConstruirDialogueSystem()
        {
            const string rutaProyecto = "Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject";

            var existente = Object.FindAnyObjectByType<Yarn.Unity.DialogueRunner>();
            if (existente == null)
            {
                existente = Yarn.Unity.Editor.CreateDialogueRunnerMenuItem.CreateDialogueRunner();
                if (existente == null) { Debug.LogError("[Mapa0] No se pudo crear el Dialogue System."); return; }
                Undo.RegisterCreatedObjectUndo(existente.gameObject, "Crear Dialogue System");
                existente.gameObject.name = "Dialogue System";
            }

            var proyecto = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(rutaProyecto);
            if (proyecto == null)
            {
                Debug.LogError($"[Mapa0] Falta el Yarn Project en {rutaProyecto}. " +
                               "Corre 'Setup > Crear Yarn Project' antes.");
            }
            else
            {
                // Por SerializedObject para que quede registrado en la escena y en el Undo.
                Set(existente, ("yarnProject", proyecto));
            }

            Comp<DialogueBridge>(existente.gameObject);
            ConstructorEscena.TicDeTexto(existente);   // S05, F22
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
            // Arte propio de cada fragmento (F25): los cuatro eran el mismo cuadro de color.
            Fragmento(padre.transform, "Frag_Foto", PosFoto, "M0_foto", "M0_Frag_Foto", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_foto_joven.png"));
            Fragmento(padre.transform, "Frag_Delantal", PosDelantal, "M0_delantal", "M0_Frag_Delantal", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_delantal.png"));
            Fragmento(padre.transform, "Frag_Petalos", PosPetalos, "M0_petalos", "M0_Frag_Petalos", oculto: false,
                      sprite: ConstructorEscena.Objeto("frag_petalos.png"));
            Fragmento(padre.transform, "Frag_Sombrero", PosSombrero, "M0_sombrero", "M0_Frag_Sombrero", oculto: true,
                      sprite: ConstructorEscena.Objeto("frag_sombrero.png"));
        }

        static void Fragmento(Transform padre, string nombre, Vector3 pos, string id, string nodo, bool oculto,
                              Sprite sprite = null)
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

            // Destello que se apaga al reunirlo
            var destello = Hijo(go.transform, "Destello");
            destello.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            destello.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var srd = Comp<SpriteRenderer>(destello);
            srd.sprite = ConstructorEscena.Objeto("destello.png") ?? PlaceholderArt.Sprite("destello", Color.white, borde: false);
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
            sr.sprite = ConstructorEscena.Objeto("memento_pan.png")
                        ?? (TilesetLPC.HayArte() ? TilesetLPC.Sprite("pan") : null)
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
                ("solvedNode", "M0_Mosaico_Resuelto"),
                ("glow", brillo),
                ("promptText", "[E] Examinar"),
                ("mosaicTitle", "El pan de muerto de dos piezas"));

            // Imagen del mosaico (F12). Si falta, el Memento se salta el puzle con un aviso.
            var imagen = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMosaico);
            if (imagen == null) Debug.LogWarning($"[Mapa0] Falta la imagen del mosaico en {RutaMosaico}.");
            Set(mem, ("mosaicImage", imagen));
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
                // Pivote al centro: el giro de 45° es sobre el pivote, y con el pivote en la
                // esquina el rombo se desplazaba en vez de girar en su sitio.
                rrt.pivot = new Vector2(0.5f, 0.5f);
                // El contador ocupa de -70 a -126: su centro está en -98, y ahí van los rombos.
                rrt.anchoredPosition = new Vector2(127f + i * 50f, -98f);
                rrt.sizeDelta = new Vector2(34f, 34f);
                // El LEEME §4.2.12 pide rombos y el sprite de relleno es cuadrado: se gira 45°.
                // A 34 px la diagonal mide 48 y aún cabe en la separación de 50.
                rrt.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var img = Comp<Image>(r);
                img.sprite = vacio;
                rombos[i] = img;
            }

            // Aviso de la pista de Tizne (F11), abajo a la izquierda.
            var pista = Hijo(go.transform, "PistaTizne");
            var prt = Comp<RectTransform>(pista);
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0f, 0f);
            prt.anchoredPosition = new Vector2(40f, 36f);
            prt.sizeDelta = new Vector2(720f, 50f);
            var tp = Comp<TextMeshProUGUI>(pista);
            tp.text = "[H] Tizne: olfatear";
            tp.fontSize = 32f;
            tp.alignment = TextAlignmentOptions.BottomLeft;
            tp.color = new Color(1f, 0.86f, 0.55f);
            var th = Comp<TizneHintUI>(go);
            Set(th, ("label", tp));

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

        // ============================ Tizne (F11) ============================
        static void ConstruirTizne(GameObject ambar)
        {
            var go = Raiz("Tizne");
            go.transform.position = PosTizne;

            var sr = Comp<SpriteRenderer>(go);
            var frente = HojaTizne.Cuadros("down");
            sr.sprite = frente != null ? frente[0] : PlaceholderArt.Sprite("tizne", ColTizne);
            sr.sortingOrder = OrdenAmbar - 1;
            // Sin collider a propósito: sigue la estela de Ámbar y, al olfatear, puede entrar
            // en la niebla (Documento de Diseño, personaje Tizne).

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
                var cuadros = HojaTizne.Cuadros(dir);
                string campo = "walk" + char.ToUpperInvariant(dir[0]) + dir.Substring(1);
                if (cuadros != null) SetArray(t, campo, cuadros);
                else Debug.LogWarning($"[Mapa0] Tizne sin cuadros '{dir}': falta cortar tizne_dog.png.");
            }
            var variantes = HojaTizne.CuadrosVariantes();   // accesorios (F23)
            if (variantes != null) SetArray(t, "variantes", variantes);
        }

        // ============================ Puzle de mosaico (F12) ============================
        /// <summary>
        /// Canvas del puzle, oculto hasta que el Memento lo abre. Las 16 piezas las crea
        /// MosaicPuzzle al abrirse, con la imagen del memento de turno.
        /// </summary>
        static void ConstruirMosaico()
        {
            var go = Raiz("Mosaico");
            var canvas = Comp<Canvas>(go);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;                          // sobre el HUD
            var escala = Comp<CanvasScaler>(go);
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 1f;                    // manda el alto: el tablero siempre cabe
            Comp<GraphicRaycaster>(go);

            var panel = Hijo(go.transform, "Panel");
            var prt = Comp<RectTransform>(panel);
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = prt.offsetMax = Vector2.zero;
            var fondo = Comp<Image>(panel);
            fondo.color = new Color(0.04f, 0.03f, 0.06f, 0.94f);

            var tablero = Hijo(panel.transform, "Tablero");
            var trt = Comp<RectTransform>(tablero);
            trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0f, -6f);
            trt.sizeDelta = new Vector2(830f, 830f);

            var titulo = TextoPuzle(panel.transform, "Titulo", "", 50f,
                                    new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(1600f, 70f),
                                    TextAlignmentOptions.Center, new Color(1f, 0.86f, 0.55f));
            var estado = TextoPuzle(panel.transform, "Estado", "Piezas en su lugar: 0/16", 34f,
                                    new Vector2(0f, 0.5f), new Vector2(60f, 40f), new Vector2(460f, 120f),
                                    TextAlignmentOptions.Left, Color.white);
            var pista = TextoPuzle(panel.transform, "Pista", "[H] Pista de Tizne", 30f,
                                   new Vector2(0f, 0.5f), new Vector2(60f, -60f), new Vector2(460f, 120f),
                                   TextAlignmentOptions.Left, new Color(1f, 0.86f, 0.55f));
            TextoPuzle(panel.transform, "Controles",
                       "Flechas, WASD o mouse: elegir pieza   ·   Q / E o clic izq. / der.: girar   ·   R: reiniciar   ·   Esc: salir",
                       28f, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(1800f, 50f),
                       TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.85f));

            var puzle = Comp<MosaicPuzzle>(go);
            Set(puzle, ("panel", panel), ("board", trt), ("titleText", titulo),
                       ("statusText", estado), ("hintText", pista));

            panel.SetActive(false);
        }

        static TMP_Text TextoPuzle(Transform padre, string nombre, string texto, float tam, Vector2 ancla,
                                   Vector2 pos, Vector2 tamRect, TextAlignmentOptions alin, Color color)
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
            if (t != null)
            {
                // Normaliza también lo que ya existía: una construcción anterior pudo dejar
                // la escala inflada. Si el llamador quiere otra escala, la pone después.
                t.localScale = Vector3.one;
                return t.gameObject;
            }
            var go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            Undo.SetTransformParent(go.transform, padre, $"Anidar {nombre}");
            go.transform.localPosition = Vector3.zero;
            // SetTransformParent conserva la escala de MUNDO. Bajo un Canvas, cuyo transform
            // lleva el factor de escala de la interfaz, eso le mete al hijo una escala
            // compensatoria (3x con referencia 1920x1080): el HUD salía gigante y encimado.
            go.transform.localScale = Vector3.one;
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
