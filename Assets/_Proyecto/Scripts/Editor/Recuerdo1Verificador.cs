using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>Comprueba la escena del Recuerdo 1 (F14) contra su tabla del Documento de Diseño.</summary>
    public static class Recuerdo1Verificador
    {
        [MenuItem("Mientras arda la vela/Verificar Recuerdo 1")]
        public static void Verificar() => VerificadorRecuerdo.Verificar(
            "Recuerdo1_Panaderia", "R1", SceneNames.Recuerdo2,
            new[] { "R1_charola", "R1_foto", "R1_lata", "R1_pregunta" }, oculto: "R1_lata",
            ecos: new[] { ("R1_eco", "R1_lata") }, nieblas: 2, actores: new[] { "Aurelio69", "Marisol", "Lucia9" });
    }

    /// <summary>Comprueba la escena del Recuerdo 2 (F15): 4 fragmentos, 2 Ecos, Olvido en zaguán y mesa.</summary>
    public static class Recuerdo2Verificador
    {
        [MenuItem("Mientras arda la vela/Verificar Recuerdo 2")]
        public static void Verificar() => VerificadorRecuerdo.Verificar(
            "Recuerdo2_Boda", "R2", SceneNames.Recuerdo3,
            new[] { "R2_brindis", "R2_rosa", "R2_tarjeta", "R2_telegrama" }, oculto: "R2_telegrama",
            ecos: new[] { ("R2_eco1", "R2_tarjeta"), ("R2_eco2", "R2_telegrama") }, nieblas: 2,
            actores: new[] { "Aurelio42", "Rosa", "Marisol" });
    }

    /// <summary>Recuerdo 3 (F19): 4 fragmentos (uno lo da la cinemática), 3 Ecos, 3 zonas de Olvido.</summary>
    public static class Recuerdo3Verificador
    {
        [MenuItem("Mientras arda la vela/Verificar Recuerdo 3")]
        public static void Verificar() => VerificadorRecuerdo.Verificar(
            "Recuerdo3_Horno", "R3", SceneNames.Recuerdo4,
            new[] { "R3_cama", "R3_lena", "R3_nota" }, oculto: "R3_nota",
            ecos: new[] { ("R3_eco1", "R3_cama"), ("R3_eco2", "R3_nota"), ("R3_eco3", "R3_lena") }, nieblas: 3,
            actores: new[] { "Ernesto", "Refugio23", "Aurelio20" });
    }

    /// <summary>Recuerdo 4 (F20): sin Ecos, 3 franjas de Olvido, la canica la da la cinemática.</summary>
    public static class Recuerdo4Verificador
    {
        [MenuItem("Mientras arda la vela/Verificar Recuerdo 4")]
        public static void Verificar() => VerificadorRecuerdo.Verificar(
            "Recuerdo4_Campo", "R4", SceneNames.Epilogo,
            new[] { "R4_huellas", "R4_promesa", "R4_vela" }, oculto: "R4_vela",
            ecos: new (string, string)[0], nieblas: 3, actores: new[] { "Aurelio9", "Refugio12" });
    }

    /// <summary>Comprobaciones comunes a todas las escenas de recuerdo.</summary>
    public static class VerificadorRecuerdo
    {
        static int fallos;
        static string etiqueta;

        /// <summary>
        /// F24: exactamente cinco pétalos con IDs &lt;mapa&gt;_petalo1..5, con sprite, trigger y sin
        /// tapar otro objeto interactuable. Lo usa también el verificador del Mapa 0.
        /// </summary>
        public static bool PetalosOk(string mapId, out string detalle)
        {
            var petalos = Object.FindObjectsByType<Petalo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var ids = petalos.Select(p => new SerializedObject(p).FindProperty("petalId").stringValue).OrderBy(x => x).ToArray();
            var esperados = Enumerable.Range(1, 5).Select(i => $"{mapId}_petalo{i}").ToArray();
            detalle = string.Join(",", ids);
            if (!ids.SequenceEqual(esperados)) return false;
            foreach (var p in petalos)
            {
                var sr = p.GetComponent<SpriteRenderer>();
                var col = p.GetComponent<Collider2D>();
                if (sr == null || sr.sprite == null) { detalle = $"{p.name} sin sprite"; return false; }
                if (col == null || !col.isTrigger) { detalle = $"{p.name} sin trigger"; return false; }
                var encima = Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(i => Vector2.Distance(i.transform.position, p.transform.position) < 0.6f);
                if (encima != null) { detalle = $"{p.name} tapa a {encima.name}"; return false; }
            }
            return true;
        }

        public static void Verificar(string escena, string mapId, string siguiente, string[] fragmentos, string oculto,
                                     (string id, string fragmento)[] ecos, int nieblas, string[] actores)
        {
            fallos = 0;
            etiqueta = escena;
            string ruta = $"Assets/_Proyecto/Scenes/{escena}.unity";
            if (!System.IO.File.Exists(ruta)) { Falla($"no existe {ruta}: corre su constructor"); Fin(); return; }
            if (EditorSceneManager.GetActiveScene().path != ruta) EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            Ok("Muros sólidos (todos los tiles de Paredes con collider)", MurosSolidos(out var dm), dm);
            Ok("La escena está en la lista de compilación",
               EditorBuildSettings.scenes.Any(s => s.path == ruta && s.enabled));

            var mm = Object.FindAnyObjectByType<MemoryMap>();
            if (Ok("MemoryMap presente", mm != null))
            {
                Ok($"  mapId {mapId}, prefijo {mapId}_, 4 fragmentos", mm.mapId == mapId && mm.fragmentPrefix == mapId + "_" && mm.requiredFragments == 4);
                Ok($"  siguiente escena = {siguiente}", mm.nextScene == siguiente, mm.nextScene);
            }

            var nodos = new List<string>();
            var frags = Object.FindObjectsByType<MemoryFragment>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var ids = frags.Select(f => new SerializedObject(f).FindProperty("fragmentId").stringValue).OrderBy(x => x).ToArray();
            Ok($"Fragmentos en la escena ({string.Join(", ", fragmentos)})",
               ids.SequenceEqual(fragmentos.OrderBy(x => x)), string.Join(",", ids));
            foreach (var f in frags)
            {
                var so = new SerializedObject(f);
                nodos.Add(so.FindProperty("yarnNode").stringValue);
                Ok($"  {f.name} en la capa Interactable", f.gameObject.layer == LayerMask.NameToLayer("Interactable"));
            }
            var enDiario = Diario.Recuerdos.Where(r => r.MapId == mapId).SelectMany(r => r.Fragmentos).Select(f => f.Id).ToList();
            Ok("Cada fragmento tiene su entrada en el Diario (Core/Diario.cs)", fragmentos.All(enDiario.Contains),
               string.Join(",", fragmentos.Where(f => !enDiario.Contains(f))));
            var escondido = frags.FirstOrDefault(f => f.FragmentId == oculto);
            Ok($"'{oculto}' está oculto (Revealable, se descubre con la vela)", escondido != null && escondido.GetComponent<Revealable>() != null);

            Ok($"Cinco pétalos de cempasúchil {mapId}_petalo1..5 (F24)", PetalosOk(mapId, out var dp), dp);

            var todosEcos = Object.FindObjectsByType<Echo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Ok($"{ecos.Length} Eco(s) (tabla del nivel)", todosEcos.Length == ecos.Length, todosEcos.Length.ToString());
            foreach (var (id, fragmento) in ecos)
            {
                var eco = todosEcos.FirstOrDefault(e => new SerializedObject(e).FindProperty("echoId").stringValue == id);
                if (!Ok($"Eco '{id}'", eco != null)) continue;
                var so = new SerializedObject(eco);
                Ok($"  lo desmiente '{fragmento}'", so.FindProperty("requiredFragment").stringValue == fragmento);
                var col = eco.GetComponent<Collider2D>();
                Ok("  bloquea el paso (collider sólido)", col != null && !col.isTrigger);
                nodos.Add(so.FindProperty("falseNode").stringValue);
                nodos.Add(so.FindProperty("truthNode").stringValue);
            }

            var todasNieblas = Object.FindObjectsByType<OblivionFog>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Ok($"Zonas de Olvido: {nieblas}", todasNieblas.Length == nieblas, todasNieblas.Length.ToString());

            var mem = Object.FindAnyObjectByType<Memento>();
            if (Ok("Memento presente", mem != null))
            {
                var so = new SerializedObject(mem);
                foreach (var p in new[] { "lockedNode", "activateNode", "solvedNode" }) nodos.Add(so.FindProperty(p).stringValue);
                Ok("  imagen del mosaico asignada", so.FindProperty("mosaicImage").objectReferenceValue != null);
            }

            foreach (var t in Object.FindObjectsByType<DialogueTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                nodos.Add(new SerializedObject(t).FindProperty("yarnNode").stringValue);
            foreach (var d in Object.FindObjectsByType<DialogueInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                nodos.Add(new SerializedObject(d).FindProperty("yarnNode").stringValue);

            foreach (var id in actores)
            {
                var actor = Object.FindObjectsByType<CinematicActor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                                  .FirstOrDefault(a => a.ActorId == id);
                if (!Ok($"Actor de cinemática '{id}'", actor != null)) continue;
                var so = new SerializedObject(actor);
                Ok($"  '{id}' con 9 cuadros en cada dirección",
                   new[] { "up", "left", "down", "right" }.All(p => so.FindProperty(p).arraySize == 9));
            }

            Ok("Ámbar, Tizne, HUD y mosaico presentes",
               GameObject.Find("Ambar") && GameObject.Find("Tizne") && GameObject.Find("HUD") && Object.FindAnyObjectByType<MosaicPuzzle>());

            var runner = Object.FindAnyObjectByType<Yarn.Unity.DialogueRunner>();
            if (Ok("Dialogue System con Yarn Project", runner != null && runner.YarnProject != null))
            {
                Ok("  DialogueBridge y CinematicCommands en el mismo objeto",
                   runner.GetComponent<DialogueBridge>() && runner.GetComponent<CinematicCommands>());
                var yp = runner.YarnProject;
                var existentes = new HashSet<string>(yp.NodeNames);
                foreach (var n in nodos.Where(x => !string.IsNullOrEmpty(x)).Distinct())
                    Ok($"Nodo Yarn '{n}' existe", existentes.Contains(n));
            }
            Fin();
        }


        /// <summary>Todos los tiles de "Paredes" deben tener collider (bug del 2026-09-27: se atravesaban).</summary>
        static bool MurosSolidos(out string detalle)
        {
            detalle = "sin Grid/Paredes";
            var t = GameObject.Find("Grid")?.transform.Find("Paredes");
            var tm = t != null ? t.GetComponent<UnityEngine.Tilemaps.Tilemap>() : null;
            if (tm == null) return false;
            int total = 0, sinCollider = 0;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                if (!tm.HasTile(pos)) continue;
                total++;
                if (tm.GetColliderType(pos) == UnityEngine.Tilemaps.Tile.ColliderType.None) sinCollider++;
            }
            var comp = tm.GetComponent<CompositeCollider2D>();
            int caminos = comp != null ? comp.pathCount : -1;
            detalle = $"{sinCollider} de {total} sin collider; composite con {caminos} contornos";
            // B17: un composite guardado sin contornos no bloquea nada en juego.
            return total > 0 && sinCollider == 0 && (comp == null || caminos > 0);
        }

        static void Fin() => Debug.Log($"### {etiqueta}: {fallos} fallos ###");

        static bool Ok(string desc, bool cond, string detalle = null)
        {
            if (cond) Debug.Log($"  ok   · {desc}");
            else Falla(desc + (detalle != null ? $" (valor: '{detalle}')" : ""));
            return cond;
        }

        static void Falla(string desc) { fallos++; Debug.LogError($"FALLA  · {desc}"); }
    }
}
