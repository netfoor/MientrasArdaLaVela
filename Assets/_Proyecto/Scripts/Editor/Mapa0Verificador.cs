using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace MientrasArdaLaVela.EditorTools
{
    /// <summary>
    /// Comprueba la escena Mapa0_Ofrenda contra la sección 4.2 de LEEME_SETUP.md.
    /// Verifica también que todo nodo de Yarn referenciado exista en el Yarn Project.
    /// </summary>
    public static class Mapa0Verificador
    {
        static int fallos, avisos;
        static readonly List<string> nodosUsados = new List<string>();

        [MenuItem("Mientras arda la vela/Verificar Mapa 0")]
        public static void Verificar()
        {
            fallos = 0; avisos = 0; nodosUsados.Clear();
            const string ruta = "Assets/_Proyecto/Scenes/Mapa0_Ofrenda.unity";
            if (EditorSceneManager.GetActiveScene().path != ruta)
                EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

            VerificarTilemaps();
            VerificarLuz();
            VerificarAmbar();
            VerificarCamara();
            VerificarMemoryMap();
            VerificarFragmentos();
            VerificarNiebla();
            VerificarMemento();
            VerificarLucia();
            VerificarTriggers();
            VerificarHUD();
            VerificarNodosYarn();

            Debug.Log($"### RESULTADO: {fallos} fallos, {avisos} avisos ###");
        }

        // ---------- comprobaciones ----------
        static void VerificarTilemaps()
        {
            var grid = Buscar("Grid"); if (grid == null) return;
            Ok("Grid tiene componente Grid", grid.GetComponent<Grid>() != null);
            foreach (var capa in new[] { "Suelo", "Paredes", "Muebles", "Decoracion" })
            {
                var t = grid.transform.Find(capa);
                if (!Ok($"Tilemap '{capa}' existe", t != null)) continue;
                var tm = t.GetComponent<Tilemap>();
                Ok($"  '{capa}' tiene tiles pintados", tm != null && tm.GetUsedTilesCount() > 0,
                   $"count={tm?.GetUsedTilesCount()}");
            }
            var paredes = grid.transform.Find("Paredes");
            if (paredes != null)
            {
                var rb = paredes.GetComponent<Rigidbody2D>();
                Ok("Paredes: Rigidbody2D Static", rb != null && rb.bodyType == RigidbodyType2D.Static);
                Ok("Paredes: CompositeCollider2D", paredes.GetComponent<CompositeCollider2D>() != null);
                var tc = paredes.GetComponent<TilemapCollider2D>();
                Ok("Paredes: TilemapCollider2D en modo Merge",
                   tc != null && tc.compositeOperation == Collider2D.CompositeOperation.Merge);
            }
            var deco = grid.transform.Find("Decoracion");
            Ok("Decoracion SIN collider", deco != null && deco.GetComponent<TilemapCollider2D>() == null);
        }

        static void VerificarLuz()
        {
            var go = Buscar("Global Light 2D"); if (go == null) return;
            var l = go.GetComponent<Light2D>();
            Ok("Luz global: tipo Global", l != null && l.lightType == Light2D.LightType.Global);
            Ok("Luz global: intensidad 0.25", l != null && Mathf.Approximately(l.intensity, 0.25f), $"={l?.intensity}");
        }

        static void VerificarAmbar()
        {
            var go = Buscar("Ambar"); if (go == null) return;
            var rb = go.GetComponent<Rigidbody2D>();
            Ok("Ámbar: Rigidbody2D Dynamic", rb != null && rb.bodyType == RigidbodyType2D.Dynamic);
            Ok("Ámbar: gravityScale 0", rb != null && rb.gravityScale == 0f);
            Ok("Ámbar: Interpolate", rb != null && rb.interpolation == RigidbodyInterpolation2D.Interpolate);
            // "A los pies" = pequeño y en la parte baja del sprite. Con el arte LPC el pivote
            // está en los pies (offset positivo); con el cuadro de relleno está al centro
            // (offset negativo). La comprobación no debe depender de eso.
            var cap = go.GetComponent<CapsuleCollider2D>();
            Ok("Ámbar: CapsuleCollider2D pequeño y a la altura de los pies",
               cap != null && cap.size.y <= 0.4f && cap.offset.y + cap.size.y * 0.5f < 0.5f,
               cap != null ? $"offset.y={cap.offset.y}, size.y={cap.size.y}" : "sin collider");
            Ok("Ámbar: PlayerMovement", go.GetComponent<PlayerMovement>() != null);

            var pi = go.GetComponent<PlayerInteractor>();
            if (Ok("Ámbar: PlayerInteractor", pi != null))
            {
                var so = new SerializedObject(pi);
                int mask = so.FindProperty("interactableMask").intValue;
                int capa = LayerMask.NameToLayer("Interactable");
                Ok("  Interactable Mask incluye la capa Interactable", capa >= 0 && (mask & (1 << capa)) != 0,
                   $"mask={mask}, capa={capa}");
                Ok("  Prompt TMP asignado", so.FindProperty("prompt").objectReferenceValue != null);
            }

            var cl = go.GetComponent<CandleLight>();
            if (Ok("Ámbar: CandleLight", cl != null))
            {
                var so = new SerializedObject(cl);
                var luz = so.FindProperty("lanternLight").objectReferenceValue as Light2D;
                Ok("  lanternLight apunta a una Light2D Point", luz != null && luz.lightType == Light2D.LightType.Point);
                Ok("  esa luz es el hijo 'Farol'", luz != null && luz.gameObject.name == "Farol");
            }
        }

        static void VerificarCamara()
        {
            var cam = Buscar("Main Camera"); if (cam == null) return;
            var ppc = cam.GetComponent<PixelPerfectCamera>();
            Ok("Cámara: Pixel Perfect PPU 32", ppc != null && ppc.assetsPPU == 32);
            Ok("Cámara: resolución de referencia 640×360",
               ppc != null && ppc.refResolutionX == 640 && ppc.refResolutionY == 360);
            Ok("Cámara: CinemachineBrain", cam.GetComponent<CinemachineBrain>() != null);

            var vcam = Buscar("Cinemachine Camera"); if (vcam == null) return;
            var cc = vcam.GetComponent<CinemachineCamera>();
            Ok("Cinemachine Camera sigue a Ámbar",
               cc != null && cc.Target.TrackingTarget != null && cc.Target.TrackingTarget.name == "Ambar");
            var conf = vcam.GetComponent<CinemachineConfiner2D>();
            Ok("Confiner 2D con PolygonCollider2D",
               conf != null && conf.BoundingShape2D is PolygonCollider2D);
        }

        static void VerificarMemoryMap()
        {
            var go = Buscar("MemoryMap"); if (go == null) return;
            var mm = go.GetComponent<MemoryMap>();
            if (!Ok("MemoryMap presente", mm != null)) return;
            Ok("MemoryMap: mapId M0", mm.mapId == "M0", mm.mapId);
            Ok("MemoryMap: prefijo M0_", mm.fragmentPrefix == "M0_", mm.fragmentPrefix);
            Ok("MemoryMap: 4 fragmentos requeridos", mm.requiredFragments == 4, $"{mm.requiredFragments}");
            Ok("MemoryMap: nextScene Recuerdo1_Panaderia", mm.nextScene == SceneNames.Recuerdo1, mm.nextScene);
            Ok("MemoryMap: displayName", mm.displayName == "La ofrenda — 2025", mm.displayName);
        }

        static void VerificarFragmentos()
        {
            var tabla = new (string obj, string id, string nodo, bool oculto)[]
            {
                ("Frag_Foto",     "M0_foto",     "M0_Frag_Foto",     false),
                ("Frag_Delantal", "M0_delantal", "M0_Frag_Delantal", false),
                ("Frag_Petalos",  "M0_petalos",  "M0_Frag_Petalos",  false),
                ("Frag_Sombrero", "M0_sombrero", "M0_Frag_Sombrero", true),
            };
            var padre = Buscar("Fragmentos"); if (padre == null) return;
            foreach (var (obj, id, nodo, oculto) in tabla)
            {
                var t = padre.transform.Find(obj);
                if (!Ok($"Fragmento '{obj}' existe", t != null)) continue;
                var f = t.GetComponent<MemoryFragment>();
                if (!Ok($"  '{obj}' tiene MemoryFragment", f != null)) continue;
                var so = new SerializedObject(f);
                Ok($"  '{obj}' fragmentId = {id}", so.FindProperty("fragmentId").stringValue == id,
                   so.FindProperty("fragmentId").stringValue);
                string n = so.FindProperty("yarnNode").stringValue;
                Ok($"  '{obj}' yarnNode = {nodo}", n == nodo, n);
                nodosUsados.Add(n);
                Ok($"  '{obj}' en la capa Interactable", t.gameObject.layer == LayerMask.NameToLayer("Interactable"));
                var col = t.GetComponent<Collider2D>();
                Ok($"  '{obj}' collider Is Trigger", col != null && col.isTrigger);
                var rev = t.GetComponent<Revealable>();
                Ok($"  '{obj}' {(oculto ? "TIENE" : "no tiene")} Revealable", (rev != null) == oculto);
                if (oculto && rev != null)
                    Ok($"  '{obj}' revealId = {id}_rev",
                       new SerializedObject(rev).FindProperty("revealId").stringValue == $"{id}_rev");
            }
        }

        static void VerificarNiebla()
        {
            var go = Buscar("Niebla del Olvido"); if (go == null) return;
            var f = go.GetComponent<OblivionFog>();
            if (!Ok("Niebla: OblivionFog", f != null)) return;
            var so = new SerializedObject(f);
            Ok("Niebla: Unlock By = Fragment", so.FindProperty("unlockBy").enumValueIndex == (int)OblivionFog.Unlock.Fragment);
            Ok("Niebla: unlockId = M0_delantal", so.FindProperty("unlockId").stringValue == "M0_delantal",
               so.FindProperty("unlockId").stringValue);
            var col = go.GetComponent<Collider2D>();
            Ok("Niebla: collider NO trigger (bloquea el paso)", col != null && !col.isTrigger);
        }

        static void VerificarMemento()
        {
            var go = Buscar("Memento_PanDeMuerto"); if (go == null) return;
            var m = go.GetComponent<Memento>();
            if (!Ok("Memento presente", m != null)) return;
            var so = new SerializedObject(m);
            string b = so.FindProperty("lockedNode").stringValue, a = so.FindProperty("activateNode").stringValue;
            Ok("Memento: lockedNode = M0_Memento_Bloqueado", b == "M0_Memento_Bloqueado", b);
            Ok("Memento: activateNode = M0_Memento_Activar", a == "M0_Memento_Activar", a);
            Ok("Memento: glow asignado", so.FindProperty("glow").objectReferenceValue != null);
            nodosUsados.Add(b); nodosUsados.Add(a);
        }

        static void VerificarLucia()
        {
            var go = Buscar("Lucia"); if (go == null) return;
            var d = go.GetComponent<DialogueInteractable>();
            if (!Ok("Lucía: DialogueInteractable", d != null)) return;
            string n = new SerializedObject(d).FindProperty("yarnNode").stringValue;
            Ok("Lucía: yarnNode = M0_Lucia", n == "M0_Lucia", n);
            nodosUsados.Add(n);
        }

        static void VerificarTriggers()
        {
            var tabla = new (string obj, string nodo, string saveId, bool onStart)[]
            {
                ("Trigger_Prologo",        "M0_Prologo",        "M0_prologo",    true),
                ("Trigger_TutorialVela",   "M0_TutorialVela",   "M0_tut_vela",   false),
                ("Trigger_TutorialNiebla", "M0_TutorialNiebla", "M0_tut_niebla", false),
            };
            var padre = Buscar("Triggers"); if (padre == null) return;
            foreach (var (obj, nodo, saveId, onStart) in tabla)
            {
                var t = padre.transform.Find(obj);
                if (!Ok($"Trigger '{obj}' existe", t != null)) continue;
                var d = t.GetComponent<DialogueTrigger>();
                if (!Ok($"  '{obj}' tiene DialogueTrigger", d != null)) continue;
                var so = new SerializedObject(d);
                string n = so.FindProperty("yarnNode").stringValue;
                Ok($"  '{obj}' yarnNode = {nodo}", n == nodo, n);
                Ok($"  '{obj}' saveId = {saveId}", so.FindProperty("saveId").stringValue == saveId);
                Ok($"  '{obj}' playOnStart = {onStart}", so.FindProperty("playOnStart").boolValue == onStart);
                nodosUsados.Add(n);
            }
        }

        static void VerificarHUD()
        {
            var go = Buscar("HUD"); if (go == null) return;
            var ui = go.GetComponent<FragmentCounterUI>();
            if (!Ok("HUD: FragmentCounterUI", ui != null)) return;
            var so = new SerializedObject(ui);
            Ok("HUD: mapNameText asignado", so.FindProperty("mapNameText").objectReferenceValue != null);
            Ok("HUD: counterText asignado", so.FindProperty("counterText").objectReferenceValue != null);
            var slots = so.FindProperty("slots");
            Ok("HUD: 4 rombos", slots.arraySize == 4, $"{slots.arraySize}");
            bool todos = true;
            for (int i = 0; i < slots.arraySize; i++)
                if (slots.GetArrayElementAtIndex(i).objectReferenceValue == null) todos = false;
            Ok("HUD: los 4 rombos asignados", todos);
        }

        static void VerificarNodosYarn()
        {
            var yp = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(
                "Assets/_Proyecto/Dialogue/MientrasArdaLaVela.yarnproject");
            if (!Ok("Yarn Project encontrado", yp != null)) return;
            var existentes = new HashSet<string>(yp.NodeNames);
            foreach (var n in nodosUsados.Distinct().Where(s => !string.IsNullOrEmpty(s)))
                Ok($"Nodo Yarn '{n}' existe en el proyecto", existentes.Contains(n));
        }

        // ---------- utilidades ----------
        static GameObject Buscar(string nombre)
        {
            var go = GameObject.Find(nombre);
            if (go == null) { fallos++; Debug.LogError($"FALLA  · falta el objeto '{nombre}'"); }
            return go;
        }

        static bool Ok(string desc, bool cond, string detalle = null)
        {
            if (cond) Debug.Log($"  ok   · {desc}");
            else { fallos++; Debug.LogError($"FALLA  · {desc}{(detalle != null ? $" (valor: '{detalle}')" : "")}"); }
            return cond;
        }
    }
}
