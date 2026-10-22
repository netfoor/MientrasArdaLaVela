using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static MientrasArdaLaVela.EditorTools.ConstructorEscena;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Epílogo "Dos velas" (2 de noviembre de 2025, madrugada) — F21. La misma casa del Mapa 0,
    /// con la vela de la ofrenda casi consumida. Escena guiada, sin fragmentos ni mosaico:
    /// don Aurelio pide la segunda vela, Ámbar la enciende, Refugio llega por el otro camino
    /// de pétalos, los hermanos se reencuentran y se van juntos; Lucía despierta; créditos.
    /// </summary>
    public static class EpilogoBuilder
    {
        const string Ruta = "Assets/_Proyecto/Scenes/Epilogo_DosVelas.unity";
        const string Obj = "Assets/_Proyecto/Art/Objetos/";

        // Mismo trazado que Mapa0Builder: la casa es la misma.
        static readonly RectInt Sala    = new RectInt(2, 10, 16, 11);
        static readonly RectInt Cocina  = new RectInt(19, 10, 16, 11);
        static readonly RectInt Puerta  = new RectInt(18, 14, 1, 3);
        static readonly RectInt Pasillo = new RectInt(16, 7, 4, 3);
        static readonly RectInt Patio   = new RectInt(6, 1, 24, 6);

        static readonly Vector3 PosAmbar   = new Vector3(9.5f, 15.5f, 0f);
        static readonly Vector3 PosTizne   = new Vector3(8.3f, 15.3f, 0f);
        static readonly Vector3 PosAurelio = new Vector3(7.5f, 18.2f, 0f);
        static readonly Vector3 PosRefugio = new Vector3(22.5f, 2.4f, 0f);   // al final del otro camino
        static readonly Vector3 PosLucia   = new Vector3(13.5f, 12.5f, 0f);
        static readonly Vector3 PosVela2   = new Vector3(5.5f, 19.4f, 0f);

        [MenuItem("Mientras arda la vela/Construir Epílogo (dos velas)")]
        public static void ConstruirDesdeMenu()
        {
            if (!EscenaCorrecta(Ruta)) return;
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log("[Epilogo] Escena construida y guardada.");
        }

        public static void CrearEscenaYConstruir()
        {
            AbrirOCrear(Ruta);
            Construir();
            GuardarYRegistrar(Ruta);
            Debug.Log($"[Epilogo] Escena guardada en {Ruta}");
        }

        public static void Construir()
        {
            Undo.IncrementCurrentGroup();
            int grupo = Undo.GetCurrentGroup();
            if (TilesetLPC.HayArte()) TilesetLPC.Preparar();

            ConstruirTilemaps();
            LuzGlobal(0.12f, new Color(0.62f, 0.66f, 0.95f));       // MADRUGADA
            var ambar = Ambar(PosAmbar);
            ConstructorEscena.Tizne(ambar, PosTizne);
            Camara(ambar, new Rect(Sala.xMin - 1, Patio.yMin - 1, Cocina.xMax + 2 - Sala.xMin, Sala.yMax + 2 - Patio.yMin));

            var espiritu = new Color(0.78f, 0.86f, 1f, 0.72f);    // translúcidos, gris azulados
            Npc("Aurelio", PosAurelio, "aurelio_espiritu", "Aurelio", "abajo", "E_Aurelio", "[E] Hablar", espiritu);
            var refugio = Npc("Refugio", PosRefugio, "cuco_77", "Refugio", "arriba", null, "", espiritu);
            Set(refugio.GetComponent<CinematicActor>(), ("startHidden", true));
            Npc("Lucia", PosLucia, "lucia_19", "Lucia", "abajo");

            // La segunda vela: un actor que aparece con su luz al encenderla.
            var vela = Raiz("Vela2");
            vela.transform.position = PosVela2;
            var sr = Comp<SpriteRenderer>(vela);
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Obj + "veladora.png");
            sr.sortingOrder = OrdenObjeto + 1;
            LuzPuntual(vela.transform, "Luz", PosVela2 + new Vector3(0f, 0.3f, 0f), new Color(1f, 0.8f, 0.45f), 1.3f, 4f);
            Set(Comp<CinematicActor>(vela), ("actorId", "Vela2"), ("spriteRenderer", sr), ("startHidden", true));

            // Dónde se enciende: junto a la ofrenda.
            var lugar = Raiz("Lugar de la segunda vela");
            lugar.transform.position = PosVela2 + new Vector3(0f, -0.9f, 0f);
            lugar.layer = LayerMask.NameToLayer("Interactable");
            var col = Comp<BoxCollider2D>(lugar);
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 1f);
            Set(Comp<DialogueInteractable>(lugar), ("yarnNode", "E_Vela"), ("promptText", "[E] Encender la segunda vela"));

            var triggers = Raiz("Triggers").transform;
            Trigger(triggers, "Trigger_Escena", PosAmbar, new Vector2(2f, 2f), "E_Escena", "E_escena", true);

            ConstruirDecorado();
            DialogueSystem();

            Undo.SetCurrentGroupName("Construir Epílogo");
            Undo.CollapseUndoOperations(grupo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        static void ConstruirTilemaps()
        {
            var c = Tilemaps();
            var suelo = Tile("suelo_casa", new Color(0.44f, 0.34f, 0.26f));
            var patio = Tile("suelo_patio", new Color(0.30f, 0.36f, 0.24f));
            var muroArriba = Tile("muro_arriba", new Color(0.26f, 0.21f, 0.19f), collider: true);
            var muroCara = Tile("muro_cara", new Color(0.26f, 0.21f, 0.19f), collider: true);
            var mueble = Tile("mueble", new Color(0.52f, 0.36f, 0.22f), collider: true);
            var petalo = Tile("petalos", new Color(0.85f, 0.45f, 0.10f));
            var ralo = Tile("petalos_ralo", new Color(0.85f, 0.45f, 0.10f));

            var transitable = new HashSet<Vector2Int>();
            Pintar(c.Suelo, Sala, suelo, transitable);
            Pintar(c.Suelo, Cocina, suelo, transitable);
            Pintar(c.Suelo, Puerta, suelo, transitable);
            Pintar(c.Suelo, Pasillo, suelo, transitable);
            Pintar(c.Suelo, Patio, patio, transitable);
            var divisorio = Enumerable.Range(Sala.yMin, Sala.height).Select(y => new Vector2Int(18, y));
            Muros(c.Paredes, transitable, divisorio, muroArriba, muroCara);

            Pintar(c.Muebles, new RectInt(24, 15, 5, 2), mueble);   // mesa de la cocina
            Pintar(c.Muebles, new RectInt(3, 19, 4, 1), mueble);    // base de la ofrenda

            // Los dos caminos de pétalos del Mapa 0: el segundo es el de Refugio.
            Undo.RegisterCompleteObjectUndo(c.Deco, "Deco");
            UnityEngine.Tilemaps.TileBase P(int k) => k % 2 == 0 ? petalo : ralo;
            for (int x = 5; x <= 17; x++) c.Deco.SetTile(new Vector3Int(x, 18, 0), P(x));
            for (int y = 4; y <= 17; y++) c.Deco.SetTile(new Vector3Int(17, y, 0), P(y));
            for (int i = 1; i <= 5; i++)
            {
                int y = 4 - i > 1 ? 4 - i : 2;
                c.Deco.SetTile(new Vector3Int(17 - i, y, 0), P(i));
                c.Deco.SetTile(new Vector3Int(17 + i, y, 0), P(i + 1));
            }
        }

        static void ConstruirDecorado()
        {
            var p = Raiz("Decorado").transform;
            if (TilesetLPC.HayArte())
            {
                Adorno(p, "Candelabro", "velas", new Vector3(3.5f, 19.5f, 0f));
                Adorno(p, "Pan", "pan", new Vector3(4.5f, 19.5f, 0f), OrdenObjeto);
                Adorno(p, "Candelabro2", "velas", new Vector3(6.5f, 19.5f, 0f));
                Adorno(p, "Fogon", "estufa", new Vector3(21.5f, 19.5f, 0f));
                Adorno(p, "Maceta", "maceta", new Vector3(9.5f, 3.5f, 0f));
                Adorno(p, "Maceta2", "maceta", new Vector3(26.5f, 3.5f, 0f));
            }
            // La primera vela, casi consumida: luz baja que parpadea.
            var luces = Raiz("Luces").transform;
            LuzPuntual(luces, "Vela de la ofrenda", new Vector3(3.5f, 19.8f, 0f), new Color(1f, 0.72f, 0.4f), 0.55f, 2.2f);
            Comp<Parpadeo>(luces.Find("Vela de la ofrenda").gameObject);
        }
    }

    /// <summary>Comprueba el Epílogo contra su tabla del Documento de Diseño.</summary>
    public static class EpilogoVerificador
    {
        static int fallos;

        [MenuItem("Mientras arda la vela/Verificar Epílogo")]
        public static void Verificar()
        {
            fallos = 0;
            const string ruta = "Assets/_Proyecto/Scenes/Epilogo_DosVelas.unity";
            if (!System.IO.File.Exists(ruta)) { Ok("existe la escena del Epílogo", false); Fin(); return; }
            if (EditorSceneManager.GetActiveScene().path != ruta) EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            Ok("en la lista de compilación", EditorBuildSettings.scenes.Any(s => s.path == ruta && s.enabled));

            var actores = Object.FindObjectsByType<CinematicActor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var id in new[] { "Aurelio", "Refugio", "Lucia", "Vela2" })
                Ok($"actor '{id}'", actores.Any(a => a.ActorId == id));
            var vela = actores.FirstOrDefault(a => a.ActorId == "Vela2");
            Ok("la segunda vela lleva su luz", vela != null && vela.GetComponentInChildren<Light2D>() != null);

            var nodos = new List<string>();
            foreach (var d in Object.FindObjectsByType<DialogueInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                nodos.Add(new SerializedObject(d).FindProperty("yarnNode").stringValue);
            foreach (var t in Object.FindObjectsByType<DialogueTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                nodos.Add(new SerializedObject(t).FindProperty("yarnNode").stringValue);
            Ok("se puede encender la segunda vela (E_Vela)", nodos.Contains("E_Vela"));

            var runner = Object.FindAnyObjectByType<Yarn.Unity.DialogueRunner>();
            if (Ok("Dialogue System con Yarn Project y cinemáticas", runner != null && runner.YarnProject != null
                   && runner.GetComponent<CinematicCommands>() != null))
            {
                var existentes = new HashSet<string>(runner.YarnProject.NodeNames);
                foreach (var n in nodos.Where(x => !string.IsNullOrEmpty(x)).Distinct())
                    Ok($"Nodo Yarn '{n}' existe", existentes.Contains(n));
            }
            var tm = GameObject.Find("Grid")?.transform.Find("Paredes")?.GetComponent<CompositeCollider2D>();
            Ok("muros con geometría (B17)", tm != null && tm.pathCount > 0);
            Fin();
        }

        static void Fin() => Debug.Log($"### EPÍLOGO: {fallos} fallos ###");

        static bool Ok(string desc, bool cond)
        {
            if (cond) Debug.Log($"  ok   · {desc}");
            else { fallos++; Debug.LogError($"FALLA  · {desc}"); }
            return cond;
        }
    }
}
